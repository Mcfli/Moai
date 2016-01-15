using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerationManager : MonoBehaviour {

    public float chunk_size=10;
    public int chunk_load_dist = 1;
    public GameObject player;

    Vector2 cur_chunk;
    List<Vector2> loaded_chunks;
    
	void Awake () {
        player = GameObject.FindGameObjectWithTag("player");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // Checks where player is in current chunk. If outside current chunk, set new chunk to current, and reload surrounding chunks
    void checkPosition () {
        float player_x = player.transform.position.x;
        float player_y = player.transform.position.z; // In unity, y is vertical displacement
        Vector2 player_chunk = new Vector2(Mathf.FloorToInt(player_x/chunk_size), Mathf.FloorToInt(player_y / chunk_size));
        if(cur_chunk != player_chunk)
        {
            cur_chunk = player_chunk;
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

    /// <summary>
    /// STUB
    /// Generates a chunk from (noise function), using CreatePlane
    /// </summary>
    /// <param name="chunk_x"></param>
    /// <param name="chunk_y"></param>
    void generateChunk(int chunk_x, int chunk_y)
    {
        // Implement here
    }
}
