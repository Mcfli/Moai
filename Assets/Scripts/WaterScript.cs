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
    public float waveHeight = 10.0f;
    public float waveSpeed = 1.0f;
    public float waveLength = 1.0f;
    public Vector2 randomHeight = new Vector2(0, 0.2f); //range
    public Vector2 randomSpeed = new Vector2(0, 5.0f); //range
    public NoiseGen randomHeightMap;
    public NoiseGen randomSpeedMap;

    //references
    private GenerationManager genManager;

    private Vector2 oldChunk;
    private List<Mesh> animatedMeshes;
    private List<Vector2[]> randomHeightSpeed;
    private Vector2 curr_randomHeight; //range
    private Vector2 curr_randomSpeed; //range

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

        animatedMeshes = new List<Mesh>();
        randomHeightSpeed = new List<Vector2[]>();

        // underwater effects
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
    }

    void Start() {
        oldChunk = Globals.cur_chunk;
        updateAnimatedMeshes();
    }
	
	// Update is called once per frame
	void Update () {
        WaterParent.transform.position = new Vector3(0, Globals.water_level, 0);

        if(oldChunk != Globals.cur_chunk || randomHeight != curr_randomHeight || randomSpeed != curr_randomSpeed) {
            oldChunk = Globals.cur_chunk;
            updateAnimatedMeshes();
        }

        //wave
        for(int i = 0; i < animatedMeshes.Count; i++) {
            Vector3[] vertices = animatedMeshes[i].vertices;
            int chunkX = animatedMeshes[i].name[7] - '0';
            //int chunkY = animatedMeshes[i].name[9] - '0';
            for(int j = 0; j < vertices.Length; j++) {
                Vector3 vertex = vertices[j];
                vertex.y = Mathf.Sin(Globals.time / Globals.time_resolution * waveSpeed + (chunkX * chunk_size + vertex.x) * waveLength) * waveHeight;  // wave
                vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + 10) * randomHeightSpeed[i][j].y) * randomHeightSpeed[i][j].x;           // individual
                vertices[j] = vertex;
            }
            animatedMeshes[i].vertices = vertices;
            animatedMeshes[i].RecalculateNormals();
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
        water.layer = LayerMask.NameToLayer("Water");
        water.name = "water (" + coordinates.x + "," + coordinates.y + ")";
        water.transform.parent = WaterParent.transform;
        MeshRenderer mr = water.AddComponent<MeshRenderer>();
        mr.material = WaterMaterial;
        MeshFilter mf = water.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.name = "water (" + coordinates.x + "," + coordinates.y + ")";

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
        //mf.mesh.RecalculateBounds();
        //mf.mesh.RecalculateNormals();
        //water.AddComponent(typeof(MeshCollider));
    }

    private void updateAnimatedMeshes() {
        curr_randomSpeed = randomSpeed;
        curr_randomHeight = randomHeight;
        animatedMeshes = new List<Mesh>();
        randomHeightSpeed = new List<Vector2[]>();
        for(float x = Globals.cur_chunk.x - waveLoadDist; x <= Globals.cur_chunk.x + waveLoadDist; x++) {
            for(float y = Globals.cur_chunk.y - waveLoadDist; y <= Globals.cur_chunk.y + waveLoadDist; y++) {
                Mesh m = GameObject.Find("water (" + x + "," + y + ")").GetComponent<MeshFilter>().mesh;
                animatedMeshes.Add(m);
                Vector2[] heightSpeed = new Vector2[m.vertexCount];
                for(int i = 0; i < m.vertexCount; i++) {
                    float height, speed;

                    if(randomHeight.x > randomHeight.y || randomHeight.y == 0) height = 0;
                    else if(randomHeight.x == randomHeight.y) height = randomHeight.y;
                    else height = Mathf.Repeat(randomHeightMap.genPerlin(x * chunk_size + m.vertices[i].x, y * chunk_size + m.vertices[i].z, 0), randomHeight.y - randomHeight.x) + randomHeight.x;
                    if(Mathf.Repeat(Mathf.Ceil((m.vertices[i].x + m.vertices[i].z) / chunk_size * (chunk_resolution - 1)) + x + y, 2) == 1) height *= -1;

                    if(randomSpeed.x > randomSpeed.y || randomSpeed.y == 0) speed = 0;
                    else if(randomSpeed.x == randomSpeed.y) speed = randomSpeed.y;
                    else speed = Mathf.Repeat(randomSpeedMap.genPerlin(x * chunk_size + m.vertices[i].x, y * chunk_size + m.vertices[i].z, 0), randomSpeed.y - randomSpeed.x) + randomSpeed.x;

                    heightSpeed[i] = new Vector2(height, speed);
                }
                randomHeightSpeed.Add(heightSpeed);
            }
        }
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
