using UnityEngine;
using System.Collections;

public class NoiseGen : MonoBehaviour {

    public static int octaves = 2;
    public static float persistence = 0.5f;
    public static float smoothness = 0.02f;

    private static UnityRandom urand;

    void Awake()
    {
        UnityRandom urand = new UnityRandom();
    }

    // Cubic interpolation

    static float cosInterpolate(float a, float b, float x)
    {
        float ft = x * Mathf.PI;
        float f = (1 - Mathf.Cos(ft)) * 0.5f;

        return a * (1 - f) + b * f;
    }

    // Computes the dot product of the distance and gradient vectors.
    static float dotGridGradient(int ix, int iy,int iz, float x, float y, float z)
    {
        // Compute the distance vector
        float dx = x - (float)ix;
        float dy = y - (float)iy;
        float dz = z - (float)iz;
 
        // Compute the dot-product
        return (dx* getNodeVector(ix,iy,iz).x + dy * getNodeVector(ix, iy,iz).y + dz * getNodeVector(ix, iy,iz).y);
    }
    
    // Generate unsmoothed Perlin noise value at (x, y) 
    public static float unsmoothedPerlin(float x,float y,float time)
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
        float n0, n1, ix0, ix1, ix2, ix3, p1, p2, value;

        // Front plane in cube 

        n0 = dotGridGradient(x0, y0, t0, x, y,time);
        n1 = dotGridGradient(x1, y0, t0, x, y,time);
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

        return value;
    }

    // Generate a smoothed Perlin noise value at (x, y, time) 
    public static float genPerlin(float x, float y, float time)
    {
        float adjusted_x = x / smoothness;
        float adjusted_y = y / smoothness;

        float total = 0;

        for(int i=0;i< octaves; i++)
        {
            float frequency = Mathf.Pow(2, i);
            float amplitude = Mathf.Pow(persistence, i);

            total += unsmoothedPerlin(adjusted_x * frequency, adjusted_y * frequency,time*frequency) * amplitude;
        }

        return total;
    }

    // Generates a vector for a grid node at x,y,z
    private static Vector2 getNodeVector(int x, int y,int z)
    {
        Random.seed = hash(x, y, z);
        return new Vector2(Random.value, Random.value);
        //Vector2 grid_node = new Vector2(x, y);
        int seed = hash(x, y, z);
        //Debug.Log(seed);
        //urand. = (seed);

        return urand.PointInASquare();
    }

    // Generates an int from an x and a y value
    private static int hash(int x,int y, int z)
    {
        int total = ((x * 73856093) ^ (y * 19349663) ^ (z * 83492791)) % 263;

        return total;
    }

}
