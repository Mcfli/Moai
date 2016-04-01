using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour {
    

    private GenerationManager gen_manager;
    private static Dictionary<Vector2, List<treeStruct>> trees;

    // Use this for initialization
    void Start() {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        trees = new Dictionary<Vector2, List<treeStruct>>();
    }

    public static void saveTree(Vector2 chunk, TreeScript tree){
        treeStruct v = new treeStruct(tree);
        if(!trees.ContainsKey(chunk)||trees[chunk] == null) trees[chunk] = new List<treeStruct>();
        trees[chunk].Add(v);
    }


    public void loadTrees(Vector2 key, Biome biome){
        if (biome.treeTypes.Count < 1) return;
        if (trees.ContainsKey(key) && trees[key] != null){
            List<treeStruct> trees_in_chunk = trees[key];
            for (int i = trees_in_chunk.Count-1; i >= 0; i--) {
                treeStruct tree = trees_in_chunk[i];
                if (tree.prefab == null) continue;
                GameObject new_tree = Instantiate(tree.prefab, tree.position, tree.rotation) as GameObject;
                TreeScript new_treeScript = new_tree.GetComponent<TreeScript>();
                new_treeScript.age = tree.age;
                new_treeScript.lifeSpan = tree.life_span;
                new_treeScript.prefab = tree.prefab;
                trees[key].Remove(tree);
            }
        }
        else
        {
            List<GameObject> treesInChunk = new List<GameObject>();
            trees[key] = new List<treeStruct>();
            float step_size = gen_manager.chunk_size / biome.treeDensity;

            // When Advanced terrain is implemented...
            // Instead, check if moisture and heat are sufficient for foliage at each point

            for (float i = key.x * gen_manager.chunk_size + 0.5f*step_size; i < key.x * gen_manager.chunk_size + gen_manager.chunk_size; i += step_size){
                for (float j = key.y * gen_manager.chunk_size + 0.5f * step_size; j < key.y * gen_manager.chunk_size + gen_manager.chunk_size; j += step_size){
                    Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    float xpos = i + step_size * Random.value - 0.5f * step_size;
                    float zpos = j + step_size * Random.value - 0.5f * step_size;
                    GameObject treePrefab = biome.treeTypes[Random.Range(0, (biome.treeTypes.Count))];
                    GameObject new_tree = createNewTree(treePrefab, new Vector3(xpos, 0, zpos));

                    
                    treesInChunk.Add(new_tree);
                }
            }
            for (int i = 0; i < biome.treeGrowthIterations; i++)
            {
                treesInChunk = growTrees(biome,treesInChunk);
            }

        }
    }

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
            Vector3 offset = new Vector3(Mathf.Cos(theta) * dist, 0, Mathf.Sin(theta) * dist);
                        Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject new_tree = createNewTree(treePrefab,tree.transform.position + offset);
            if (new_tree != null) new_trees.Add(new_tree);
        }
        return new_trees;
    }

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
        Vector3 center = new Vector3(x * gen_manager.chunk_size + gen_manager.chunk_size*0.5f,0, y * gen_manager.chunk_size + gen_manager.chunk_size * 0.5f);
        Vector3 half_extents = new Vector3(gen_manager.chunk_size*0.5f,100000, gen_manager.chunk_size*0.5f );
        LayerMask tree_mask = LayerMask.GetMask("Tree");
        Vector2 chunk = new Vector2(x, y);

        Collider[] colliders = Physics.OverlapBox(center, half_extents,Quaternion.identity,tree_mask);

        for (int i = 0;i < colliders.Length; i++){
            
            GameObject tree = colliders[i].gameObject;
            saveTree(chunk, tree.GetComponent<TreeScript>());
            
            Destroy(tree);
        }
    }

    // Creates a new tree of type prefab at postition pos
    private GameObject createNewTree(GameObject prefab, Vector3 pos)
    {
        Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        GameObject new_tree = Instantiate(prefab, pos, RandomRotation) as GameObject;
        TreeScript new_treeScript = new_tree.GetComponent<TreeScript>();
        new_treeScript.lifeSpan = new_treeScript.lifeSpan * Random.Range(1 - new_treeScript.lifeSpanVariance, 1 + new_treeScript.lifeSpanVariance);
        new_treeScript.age = Random.value * new_treeScript.lifeSpan;
        new_treeScript.prefab = prefab;

        return new_tree;
    }

    private struct treeStruct {
        public Vector3 position;
        public Quaternion rotation;
        public float age;
        public float life_span;
        public GameObject prefab;

        public treeStruct(TreeScript t) {
            position = t.gameObject.transform.position;
            rotation = t.gameObject.transform.rotation;
            age = t.age;
            life_span = t.lifeSpan;
            prefab = t.prefab;
        }
    }
}
