using UnityEngine;
using System.Collections;

public class NoiseGen : MonoBehaviour
{

    public int octaves = 1;
    public float persistence = 0.5f;
    public float smoothness = 100.0f;
    public float amplitude = 100.0f;
    public int seed = 9647;

    private static int x_o;
    private static int y_o;
    private static int z_o;
    private static int w_o;

    private float smoothness_inv;

    public void Init()
    {
        smoothness_inv = 1 / smoothness;
        Random.seed += seed;
        x_o = Random.Range(2,int.MaxValue);
        y_o = Random.Range(2, int.MaxValue);
        z_o = Random.Range(2, int.MaxValue);
        w_o = Random.Range(2, int.MaxValue);
        Random.seed -= seed;
    }

    // Cubic interpolation

    public static float cosInterpolate(float a, float b, float x)
    {
        float ft = x * Mathf.PI;
        float f = (1 - Mathf.Cos(ft)) * 0.5f;

        return a * (1 - f) + b * f;
    }

    // Computes the dot product of the distance and gradient vectors.
    private float dotGridGradient(int ix, int iy, int iz, float x, float y, float z)
    {
        // Compute the distance vector
        float dx = x - (float)ix;
        float dy = y - (float)iy;
        float dz = z - (float)iz;

        Vector3 dist = new Vector3(dx, dy, dz);
        Vector3 node_vec = getNodeVector(ix, iy, iz);

        // Compute the dot-product
        return Vector3.Dot(dist,node_vec);
    }

    // Generate unsmoothed Perlin noise value at (x, y) 
    private float unsmoothedPerlin(float x, float y, float time)
    {
        // Determine grid cell coordinates
        int x0 = (x > 0.0 ? (int)x : (int)x - 1);
        int x1 = x0 + 1;
        int y0 = (y > 0.0 ? (int)y : (int)y - 1);
        int y1 = y0 + 1;
        int t0 = (time > 0.0 ? (int)time : (int)time - 1);
        int t1 = t0 + 1;

        // Determine interpolation weights
        // Could also use higher order polynomial/s-curve here
        float sx = x - (float)x0;
        float sy = y - (float)y0;
        float st = time - (float)t0;

        // Interpolate between grid point gradients
        float n0, n1, ix0, ix1, p1, p2, value;

        // Front plane in cube 

        n0 = dotGridGradient(x0, y0, t0, x, y, time);
        n1 = dotGridGradient(x1, y0, t0, x, y, time);
        ix0 = cosInterpolate(n0, n1, sx);

        n0 = dotGridGradient(x0, y1, t0, x, y, time);
        n1 = dotGridGradient(x1, y1, t0, x, y, time);
        ix1 = cosInterpolate(n0, n1, sx);

        p1 = cosInterpolate(ix0, ix1, sy);

        // Rear plane in cube 

        n0 = dotGridGradient(x0, y0, t1, x, y, time);
        n1 = dotGridGradient(x1, y0, t1, x, y, time);
        ix0 = cosInterpolate(n0, n1, sx);

        n0 = dotGridGradient(x0, y1, t1, x, y, time);
        n1 = dotGridGradient(x1, y1, t1, x, y, time);
        ix1 = cosInterpolate(n0, n1, sx);

        p2 = cosInterpolate(ix0, ix1, sy);

        value = cosInterpolate(p1, p2, st);

        value += 1;
        value *= 0.5f;

        return value;
    }

    // Generate a smoothed Perlin noise value at (x, y, time) 
    public float genPerlin(float x, float y, float time)
    {
        float adjusted_x = x / smoothness;
        float adjusted_y = y / smoothness;

        float total = 0f;
        float freq = 1f;
        float amp = 1f;

        for (int i = 0; i < octaves; i++)
        {
            total += unsmoothedPerlin(adjusted_x * freq, adjusted_y * freq, time * freq) * amp;
            freq *= 2;
            amp *= persistence;
        }

        return amplitude*total;
    }

    // Generate a 0-1 smoothed Perlin noise value at (x, y) 
    public float genPerlinUnscaled(float x, float y, float z)
    {

        float adjusted_x = x * smoothness_inv;
        float adjusted_y = y * smoothness_inv;
        float adjusted_z = z * smoothness_inv;

        float total = 0;
        float freq = 1;

        for (int i = 0; i < octaves; i++)
        {
            total += unsmoothedPerlin(adjusted_x * freq, adjusted_y * freq,adjusted_z * freq);
            freq *= 2;
        }
        total /= octaves;

        return total;
    }


    // Generate a craggy smoothed Perlin noise value at (x, y) 
    // Implemented from https://dip.felk.cvut.cz/browse/pdfcache/lindao1_2007bach.pdf
    public float genPerlinRidged(float x, float y, float z)
    {
        float adjusted_x = x * smoothness_inv;
        float adjusted_y = y * smoothness_inv;
        float adjusted_z = z * smoothness_inv;

        float total = 0;
        float lam = 2;
        float amp = 2;

        for (int i = 1; i < octaves + 1; i++)
        {
            amp *= persistence;
            lam *= 0.5f;
            float val = unsmoothedPerlin(adjusted_x / lam, adjusted_y / lam, adjusted_z / lam);
            val = val * 2f - 1f;
            if (val > 0) val = 1 - val;
            else val = val + 1f;
            total += 2 * (val + 1f) * amp;
        }

        return amplitude * total;
    }

    // Generates a vector for a grid node at x,y,z
    private Vector3 getNodeVector(int x, int y, int z)
    {
        int origSeed = Random.seed;
        Random.seed = hash(x, y, z);
        Vector3 result = new Vector3(Mathf.RoundToInt(2*Random.value-1), Mathf.RoundToInt(2 *Random.value-1),
            Mathf.RoundToInt(2 * Random.value - 1));
        Random.seed = origSeed;
        return result;

    }

    // Generates an int from an x and a y value
    public static int hash(int x, int y, int z)
    {
        int total = ((x * x_o) ^ (y * y_o) ^ (z * z_o)) % w_o;

        return total;
    }

}
