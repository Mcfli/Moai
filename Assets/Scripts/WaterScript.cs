using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterScript : MonoBehaviour {
    public float InitialWaterLevel;
    public Material WaterMaterial;
    //underwater effects
    public Color fogColor;
    public float fogDensity;

    //waves
    public int waveLoadDist = 1;
    public Vector3 waveHeight;
    public Vector3 waveSpeed;
    public Vector3 waveLength;
    public Vector3 waveOffset;

    //references
    private GenerationManager genManager;
    private Dictionary<Vector2, GameObject> waterObjects;
    private Dictionary<Vector2, Mesh> waterMeshes;

    //finals
    private int chunk_resolution;
    private float chunk_size;
    private GameObject WaterParent;

    //underwater effects
    private bool defaultFog;
    private Color defaultFogColor;
    private float defaultFogDensity;

    // Use this for initialization
    void Awake () {
        genManager = GetComponent<GenerationManager>();
        chunk_resolution = genManager.chunk_resolution;
        chunk_size = genManager.chunk_size;
        Globals.water_level = InitialWaterLevel;
        WaterParent = new GameObject("Water");
        WaterParent.transform.parent = transform;
        WaterParent.transform.position = new Vector3(0, Globals.water_level, 0);

        waterMeshes = new Dictionary<Vector2, Mesh>();
        waterObjects = new Dictionary<Vector2, GameObject>();

        // underwater effects
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
    }
	
	// Update is called once per frame
	void Update () {
        WaterParent.transform.position = new Vector3(0, Globals.water_level, 0);

        //waves
        for(float x = Globals.cur_chunk.x - waveLoadDist; x <= Globals.cur_chunk.x + waveLoadDist; x++) {
            for(float z = Globals.cur_chunk.y - waveLoadDist; z <= Globals.cur_chunk.y + waveLoadDist; z++) {
                Mesh m = waterMeshes[new Vector2(x, z)];
                Vector3[] vertices = m.vertices;
                for(int j = 0; j < vertices.Length; j++) {
                    Vector3 vertex = vertices[j];
                    vertex.y  = Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.x) * waveSpeed.x + ( x      * chunk_size + vertex.x           ) * waveLength.x) * waveHeight.x;  // X
                    vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.y) * waveSpeed.y + ((x + z) * chunk_size + vertex.x + vertex.z) * waveLength.y) * waveHeight.y; // D (labelled Y)
                    vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.z) * waveSpeed.z + (     z  * chunk_size +            vertex.z) * waveLength.z) * waveHeight.z; // Z
                    vertices[j] = vertex;
                }
                m.vertices = vertices;
                m.RecalculateNormals();
            }
        }

        //if underwater
        if (Globals.PlayerScript.isUnderwater()){
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }else{
            RenderSettings.fog = defaultFog;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;
        }
	}

    public void generate(Vector2 coordinates){
        GameObject water = new GameObject();
        waterObjects.Add(coordinates, water);
        water.layer = LayerMask.NameToLayer("Water");
        water.name = "water (" + coordinates.x + "," + coordinates.y + ")";
        water.transform.parent = WaterParent.transform;
        MeshRenderer mr = water.AddComponent<MeshRenderer>();
        mr.material = WaterMaterial;
        MeshFilter mf = water.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.name = "water (" + coordinates.x + "," + coordinates.y + ")";
        waterMeshes.Add(coordinates, mf.mesh);

        water.transform.localPosition = new Vector3(coordinates.x * chunk_size, 0, coordinates.y * chunk_size); //position

        // Generate verticies
        Vector3[] vertices = new Vector3[(chunk_resolution * chunk_resolution)];
        for (int iy = 0; iy < chunk_resolution; iy++){
            for (int ix = 0; ix < chunk_resolution; ix++){
                float x = ix * chunk_size / (chunk_resolution - 1);
                float y = iy * chunk_size / (chunk_resolution - 1);
                vertices[iy * chunk_resolution + ix] = new Vector3(x, 0, y);
            }
        }
        mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(chunk_resolution - 1) * (chunk_resolution - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < chunk_resolution - 1; y++){
            for (int x = 0; x < chunk_resolution - 1; x++){
                int v1 = x + y * chunk_resolution;
                int v2 = (x + 1) + y * chunk_resolution;
                int v3 = x + (y + 1) * chunk_resolution;
                int v4 = (x + 1) + (y + 1) * chunk_resolution;

                if(Mathf.Repeat(x+y,2) == 1) { //top left to bottom right
                    triangles[i] = v4;
                    triangles[i + 1] = v1;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v1;
                    triangles[i + 4] = v4;
                    triangles[i + 5] = v2;
                } else { //top right to bottom left
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
        //water.AddComponent(typeof(MeshCollider));
    }

    public void destroyWater(Vector2 coordinates) {
        Destroy(waterObjects[coordinates]);
        waterObjects.Remove(coordinates);
        waterMeshes.Remove(coordinates);
    }

    private void ReCalcTriangles(Mesh mesh) {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];


        for(int i = 0; i < triangles.Length; i++) {
            vertices[i] = oldVerts[triangles[i]];

            triangles[i] = i;

        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
