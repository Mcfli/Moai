using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {

    public float time = 0.0f;

    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 1;
    public int tree_load_dist = 1;

    public GameObject player;
    public ChunkGenerator chunkGen;
    public TreeManager tree_manager;
    public WeatherManager weather_manager;
    public List<Biome> biomes;

    public NoiseGen heatMap;
    public NoiseGen mountainMap;
    public NoiseGen moistureMap;

    public Vector2 cur_chunk;
    List<Vector2> loaded_chunks;
    List<Vector2> loaded_tree_chunks;
    private Dictionary<Vector2, Biome> chunkBiomes;
    private NoiseSynth noiseSynth;

    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        tree_manager = gameObject.GetComponent<TreeManager>();
        chunkGen = gameObject.GetComponent<ChunkGenerator>();
        weather_manager = gameObject.GetComponent<WeatherManager>();
        chunkGen.chunk_size = chunk_size;
        chunkGen.chunk_resolution = chunk_resolution;
        cur_chunk = new Vector2(-1, -1);
        loaded_chunks = new List<Vector2>();
        loaded_tree_chunks = new List<Vector2>();
        chunkBiomes = new Dictionary<Vector2, Biome>();
        noiseSynth = GetComponent<NoiseSynth>();
        moistureMap.Init();
        heatMap.Init();
        
    }
	
	// Update is called once per frame
	void Update () {
        checkPosition();
        //if(Globals.time_scale > 1.0f) updateChunks();
    }

    // Checks where player is in current chunk. If outside current chunk, set new chunk to current, and reload surrounding chunks
    void checkPosition () {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.z; // In unity, y is vertical displacement
        Vector2 player_chunk = new Vector2(Mathf.FloorToInt(player_x/chunk_size), Mathf.FloorToInt(player_y / chunk_size));
        if(cur_chunk != player_chunk)
        { 
            cur_chunk = player_chunk;
            unloadChunks();
            unloadTrees();
            loadChunks();
            loadTrees();
            Globals.cur_biome = chunkBiomes[cur_chunk];
            weather_manager.updateWeather();
            weather_manager.moveWithPlayer();
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

    void loadTrees()
    {
        for (int x = (int)cur_chunk.x - tree_load_dist; x <= (int)cur_chunk.x + tree_load_dist; x++)
        {
            for (int y = (int)cur_chunk.y - tree_load_dist; y <= (int)cur_chunk.y + tree_load_dist; y++)
            {
                Vector2 this_chunk = new Vector2(x, y);
                Biome curBiome = chooseBiome(this_chunk);
                if (!loaded_tree_chunks.Contains(this_chunk))
                {
                    generateChunk(this_chunk);
                    tree_manager.loadTrees(this_chunk,curBiome.treeTypes);
                    loaded_tree_chunks.Add(this_chunk);
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

    private Biome chooseBiome(Vector2 chunk)
    {
        
        // Get the heat and moisture values at chunk coordinates
        float heat = heatMap.genPerlin(chunk.x*chunk_size+ chunk_size * 0.5f, chunk.y* chunk_size + chunk_size * 0.5f, 0) - noiseSynth.heightAt(chunk.x * chunk_size + chunk_size*0.5f, chunk.y * chunk_size + +chunk_size * 0.5f, 0)*0.5f;
        float moisture = moistureMap.genPerlin(chunk.x* chunk_size+1, chunk.y* chunk_size+1, 0);

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
        Biome b;
        if (chunkBiomes.ContainsKey(chunk) && chunkBiomes[chunk] != null)
            b = chunkBiomes[chunk];
        else
        {
            b = chooseBiome(chunk);
            chunkBiomes[chunk] = b;
        }
            
        chunkGen.generate((int)chunk.x, (int)chunk.y,time,b);
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

    void smoothChunkColors(Vector2 chunk)
    {
        string chunk_name = "chunk (" + chunk.x + "," + chunk.y + ")";
        GameObject chunk_obj = GameObject.Find(chunk_name);
        Biome curBiome = chunkBiomes[chunk];

        Biome up = chooseBiome(chunk + Vector2.up);
        Biome down = chooseBiome(chunk + Vector2.down);
        Biome left = chooseBiome(chunk + Vector2.left);
        Biome right = chooseBiome(chunk + Vector2.right);

        chunkGen.colorChunk(chunk_obj, curBiome,up,down,left,right);
    }

}
