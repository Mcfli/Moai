using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Biome : MonoBehaviour {
    // Climate properties
    public float heatAvg;
    public float heatVariance;
    public float moistureAvg;
    public float moistureVariance;

    public List<Weather> weatherTypes;
    public List<float> weatherChance; //must be same size as weatherTypes
    //sky color goes here

    // Curves that define a property given what time of year it is
    public AnimationCurve temperatureCurve; 
    public AnimationCurve precipitationCurve;

    // Tree types
    public List<GameObject> treeTypes;
    public List<GameObject> doodads;

    // Terrain color
    public Color[] colors;
    public float amplitude;
    public AnimationCurve color_curve;

    // Returns the vertex color for a vertex in this Biome at a given height value
    public Color colorAt(float height){
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
    
    // Creates a new Biome whose parameters consist of a random mixing of two input Biomes' paramaters
    public static Biome Combine(Biome a, Biome b){
        Biome c = new Biome();

        // Combine climate values
        float weight = Random.Range(0.1f, 0.9f);
        c.heatAvg =  weight * a.heatAvg + (1 - weight) * b.heatAvg;
        weight = Random.Range(0.1f, 0.9f);
        c.heatVariance = weight * a.heatVariance + (1 - weight) * b.heatVariance;
        weight = Random.Range(0.1f, 0.9f);
        c.moistureAvg = weight * a.moistureAvg + (1 - weight) * b.moistureAvg;
        weight = Random.Range(0.1f, 0.9f);
        c.moistureVariance = weight * a.moistureVariance + (1 - weight) * b.moistureVariance;
        weight = Random.Range(0.0f, 1.0f);
        c.temperatureCurve = weight < 0.5f ? a.temperatureCurve : b.temperatureCurve;
        weight = Random.Range(0.0f, 1.0f);
        c.precipitationCurve = weight < 0.5f ? a.precipitationCurve : b.precipitationCurve;

        // Combine trees and doodads
        int minLen = a.treeTypes.Count < b.treeTypes.Count ? a.treeTypes.Count : b.treeTypes.Count;
        int i;
        for (i = 0; i < minLen; i++)
        {
            weight = Random.Range(0.0f, 1.0f);
            if (weight < 0.5f) c.treeTypes.Add(a.treeTypes[i]);
            else c.treeTypes.Add(b.treeTypes[i]);
        }

        return c;
    }
}
