using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour {
    public float seedToTreeRatio = 0.5f;
    public float secondsToPropogate = 1800;

    private GenerationManager gen_manager;
    public static Dictionary<Vector2, List<ForestScript.forestStruct>> trees; //actually a dictionary of forests
    public static Dictionary<Vector2, List<ForestScript>> loadedForests;

    // Use this for initialization
    void Awake() {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        trees = new Dictionary<Vector2, List<ForestScript.forestStruct>>();
        loadedForests = new Dictionary<Vector2, List<ForestScript>>();
    }

    public static void saveTree(Vector2 chunk, ForestScript forest) {
        ForestScript.forestStruct e = forest.export();
        if(!trees.ContainsKey(chunk)||trees[chunk] == null) trees[chunk] = new List<ForestScript.forestStruct>();
        trees[chunk].Add(e);
    }

    public void loadTrees(Vector2 key, Biome biome){
        List<ForestScript> loaded;
        if (biome.treeTypes.Count < 1) return;
        if (trees.ContainsKey(key) && trees[key] != null){
            loaded = loadedForests[key];
            List<ForestScript.forestStruct> trees_in_chunk = trees[key];
            foreach(ForestScript.forestStruct f in trees_in_chunk) {
                GameObject g = new GameObject();
                ForestScript newForest = g.AddComponent(typeof(ForestScript)) as ForestScript;
                newForest.loadForest(f);
                loaded.Add(newForest);
                trees.Remove(key);
            }
        }
        else
        {
            loaded = new List<ForestScript>();
            loadedForests.Add(key, loaded);
            float step_size = gen_manager.chunk_size / biome.treeDensity;

            // When Advanced terrain is implemented...
            // Instead, check if moisture and heat are sufficient for foliage at each point

            for (float i = key.x * gen_manager.chunk_size + 0.5f*step_size; i < key.x * gen_manager.chunk_size + gen_manager.chunk_size; i += step_size){
                for (float j = key.y * gen_manager.chunk_size + 0.5f * step_size; j < key.y * gen_manager.chunk_size + gen_manager.chunk_size; j += step_size){
                    Vector3 position = new Vector3(i + step_size * Random.value - 0.5f * step_size, 0, j + step_size * Random.value - 0.5f * step_size);
                    
                    RaycastHit hit;
                    Ray rayDown = new Ray(new Vector3(position.x, 10000000, position.z), Vector3.down);
                    int terrain = LayerMask.GetMask("Terrain");
                    if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)) {
                        if(hit.point.y < Globals.water_level) continue;
                        else position.y = hit.point.y - 1;
                    } else continue;

                    GameObject g = new GameObject();
                    ForestScript newForest = g.AddComponent(typeof(ForestScript)) as ForestScript;
                    newForest.createForest(position, 100, biome.treeTypes, 16); //radius and max trees should be pulled from biome prefab
                    loaded.Add(newForest);
                }
            }
        }
        loadedForests.Add(key, loaded);
    }

    /*
    // Takes a list of trees and puts trees naturally around each
    public List<GameObject> growTrees(Biome biome,List<GameObject> initial_trees)
    {

        List<GameObject> new_trees = new List<GameObject>();
        foreach (GameObject tree in initial_trees)
        {
            new_trees.Add(tree);
            // Grow another nearby tree

            GameObject treePrefab = biome.treeTypes[Random.Range(0, (biome.treeTypes.Count))];

            float theta = Random.Range(0, 2 * Mathf.PI);
            float dist = biome.treeSpreadMin + Random.Range(0, biome.treeSpeadRange);
            //Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            Vector3 position = tree.transform.position + new Vector3(Mathf.Cos(theta) * dist, 0, Mathf.Sin(theta) * dist);

            RaycastHit hit;
            Ray rayDown = new Ray(new Vector3(position.x, 10000000, position.z), Vector3.down);
            int terrain = LayerMask.GetMask("Terrain");
            if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)) {
                if(hit.point.y < Globals.water_level) continue;
                else position.y = hit.point.y - 1;
            } else continue;

            GameObject new_tree = createNewTree(treePrefab, position);
            if (new_tree != null) new_trees.Add(new_tree);
        }
        return new_trees;
    }
    */

    // Remove the trees on chunk(x,y) from our saved tree dict and unload all of those trees
    public void forgetTrees(int x,int y)
    {

        Vector2 chunk = new Vector2(x, y);
        unloadTrees(x, y);
        if (trees.ContainsKey(chunk)){
            trees[chunk] = null;
        }
        
    }

    public void unloadTrees(int x, int y){
        Vector2 chunk = new Vector2(x, y);

        foreach(ForestScript f in loadedForests[chunk]){
            saveTree(chunk, f);
            f.destroyForest();
        }
    }

    /*
    // Creates a new tree of type prefab at postition pos
    private GameObject createNewTree(GameObject prefab, Vector3 pos)
    {
        Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        GameObject new_tree = Instantiate(prefab, pos, RandomRotation) as GameObject;
        TreeScript new_treeScript = new_tree.GetComponent<TreeScript>();
        float scaleFactor = Random.Range(1f, 3.5f);
        new_tree.transform.localScale = prefab.transform.localScale * scaleFactor;
        new_treeScript.lifeSpan = new_treeScript.lifeSpan * Random.Range(1 - new_treeScript.lifeSpanVariance, 1 + new_treeScript.lifeSpanVariance);
        new_treeScript.age = Random.value * new_treeScript.lifeSpan;
        new_treeScript.prefab = prefab;

        return new_tree;
    }
    */
}
