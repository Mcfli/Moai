using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterManager : MonoBehaviour {
    // Tuning variables
    public float waterResolution = 0.4f; // Number of vertices in one unity unit length


    // Chunk -> water object list dictionary
    private static Dictionary<Vector2, List<GameObject>> waterBodies;
    
    // References
    private GameObject waterParent;

    // Use this for initialization
    void Start () {
        waterBodies = new Dictionary<Vector2, List<GameObject>>();
        waterParent = new GameObject("Water");
        waterParent.transform.parent = transform;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // Creates a water body game object of specified size at chunk specified by key
    public void createWater(Vector2 chunk,Vector3 center, Vector3 size, Biome biome)
    {
        // Set up empty game object
        GameObject water = new GameObject();
        water.layer = LayerMask.NameToLayer("Water");
        water.name = "Lake";
        water.transform.parent = waterParent.transform;
        MeshRenderer mr = water.AddComponent<MeshRenderer>();
        mr.material = biome.waterMaterial;
        MeshFilter mf = water.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.name = "LakeMesh";
        

        // Stick water to ground
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(center.x, 10000000, center.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {
            water.transform.position = new Vector3(center.x, hit.point.y , center.z);
        }
        else return;

        // Keep track of this water body
        if (!waterBodies.ContainsKey(chunk) || waterBodies[chunk] == null) waterBodies[chunk] = new List<GameObject>();
        waterBodies[chunk].Add(water);

        // Generate verticies
        int resolution = Mathf.CeilToInt(size.x * waterResolution);
        float stepSize = size.x / (resolution - 1);
        Vector3[] vertices = new Vector3[(resolution * resolution)];
        for (int iy = 0; iy < resolution; iy++)
        {
            for (int ix = 0; ix < resolution; ix++)
            {
                float x = ix * stepSize - size.x * 0.5f;
                float y = iy * stepSize - size.x * 0.5f;
                vertices[iy * resolution + ix] = new Vector3(x, 0, y);
            }
        }
        mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int v1 = x + y * resolution;
                int v2 = (x + 1) + y * resolution;
                int v3 = x + (y + 1) * resolution;
                int v4 = (x + 1) + (y + 1) * resolution;

                if (Mathf.Repeat(x + y, 2) == 1)
                { //top left to bottom right
                    triangles[i] = v4;
                    triangles[i + 1] = v1;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v1;
                    triangles[i + 4] = v4;
                    triangles[i + 5] = v2;
                }
                else
                { //top right to bottom left
                    triangles[i] = v4;
                    triangles[i + 1] = v2;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v2;
                    triangles[i + 4] = v1;
                    triangles[i + 5] = v3;
                }

                i += 6;
            }
        }
        mf.mesh.triangles = triangles;

        ReCalcTriangles(mf.mesh);
    }

    // unloads all water bodies in chunk specified by key
    public void unloadWater(Vector2 key)
    {

    }

    private void ReCalcTriangles(Mesh mesh)
    {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];


        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];

            triangles[i] = i;

        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
