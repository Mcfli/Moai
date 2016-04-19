using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForestScript : MonoBehaviour {
    private float radius;
    private float nextPropogationTime;
    private Dictionary<int, TreeScript> trees;
    private Dictionary<int, TreeScript> newTrees; //temp when chaning forest

    void Awake() {
        radius = -1;
        nextPropogationTime = -1;
        trees = new Dictionary<int, TreeScript>();
        gameObject.layer = LayerMask.NameToLayer("Forest");
    }

    void Start() {
    }
	
	// Update is called once per frame
	void Update () {
        if(trees.Count == 0) {
            destroyForest();
            TreeManager.loadedForests[GenerationManager.worldToChunk(transform.position)].Remove(GetInstanceID());
            return;
        }
        //for(int i = 0; i < trees.Count; i++) if(!trees[i]) trees.RemoveAt(i);
        if(Globals.time > nextPropogationTime)
            propogate(Mathf.RoundToInt(trees.Count * Globals.TreeManagerScript.seedToTreeRatio));
	}

    // "new" initialization function (make maxTrees 0 when spawning a forest)
    // will choose one of the treeTypes at random
    public void createForest(Vector3 position, float radius, List<GameObject> treeTypes, int maxTrees) {
        GameObject type = treeTypes[Random.Range(0, treeTypes.Count)];
        transform.position = position;
        this.radius = radius;
        createSphereCollider(radius);

        int originalSeed = Random.seed;
        Random.seed = position.GetHashCode();

        if(maxTrees >= 1) for(int i = 0; i < maxTrees; i++) // will attempt maxTrees times
            createTree(type, new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius);

        propogate(Mathf.RoundToInt(trees.Count * Globals.TreeManagerScript.seedToTreeRatio));

        Random.seed = originalSeed;
    }

    // load initialization function
    public void loadForest(forestStruct forest) {
        transform.position = forest.position;
        radius = forest.radius;
        createSphereCollider(radius);
        nextPropogationTime = forest.nextPropogationTime;
        foreach(TreeScript.treeStruct t in forest.trees) {
            TreeScript newTree = loadTree(t, (Globals.time - forest.timeUnloaded) / Globals.time_resolution);
            if(!newTree) newTree = createTree(t.prefab, new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius); // if too old, replace with new tree
        }
        // will attempt to propogate on next update
    }

    private void createSphereCollider(float radius) {
        SphereCollider collider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        collider.radius = radius;
    }

    public void destroyForest() {
        //foreach(TreeScript t in trees.Values) if(t) Destroy(t.gameObject);
        Destroy(gameObject); // will also destroy children
    }

    public void propogate(int maxSeeds) {
        int numSeeds = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Seed")).Length;
        for(int i = 0; i < maxSeeds && numSeeds < maxSeeds; i++) {
            TreeScript randomTree = new List<TreeScript>(trees.Values)[Random.Range(0, trees.Count)];
            if(!randomTree.canPropogate()) continue;

            Vector2 twoPos = new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius;
            float ground = findGround(twoPos);
            if(ground == -Mathf.Infinity) continue;
            Vector3 pos = new Vector3(twoPos.x, ground, twoPos.y);

            GameObject seed = Instantiate(randomTree.seed_object);
            seed.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            seed.GetComponent<InteractableObject>().plant(pos);
            numSeeds++;
        }
        nextPropogationTime = Globals.time + (Globals.TreeManagerScript.secondsToPropogate + Globals.TreeManagerScript.secondsToPropogate * Random.Range(-Globals.TreeManagerScript.propogationTimeVariance, Globals.TreeManagerScript.propogationTimeVariance)) * Globals.time_resolution;
        //return numSeeds;
    }

    public void addTree(TreeScript tree) {
        trees.Add(tree.GetInstanceID(), tree);
        tree.transform.parent = transform;
    }

    public void removeTree(int id) {
        trees.Remove(id);
    }

    public int amountOfTrees() {
        return trees.Count;
    }
    
    public forestStruct export() {
        forestStruct export = new forestStruct();
        export.position = transform.position;
        export.radius = radius;
        export.nextPropogationTime = nextPropogationTime;
        export.trees = new List<TreeScript.treeStruct>();
        foreach(KeyValuePair<int, TreeScript> t in trees)
            if(t.Value) export.trees.Add(new TreeScript.treeStruct(t.Value));
        export.timeUnloaded = Globals.time;
        return export;
    }

    public void changeForest(float radius, List<GameObject> treeTypes, int maxTrees) {
        
    }

    public void switchOutTree() {

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
        tree.setForestParent(this);
        trees.Add(tree.GetInstanceID(), tree);
        tree.grow();
        return tree;
    }

    // for instantiating trees - will return null if position is inadequate
    private TreeScript createTree(GameObject type, Vector2 position) {
        float ground = findGround(position);
        if(ground == -Mathf.Infinity) return null;
        Vector3 pos = new Vector3(position.x, ground - 1, position.y);

        //if there's a tree too close by
        float cull_radius = type.GetComponent<TreeScript>().seed_object.GetComponent<InteractableObject>().cull_radius; // kind of inefficient
        if(Physics.OverlapSphere(pos, cull_radius, LayerMask.GetMask("Tree")).Length > 0) return null; //by spherecast - better for overlapping forests
        //foreach(TreeScript t in trees) if(Vector3.Distance(t.gameObject.transform.position, pos) < cull_radius) return null; //by list iteration - will not take into account of overlapping forest

        GameObject g = Instantiate(type, pos, Quaternion.Euler(0, Random.Range(0, 360), 0)) as GameObject;
        TreeScript tree = g.GetComponent<TreeScript>();
        tree.age = Random.Range(0, tree.lifeSpan);
        tree.transform.parent = transform;
        tree.setForestParent(this);
        trees.Add(tree.GetInstanceID(), tree);
        tree.grow();

        return tree;
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
        public float nextPropogationTime;
        public List<TreeScript.treeStruct> trees;
        public float timeUnloaded;
    }
}
