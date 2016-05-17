using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoodadManager : MonoBehaviour
{
    public int placementSeed;
    private GenerationManager gen_manager;
    public static Dictionary<Vector2, List<GameObject>> loaded_doodads;

    private LayerMask mask;
    private bool loading = false;
    // Use this for initialization
    void Awake()
    {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        loaded_doodads = new Dictionary<Vector2, List<GameObject>>();
        mask = LayerMask.GetMask("Terrain", "Water");
    }

    public void loadDoodads(Vector2 key, Biome biome){
        StartCoroutine(gradualLoad(key, biome));
    }

    private IEnumerator gradualLoad(Vector2 key, Biome biome)
    {
        
        if (biome.bigDoodads.Count < 1) yield break;
        loading = true;

        int originalSeed = Random.seed;
        Random.seed = Globals.SeedScript.seed + placementSeed + key.GetHashCode();

        float step_size = gen_manager.chunk_size / biome.doodadDensity;
        for (float i = key.x * gen_manager.chunk_size + 0.5f * step_size; i < key.x * gen_manager.chunk_size + gen_manager.chunk_size; i += step_size)
        {
            for (float j = key.y * gen_manager.chunk_size + 0.5f * step_size; j < key.y * gen_manager.chunk_size + gen_manager.chunk_size; j += step_size)
            {
                
                Vector2 position = new Vector3(i + step_size * Random.value - 0.5f * step_size, j + step_size * Random.value - 0.5f * step_size);
                float ratioTotal = 0;
                foreach (float f in biome.bigDoodadChance) ratioTotal += f;

                int bigDoodadID = -1;

                float roll = Random.value * ratioTotal;
                float ratioAdditive = 0;

                for (int k = 0; k < biome.bigDoodads.Count; k++)
                {
                    ratioAdditive += biome.bigDoodadChance[k];
                    if (roll < ratioAdditive)
                    {
                        bigDoodadID = k;
                        if (biome.bigDoodads[k])
                        {
                            GameObject bigDoodad = createDoodad(position, biome.bigDoodads[k]);
                            if (!loaded_doodads.ContainsKey(key)) loaded_doodads[key] = new List<GameObject>();
                            loaded_doodads[key].Add(bigDoodad);
                           // if (System.DateTime.Now >= gen_manager.endTime) yield return null;
                        }
                        break;
                    }

                }

                ratioTotal = 0;
                foreach (float f in biome.smallDoodadChance) ratioTotal += f;

                roll = Random.value;
                int numofSmallDoodads = Mathf.RoundToInt(roll * (biome.numOfSmallDoodads.y - biome.numOfSmallDoodads.x) + biome.numOfSmallDoodads.x);
                float clusterRadius = roll * (biome.doodadClusterRadius.y - biome.doodadClusterRadius.x) + biome.doodadClusterRadius.x;

                for (int k = 0; k < numofSmallDoodads; k++)
                {
                    roll = Random.value * ratioTotal;
                    ratioAdditive = 0;
                    for (int l = 0; l < biome.smallDoodads.Count; l++)
                    {
                        GameObject smallDoodad = null;
                        ratioAdditive += biome.smallDoodadChance[l];
                        if (roll < ratioAdditive)
                        {
                            float theta = Random.Range(0, Mathf.PI * 2);
                            float dist = Random.Range(biome.bigDoodadRadius[bigDoodadID], clusterRadius);
                            Vector2 randomPos = position;
                            randomPos.x += Mathf.Cos(theta) * dist;
                            randomPos.y += Mathf.Sin(theta) * dist;
                            smallDoodad = createDoodad(randomPos, biome.smallDoodads[l]);
                            if (!loaded_doodads.ContainsKey(key)) loaded_doodads[key] = new List<GameObject>();
                            if (smallDoodad) loaded_doodads[key].Add(smallDoodad);
                           // if (System.DateTime.Now >= gen_manager.endTime) yield return null;
                            break;
                        }
                    }
                }
            }
        }
        Random.seed = originalSeed;
        loading = false;
    }
    
    private GameObject createDoodad(Vector2 position, GameObject doodad){
        Vector3 pos = new Vector3(position.x, 10000000, position.y);
        RaycastHit hit;
        Ray rayDown = new Ray(pos, Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, mask)){
            if(hit.collider.gameObject.GetComponent<WaterBody>() == null)
            {
                pos.y = hit.point.y;
                GameObject d = Instantiate(doodad, pos + doodad.transform.position, doodad.transform.rotation) as GameObject;
                d.transform.Rotate(Vector3.forward * Random.Range(0f, 360f));
                InteractableObject o = d.GetComponent<InteractableObject>();
                if (o) o.plant(d.transform.position);
                return d;
            }
            
        }
        return null;
    }

    public void unloadDoodads(Vector2 chunk)
    {
        StartCoroutine(gradualUnload(chunk));
    }

    private IEnumerator gradualUnload(Vector2 chunk)
    {
        if (loading) yield return null;
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
