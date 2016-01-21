﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {

    public float chunk_size = 10;
    public int chunk_resolution = 10;
    public int chunk_load_dist = 1;

    public GameObject player;
    public Grid chunkGen;

    public Vector2 cur_chunk;
    List<Vector2> loaded_chunks;
    
	void Awake () {
        player = GameObject.FindGameObjectWithTag("Player");
        chunkGen = gameObject.GetComponent<Grid>();
        chunkGen.chunk_size = chunk_size;
        chunkGen.chunk_resolution = chunk_resolution;
        cur_chunk = new Vector2(-1, -1);
        loaded_chunks = new List<Vector2>();
    }
	
	// Update is called once per frame
	void Update () {
        checkPosition();
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
            loadChunks();
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
                    generateChunk(x,y);
                    loaded_chunks.Add(this_chunk);
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

    /// <summary>
    /// STUB
    /// Generates a chunk from (noise function), using CreatePlane
    /// </summary>
    /// <param name="chunk_x"></param>
    /// <param name="chunk_y"></param>
    void generateChunk(int chunk_x, int chunk_y)
    {
        // Implement here
        chunkGen.generate(chunk_x, chunk_y,10);
    }
}
