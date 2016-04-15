using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoodadManager : MonoBehaviour
{
    private GenerationManager gen_manager;

    // Use this for initialization
    void Awake()
    {
        gen_manager = gameObject.GetComponent<GenerationManager>();
    }

    public void loadDoodads(Vector2 key, Biome biome)
    {
        if (biome.doodads.Count < 1) return;
        // 
        for(int i = 0; i < biome.doodads.Count; i++)
        {
            loadDoodadType(key,biome.doodads[i], biome.doodadDensity[i]);
        }
    }
    
    // Populates the chunk with the number of doodad specified as count. 
    private void loadDoodadType(Vector2 key,GameObject doodad,int count)
    {
        for (int i = 0; i < count; i++)
        {


            float xpos = key.x * gen_manager.chunk_size + gen_manager.chunk_size * Random.value;
            float zpos = key.y * gen_manager.chunk_size + gen_manager.chunk_size * Random.value;
            Vector3 pos = new Vector3(xpos, 0, zpos);

            RaycastHit hit;
            Ray rayDown = new Ray(new Vector3(xpos, 10000000, zpos), Vector3.down);
            int terrain = LayerMask.GetMask("Terrain");
            if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
            {
                pos.y = hit.point.y;
                GameObject d = Instantiate(doodad, pos,doodad.transform.rotation) as GameObject;
                d.transform.Rotate(Vector3.up*Random.Range(0,2*Mathf.PI));
            }
            
        }
    }

    public void unloadDoodads(Vector2 chunk)
    {
        Vector3 center = new Vector3(chunk.x * gen_manager.chunk_size + gen_manager.chunk_size * 0.5f, 0, chunk.y * gen_manager.chunk_size + gen_manager.chunk_size * 0.5f);
        Vector3 half_extents = new Vector3(gen_manager.chunk_size * 0.5f, 100000, gen_manager.chunk_size * 0.5f);
        LayerMask doodad_mask = LayerMask.GetMask("Doodad");

        Collider[] colliders = Physics.OverlapBox(center, half_extents, Quaternion.identity, doodad_mask);

        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject doodad = colliders[i].gameObject;
            Destroy(doodad);
        }
    }

}
