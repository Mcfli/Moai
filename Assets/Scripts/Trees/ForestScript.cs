using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForestScript : MonoBehaviour {
    private float radius = -1;
    private int maxTrees = -1;
    private int maxSeeds = -1;
    private List<GameObject> treeTypes;

    private List<TreeScript> trees = new List<TreeScript>();
    private List<InteractableObject> seeds = new List<InteractableObject>();
    private float lastPropogated = -1;
	
	// Update is called once per frame
	void Update () {
	    if(trees.Count == 0 && seeds.Count == 0) 
	}

    // initialization function
    public void createForest(float radius, List<GameObject> treeTypes, int maxTrees, int maxSeeds) {
        this.radius = radius;
        this.treeTypes = treeTypes;
        this.maxTrees = maxTrees;
        this.maxSeeds = maxSeeds;



        trees.Add(createTree(Random.insideUnitCircle * radius));
    }

    public void propogate() {

    }

    public void loadForest() {

    }

    private TreeScript createTree(treeStruct t) { // from treeStruct
        TreeScript tree = Instantiate(t.prefab, t.position, t.rotation) as TreeScript;
        tree.gameObject.transform.localScale = t.scale;
        tree.age = t.age;
        tree.lifeSpan = t.life_span;
        return tree;
    }

    private TreeScript createTree(Vector2 position) { // new tree
        Vector3 pos = new Vector3(position.x, 0, position.y);
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(position.x, 10000000, position.y), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)) {
            if(hit.point.y < Globals.water_level) return null;
            else pos.y = hit.point.y - 1;
        } else return null;

        return Instantiate(treeTypes[Random.Range(0, treeTypes.Count)], pos, Quaternion.Euler(0, Random.Range(0, 360), 0)) as TreeScript;
    }

    public treeStruct[] export() {

    }

    public struct treeStruct {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public float age;
        public float life_span;
        public GameObject prefab;

        public treeStruct(TreeScript t) {
            position = t.gameObject.transform.position;
            rotation = t.gameObject.transform.rotation;
            scale = t.gameObject.transform.localScale;
            age = t.age;
            life_span = t.lifeSpan;
            prefab = t.prefab;
        }
    }

    public struct forestStruct {
        public List<TreeScript> trees;
        public List<InteractableObject> seeds;
    }
}
