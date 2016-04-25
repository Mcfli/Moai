using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Biome : MonoBehaviour {
    // Climate properties
    public float WaterFire;
    public float EarthAir;

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

    // Doodads, type and desnity. doodads[0] gives first kind of doodad and 
    // doodadDensity[0] gives how many of that doodad to spawn
    public List<GameObject> doodads;
    public List<float> doodadDensity;
    public Vector2 numOfDoodadsMinMax;

    public int treeDensity = 2;
    public float forestRadius = 50;
    public int forestMaxTrees = 16;
    public bool mixedForests = false;

    // Terrain color
    public List<Color> colors;
    public float amplitude;
    public AnimationCurve colorCurve;

    public void Init()
    {
        colors = new List<Color>();
        treeTypes = new List<GameObject>();
        doodads = new List<GameObject>();
        weatherTypes = new List<Weather>();
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
}
