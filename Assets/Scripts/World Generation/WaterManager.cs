using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterManager : MonoBehaviour {
    // Tuning variables
    public float waterResolution = 0.4f; // Number of vertices in one unity unit length
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
    public void groupBodiesInChunk(Vector2 chunk)
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
                    
                    // Create a new water body, forget its two parents
                    GameObject newWater =
                        createWater(chunk, (water.center + otherWater.center) * 0.5f, water.size + otherWater.size, water.biome);
                    /*
                    Destroy(body);
                    Destroy(other);
                    waterBodies[chunk].Remove(body);
                    waterBodies[chunk].Remove(other);
                    waterBodies[chunk].Add(newWater);
                    i = 0;
                    j = 0;*/
                }

                // Otherwise just keep looking for overlaps
            }
        }
    }

    // Creates a water body game object of specified size at chunk specified by key
    public GameObject createWater(Vector2 chunk,Vector3 center, Vector3 size, Biome biome)
    {
        float sideLength = 1.5f * size.x;
        int resolution = Mathf.CeilToInt(size.x * waterResolution);
        float stepSize = sideLength / (resolution - 1);

        // Stick water to ground
        RaycastHit hit;
        Ray rayTopLeft = new Ray(new Vector3(center.x - sideLength * 0.35f, 10000000, center.z - sideLength * 0.35f), Vector3.down);
        Ray rayTopRight = new Ray(new Vector3(center.x + sideLength * 0.35f, 10000000, center.z - sideLength * 0.35f), Vector3.down);
        Ray rayBottomLeft = new Ray(new Vector3(center.x - sideLength * 0.35f, 10000000, center.z + sideLength * 0.35f), Vector3.down);
        Ray rayBottomRight = new Ray(new Vector3(center.x + sideLength * 0.35f, 10000000, center.z + sideLength * 0.35f), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        float tlHeight = -1f, trHeight = -1f, blHeight = -1f, brHeight = -1f, minHeight, avgHeight;

        if (Physics.Raycast(rayTopLeft, out hit, Mathf.Infinity, terrain))
            tlHeight = hit.point.y;
        if (Physics.Raycast(rayTopRight, out hit, Mathf.Infinity, terrain))
            trHeight = hit.point.y;
        if (Physics.Raycast(rayBottomLeft, out hit, Mathf.Infinity, terrain))
            blHeight = hit.point.y;
        if (Physics.Raycast(rayBottomRight, out hit, Mathf.Infinity, terrain))
            brHeight = hit.point.y;

        avgHeight = tlHeight + trHeight + blHeight + brHeight;
        float tlDev = Mathf.Abs(avgHeight - tlHeight);
        float trDev = Mathf.Abs(avgHeight - trHeight);
        float blDev = Mathf.Abs(avgHeight - blHeight);
        float brDev = Mathf.Abs(avgHeight - brHeight);

        if (tlDev + trDev + blDev + brDev > acceptableHeightDiff) return null;

        minHeight = Mathf.Min(tlHeight, trHeight, blHeight, brHeight);

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

        water.transform.position = new Vector3(center.x, minHeight - 0.2f, center.z);

        // Keep track of this water body
        if (!waterBodies.ContainsKey(chunk) || waterBodies[chunk] == null) waterBodies[chunk] = new List<GameObject>();
        waterBodies[chunk].Add(water);

        // Generate verticies
       
        Vector3[] vertices = new Vector3[(resolution * resolution)];
        for (int iy = 0; iy < resolution; iy++)
        {
            for (int ix = 0; ix < resolution; ix++)
            {
                float x = ix * stepSize - sideLength * 0.5f;
                float y = iy * stepSize - sideLength * 0.5f;
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
