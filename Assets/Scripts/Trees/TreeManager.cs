using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour {
<<<<<<< HEAD
    public int tree_resolution = 2;

    private GenerationManager gen_manager;
    private static Dictionary<Vector2, List<TreeScript>> trees;
=======


    private GenerationManager gen_manager;
    private static Dictionary<Vector2, List<Tree>> trees;
>>>>>>> Biome-Merging

    // Use this for initialization
    void Start() {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        trees = new Dictionary<Vector2, List<TreeScript>>();
    }

    public static void saveTree(Vector2 chunk, GameObject tree){
        tree.GetComponent<TreeScript>().saveTransforms();
        if(!trees.ContainsKey(chunk)||trees[chunk] == null) trees[chunk] = new List<TreeScript>();
        trees[chunk].Add(tree.GetComponent<TreeScript>());
    }

<<<<<<< HEAD
    public void loadTrees(Vector2 key,List<GameObject> tree_types){
        if (tree_types.Count < 1) return;
        if (trees.ContainsKey(key)){
            List<TreeScript> trees_in_chunk = trees[key];
            for (int i = trees_in_chunk.Count-1; i >= 0; i--){
                TreeScript tree = trees_in_chunk[i];
=======
    public void loadTrees(Vector2 key,Biome biome)
    {
        if (biome.treeTypes.Count < 1) return;
        if (trees.ContainsKey(key) && trees[key] != null)
        {  
            List<Tree> trees_in_chunk = trees[key];
            for (int i = trees_in_chunk.Count-1; i >= 0; i--)
            {
                Tree tree = trees_in_chunk[i];
>>>>>>> Biome-Merging
                if (tree.prefab == null) continue;
                GameObject new_tree = Instantiate(tree.prefab, tree.saved_position, tree.saved_rotation) as GameObject;
                new_tree.GetComponent<TreeScript>().copyFrom(tree);
                trees[key].Remove(tree);
            }
<<<<<<< HEAD
        }else{
            trees[key] = new List<TreeScript>();
            float step_size = gen_manager.chunk_size / tree_resolution;
=======
        }
        else
        {
            trees[key] = new List<Tree>();
            float step_size = gen_manager.chunk_size / biome.treeDensity;
>>>>>>> Biome-Merging

            // When Advanced terrain is implemented...
            // Instead, check if moisture and heat are sufficient for foliage at each point

            for (float i = key.x * gen_manager.chunk_size + 0.5f*step_size; i < key.x * gen_manager.chunk_size + gen_manager.chunk_size; i += step_size){
                for (float j = key.y * gen_manager.chunk_size + 0.5f * step_size; j < key.y * gen_manager.chunk_size + gen_manager.chunk_size; j += step_size){
                    Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    float xpos = i + step_size * Random.value - 0.5f * step_size;
                    float zpos = j + step_size * Random.value - 0.5f * step_size;
                    GameObject treePrefab = biome.treeTypes[Random.Range(0, (biome.treeTypes.Count))];
                    GameObject new_tree = Instantiate(treePrefab, new Vector3(xpos, 0, zpos), RandomRotation) as GameObject;
                    new_tree.GetComponent<TreeScript>().age = Random.value * new_tree.GetComponent<TreeScript>().life_span;
                }
            }
        }
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
            tree.GetComponent<TreeScript>().saveTransforms();
            saveTree(chunk, tree);
            
            Destroy(tree);
        }
    }
<<<<<<< HEAD
=======

    // Use this for initialization
    void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        trees = new Dictionary<Vector2, List<Tree>>();
	}
>>>>>>> Biome-Merging
}
