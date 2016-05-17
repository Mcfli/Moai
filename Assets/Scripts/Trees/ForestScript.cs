using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForestScript : MonoBehaviour {
    public float radius = -1;
    public int maxTrees = -1;

    private float nextPropogationTime = -1;
    private Dictionary<int, TreeScript> trees = new Dictionary<int, TreeScript>();
    private SphereCollider sphereCol;
    private bool unloaded = false;

    /*
    //temp when changing forest - if this is not null, that means forest is in the process of changing
    private List<GameObject> newTreeTypes = null; // this is for reference, don't change the actual list or the biome prefab will get messed up
    private bool createNext = false;
    private int createAttempts = 0;
    private bool doneDestroying = false;
    */

    void Awake() {
        gameObject.layer = LayerMask.NameToLayer("Forest");
    }
    
	// Update is called once per frame
	void Update () {
        if ((trees.Count == 0 && !unloaded)) {

            if (TreeManager.loadedForests.ContainsKey(GenerationManager.worldToChunk(transform.position)))
            {

                TreeManager.loadedForests[GenerationManager.worldToChunk(transform.position)].Remove(GetInstanceID());
                unloaded = true;
                destroyForest();

                return;
            }

        }

        /*if(newTreeTypes != null) switchOutTree();
        else */if(Globals.time > nextPropogationTime) propogate(Mathf.CeilToInt(trees.Count * Globals.TreeManagerScript.seedToTreeRatio));
	}

    // "new" initialization function (make maxTrees 0 when spawning a forest)
    public void createForest(Vector3 position, float radius, int maxTrees, List<GameObject> treeTypes, bool mixedForest) {
        transform.position = position;
        this.radius = radius;
        this.maxTrees = maxTrees;
        createSphereCollider(radius);

        if(treeTypes != null) if(treeTypes.Count > 0){
            GameObject type = treeTypes[Random.Range(0, treeTypes.Count)];
            for(int i = 0; i < maxTrees; i++) { // will attempt maxTrees times
                createTree(type, new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius);
                if(mixedForest) type = treeTypes[Random.Range(0, treeTypes.Count)];
            }
        }

        propogate(Mathf.CeilToInt(trees.Count * Globals.TreeManagerScript.seedToTreeRatio));
    }

    // for new tree created forests
    public void createForest(Vector3 position, float radius, int maxTrees) {
        createForest(position, radius, maxTrees, null, false);
    }

    // load initialization function
    public void loadForest(forestStruct forest) {
        transform.position = forest.position;
        radius = forest.radius;
        maxTrees = forest.maxTrees;
        createSphereCollider(radius);
        nextPropogationTime = forest.nextPropogationTime;
        foreach(TreeScript.treeStruct t in forest.trees) {
            TreeScript newTree = loadTree(t, (Globals.time - forest.timeUnloaded) / Globals.time_resolution);
            if(!newTree) newTree = createTree(t.prefab, new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius); // if too old, replace with new tree
        }
        // will attempt to propogate on next update
    }
    
    /* for changing biomes
    public void changeForest(float radius, int maxTrees, List<GameObject> treeTypes, bool mixedForest) {
        foreach(Collider c in Physics.OverlapSphere(transform.position, this.radius, LayerMask.GetMask("Seed"))) Destroy(c.gameObject); // delete seeds
        this.radius = radius;
        sphereCol.radius = radius;
        this.maxTrees = maxTrees;
        if(mixedForest) newTreeTypes = treeTypes;
        else {
            newTreeTypes = new List<GameObject>();
            newTreeTypes.Add(treeTypes[Random.Range(0, treeTypes.Count)]);
        }
        createNext = false;
        createAttempts = 0;
        doneDestroying = false;
    }

    //deletes or creates one tree
    //sets newTreeTypes to null when finished
    private void switchOutTree() {
        if(createNext && createAttempts < maxTrees) { // delete tree
            createTree(newTreeTypes[Random.Range(0, newTreeTypes.Count)], new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius);
            createAttempts++;
        } else { // create tree
            foreach(TreeScript t in trees.Values) {
                bool contains = false;
                foreach(GameObject g in newTreeTypes) if(t.prefabPath.Equals(g.GetComponent<TreeScript>().prefabPath)) contains = true;
                if(contains) { } else {
                    trees.Remove(t.GetInstanceID());
                    Destroy(t.gameObject);
                    break;
                }
            }
            //doneDestroying = true;
        }
        createNext = !createNext;
    }
    */

    private void createSphereCollider(float radius) {
        sphereCol = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        sphereCol.radius = radius;
    }

    public void destroyForest() {
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
            RaycastHit raycastHit;
            LayerMask shrine = LayerMask.GetMask("Shrine");
            if (Physics.SphereCast(pos, 25.0f, pos, out raycastHit, 25.0f, shrine))
            {
                continue;
            }
            else
            {
                GameObject seed = Instantiate(randomTree.seed_object);
                Vector2 seed_chunk = GenerationManager.worldToChunk(seed.transform.position);
                if (!DoodadManager.loaded_doodads.ContainsKey(seed_chunk))
                {
                    DoodadManager.loaded_doodads[seed_chunk] = new List<GameObject>();
                }
                DoodadManager.loaded_doodads[seed_chunk].Add(seed);
                seed.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                seed.GetComponent<InteractableObject>().plant(pos);
                numSeeds++;
            }
            
        }
        nextPropogationTime = Globals.time + (Globals.TreeManagerScript.secondsToPropogate + Globals.TreeManagerScript.secondsToPropogate * Random.Range(-Globals.TreeManagerScript.propogationTimeVariance, Globals.TreeManagerScript.propogationTimeVariance)) * Globals.time_resolution;
        //return numSeeds;
    }

    public void addTree(TreeScript tree) {
        trees.Add(tree.GetInstanceID(), tree);
        tree.transform.parent = transform;
    }

    public bool containsTree(int id)
    {
        return trees.ContainsKey(id);
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
        export.maxTrees = maxTrees;
        export.nextPropogationTime = nextPropogationTime;
        export.trees = new List<TreeScript.treeStruct>();
        foreach(KeyValuePair<int, TreeScript> t in trees) if(t.Value) export.trees.Add(new TreeScript.treeStruct(t.Value));
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
        tree.playerPlanted = t.playerPlanted;
        tree.setForestParent(this);
        addTree(tree);
        tree.grow();
        return tree;
    }

    // for instantiating trees - will return null if position is inadequate
    private TreeScript createTree(GameObject type, Vector2 position) {
        float ground = findGround(position);
        if(ground == -Mathf.Infinity) return null;
        Vector3 pos = new Vector3(position.x, ground, position.y);

        //if there's a tree too close by
        float cull_radius = type.GetComponent<TreeScript>().seed_object.GetComponent<InteractableObject>().cull_radius; // kind of inefficient
        if(Physics.OverlapSphere(pos, cull_radius, LayerMask.GetMask("Tree")).Length > 0) return null; //by spherecast - better for overlapping forests
        //foreach(TreeScript t in trees) if(Vector3.Distance(t.gameObject.transform.position, pos) < cull_radius) return null; //by list iteration - will not take into account of overlapping forest

        GameObject g = Instantiate(type, pos + type.transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0)) as GameObject;
        TreeScript tree = g.GetComponent<TreeScript>();
        tree.age = Random.Range(0, tree.lifeSpan);
        tree.setForestParent(this);
        addTree(tree);
        tree.grow();

        return tree;
    }

    private float findGround(Vector2 position, bool mindWater = true) { //returns -Mathf.Infinity if invalid
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(position.x, 10000000, position.y), Vector3.down);
        if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain","Water"))) {
            if(mindWater && hit.collider.gameObject.GetComponent<WaterBody>() != null) return -Mathf.Infinity; //if it hits water
            else return hit.point.y;
        } else return -Mathf.Infinity; //if it didn't hit
    }

    public struct forestStruct {
        public Vector3 position;
        public float radius;
        public int maxTrees;
        public float nextPropogationTime;
        public List<TreeScript.treeStruct> trees;
        public float timeUnloaded;
    }
}
