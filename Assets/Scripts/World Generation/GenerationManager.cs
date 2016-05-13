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
            Globals.PlayerScript.warpToGround(3000, true);
        }
    }
	
	// Update is called once per frame
	void Update () {
        Shader.SetGlobalFloat("_TimeVar", Globals.time / Globals.time_resolution);
        Vector2 current_chunk = worldToChunk(Globals.Player.transform.position);
        if(Globals.cur_chunk != current_chunk) {
            Globals.cur_chunk = current_chunk;
            doneLoading = false;
        }
        if(!doneLoading && Globals.mode > -1) {
            System.DateTime endTime = System.DateTime.Now.AddSeconds(allottedLoadSeconds);
            doneLoading = loadUnload(Globals.cur_chunk);
        }
    }

    // Merges the biome at pos with other_biome
    /*
    public void mergeBiomes(Vector3 pos,Biome other_biome)
    {
        Vector2 chunk = worldToChunk(pos);
        Biome old_biome = chunkBiomes[chunk];
        Biome new_biome = Biome.Combine(old_biome,other_biome);
        chunkBiomes[chunk] = new_biome;
        smoothChunkColors(chunk);
        unloadTrees();
        loadTrees();
    }
    */

    public void initiateWorld() {
        Globals.time = 0;
        //initiateChunks(Globals.cur_chunk);
        doneLoading = loadUnload(Globals.cur_chunk);
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
    
    private bool loadUnload(Vector2 position) {
        bool done = true;
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);

        
        // Unload chunks
        List<Vector2> l = new List<Vector2>(loaded_chunks.Keys);
        foreach (Vector2 coordinates in l)
        {
            // Unload topology
            if (!inLoadDistance(position, coordinates, chunk_unload_dist))
            {
                ChunkMeshes chunkObj = loaded_chunks[coordinates].GetComponent<ChunkMeshes>();

                // Unload base stuff
                if (!chunkObj.unloadedBase)
                {
                    done = false;
                    chunkObj.unloadBase();
                }

                // Destroy the chunk if necessary
                else if (chunkObj.unloadedBase && chunkObj.unloadedObjects)
                {
                    Destroy(loaded_chunks[coordinates]);
                    loaded_chunks.Remove(coordinates);
                }
            }

            // Unload objects
            if (!inLoadDistance(position, coordinates, tree_unload_dist))
            {
                ChunkMeshes chunkObj = loaded_chunks[coordinates].GetComponent<ChunkMeshes>();
                if (!chunkObj.unloadedObjects)
                {
                    done = false;
                    chunkObj.unloadObjects();
                }
            }    
        }
            
        // Load chunks
        int curDist = 0;
        while(curDist < chunk_load_dist)
        {
            for(int i = -curDist; i <= curDist; i++)
            {
                for (int j = -curDist; j <= curDist; j++)
                {
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
                    if (inLoadDistance(position, thisChunk, tree_load_dist) && !chunkObj.doneObjects)
                    {
                        done = false;
                    }
                }
            }
        } 
        
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);
        return done;
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
        loaded_chunks[coordinates] = chunk;
    }

    /*
    private IEnumerator coLoadUnload(Vector2 position) {
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);

        unloadChunks(position);
        unloadTrees(position);
        unloadShrines(position);
        unloadDoodads(position);
        loadChunks(position);
        detailChunks(position);
        undetailChunks(position);
        loadTrees(position);
        loadShrines(position);
        loadDoodads(position);

        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);

        yield return "";
    }
    */
    /*
    private void initiateChunks(Vector2 position) {
        for(float x = position.x - chunk_load_dist; x <= position.x + chunk_load_dist; x++) {
            for(float y = position.y - chunk_load_dist; y <= position.y + chunk_load_dist; y++) {
                Vector2 coordinates = new Vector2(x, y);
                createChunk(coordinates);
            }
        }
    }

    
    // Loads chunks within chunk_load_dist range
    // Changes chunks within chunk_detail_dist range to detailed chunk
    // returns true if all chunks are finished loading
    private bool loadChunks(Vector2 position) {
        System.DateTime endTime = System.DateTime.Now.AddSeconds(allottedLoadSeconds);
        for(float x = position.x - chunk_load_dist; x <= position.x + chunk_load_dist; x++) {
            for(float y = position.y - chunk_load_dist; y <= position.y + chunk_load_dist; y++) {
                Vector2 coordinates = new Vector2(x, y);
                if(inLoadDistance(position, coordinates, chunk_load_dist) && !loaded_chunks.ContainsKey(coordinates)) {
                    createChunk(coordinates);
                    if(System.DateTime.Now > endTime && allottedLoadSeconds > 0) return false;
                }
            }
        }
        return true;
    }

    private bool unloadChunks(Vector2 position) {
        List<Vector2> l = new List<Vector2>(loaded_chunks.Keys);
        foreach (Vector2 coodinates in l)
            if (!inLoadDistance(position, coodinates, chunk_unload_dist))
            {
                destroyChunk(coodinates);
            }
        return true;
    }

    private bool detailChunks(Vector2 position) {
        for(float x = position.x - chunk_detail_dist; x <= position.x + chunk_detail_dist; x++) {
            for(float y = position.y - chunk_detail_dist; y <= position.y + chunk_detail_dist; y++) {
                Vector2 coordinates = new Vector2(x, y);
                if(inLoadDistance(position, coordinates, chunk_detail_dist) && !detailed_chunks.ContainsKey(coordinates) && loaded_chunks.ContainsKey(coordinates)) {
                    ChunkMeshes cm = loaded_chunks[coordinates].GetComponent<ChunkMeshes>();
                    cm.mf.mesh = cm.highMesh;
                    detailed_chunks.Add(coordinates, cm);
                }
            }
        }
        return true;
    }

    private bool undetailChunks(Vector2 position) {
        foreach(Vector2 coordinates in new List<Vector2>(detailed_chunks.Keys))
            if(!inLoadDistance(position, coordinates, chunk_detail_dist) && detailed_chunks.ContainsKey(coordinates)) {
                ChunkMeshes cm = detailed_chunks[coordinates];
                cm.mf.mesh = cm.lowMesh;
                detailed_chunks.Remove(coordinates);
            }
        return true;
    }
    


    private void destroyChunk(Vector2 coordinates) {
        Destroy(loaded_chunks[coordinates]);
        loaded_chunks.Remove(coordinates);
        water_manager.unloadWater(coordinates);
    }

    private bool loadTrees(Vector2 position) {
		for (int x = (int)Globals.cur_chunk.x - tree_load_dist; x <= (int)Globals.cur_chunk.x + tree_load_dist; x++){
			for (int y = (int)Globals.cur_chunk.y - tree_load_dist; y <= (int)Globals.cur_chunk.y + tree_load_dist; y++){
                Vector2 this_chunk = new Vector2(x, y);
                Biome curBiome = chooseBiome(this_chunk);
                tree_manager.loadTrees(this_chunk, curBiome);
            }
        }
        return true;
    }

    private bool unloadTrees(Vector2 position) {
        List<Vector2> keys = new List<Vector2>(TreeManager.loadedForests.Keys);
        foreach(Vector2 key in keys) {
            if(Mathf.Abs(key.x - Globals.cur_chunk.x) > tree_unload_dist ||
               Mathf.Abs(key.y - Globals.cur_chunk.y) > tree_unload_dist) {
                tree_manager.unloadTrees(key);
            }
        }
        return true;
    }

    private bool loadDoodads(Vector2 position)
    {
        for (int x = (int)Globals.cur_chunk.x - tree_load_dist; x <= (int)Globals.cur_chunk.x + tree_load_dist; x++)
        {
            for (int y = (int)Globals.cur_chunk.y - tree_load_dist; y <= (int)Globals.cur_chunk.y + tree_load_dist; y++)
            {
                Vector2 this_chunk = new Vector2(x, y);
                if (!loaded_doodad_chunks.Contains(this_chunk))
                {
                    Biome curBiome = chooseBiome(this_chunk);
                    doodad_manager.loadDoodads(this_chunk,curBiome);
                    loaded_doodad_chunks.Add(this_chunk);
                }
            }
        }
        return true;
    }

    private bool unloadDoodads(Vector2 position)
    {
        for (int i = loaded_doodad_chunks.Count - 1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_doodad_chunks[i];
            if (Mathf.Abs(this_chunk.x - Globals.cur_chunk.x) > tree_unload_dist ||
                Mathf.Abs(this_chunk.y - Globals.cur_chunk.y) > tree_unload_dist)
            {
                doodad_manager.unloadDoodads(this_chunk);
                loaded_doodad_chunks.RemoveAt(i);
            }
        }
        return true;
    }

    private bool loadShrines(Vector2 position) {
		for (int x = (int)Globals.cur_chunk.x - tree_load_dist; x <= (int)Globals.cur_chunk.x + tree_load_dist; x++){
			for (int y = (int)Globals.cur_chunk.y - tree_load_dist; y <= (int)Globals.cur_chunk.y + tree_load_dist; y++){
				Vector2 this_chunk = new Vector2(x, y);
				if (!loaded_shrine_chunks.Contains(this_chunk)){
					shrine_manager.loadShrines(x, y);
					loaded_shrine_chunks.Add(this_chunk);
				}
			}
        }
        return true;
    }

    private bool unloadShrines(Vector2 position) {
        for (int i = loaded_shrine_chunks.Count - 1; i >= 0; i--){
			Vector2 this_chunk = loaded_shrine_chunks[i];
			if (Mathf.Abs(this_chunk.x - Globals.cur_chunk.x) > tree_unload_dist ||
				Mathf.Abs(this_chunk.y - Globals.cur_chunk.y) > tree_unload_dist)
			{
				shrine_manager.unloadShrines((int)this_chunk.x, (int)this_chunk.y);
				loaded_shrine_chunks.RemoveAt(i);
			}
		}
        return true;
	}

    private bool loadObelisks(Vector2 position)
    {
        for (int x = (int)Globals.cur_chunk.x - tree_load_dist; x <= (int)Globals.cur_chunk.x + tree_load_dist; x++)
        {
            for (int y = (int)Globals.cur_chunk.y - tree_load_dist; y <= (int)Globals.cur_chunk.y + tree_load_dist; y++)
            {
                Vector2 this_chunk = new Vector2(x, y);
                if (!loaded_obelisk_chunks.Contains(this_chunk))
                {
                    shrine_manager.loadObelisks(x, y);
                    loaded_obelisk_chunks.Add(this_chunk);
                }
            }
        }
        return true;
    }

    private bool unloadObelisks(Vector2 position)
    {
        for (int i = loaded_obelisk_chunks.Count - 1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_obelisk_chunks[i];
            if (Mathf.Abs(this_chunk.x - Globals.cur_chunk.x) > tree_unload_dist ||
                Mathf.Abs(this_chunk.y - Globals.cur_chunk.y) > tree_unload_dist)
            {
                shrine_manager.unloadObelisks((int)this_chunk.x, (int)this_chunk.y);
                loaded_obelisk_chunks.RemoveAt(i);
            }
        }
        return true;
    }
    */
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
    /*
    public void generateChunk(Vector2 coordinates, bool detailed) {
        //if(!loadDoodads(position)) done = false;
        Biome curBiome = chooseBiome(coordinates);
        createChunk(coordinates);
        if(detailed) {
            ChunkMeshes cm = loaded_chunks[coordinates].GetComponent<ChunkMeshes>();
            cm.mf.mesh = cm.highMesh;
            detailed_chunks.Add(coordinates, cm);
        }
        tree_manager.loadTrees(coordinates, curBiome);
        shrine_manager.loadShrines(Mathf.RoundToInt(coordinates.x), Mathf.RoundToInt(coordinates.y));
        loaded_shrine_chunks.Add(coordinates);
        shrine_manager.loadShrines(Mathf.RoundToInt(coordinates.x), Mathf.RoundToInt(coordinates.y));
        loaded_shrine_chunks.Add(coordinates);
        doodad_manager.loadDoodads(coordinates, curBiome);
        loaded_doodad_chunks.Add(coordinates);
    }
    */
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
