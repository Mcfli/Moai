using UnityEngine;
using System.Collections;

public class NoiseSynth : MonoBehaviour {

    public NoiseGen base_map;
    public NoiseGen type_map;
    public NoiseGen mountain_map;
    public Color[] colors;
    public AnimationCurve color_curve;
    public float amplitude;

    public void Init()
    {
        base_map.Init();
        type_map.Init();
        mountain_map.Init();
    }

    public float heightAt(float x, float y, float z)
    {
        float total = 0.0f;

        float m = mountain_map.genPerlinRidged(x,y,z)-3500f;
        float b = base_map.genPerlin(x,y,z);
        float w = type_map.genPerlinUnscaled(x,y,z);

        total = select(b, m, w, 0.6f, 0.125f);
        //Debug.Log(w);
        return total;
    }


    public Color colorAt(float height)
    {

        float h = height / amplitude;
        if (h > 1.000f) h = 1.000f;
        else if (h < 0f) h = 0f;

        float val = (colors.Length - 1) * color_curve.Evaluate(h);


        int i0 = Mathf.FloorToInt(val);
        int i1 = Mathf.CeilToInt(val);

        if (i0 < 0) i0 = 0;
        if (i1 > colors.Length - 1) i1 = colors.Length - 1;



        float weight = i1 - val;
        if (weight == 0) return colors[i0];

        Color color = Color.Lerp(colors[i1], colors[i0], weight);

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
