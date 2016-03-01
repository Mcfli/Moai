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

    public Vector2 cur_chunk;
    List<Vector2> loaded_chunks;
    Dictionary<Vector2,Biome> chunkBiomes;
    List<Vector2> loaded_tree_chunks;

    void Start () {
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
                    tree_manager.loadTrees(this_chunk);
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
        //Random.seed = chunk.GetHashCode();
        return biomes[Random.Range(0, biomes.Count)];
    }


    // Generates a chunk from (noise function), using CreatePlane
    void generateChunk(Vector2 chunk)
    {
        // Implement here
        Biome biome = chooseBiome(chunk);
        chunkBiomes[chunk] = biome;
        chunkGen.generate((int)chunk.x, (int)chunk.y,time,biome);
    }

    void updateChunks()
    {
        for (int i = loaded_chunks.Count - 1; i >= 0; i--)
        {
            Vector2 this_chunk = loaded_chunks[i];
            string chunk_name = "chunk (" + this_chunk.x + "," + this_chunk.y + ")";
            GameObject chunk = GameObject.Find(chunk_name);

            chunkGen.refresh(chunk);
        }
    }

}
