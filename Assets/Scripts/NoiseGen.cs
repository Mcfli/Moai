using UnityEngine;
using System.Collections;

public class NoiseGen : MonoBehaviour {
    
    Vector2 cur_chunk = Vector2.zero;
    float chunk_size = 0;
    Vector2[,] permutation_table;

    // Sets the chunk
    void setChunk(int x,int y)
    {
        cur_chunk = new Vector2(x,y);
        generatePermTable();
    }

    // Sets chunk_size
    void setChunkSize(float size)
    {
        chunk_size = size;
    }

   

    // Generate Perlin noise value at(x, y) for current chunk
    float genPerlin(float x,float y)
    {

        return 0.0f;
    }

    // Generates random values in permutation table
    private float permutationValue(int x, int y)
    {
        int n = x + y * 57;
        n = (n << 13) ^ n;
        return chunk_size*(1.0f - ((n * (n * n * 15731 + 789221) + 1376312589)) / 1073741824.0f);
    }

    // Generates the permuation table for the current chunk
    private void generatePermTable()
    {
        for (int x = (int)(cur_chunk.x * chunk_size); x < cur_chunk.x * chunk_size + chunk_size; x++)
        {
            for (int y = (int)(cur_chunk.y * chunk_size); y < cur_chunk.y * chunk_size + chunk_size; y++)
            {
                permutation_table[x,y] = permutationValue(x, y);
            }
        }
    }

}
