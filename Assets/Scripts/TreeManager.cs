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
        //if (trees.ContainsKey(key))
        if (false)
        {
            List<GameObject> trees_in_chunk = trees[key];
            for (int i = 0; i < trees_in_chunk.Count; i++)
            {
                //Instantiate(trees_in_chunk[i]);
            }
        }
        
        else
        {
            trees[key] = new List<GameObject>();
            float step_size = gen_manager.chunk_size / tree_resolution;

            // When Advanced terrain is implemented...
            // Instead, check if moisture and heat are sufficient for foliage at each point

            for (float i = x * gen_manager.chunk_size + 0.5f*step_size; i < x * gen_manager.chunk_size + gen_manager.chunk_size; i += step_size)
            {
                for (float j = y * gen_manager.chunk_size + 0.5f * step_size; j < y * gen_manager.chunk_size + gen_manager.chunk_size; j += step_size)
                {

                    Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    float xpos = i + step_size * Random.value - 0.5f * step_size;
                    float zpos = j + step_size * Random.value - 0.5f * step_size;

                    GameObject new_tree = Instantiate(prefab, new Vector3(xpos, 0, zpos), RandomRotation) as GameObject;
                    trees[key].Add(new_tree);
                }     
            }
        }
    }

    public void unloadTrees(int x, int y)
    {
        Vector3 center = new Vector3(x * gen_manager.chunk_size + gen_manager.chunk_size*0.5f,0, y * gen_manager.chunk_size + gen_manager.chunk_size * 0.5f);
        Vector3 half_extents = new Vector3(gen_manager.chunk_size,gen_manager.amplitude, gen_manager.chunk_size );
        LayerMask tree_mask = LayerMask.GetMask("Tree");

        Collider[] colliders = Physics.OverlapBox(center, half_extents,Quaternion.identity,tree_mask);

        for (int i = 0;i < colliders.Length; i++)
        {
            GameObject tree = colliders[i].gameObject;
            Destroy(tree);
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
