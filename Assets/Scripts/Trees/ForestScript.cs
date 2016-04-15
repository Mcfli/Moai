using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForestScript : MonoBehaviour {
    private float radius;
    private float lastPropogated;
    private List<TreeScript> trees;

    void Awake() {
        radius = -1;
        lastPropogated = -1;
        trees = new List<TreeScript>();
    }

    void Start() {
        lastPropogated = Globals.time;
    }
	
	// Update is called once per frame
	void Update () {
	    if(trees.Count == 0) destroyForest();
        if(Globals.time - lastPropogated > Globals.TreeManagerScript.secondsToPropogate * Globals.time_resolution)
            propogate(Mathf.RoundToInt(trees.Count * Globals.TreeManagerScript.seedToTreeRatio));
	}

    // "new" initialization function (make maxTrees 0 when spawning a forest)
    public void createForest(Vector3 position, float radius, List<GameObject> treeTypes, int maxTrees) {
        transform.position = position;
        this.radius = radius;
        createSphereCollider(radius);

        if(maxTrees >= 1) {
            int originalSeed = Random.seed;
            Random.seed = position.GetHashCode();
            for(int i = 0; i < maxTrees; i++) { // will attempt maxTrees times
                TreeScript t = createTree(treeTypes[Random.Range(0, treeTypes.Count)], new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius);
                if(t) trees.Add(t);
            }
            Random.seed = originalSeed;
        }
    }

    // load initialization function
    public void loadForest(forestStruct forest) {
        transform.position = forest.position;
        radius = forest.radius;
        createSphereCollider(radius);
        lastPropogated = forest.lastPropogated;
        foreach(TreeScript.treeStruct t in forest.trees) {
            TreeScript newTree = loadTree(t, (Globals.time - forest.timeUnloaded) / Globals.time_resolution);
            // if too old, replace with new tree
            if(!newTree) newTree = createTree(t.prefab, new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius);
            if(newTree) trees.Add(newTree);
        }
        // will attempt to propogate on next update
    }

    private void createSphereCollider(float radius) {
        SphereCollider collider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        collider.radius = radius;
    }

    public void destroyForest() {
        foreach(TreeScript t in trees) Destroy(t.gameObject);
        Destroy(gameObject);
    }

    public void propogate(int maxSeeds) {
        int numSeeds = Physics.SphereCastAll(new Ray(transform.position, Vector3.down), radius, 0, LayerMask.GetMask("Seed")).Length;
        for(int i = 0; i < maxSeeds && numSeeds < maxSeeds; i++) {
            Vector2 twoPos = new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius;
            float ground = findGround(twoPos);
            if(ground == -Mathf.Infinity) continue;
            Vector3 pos = new Vector3(twoPos.x, ground, twoPos.y);

            GameObject seed = Instantiate(trees[Random.Range(0, trees.Count)].seed_object);
            seed.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            seed.GetComponent<InteractableObject>().plant(pos);
            numSeeds++;
        }
        lastPropogated = Globals.time;
        //return numSeeds;
    }

    public void addTree(TreeScript tree) {
        trees.Add(tree);
        tree.transform.parent = transform;
    }
    
    public forestStruct export() {
        forestStruct export = new forestStruct();
        export.position = transform.position;
        export.radius = radius;
        export.lastPropogated = lastPropogated;
        export.trees = new List<TreeScript.treeStruct>();
        foreach(TreeScript t in trees) export.trees.Add(new TreeScript.treeStruct(t));
        export.timeUnloaded = Globals.time;
        return export;
    }

    // from treeStruct - will force tree placement
    // will return null if tree has died
    private TreeScript loadTree(TreeScript.treeStruct t, float timePassed) {
        if(t.age + timePassed > t.lifeSpan) return null;
        GameObject g = Instantiate(t.prefab, t.position, t.rotation) as GameObject;
        TreeScript tree = g.GetComponent<TreeScript>();
        tree.gameObject.transform.localScale = t.scale;
        tree.age = t.age + timePassed;
        tree.lifeSpan = t.lifeSpan;
        tree.transform.parent = transform;
        return tree;
    }

    // for instantiating trees - will return null if position is inadequate
    private TreeScript createTree(GameObject type, Vector2 position) {
        float ground = findGround(position);
        if(ground == -Mathf.Infinity) return null;
        Vector3 pos = new Vector3(position.x, ground - 1, position.y);

        //if there's a tree too close by
        float cull_radius = type.GetComponent<TreeScript>().seed_object.GetComponent<InteractableObject>().cull_radius; // kind of inefficient
        if(Physics.SphereCast(new Ray(pos, Vector3.down), cull_radius, 0, LayerMask.GetMask("Tree"))) return null; //by spherecast - better for overlapping forests
        //foreach(TreeScript t in trees) if(Vector3.Distance(t.gameObject.transform.position, pos) < cull_radius) return null; //by list iteration - will not take into account of overlapping forest

        GameObject g = Instantiate(type, pos, Quaternion.Euler(0, Random.Range(0, 360), 0)) as GameObject;
        TreeScript newTree = g.GetComponent<TreeScript>();
        newTree.age = Random.Range(0, newTree.lifeSpan);
        newTree.transform.parent = transform;

        return newTree;
    }

    private float findGround(Vector2 position, bool mindWater = true) { //returns -Mathf.Infinity if invalid
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(position.x, 10000000, position.y), Vector3.down);
        if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))) {
            if(hit.point.y < Globals.water_level && mindWater) return -Mathf.Infinity; //if it hits water
            else return hit.point.y;
        } else return -Mathf.Infinity; //if it didn't hit
    }

    public struct forestStruct {
        public Vector3 position;
        public float radius;
        public float lastPropogated;
        public List<TreeScript.treeStruct> trees;
        public float timeUnloaded;
    }
}
