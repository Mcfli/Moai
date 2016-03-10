using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {

    public float time = 0.0f;

    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 1;
    public int tree_load_dist = 1;
    public List<Biome> biomes;
    public NoiseGen heatMap;
    public NoiseGen mountainMap;
    public NoiseGen moistureMap;

    private GameObject player;
    private ChunkGenerator chunkGen;
    private TreeManager tree_manager;
    private WeatherManager weather_manager;
    private ShrineManager shrine_manager;
    private Vector2 cur_chunk;
    private List<Vector2> loaded_chunks;
    private List<Vector2> loaded_tree_chunks;
	private List<Vector2> loaded_shrine_chunks;
    //private Dictionary<Vector2, Biome> chunkBiomes;  // keeps track of what chunk is at what biome

    // Heat/Moisture modifiers per chunk.
    // maps chunk -> (delta_heat,delta_moisture)
    private Dictionary<Vector2, Vector2> mapChanges;
    private NoiseSynth noiseSynth;

    void Awake() {
        tree_manager = gameObject.GetComponent<TreeManager>();
        chunkGen = gameObject.GetComponent<ChunkGenerator>();
        shrine_manager = gameObject.GetComponent<ShrineManager>();
        weather_manager = GameObject.Find("Weather").GetComponent<WeatherManager>();
        chunkGen.chunk_size = chunk_size;
        chunkGen.chunk_resolution = chunk_resolution;
        cur_chunk = new Vector2(-1, -1);
        loaded_chunks = new List<Vector2>();
        loaded_tree_chunks = new List<Vector2>();
		loaded_shrine_chunks = new List<Vector2>();
        mapChanges = new Dictionary<Vector2, Vector2>();
        noiseSynth = GetComponent<NoiseSynth>();
        moistureMap.Init();
        heatMap.Init();
      
    }
	
	// Update is called once per frame
	void Update () {
        checkPosition();
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
    public void modifyChunk(Vector3 pos, Vector2 delta)
    {
        Vector2 chunk = worldToChunk(pos);
        if (!mapChanges.ContainsKey(chunk))
            mapChanges[chunk] = Vector2.zero;
        mapChanges[chunk] += delta;
        updateChunks();
        unloadTrees();
        tree_manager.forgetTrees((int)chunk.x, (int)chunk.y);
        tree_manager.loadTrees(chunk, chooseBiome(chunk));

    }

    // Checks where player is in current chunk. If outside current chunk, set new chunk to current, and reload surrounding chunks
    void checkPosition () {
        float player_x = Globals.Player.transform.position.x;
        float player_y = Globals.Player.transform.position.z; // In unity, y is vertical displacement
        Vector2 player_chunk = new Vector2(Mathf.FloorToInt(player_x/chunk_size), Mathf.FloorToInt(player_y / chunk_size));
        if(cur_chunk != player_chunk)
        { 
            cur_chunk = player_chunk;
            unloadChunks();
            unloadTrees();
			unloadShrines();
            loadChunks();
            loadTrees();
			loadShrines();
            weather_manager.moveParticles();
            Globals.cur_biome = chooseBiome(cur_chunk);
            weather_manager.changeWeather();
        }
    }

    // Loads surrounding chunks within chunk_load_dist range
    void loadChunks() {
        for (int x = (int)cur_chunk.x - chunk_load_dist; x <= (int)cur_chunk.x + chunk_load_dist; x++)
        {
            for (int y = (int)cur_chunk.y - chunk_load_dist; y <= (int)cur_chunk.y + chunk_load_dist; y++)
            {
                Vector2 this_chunk = new Vector2(x, y);
                if (!loaded_chunks.Contains(this_chunk))
                {
                    generateChunk(this_chunk);
                    loaded_chunks.Add(this_chunk);
                }
            }
        }
    }

    void loadTrees(){
        for (int x = (int)cur_chunk.x - tree_load_dist; x <= (int)cur_chunk.x + tree_load_dist; x++)
        {
            for (int y = (int)cur_chunk.y - tree_load_dist; y <= (int)cur_chunk.y + tree_load_dist; y++)
            {
                Vector2 this_chunk = new Vector2(x, y);
                Biome curBiome = chooseBiome(this_chunk);
                if (!loaded_tree_chunks.Contains(this_chunk))
                {
                    tree_manager.loadTrees(this_chunk,curBiome);
                    loaded_tree_chunks.Add(this_chunk);
                }
            }
        }
    }

	void loadShrines()
	{
		for (int x = (int)cur_chunk.x - tree_load_dist; x <= (int)cur_chunk.x + tree_load_dist; x++)
		{
			for (int y = (int)cur_chunk.y - tree_load_dist; y <= (int)cur_chunk.y + tree_load_dist; y++)
			{
				Vector2 this_chunk = new Vector2(x, y);
				if (!loaded_shrine_chunks.Contains(this_chunk))
				{
					shrine_manager.loadShrines(x, y);
					loaded_shrine_chunks.Add(this_chunk);
				}
			}
		}
	}

    void unloadChunks()
    {
        for (int i = loaded_chunks.Count-1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_chunks[i];
            if (Mathf.Abs(this_chunk.x - cur_chunk.x) > chunk_load_dist ||
                Mathf.Abs(this_chunk.y - cur_chunk.y) > chunk_load_dist)
            {
                string chunk_name = "chunk (" + this_chunk.x + "," + this_chunk.y + ")";
                GameObject chunk = GameObject.Find(chunk_name);
                Destroy(chunk);
                loaded_chunks.RemoveAt(i);
            }
        }
    }

    void unloadTrees()
    {
        for (int i = loaded_tree_chunks.Count - 1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_tree_chunks[i];
            if (Mathf.Abs(this_chunk.x - cur_chunk.x) > tree_load_dist ||
                Mathf.Abs(this_chunk.y - cur_chunk.y) > tree_load_dist)
            {
                tree_manager.unloadTrees((int)this_chunk.x, (int)this_chunk.y);
                loaded_tree_chunks.RemoveAt(i);
            }
        }
    }

	void unloadShrines()
	{
		for (int i = loaded_shrine_chunks.Count - 1; i >= 0; i--)
		{
			Vector2 this_chunk = loaded_shrine_chunks[i];
			if (Mathf.Abs(this_chunk.x - cur_chunk.x) > tree_load_dist ||
				Mathf.Abs(this_chunk.y - cur_chunk.y) > tree_load_dist)
			{
				shrine_manager.unloadShrines((int)this_chunk.x, (int)this_chunk.y);
				loaded_shrine_chunks.RemoveAt(i);
			}
		}
	}
	
    public Biome chooseBiome(Vector2 chunk)
    {

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

    /// <summary>
    /// STUB
    /// Generates a chunk from (noise function), using CreatePlane
    /// </summary>
    /// <param name="chunk_x"></param>
    /// <param name="chunk_y"></param>
    void generateChunk(Vector2 chunk)
    { 
        chunkGen.generate((int)chunk.x, (int)chunk.y,time,chooseBiome(chunk));
        smoothChunkColors(chunk);
    }

    void updateChunks()
    {
        for (int i = loaded_chunks.Count - 1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_chunks[i];
            string chunk_name = "chunk (" + this_chunk.x + "," + this_chunk.y + ")";
            GameObject chunk = GameObject.Find(chunk_name);

            chunkGen.refresh(chunk);
            smoothChunkColors(new Vector2(this_chunk.x,this_chunk.y));
        }
    }

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
	
    void smoothChunkColors(Vector2 chunk)
    {
        string chunk_name = "chunk (" + chunk.x + "," + chunk.y + ")";
        GameObject chunk_obj = GameObject.Find(chunk_name);
        Biome curBiome = chooseBiome(chunk);
        chunkGen.colorChunk(chunk_obj);
    }

}
