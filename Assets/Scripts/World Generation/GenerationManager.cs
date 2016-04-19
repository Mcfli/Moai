using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {
    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 6;
    public int chunk_unload_dist = 6;
    public int chunk_detail_dist = 1;
    public float allottedLoadSeconds = 1;
	public int tree_load_dist = 1;
    public List<Biome> biomes;
    public NoiseGen heatMap;
    public NoiseGen mountainMap;
    public NoiseGen moistureMap;

    //lists
    private Dictionary<Vector2, GameObject> loaded_chunks;
    private Dictionary<Vector2, ChunkMeshes> detailed_chunks;
    private Dictionary<Vector2, GameObject> loaded_water;
	private List<Vector2> loaded_shrine_chunks;
    //private Dictionary<Vector2, Biome> chunkBiomes;  // keeps track of what chunk is at what biome

    // Heat/Moisture modifiers per chunk.
    // maps chunk -> (delta_heat,delta_moisture)
    private Dictionary<Vector2, Vector2> mapChanges;
    private bool doneLoading = false;

    //references
    private ChunkGenerator chunkGen;
    private WaterScript waterScript;
    private TreeManager tree_manager;
    private WeatherManager weather_manager;
    private ShrineManager shrine_manager;

    void Awake() {
        //lists
        loaded_chunks = new Dictionary<Vector2, GameObject>();
        detailed_chunks = new Dictionary<Vector2, ChunkMeshes>();
        loaded_water = new Dictionary<Vector2, GameObject>();
		loaded_shrine_chunks = new List<Vector2>();
        mapChanges = new Dictionary<Vector2, Vector2>();

        //references
        chunkGen = GetComponent<ChunkGenerator>();
        waterScript = GetComponent<WaterScript>();
        tree_manager = GetComponent<TreeManager>();
        weather_manager = GameObject.Find("Weather").GetComponent<WeatherManager>();
        shrine_manager = GetComponent<ShrineManager>();

        Globals.cur_chunk = worldToChunk(Globals.Player.transform.position);

        moistureMap.Init();
        heatMap.Init();

        if(chunk_unload_dist < chunk_load_dist) chunk_unload_dist = chunk_load_dist;
        if(chunk_detail_dist > chunk_load_dist) chunk_detail_dist = chunk_load_dist;
    }

    void Start() {
        initiateChunks(Globals.cur_chunk);
        doneLoading = loadUnload(Globals.cur_chunk);
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 current_chunk = worldToChunk(Globals.Player.transform.position);
        if(Globals.cur_chunk != current_chunk) {
            Globals.cur_chunk = current_chunk;
            doneLoading = false;
        }
        if(!doneLoading) doneLoading = loadUnload(Globals.cur_chunk);
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

    // Changes the heat/moisture values at chunk by heat:delta.x,moisture:delta.y
    public void modifyChunk(Vector3 pos, Vector2 delta){
        Vector2 chunk = worldToChunk(pos);
        if (!mapChanges.ContainsKey(chunk))
            mapChanges[chunk] = Vector2.zero;
        mapChanges[chunk] += delta;
        updateChunks();

        //comment this out when we have forest changing
        unloadTrees(pos);
        tree_manager.forgetTrees((int)chunk.x, (int)chunk.y);
        tree_manager.loadTrees(chunk, chooseBiome(chunk));

        /* comment this back in when forest changing is in
        Biome biome = chooseBiome(chunk);
        foreach(ForestScript forest in TreeManager.loadedForests[chunk].Values) forest.changeForest(biome.forestRadius, biome.forestMaxTrees, biome.treeTypes, biome.mixedForests);
        */
    }
    
    private bool loadUnload(Vector2 position) {
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);

        bool done = true;
        if(!unloadChunks(position)) done = false;
        if(!unloadTrees(position)) done = false;
        if(!unloadShrines(position)) done = false;
        if(!loadChunks(position)) done = false;
        if(!detailChunks(position)) done = false;
        if(!undetailChunks(position)) done = false;
        if(!loadTrees(position)) done = false;
        if(!loadShrines(position)) done = false;
        
        return done;
    }

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
        foreach(Vector2 coodinates in l)
            if(!inLoadDistance(position, coodinates, chunk_unload_dist)) destroyChunk(coodinates);
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

    private void createChunk(Vector2 coordinates) {
        GameObject newChunk = chunkGen.generate(coordinates);
        chunkGen.colorChunk(newChunk, chunk_size);
        loaded_chunks.Add(coordinates, newChunk);
        loaded_water.Add(coordinates, waterScript.generate(coordinates));
    }

    private void destroyChunk(Vector2 coordinates) {
        Destroy(loaded_chunks[coordinates]);
        loaded_chunks.Remove(coordinates);
        Destroy(loaded_water[coordinates]);
        loaded_water.Remove(coordinates);
        waterScript.removeMesh(coordinates);
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
            if(Mathf.Abs(key.x - Globals.cur_chunk.x) > tree_load_dist ||
               Mathf.Abs(key.y - Globals.cur_chunk.y) > tree_load_dist) {
                tree_manager.unloadTrees(key);
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
			if (Mathf.Abs(this_chunk.x - Globals.cur_chunk.x) > tree_load_dist ||
				Mathf.Abs(this_chunk.y - Globals.cur_chunk.y) > tree_load_dist)
			{
				shrine_manager.unloadShrines((int)this_chunk.x, (int)this_chunk.y);
				loaded_shrine_chunks.RemoveAt(i);
			}
		}
        return true;
	}
	
    public Biome chooseBiome(Vector2 chunk){
        // Get the heat and moisture values at chunk coordinates
        float heat = heatMap.genPerlin(chunk.x * chunk_size + chunk_size * 0.5f, chunk.y * chunk_size + chunk_size * 0.5f, 0);
        float moisture = moistureMap.genPerlin(chunk.x* chunk_size+1, chunk.y* chunk_size+1, 0);

        if (mapChanges.ContainsKey(chunk))
        {
            heat += mapChanges[chunk].x;
            moisture += mapChanges[chunk].y;
        }

        // Find the most appropriate biome
        float lowestError = 100000;
        Biome ret = biomes[0];
        foreach(Biome biome in biomes)
        {
            float heat_error = Mathf.Abs(biome.heatAvg - heat);
            float moisture_error = Mathf.Abs(biome.moistureAvg - moisture);
            if (heat_error + moisture_error < lowestError)
            {
                lowestError = heat_error + moisture_error;
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
