using UnityEngine;
using System.Collections;

public class NoiseGen : MonoBehaviour {

    public static int octaves = 2;
    public static float persistence = 0.5f;
    public static float smoothness = 0.02f;

    // Cubic interpolation

    static float cosInterpolate(float a, float b, float x)
    {
        float ft = x * Mathf.PI;
        float f = (1 - Mathf.Cos(ft)) * 0.5f;

        return a * (1 - f) + b * f;
    }

    // Computes the dot product of the distance and gradient vectors.
    static float dotGridGradient(int ix, int iy, float x, float y)
    {
        // Compute the distance vector
        float dx = x - (float)ix;
        float dy = y - (float)iy;
 
        // Compute the dot-product
        return (dx* get2DNodeVector(ix,iy).x + dy * get2DNodeVector(ix, iy).y);
    }
    
    // Generate unsmoothed Perlin noise value at (x, y) 
    public static float unsmoothedPerlin(float x,float y)
    {
        // Determine grid cell coordinates
        int x0 = (x > 0.0 ? (int)x : (int)x - 1);
        int x1 = x0 + 1;
        int y0 = (y > 0.0 ? (int)y : (int)y - 1);
        int y1 = y0 + 1;

        // Determine interpolation weights
        // Could also use higher order polynomial/s-curve here
        float sx = x - (float)x0;
        float sy = y - (float)y0;

        // Interpolate between grid point gradients
        float n0, n1, ix0, ix1, value;
        n0 = dotGridGradient(x0, y0, x, y);
        n1 = dotGridGradient(x1, y0, x, y);
        ix0 = cosInterpolate(n0, n1, sx);
        n0 = dotGridGradient(x0, y1, x, y);
        n1 = dotGridGradient(x1, y1, x, y);
        ix1 = cosInterpolate(n0, n1, sx);
        value = cosInterpolate(ix0, ix1, sy);

        return value;
    }

    // Generate a smoothed Perlin noise value at (x, y) 
    public static float genPerlin(float x, float y)
    {
        float adjusted_x = x / smoothness;
        float adjusted_y = y / smoothness;
        float total = 0;

        for(int i=0;i< octaves; i++)
        {
            float frequency = Mathf.Pow(2, i);
            float amplitude = Mathf.Pow(persistence, i);

            total += unsmoothedPerlin(adjusted_x * frequency, adjusted_y * frequency) * amplitude;
        }



        return total;
    }

    // Generates a vector for a grid node at x,y
    private static Vector2 get2DNodeVector(int x, int y)
    {
        //Vector2 grid_node = new Vector2(x, y);
        int seed = hash(x, y);
        //Debug.Log(seed);
        UnityRandom urand = new UnityRandom(seed);

        return urand.PointInASquare();
    }

    // Generates an int from an x and a y value
    private static int hash(int x,int y)
    {
        int total = x << 16 % 3 + y % 5;

        return total;
    }

}
