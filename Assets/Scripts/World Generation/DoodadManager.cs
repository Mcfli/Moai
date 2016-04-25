using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoodadManager : MonoBehaviour
{
    private GenerationManager gen_manager;
    public static Dictionary<Vector2, List<GameObject>> loaded_doodads;


    // Use this for initialization
    void Awake()
    {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        loaded_doodads = new Dictionary<Vector2, List<GameObject>>();
    }

    public void loadDoodads(Vector2 key, Biome biome){
        if (biome.doodads.Count < 1) return;
        
        for(int i = 0; i < biome.doodads.Count; i++)
            if(Random.value * 100 < biome.doodadDensity[i]) loadDoodadType(key, biome.doodads[i]);
    }
    
    // Populates the chunk with the number of doodad specified as count. 
    private void loadDoodadType(Vector2 key,GameObject doodad){
        float xpos = key.x * gen_manager.chunk_size + gen_manager.chunk_size * Random.value;
        float zpos = key.y * gen_manager.chunk_size + gen_manager.chunk_size * Random.value;
        Vector3 pos = new Vector3(xpos, 0, zpos);

        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(xpos, 10000000, zpos), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)){
            pos.y = hit.point.y;
            GameObject d = Instantiate(doodad, pos + doodad.transform.position, doodad.transform.rotation) as GameObject;
            d.transform.Rotate(Vector3.up*Random.Range(0,2*Mathf.PI));
            InteractableObject o = d.GetComponent<InteractableObject>();
            if(o) o.plant(d.transform.position);
            if(!loaded_doodads.ContainsKey(key)) loaded_doodads[key] = new List<GameObject>();
            loaded_doodads[key].Add(d);
        }
            
    }

    public void unloadDoodads(Vector2 chunk)
    {
        if (loaded_doodads.ContainsKey(chunk))
        {
            for (int i = loaded_doodads[chunk].Count - 1; i >= 0; i--)
            {
                Destroy(loaded_doodads[chunk][i]);
                loaded_doodads[chunk].RemoveAt(i);
            }
        }
    }

}
