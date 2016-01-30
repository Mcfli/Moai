using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour {

    public int tree_resolution = 2;
    public Transform prefab;
    public GenerationManager gen_manager;

    private Dictionary<Vector2, List<GameObject>> trees;

    public void loadTrees(int x, int y)
    {
        Vector2 key = new Vector2(x, y);
        if (trees.ContainsKey(key))
        {
            List<GameObject> trees_in_chunk = trees[key];
            for (int i = 0; i < trees_in_chunk.Count; i++)
            {
                Instantiate(trees_in_chunk[i]);
            }
        }
        
        else
        {
            trees[key] = new List<GameObject>();

            // When Advanced terrain is implemented...
            // Instead, check if moisture and heat are sufficient for foliage at each point

            for (float i = x*gen_manager.chunk_size+50; i < x * gen_manager.chunk_size + gen_manager.chunk_size; i+= gen_manager.chunk_size/tree_resolution)
            {
                for (float j = y * gen_manager.chunk_size+100; j < y * gen_manager.chunk_size + gen_manager.chunk_size; j += gen_manager.chunk_size / tree_resolution)
                {
                    
                    Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    
                    GameObject new_tree = Instantiate(prefab, new Vector3(i + 60*Random.value-30, 0, j + 60 * Random.value - 30), RandomRotation) as GameObject;
                    trees[key].Add(new_tree);
                }     
            }
        }
    }

    // Use this for initialization
    void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
        trees = new Dictionary<Vector2, List<GameObject>>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
