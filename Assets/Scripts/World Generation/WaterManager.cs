using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterManager : MonoBehaviour {
    // Tuning variables
    public float waterResolution = 0.1f; // Number of vertices in one unity unit length
    public float acceptableHeightDiff = 10f;

    // Chunk -> water object list dictionary
    private static Dictionary<Vector2, List<GameObject>> waterBodies;
    
    // References
    private GameObject waterParent;

    // Use this for initialization
    void Awake (){
        waterBodies = new Dictionary<Vector2, List<GameObject>>();
        waterParent = new GameObject("Water");
        waterParent.transform.parent = transform;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // Looks through each water body in the chunk to determine interections
    public void getRidOfOverlaps(Vector2 chunk)
    {
        if (!waterBodies.ContainsKey(chunk) || waterBodies[chunk] == null) return;
        for (int i = 0; i < waterBodies[chunk].Count;i++)
        {
            for (int j = 0; j < waterBodies[chunk].Count; j++)
            {
                GameObject body = waterBodies[chunk][i];
                GameObject other = waterBodies[chunk][j];
                if (body == other) continue;
                WaterBody water = body.GetComponent<WaterBody>();
                WaterBody otherWater = other.GetComponent<WaterBody>();

                // If overlap found
                if (water.overlaps(otherWater)){
                    // Destroy overlapping water bodies
                    Destroy(body);
                    waterBodies[chunk].Remove(body);
                    i = 0;
                    j = 0;
                }

                // Otherwise just keep looking for overlaps
            }
        }
    }

    // Creates a water body game object of specified size at chunk specified by key
    public GameObject createWater(Vector2 chunk,Vector3 center, Vector3 size, Biome biome)
    {
        

        // Set up empty game object
        GameObject water = new GameObject();
        water.layer = LayerMask.NameToLayer("Water");
        water.name = "Lake";
        water.transform.parent = waterParent.transform;
        MeshRenderer mr = water.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.material = biome.waterMaterial;
        MeshFilter mf = water.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.name = "LakeMesh";

        WaterBody wb = water.AddComponent<WaterBody>();
        wb.center = center;
        wb.size = size;
        wb.biome = biome;
        wb.waterResolution = waterResolution;

        water.transform.position = new Vector3(center.x, 0, center.z);

        // Keep track of this water body
        if (!waterBodies.ContainsKey(chunk) || waterBodies[chunk] == null) waterBodies[chunk] = new List<GameObject>();
        waterBodies[chunk].Add(water);

        return water;
    }

    // unloads all water bodies in chunk specified by key
    public void unloadWater(Vector2 key)
    {
        foreach(GameObject obj in waterBodies[key])
        {
            Destroy(obj);
        }
    }
}
