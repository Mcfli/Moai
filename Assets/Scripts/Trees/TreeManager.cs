using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour {
    public float seedToTreeRatio = 0.3f;
    public float secondsToPropogate = 3600;
    public float propogationTimeVariance = 0.5f;
    public int placementSeed;
    public static Dictionary<Vector2, List<ForestScript.forestStruct>> trees; //actually a dictionary of forests
    public static Dictionary<Vector2, Dictionary<int, ForestScript>> loadedForests;

    private GenerationManager gen_manager;

    // Use this for initialization
    void Awake() {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        trees = new Dictionary<Vector2, List<ForestScript.forestStruct>>();
        loadedForests = new Dictionary<Vector2, Dictionary<int, ForestScript>>();
    }

    public void loadTrees(Vector2 key, Biome biome){
        StartCoroutine(gradualLoad(key,biome));
    }

    // Remove the trees on chunk(x,y) from our saved tree dict and unload all of those trees
    public void forgetTrees(int x,int y)
    {

        Vector2 chunk = new Vector2(x, y);
        unloadTrees(chunk);
        if (trees.ContainsKey(chunk)){
            trees[chunk] = null;
        }
        
    }

    public void unloadTrees(Vector2 chunk){
        trees[chunk] = new List<ForestScript.forestStruct>();
        foreach(ForestScript f in loadedForests[chunk].Values) {
            if(f) {
                trees[chunk].Add(f.export());
                f.destroyForest();
            }
        }
        loadedForests.Remove(chunk);
    }

    private IEnumerator gradualLoad(Vector2 key, Biome biome)
    {
        if (System.DateTime.Now >= gen_manager.endTime) yield return null;
        if (biome.treeTypes.Count < 1) yield break;
        if (loadedForests.ContainsKey(key)) yield break;
        Dictionary<int, ForestScript> loaded = new Dictionary<int, ForestScript>();
        if (trees.ContainsKey(key) && trees[key] != null)
        { //load
            List<ForestScript.forestStruct> trees_in_chunk = trees[key];
            foreach (ForestScript.forestStruct f in trees_in_chunk)
            {
                yield return null;
                GameObject g = new GameObject();
                ForestScript newForest = g.AddComponent(typeof(ForestScript)) as ForestScript;
                newForest.loadForest(f);
                loaded.Add(newForest.GetInstanceID(), newForest);
            }
        }
        else
        { //generate
            float step_size = gen_manager.chunk_size / biome.treeDensity;

            // When Advanced terrain is implemented...
            // Instead, check if moisture and heat are sufficient for foliage at each point

            int originalSeed = Random.seed;
            Random.seed = Globals.SeedScript.seed + placementSeed + key.GetHashCode();

            for (float i = key.x * gen_manager.chunk_size + 0.5f * step_size; i < key.x * gen_manager.chunk_size + gen_manager.chunk_size; i += step_size)
            {
                for (float j = key.y * gen_manager.chunk_size + 0.5f * step_size; j < key.y * gen_manager.chunk_size + gen_manager.chunk_size; j += step_size)
                {
                    yield return null;
                    Vector3 position = new Vector3(i + step_size * Random.value - 0.5f * step_size, 0, j + step_size * Random.value - 0.5f * step_size);

                    RaycastHit hit;
                    Ray rayDown = new Ray(new Vector3(position.x, 10000000, position.z), Vector3.down);
                    if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain", "Water")))
                    {
                        if (hit.collider.GetComponent<WaterBody>() != null) continue;
                        else position.y = hit.point.y - 1;
                    }
                    else continue;
                    yield return null;
                    if (Physics.OverlapSphere(transform.position, biome.forestRadius, LayerMask.GetMask("Seed")).Length > 0) continue;

                    GameObject g = new GameObject("Forest");
                    ForestScript newForest = g.AddComponent(typeof(ForestScript)) as ForestScript;
                    newForest.createForest(position, biome.forestRadius, biome.forestMaxTrees, biome.treeTypes, biome.mixedForests);
                    loaded.Add(newForest.GetInstanceID(), newForest);
                    yield return null;
                }
            }
            Random.seed = originalSeed;
        }
        loadedForests.Add(key, loaded);
    }
}
