using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {
    public GameObject prefab;

    //propogation
    public GameObject seed_object;
    public float minSpawnRadius;
    public float maxSpawnRadius;
    public float spawn_delay;
    public float spawn_variance;
    public int seed_cull_max_density;
    public int spawn_limit;

    //culling
    public float cull_radius;
    public int cull_max_density;
    public float max_health = 100;

    //lifeSpan
    public float age;
    public float lifeSpan = 657000; // base life span, in seconds, will be modified by lifeSpanVariance when instantiated
    public float lifeSpanVariance = 0.1f; // in ratio

    //animating
    public float target_scale;
    public float ratioAnimUpdates = 0.01f; // in ratio

    //animation, collision, states
    public List<string> stateAnimationNames; //names of animation, leave blank if no animation, currently unused
    public List<float> stateRatios; //ratio of each state, currently unused
    public List<PuzzleObject> statePuzzleObjects; //puzzleObject component for each state
    public List<bool> propogateDuringState;
    public List<AnimationCurve> heightVSTime;

    //fire
    public float cul_spread;
    public bool onFire;

    //dirtMound
    public GameObject dirtMound;
    public Vector3 dirtMoundOffset;

    ///-------PRIVATES-------///

    //References
    private Animation anim;
    private BoxCollider boxCollider;
    private LayerMask treeMask;
    private LayerMask treeAndSeedMask;
    private Renderer thisRenderer; // unused

    //propogation
    private bool donePropogating;
    private float lastSpawned = 0.0f;
    private int numSpawned;

    //culling
    private float health;

    //animation, collision, states
    //private bool firstAnim;
    private float ratioTotal;
    private List<float> stateMarks;
    private float lastAnimUpdate;
    private int state;
    private float haloTime;

    //fire
    private GameObject fire;
    private GameObject Torch;

    // Use this for initialization
    void Awake(){
        // finals
        anim = GetComponent<Animation>();
        boxCollider = GetComponent<BoxCollider>();
        thisRenderer = GetComponent<Renderer>();
        fire = Resources.Load("fire") as GameObject;
        treeMask = LayerMask.GetMask("Tree");
        treeAndSeedMask = LayerMask.GetMask("Tree", "Seed");
        lifeSpan += Random.Range(-lifeSpanVariance, lifeSpanVariance) * lifeSpan;
        ratioTotal = 0;
        //firstAnim = false;
        haloTime = Globals.SkyScript.timeScaleThatHaloAppears;
        stateMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            stateMarks.Add(ratioTotal);
        }

        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching

        numSpawned = 0;
        state = 0;
        age = 0.0f;
        donePropogating = false;
        health = max_health;
        lastSpawned = Globals.time;


        // tries to place on terrain. destroys otherwise.
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)){
            if (hit.point.y < Globals.water_level)
                Destroy(gameObject);
            else transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
        }else Destroy(gameObject);

        // dirtMound
        if (dirtMound) {
            dirtMound = Instantiate(dirtMound);
            dirtMound.transform.SetParent(transform, false);
            dirtMound.transform.position += dirtMoundOffset;
            dirtMound.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            dirtMound.SetActive(false);
        }
    }

    void Start() {
        //set state
        for (int i = 0; i < stateAnimationNames.Count; i++) {
            if (age / lifeSpan < (stateMarks[i + 1]) / ratioTotal) {
                state = i;
                Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]);
                break;
            }
        }
        //updateAnimation(); //update grow animation for the first time
        lastAnimUpdate = Globals.time;
        StartCoroutine("tickUpdate");
    }
	
	// Update is called once per frame
	void Update () {
        age += Globals.deltaTime / Globals.time_resolution; // update age
        if (age > lifeSpan) { // kill if too old
            Destroy(gameObject);
            return;
        }

        /*if(!firstAnim){
            if (thisRenderer.isVisible) {
                firstAnim = true;
                anim.enabled = true;
                expensiveUpdates();
            }
        }else anim.enabled = false;*/

        expensiveUpdates(); //temp
        if (Globals.time_scale < haloTime) {
            if (Globals.time > lastAnimUpdate + (lifeSpan * ratioAnimUpdates * Globals.time_resolution)) {
                //anim.enabled = true;
                expensiveUpdates();
                lastAnimUpdate = Globals.time;
            }
        }else{
            //anim.enabled = true;
            expensiveUpdates();
        }
    }

    // Coroutine is called once per second
    IEnumerator tickUpdate() {
        while (true) {
            yield return new WaitForSeconds(1);
            //if (Globals.time_scale <= 1) updateAnimation();
            stickToGround();
            //Cull();
            if (dirtMound) dirtMound.SetActive(state == 0);
            if (propogateDuringState[state]) Propogate();
            if (onFire) fireSpread();
        }
    }

    private void expensiveUpdates() {
        if (age / lifeSpan > (stateMarks[state + 1] / ratioTotal)) { //update state
            state++;
            Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]);
        }
        updateAnimation();
        updateCollision();
    }

    private void stickToGround(){
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x, 10000000, transform.position.z), Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
        else Destroy(gameObject);
    }

    // Creates seeds in a radius around this tree if it's ready to
    private void Propogate()
    {
        if (donePropogating) return;
        if (Globals.time > lastSpawned + spawn_delay)
        {
            float iterations = (Globals.time / (lastSpawned + spawn_delay));
            for(int i = 0; i < iterations; i++)
                createSeed();
        }
    }

    private void createSeed()
    {
        float randAngle = Random.Range(0, Mathf.PI * 2);
        float randDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector2 randomPoint = new Vector2(Mathf.Cos(randAngle) * randDistance + transform.position.x, Mathf.Sin(randAngle) * randDistance + transform.position.z);

        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(randomPoint.x, 10000000, randomPoint.y), Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
        {
            Collider[] objectsInRange = Physics.OverlapSphere(hit.point, cull_radius, treeAndSeedMask);
            if (objectsInRange.Length > seed_cull_max_density) return;
            GameObject seed = Instantiate(seed_object);
            seed.GetComponent<InteractableObject>().plant(hit.point);
            numSpawned++;
            lastSpawned = Globals.time + Mathf.Max(( Random.Range(-spawn_variance, spawn_variance)) * spawn_delay,0);
            if (spawn_limit > 0 && numSpawned >= spawn_limit) donePropogating = true;
        }
    }

    // If there are too many or nearby trees for too long, destroy this one
    private void Cull(){
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cull_radius, treeMask);
        if (age > lifeSpan || objectsInRange.Length > cull_max_density){
            health -= 1;
            if(health <= 0) Destroy(gameObject);
        }else if (health < max_health) health += 0.1f;
        else if (health > max_health) health = max_health;
    }

    //if anim name is blank, have it freeze at the last frame of the closest animation before it that is not blank
    //this means that the first element in stateAnimationNames cannot be blank
    private void updateAnimation() {
        if (stateAnimationNames[state] != ""){
            if (!anim.IsPlaying(stateAnimationNames[state])) anim.Play(stateAnimationNames[state]);
            anim[stateAnimationNames[state]].time = anim[stateAnimationNames[state]].length * ((age / lifeSpan) - (stateMarks[state] / ratioTotal)) / stateRatios[state];
        } else {
            for (int i = state-1; i >= 0; i--) { // look backwards for available animation
                if (stateAnimationNames[i] != ""){
                    if (!anim.IsPlaying(stateAnimationNames[i])) anim.Play(stateAnimationNames[i]);
                    anim[stateAnimationNames[i]].time = anim[stateAnimationNames[i]].length;
                    break;
                }
            }
        }
    }

    private void updateCollision(){
        // update collision
        float growth = heightVSTime[state].Evaluate(age / lifeSpan);
        boxCollider.size = new Vector3(0.4f + 0.6f * growth, target_scale * growth, 0.4f + 0.6f * growth);
        boxCollider.center = new Vector3(0, target_scale * growth * 0.5f, 0);
    }

    void OnMouseDown(){
        if (Globals.PlayerScript.has("Torch")){
            if (!onFire) onFire = true;
            /*GameObject Instance = (GameObject)*/Instantiate(fire, transform.position, Quaternion.identity);
        }
    }
    
    private void fireSpread(){
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cul_spread, treeMask);
        for (int i = 0; i < objectsInRange.Length; i++){
            GameObject curtree = objectsInRange[i].gameObject;
            TreeScript curtreeScript = curtree.GetComponent<TreeScript>();
            if (!curtreeScript.onFire){
                curtreeScript.GetComponent<TreeScript>().onFire = true;
                /*GameObject Instance = (GameObject)*/Instantiate(fire, curtree.transform.position, Quaternion.identity);
            }        
        }
    }
    
    public int getState(){ //0:growing,1:mature,2:dying,3:burnt
        if(onFire) return 3;
        else return state;
    }
}
