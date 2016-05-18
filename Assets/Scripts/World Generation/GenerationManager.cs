using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {
    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 6;
    public int chunk_unload_dist = 6;
    public int chunk_detail_dist = 1;
    public float XZDeviationRatio; //only deviates positively (sadly)
    public int XZDeviationSeed;
    public float detailDeviation;
    public int detailDeviationSeed;
    public int detailSubdivisions;
    public float allottedLoadSeconds = 1;
    public int tree_load_dist = 1;
    public int tree_unload_dist = 1;
    public List<Biome> biomes;
    public NoiseGen WaterFireMap;
    public NoiseGen mountainMap;
    public NoiseGen EarthAirMap;

    //lists
    private Dictionary<Vector2, GameObject> loaded_chunks;
   
    //private Dictionary<Vector2, Biome> chunkBiomes;  // keeps track of what chunk is at what biome

    // WaterFire/EarthAir modifiers per chunk.
    // maps chunk -> (delta_WaterFire,delta_EarthAir)
    private Dictionary<Vector2, Vector2> mapChanges;
    private bool doneLoading = false;

    //references
    private ChunkGenerator chunkGen;
    private TreeManager tree_manager;
    private WeatherManager weather_manager;
    private ShrineManager shrine_manager;
    private DoodadManager doodad_manager;
    private WaterManager water_manager;
    private NoiseSynth synth;

    private Material landMaterial;
    private GameObject TerrainParent;
    private int curDist = 0;
    private bool playerWarped = false;
    private Coroutine currentLoad;

    public System.DateTime endTime;

    void Awake() {
        //lists
        loaded_chunks = new Dictionary<Vector2, GameObject>();
        mapChanges = new Dictionary<Vector2, Vector2>();

        //references
        chunkGen = GetComponent<ChunkGenerator>();
        tree_manager = GetComponent<TreeManager>();
        weather_manager = GameObject.Find("Weather").GetComponent<WeatherManager>();
        shrine_manager = GetComponent<ShrineManager>();
        doodad_manager = GetComponent<DoodadManager>();
        water_manager = GetComponent<WaterManager>();
        synth = GetComponent<NoiseSynth>();

        TerrainParent = new GameObject("Terrain");
        TerrainParent.transform.parent = transform;

        landMaterial = Resources.Load("Materials/WorldGen/Ground") as Material;

        Globals.cur_chunk = worldToChunk(Globals.Player.transform.position);

        EarthAirMap.Init();
        WaterFireMap.Init();

        if(chunk_unload_dist < chunk_load_dist) chunk_unload_dist = chunk_load_dist;
        if(chunk_detail_dist > chunk_load_dist) chunk_detail_dist = chunk_load_dist;
    }

    void Start() {
        if(Globals.mode > -1) {
            //initiateWorld();
            
        }
    }
	
	// Update is called once per frame
	void Update () {
        Shader.SetGlobalFloat("_TimeVar", Globals.time / Globals.time_resolution);
        Vector2 current_chunk = worldToChunk(Globals.Player.transform.position);
        if(Globals.cur_chunk != current_chunk) {
            Globals.cur_chunk = current_chunk;
            weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
            Globals.cur_biome = chooseBiome(Globals.cur_chunk);
            doneLoading = false;
            curDist = 0;
            StopCoroutine(currentLoad);
            currentLoad = StartCoroutine("loadUnload", Globals.cur_chunk);
        }
        if( Globals.mode > -1) {
            endTime = System.DateTime.Now.AddSeconds(allottedLoadSeconds);
            
        }

        if (!playerWarped)
        {
            playerWarped = Globals.PlayerScript.warpToGround(10000000, true);
        }

    }

    // Called by menu on play
    public void initiateWorld() {
        Globals.time = 0;
        playerWarped = false;
        Globals.cur_chunk = worldToChunk(Globals.Player.transform.position);
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);
        doneLoading = false;
        curDist = 0;
        currentLoad = StartCoroutine("loadUnload", Globals.cur_chunk);
    }

    public void deleteWorld() { //burn it to the ground
        foreach(KeyValuePair<Vector2, Dictionary<int, ForestScript>> p in TreeManager.loadedForests) foreach(KeyValuePair<int, ForestScript> q in p.Value) q.Value.destroyForest();
        //foreach(Vector2 v in loaded_shrine_chunks) shrine_manager.unloadShrines((int)v.x, (int)v.y);
        foreach(KeyValuePair<Vector2, List<GameObject>> p in DoodadManager.loaded_doodads) foreach(GameObject g in p.Value) Destroy(g);
        foreach(KeyValuePair<Vector2, GameObject> p in loaded_chunks) {
            Destroy(loaded_chunks[p.Key]);
            water_manager.unloadWater(p.Key);
        }

        loaded_chunks = new Dictionary<Vector2, GameObject>();
        //detailed_chunks = new Dictionary<Vector2, ChunkMeshes>();
        //loaded_shrine_chunks = new List<Vector2>();
        //loaded_doodad_chunks = new List<Vector2>();
        mapChanges = new Dictionary<Vector2, Vector2>();
        TreeManager.trees = new Dictionary<Vector2, List<ForestScript.forestStruct>>();
        TreeManager.loadedForests = new Dictionary<Vector2, Dictionary<int, ForestScript>>();
        ShrineManager.shrines = new Dictionary<Vector2, List<ShrineGrid>>();
        WaterManager.waterBodies = new Dictionary<Vector2, List<GameObject>>();
        DoodadManager.loaded_doodads = new Dictionary<Vector2, List<GameObject>>();
    }
    
    private IEnumerator loadUnload(Vector2 position) {
        bool done = true;

        // Unload chunks if there are loaded chunks
        if (loaded_chunks.Keys.Count > 0)
        {
            List<Vector2> l = new List<Vector2>(loaded_chunks.Keys);
            foreach (Vector2 coordinates in l)
            {
                if (System.DateTime.Now >= endTime) yield return null;
                // Unload objects
                if (!inLoadDistance(position, coordinates, tree_unload_dist))
                {
                    if (!loaded_chunks.ContainsKey(coordinates)) continue;
                    ChunkMeshes chunkObj = loaded_chunks[coordinates].GetComponent<ChunkMeshes>();
                    if ((chunkObj.doneObjects ||chunkObj.loadingObjects) && !chunkObj.unloadedObjects)
                    {
                        done = false;
                        chunkObj.unloadObjects();
                    }
                }

                // Unload topology
                if (!inLoadDistance(position, coordinates, chunk_unload_dist))
                {
                    ChunkMeshes chunkObj = loaded_chunks[coordinates].GetComponent<ChunkMeshes>();

                    // Unload base stuff
                    if ((chunkObj.doneBase || chunkObj.loadingBase) && !chunkObj.unloadedBase)
                    {
                        done = false;
                        chunkObj.unloadBase();
                    }

                    // Destroy the chunk if necessary
                    else if (chunkObj.unloadedBase && (!chunkObj.doneObjects ||
                        chunkObj.unloadedObjects))
                    {
                        Destroy(loaded_chunks[coordinates]);
                        loaded_chunks.Remove(coordinates);
                    }
                }
            }
        }
        
        // Load chunks
        curDist = 0;
        while (curDist < chunk_load_dist)
        {
            if (System.DateTime.Now >= endTime) yield return null;
            for (int i = -curDist; i <= curDist; i++)
            {
                for (int j = -curDist; j <= curDist; j++)
                {
                    if (System.DateTime.Now >= endTime) yield return null;
                    if (i != curDist && i != -curDist &&
                       j != curDist && j != -curDist)
                        continue;
                    Vector2 thisChunk = new Vector2(position.x + i, position.y + j);
                    // If no chunk at these coordinates, make one
                    if (!loaded_chunks.ContainsKey(thisChunk))
                    {
                        createChunk(thisChunk);
                        done = false;
                    }
                    ChunkMeshes chunkObj = loaded_chunks[thisChunk].GetComponent<ChunkMeshes>();
                    
                    // If the chunk still needs loading, continue loading it
                    if (!chunkObj.doneBase)
                    {
                        chunkObj.loadBase();
                        done = false;
                    }
                    else
                    {
                        // If the chunk needs to be detailed, detail it
                        if (inLoadDistance(position, thisChunk, chunk_detail_dist) && !chunkObj.detailed)
                        {
                            chunkObj.mf.mesh = chunkObj.highMesh;
                            chunkObj.detailed = true;
                            done = false;
                        }

                        // If the chunk needs to be undetailed, undetail it
                        else if (!inLoadDistance(position, thisChunk, chunk_detail_dist) && chunkObj.detailed)
                        {
                            chunkObj.mf.mesh = chunkObj.lowMesh;
                            chunkObj.detailed = false;
                            done = false;
                        }
                    }

                    // If the chunk needs to load its objects, continue loading them
                    if (chunkObj.doneBase && inLoadDistance(position, thisChunk, tree_load_dist) && !chunkObj.doneObjects)
                    {
                        chunkObj.loadObjects();
                        done = false;
                    }
                }
            }
            curDist++;
        }
        doneLoading = done;
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);
    }

    private void createChunk(Vector2 coordinates)
    {
        GameObject chunk = new GameObject();
        ChunkMeshes chunkMeshes = chunk.AddComponent<ChunkMeshes>();
        chunkMeshes.coordinates = coordinates;
        chunkMeshes.setReferences(synth, this, tree_manager, shrine_manager, doodad_manager, water_manager);
        chunkMeshes.coordinates = coordinates;
        chunk.layer = LayerMask.NameToLayer("Terrain");
        chunk.name = "chunk (" + coordinates.x + "," + coordinates.y + ")";
        chunk.transform.parent = TerrainParent.transform;
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        mr.material = landMaterial;
        MeshFilter mf = chunk.AddComponent<MeshFilter>();
        chunkMeshes.mf = mf;
        loaded_chunks[coordinates] = chunk;
    }

    public Biome chooseBiome(Vector2 chunk)
    {

        // Get the WaterFire and EarthAir values at chunk coordinates
		float WaterFire = WaterFireMap.genPerlin(chunk.x * chunk_size + chunk_size * 0.5f + 1, chunk.y * chunk_size + chunk_size * 0.5f + 1, 0);
        float EarthAir = EarthAirMap.genPerlin(chunk.x * chunk_size + chunk_size * 0.5f, chunk.y * chunk_size + chunk_size * 0.5f, 0);
		//float WaterFire = WaterFireMap.genPerlin(chunk.x * chunk_size + chunk_size * 0.5f + 1, chunk.y * chunk_size + 1, 0);
		//float EarthAir = EarthAirMap.genPerlin (chunk.x * chunk_size + 1, chunk.y * chunk_size + chunk_size * 0.5f + 1, 0);

        if (mapChanges.ContainsKey(chunk))
        {
            WaterFire += mapChanges[chunk].x;
            EarthAir += mapChanges[chunk].y;
        }

        // Find the most appropriate biome
        float lowestError = 100000;
        Biome ret = biomes[0];
        foreach(Biome biome in biomes)
        {
            if (biome == null) Debug.Log("SHIT!");
            float WaterFire_error = Mathf.Abs(biome.WaterFire - WaterFire);
            float EarthAir_error = Mathf.Abs(biome.EarthAir - EarthAir);
           
            if (WaterFire_error + EarthAir_error < lowestError)
            {
//                Debug.Log(biome.WaterFire + "," + biome.EarthAir + ": " + WaterFire_error + EarthAir_error);
                lowestError = WaterFire_error + EarthAir_error;
                ret = biome;
            }
                
        }
        return ret;
    }

    //refreshs all loaded chunks
    private void updateChunks(){
        foreach(KeyValuePair<Vector2, GameObject> chunk in loaded_chunks) {
            chunkGen.refresh(chunk.Value);
            chunkGen.colorChunk(chunk.Value, chunk_size);
        }
    }
    //---------- HELPER FUNCTIONS ----------//
    public static Vector3 chunkToWorld(Vector2 chunk)
    {
        Vector3 pos = new Vector3(chunk.x * Globals.GenerationManagerScript.chunk_size, 0, chunk.y * Globals.GenerationManagerScript.chunk_size);
        return pos;
    }

    public static Vector2 worldToChunk(Vector3 pos)
    {
        Vector2 chunk = new Vector2(Mathf.Floor(pos.x/ Globals.GenerationManagerScript.chunk_size), Mathf.Floor(pos.z / Globals.GenerationManagerScript.chunk_size));
        return chunk;
	}

    public bool inLoadDistance(Vector2 position, Vector2 chunk, float loadDistance) {
        return chunk.x <= position.x + loadDistance && chunk.x >= position.x - loadDistance && chunk.y <= position.y + loadDistance && chunk.y >= position.y - loadDistance;
    }

}
