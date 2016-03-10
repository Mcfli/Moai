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
    public List<Material> SkyGradients;
    //must be length of 6, starting from midnight color
    //fill this with skybox materials

    // Curves that define a property given what time of year it is
    public AnimationCurve temperatureCurve; 
    public AnimationCurve precipitationCurve;

    // Tree types
    public List<GameObject> treeTypes;
    public List<GameObject> doodads;

    public int treeDensity;
    public int doodadDensity;

    // Terrain color
    public List<Color> colors;
    public float amplitude;
    public AnimationCurve colorCurve;

    public void Init()
    {
        colors = new List<Color>();
        treeTypes = new List<GameObject>();
        doodads = new List<GameObject>();
        weatherTypes = new List<string>();
    }

    // Returns the vertex color for a vertex in this Biome at a given height value
    public Color colorAt(float height){
        float h = height / amplitude;
        if (h > 1.000f) h = 1.000f;
        else if (h < 0f) h = 0f;

        float val = (colors.Count - 1) * colorCurve.Evaluate(h);

        int i0 = Mathf.FloorToInt(val);
        int i1 = Mathf.CeilToInt(val);

        if (i0 < 0) i0 = 0;
        if (i1 > colors.Count - 1) i1 = colors.Count - 1;

        float weight = i1 - val;
        if (weight == 0) return colors[i0];

        Color color = Color.Lerp(colors[i1], colors[i0], weight);

        return color;
    }
    
    // Creates a new Biome whose parameters consist of a random mixing of two input Biomes' paramaters
    public static Biome Combine(Biome a, Biome b){
        Biome c = new Biome();
        c.Init();

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
        weight = Random.Range(0.0f, 1.0f);
        c.colorCurve = weight < 0.5f ? a.colorCurve : b.colorCurve;
        c.amplitude = (a.amplitude + b.amplitude) * 0.5f;

        // Combine trees and doodads
        foreach(GameObject tree in a.treeTypes)
        {
            weight = Random.Range(0.0f, 1.0f);
            if (weight > 0.5f) c.treeTypes.Add(tree);
        }

        foreach (GameObject tree in b.treeTypes)
        {
            weight = Random.Range(0.0f, 1.0f);
            if (weight > 0.5f) c.treeTypes.Add(tree);
        }

        // Combine colors for each biome
        int ai = 0, bi = 0;

        while(ai < a.colors.Count && bi < b.colors.Count)
        {
            Color chosen;
            Color ca = Color.black,cb = Color.black;

            weight = Random.Range(0.0f, 1.0f);

            if (a.colors.Count < ai)
            {
                ca = a.colors[ai];
                
            }

            if (b.colors.Count < bi)
            {
                cb = b.colors[bi];
                
            }

            if (ca == Color.black && cb != Color.black)
                chosen = cb;
            else if (ca != Color.black && cb == Color.black)
                chosen = ca;
            else
                chosen = weight > 0.5f ? ca : cb; 

            c.colors.Add(chosen);
            bi++;
            ai++;
        }
        return c;
    }
}
