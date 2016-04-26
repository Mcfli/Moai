using UnityEngine;
using System.Collections;

public class NoiseSynth : MonoBehaviour {

    public NoiseGen base_map;
    public NoiseGen hill_map;
    public NoiseGen type_map;
    public NoiseGen mountain_map;
    public NoiseGen elevation_map;
    public NoiseGen peak_map;
    public NoiseGen lake_map;
    public Color[] colors;
    public AnimationCurve color_curve;
    public float amplitude;

    private float valleyHeight;

    public void Init()
    {
        base_map.Init();
        hill_map.Init();
        type_map.Init();
        mountain_map.Init();
        elevation_map.Init();
        peak_map.Init();
        lake_map.Init();

        valleyHeight = -0.07f * (elevation_map.amplitude + base_map.amplitude);
    }

    public float heightAt(float x, float y, float z)
    {
        float total = 0.0f;

        // Generate base value for each map
        float p = peak_map.genPerlin(x, y, z);
        float m = mountain_map.genPerlinRidged(x,y,z) + p - 13000;   
        float w = type_map.genPerlinUnscaled(x,y,z);
        float e = elevation_map.genPerlin(x, y, z);
        float b = base_map.genPerlin(x,y,z);
        float h = hill_map.genPerlin(x,y,z);
        float l = lake_map.genPerlin(x, y, z) - 50f;

        // Derived characteristics
        float elevationIndex = e / elevation_map.amplitude;
        float valley = Mathf.Max(valleyHeight, b);

        
        b = select(b, h, 0.5f * (elevationIndex + w), 0.5f, 0.05f);
        b = select(l, b, w, 0.75f, 0.07f);

        b = Mathf.Max(b, valley);

        // Select between base map and mountain map based on value from type map. Add elevation to it.
        total = select(b, m, w, 0.68f,0.175f ) 
            + e - elevation_map.amplitude * 0.5f;
        //total = m;
        //Debug.Log(w);
        return total;
    }


    public Color colorAt(float height)
    {

        float h = (height + Random.value*1500) / amplitude;
        if (h > 1.000f) h = 1.000f;
        else if (h < 0f) h = 0f;

        float val = (colors.Length - 1) * color_curve.Evaluate(h);


        int i = Mathf.RoundToInt(val);

        Color color = colors[i];

        return color;
    }

    //
    private float select(float h1, float h2, float weight, float div, float falloff)
    {
        float diff = weight - div;
        // Weight is outside of falloff range
        if (Mathf.Abs(diff) > falloff)
        {
            if (diff >= 0) return h2;
            else return h1;
        }
        else
        // Weight is inside falloff range
        {
            float w = diff / falloff;
            w = (w + 1) * 0.5f;
            return Mathf.Lerp(h1, h2, w);

        }

    }
}
