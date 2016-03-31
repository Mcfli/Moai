using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {
    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 6;
    public int chunk_detail_dist = 1;
    public List<Biome> biomes;
    public NoiseGen heatMap;
    public NoiseGen mountainMap;
    public NoiseGen moistureMap;

    private GameObject player;
    private ChunkGenerator chunkGen;
    private WaterScript waterScript;
    private TreeManager tree_manager;
    private WeatherManager weather_manager;
    private ShrineManager shrine_manager;
    private Dictionary<Vector2, GameObject> loaded_chunks;
    private Dictionary<Vector2, GameObject> loaded_water;
    private List<Vector2> loaded_tree_chunks;
	private List<Vector2> loaded_shrine_chunks;
    //private Dictionary<Vector2, Biome> chunkBiomes;  // keeps track of what chunk is at what biome

    // Heat/Moisture modifiers per chunk.
    // maps chunk -> (delta_heat,delta_moisture)
    private Dictionary<Vector2, Vector2> mapChanges;
    private bool doneLoading = false;

    void Awake() {
        tree_manager = GetComponent<TreeManager>();
        chunkGen = GetComponent<ChunkGenerator>();
        shrine_manager = GetComponent<ShrineManager>();
        waterScript = GetComponent<WaterScript>();
        weather_manager = GameObject.Find("Weather").GetComponent<WeatherManager>();
        Globals.cur_chunk = new Vector2(-1, -1);
        loaded_chunks = new Dictionary<Vector2, GameObject>();
        loaded_water = new Dictionary<Vector2, GameObject>();
        loaded_tree_chunks = new List<Vector2>();
		loaded_shrine_chunks = new List<Vector2>();
        mapChanges = new Dictionary<Vector2, Vector2>();
        moistureMap.Init();
        heatMap.Init();
    }

    void Start() {
        initiateChunks(Globals.cur_chunk);
        doneLoading = loadUnload(Globals.cur_chunk, Globals.cur_chunk);
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 current_chunk = worldToChunk(Globals.Player.transform.position);
        if(Globals.cur_chunk != current_chunk) {
            Globals.cur_chunk = current_chunk;
            doneLoading = false;
        }
        if(!doneLoading) doneLoading = loadUnload(Globals.cur_chunk, current_chunk);
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
        unloadTrees(pos, pos);
        tree_manager.forgetTrees((int)chunk.x, (int)chunk.y);
        tree_manager.loadTrees(chunk, chooseBiome(chunk));
    }

    // will only load/unload differences
    // if oldPos == newPos, unloads will do nothing and loads will try to load everything
    private bool loadUnload(Vector2 oldPos, Vector2 newPos) {
        bool done = unloadChunks(oldPos, newPos) &&
                    unloadTrees(oldPos, newPos) &&
                    unloadShrines(oldPos, newPos) &&
                    loadChunks(oldPos, newPos) &&
                    loadTrees(oldPos, newPos) &&
                    loadShrines(oldPos, newPos);
        weather_manager.moveParticles(chunkToWorld(Globals.cur_chunk) + new Vector3(chunk_size * 0.5f, 0, chunk_size * 0.5f));
        Globals.cur_biome = chooseBiome(Globals.cur_chunk);
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
    private bool loadChunks(Vector2 oldPos, Vector2 newPos) {
        for(float x = newPos.x - chunk_load_dist; x <= newPos.x + chunk_load_dist; x++) {
            for(float y = newPos.y - chunk_load_dist; y <= newPos.y + chunk_load_dist; y++) {
                Vector2 coordinates = new Vector2(x, y);
                if(inLoadDistance(newPos, coordinates, chunk_detail_dist) && (oldPos == newPos || (!inLoadDistance(oldPos, coordinates, chunk_detail_dist))))
                    loaded_chunks[coordinates].GetComponent<MeshFilter>().mesh = loaded_chunks[coordinates].GetComponent<ChunkMeshes>().highMesh;
                if((oldPos == newPos || !inLoadDistance(oldPos, coordinates, chunk_load_dist)) && !loaded_chunks.ContainsKey(coordinates)) {
                    createChunk(coordinates);
                    //return false;
                }
            }
        }
        return true;
    }

    private void createChunk(Vector2 coordinates) {
        GameObject newChunk = chunkGen.generate(coordinates);
        chunkGen.colorChunk(newChunk, chunk_size);
        loaded_chunks.Add(coordinates, newChunk);
        loaded_water.Add(coordinates, waterScript.generate(coordinates));
    }

    private bool unloadChunks(Vector2 oldPos, Vector2 newPos) {
        if(oldPos == newPos) return true;
        for(float x = (int)oldPos.x - chunk_load_dist; x <= oldPos.x + chunk_load_dist; x++) {
            for(float y = oldPos.y - chunk_load_dist; y <= oldPos.y + chunk_load_dist; y++) {
                Vector2 coordinates = new Vector2(x, y);
                if(!inLoadDistance(newPos, coordinates, chunk_load_dist)) destroyChunk(coordinates);
                if(inLoadDistance(oldPos, coordinates, chunk_detail_dist) && !inLoadDistance(newPos, coordinates, chunk_detail_dist))
                    loaded_chunks[coordinates].GetComponent<MeshFilter>().mesh = loaded_chunks[coordinates].GetComponent<ChunkMeshes>().lowMesh;
            }
        }
        return true;
    }

    private void destroyChunk(Vector2 coordinates) {
        Destroy(loaded_chunks[coordinates]);
        loaded_chunks.Remove(coordinates);
        Destroy(loaded_water[coordinates]);
        loaded_water.Remove(coordinates);
        waterScript.removeMesh(coordinates);
    }

    private bool loadTrees(Vector2 oldPos, Vector2 newPos) {
        for (int x = (int)Globals.cur_chunk.x - tree_manager.treeLoadDistance; x <= (int)Globals.cur_chunk.x + tree_manager.treeLoadDistance; x++){
            for (int y = (int)Globals.cur_chunk.y - tree_manager.treeLoadDistance; y <= (int)Globals.cur_chunk.y + tree_manager.treeLoadDistance; y++){
                Vector2 this_chunk = new Vector2(x, y);
                Biome curBiome = chooseBiome(this_chunk);
                if (!loaded_tree_chunks.Contains(this_chunk)){
                    tree_manager.loadTrees(this_chunk,curBiome);
                    loaded_tree_chunks.Add(this_chunk);
                }
            }
        }
        return true;
    }

    private bool unloadTrees(Vector2 oldPos, Vector2 newPos) {
        if(oldPos == newPos) return true;
        for(int i = loaded_tree_chunks.Count - 1; i >= 0; i--) {
            Vector2 this_chunk = loaded_tree_chunks[i];
            if(Mathf.Abs(this_chunk.x - Globals.cur_chunk.x) > tree_manager.treeLoadDistance ||
                Mathf.Abs(this_chunk.y - Globals.cur_chunk.y) > tree_manager.treeLoadDistance) {
                tree_manager.unloadTrees((int)this_chunk.x, (int)this_chunk.y);
                loaded_tree_chunks.RemoveAt(i);
            }
        }
        return true;
    }

    private bool loadShrines(Vector2 oldPos, Vector2 newPos) {
		for (int x = (int)Globals.cur_chunk.x - tree_manager.treeLoadDistance; x <= (int)Globals.cur_chunk.x + tree_manager.treeLoadDistance; x++){
			for (int y = (int)Globals.cur_chunk.y - tree_manager.treeLoadDistance; y <= (int)Globals.cur_chunk.y + tree_manager.treeLoadDistance; y++){
				Vector2 this_chunk = new Vector2(x, y);
				if (!loaded_shrine_chunks.Contains(this_chunk)){
					shrine_manager.loadShrines(x, y);
					loaded_shrine_chunks.Add(this_chunk);
				}
			}
        }
        return true;
    }

    private bool unloadShrines(Vector2 oldPos, Vector2 newPos) {
        if(oldPos == newPos) return true;
        for (int i = loaded_shrine_chunks.Count - 1; i >= 0; i--){
			Vector2 this_chunk = loaded_shrine_chunks[i];
			if (Mathf.Abs(this_chunk.x - Globals.cur_chunk.x) > tree_manager.treeLoadDistance ||
				Mathf.Abs(this_chunk.y - Globals.cur_chunk.y) > tree_manager.treeLoadDistance)
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
    public Vector3 chunkToWorld(Vector2 chunk)
    {
        Vector3 pos = new Vector3(chunk.x * chunk_size, 0, chunk.y * chunk_size);
        return pos;
    }

    public Vector2 worldToChunk(Vector3 pos)
    {
        Vector2 chunk = new Vector2(Mathf.Floor(pos.x/chunk_size), Mathf.Floor(pos.z / chunk_size));
        return chunk;
	}

    public bool inLoadDistance(Vector2 position, Vector2 chunk, float loadDistance) {
        return chunk.x <= position.x + loadDistance && chunk.x >= position.x - loadDistance && chunk.y <= position.y + loadDistance && chunk.y >= position.y - loadDistance;
    }

}
