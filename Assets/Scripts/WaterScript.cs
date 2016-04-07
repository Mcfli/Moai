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
    public int resolution;
    public int waveLoadDist = 1;
    public Vector3 waveHeight; // Y is actually Diagonal
    public Vector3 waveSpeed;
    public Vector3 waveLength;
    public Vector3 waveOffset;
    public bool invertEveryOther;

    //references
    private GenerationManager genManager;

    private Dictionary<Vector2, Mesh> waterMeshes;
    private Dictionary<Vector2, Renderer> waterRenderers;

    //finals
    private float chunk_size;
    private GameObject WaterParent;

    //underwater effects
    private bool defaultFog;
    private Color defaultFogColor;
    private float defaultFogDensity;

    // Use this for initialization
    void Awake () {
        genManager = GetComponent<GenerationManager>();
        chunk_size = genManager.chunk_size;
        Globals.water_level = InitialWaterLevel;
        WaterParent = new GameObject("Water");
        WaterParent.transform.parent = transform;
        WaterParent.transform.position = new Vector3(0, Globals.water_level, 0);

        waterMeshes = new Dictionary<Vector2, Mesh>();
        waterRenderers = new Dictionary<Vector2, Renderer>();

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
                Vector2 coordinates = new Vector2(x, z);
                if(waterRenderers[coordinates].isVisible) updateVertices(waterMeshes[coordinates], coordinates);
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

    private void updateVertices(Mesh m, Vector2 chunkCoordinates) {
        Vector3[] vertices = m.vertices;
        Vector3[] normals = m.normals;
        for(int i = 0; i < vertices.Length; i++) {
            Vector3 vertex = vertices[i];
            vertex.y = Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.x) * waveSpeed.x + (chunkCoordinates.x * chunk_size + vertex.x) * waveLength.x) * waveHeight.x;
            vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.y) * waveSpeed.y + ((chunkCoordinates.x + chunkCoordinates.y) * chunk_size + vertex.x + vertex.z) * waveLength.y) * waveHeight.y;
            vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.z) * waveSpeed.z + (chunkCoordinates.y * chunk_size + vertex.z) * waveLength.z) * waveHeight.z;
            if(Mathf.Repeat(Mathf.Round((vertex.x + vertex.z) / chunk_size * (resolution - 1)) + chunkCoordinates.x + chunkCoordinates.y, 2) == 1 && invertEveryOther) vertex.y *= -1;
            vertices[i] = vertex;
            if(Mathf.Repeat(i, 3) == 2) {
                Vector3 side1 = vertices[i-2] - vertices[i];
                Vector3 side2 = vertices[i-1] - vertices[i];
                Vector3 perp = Vector3.Cross(side1, side2);
                normals[i] = perp.normalized;
                normals[i-1] = perp.normalized;
                normals[i-2] = perp.normalized;
            }
        }
        m.vertices = vertices;
        m.normals = normals;
    }

    public GameObject generate(Vector2 coordinates){
        GameObject water = new GameObject();
        water.layer = LayerMask.NameToLayer("Water");
        water.name = "water (" + coordinates.x + "," + coordinates.y + ")";
        water.transform.parent = WaterParent.transform;
        MeshRenderer mr = water.AddComponent<MeshRenderer>();
        mr.material = WaterMaterial;
        MeshFilter mf = water.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.name = "water (" + coordinates.x + "," + coordinates.y + ")";
        waterMeshes.Add(coordinates, mf.mesh);
        waterRenderers.Add(coordinates, mr);

        water.transform.localPosition = new Vector3(coordinates.x * chunk_size, 0, coordinates.y * chunk_size); //position

        // Generate verticies
        Vector3[] vertices = new Vector3[(resolution * resolution)];
        for (int iy = 0; iy < resolution; iy++){
            for (int ix = 0; ix < resolution; ix++){
                float x = ix * chunk_size / (resolution - 1);
                float y = iy * chunk_size / (resolution - 1);
                vertices[iy * resolution + ix] = new Vector3(x, 0, y);
            }
        }
        mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < resolution - 1; y++){
            for (int x = 0; x < resolution - 1; x++){
                int v1 = x + y * resolution;
                int v2 = (x + 1) + y * resolution;
                int v3 = x + (y + 1) * resolution;
                int v4 = (x + 1) + (y + 1) * resolution;

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
        updateVertices(mf.mesh, coordinates);

        return water;
    }

    public void removeMesh(Vector2 coordinates) {
        waterMeshes.Remove(coordinates);
        waterRenderers.Remove(coordinates);
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
