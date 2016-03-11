using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {

    public GameObject seed_object;
    public GameObject prefab;
    public float max_health = 100;
    public float minSpawnRadius;
    public float maxSpawnRadius;
    //public float spawn_chance;
    public float spawn_delay;
    public float spawn_variance;
    public int seed_cull_max_density;
    public int spawn_limit;
    public float cull_radius;
    public int cull_max_density;
    public float target_scale;
    public float lifeSpan = 657000; // base life span, in seconds, will be modified by lifeSpanVariance when instantiated
    public float lifeSpanVariance = 0.1f; // in ratio
    public GameObject dirtMound;
    public Vector3 dirtMoundOffset;

    //for grow and states
    public List<string> stateAnimationNames; //names of animation, leave blank if no animation, currently unused
    public List<float> stateRatios; //ratio of each state, currently unused
    public List<PuzzleObject> statePuzzleObjects; //puzzleObject component for each state
    public List<bool> propogateDuringState;
    public List<AnimationCurve> heightVSTime;

    public float cul_spread;
    public bool onFire;

    //save values
    public Vector3 saved_position;
    public Quaternion saved_rotation;
    public float age;

    private LayerMask treeMask;
    private LayerMask treeAndSeedMask;
    private bool donePropogating;
    private float lastSpawned = 0.0f;
    private int numSpawned;
    private float health;
    private Animation anim;
    private BoxCollider boxCollider;
    
    //for grow and states
    private int state;
    private float ratioTotal;
    private List<float> animMarks;

    private GameObject fire;
    private GameObject Torch;

    private BoxCollider bc;
    private PuzzleObject puzzleObj;

    public void saveTransforms(){
        saved_position = transform.position;
        saved_rotation = transform.rotation;
    }

    // Use this for initialization
    void Awake(){
        anim = GetComponent<Animation>();
        boxCollider = GetComponent<BoxCollider>();

        fire = Resources.Load("fire") as GameObject;
        state = -1;
        age = 0.0f;
        donePropogating = false;
        health = max_health;
        treeMask = LayerMask.GetMask("Tree");
        treeAndSeedMask = LayerMask.GetMask("Tree", "Seed");
        //Collider[] hitColiders = Physics.OverlapSphere(Vector3.zero, radius);
        numSpawned = 0;
        lastSpawned = Globals.time - Random.value * spawn_delay;
        lifeSpan += Random.Range(-lifeSpanVariance, lifeSpanVariance) * lifeSpan;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
        bc = GetComponent<BoxCollider>();
        puzzleObj = GetComponent<PuzzleObject>();
        int terrain = LayerMask.GetMask("Terrain");
        
        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching
        
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)){

            if (hit.point.y < Globals.water_level)
                Destroy(gameObject);
            else
                transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
        }
        else{
            Destroy(gameObject);
        }

<<<<<<< HEAD
        // Start delayed update
        StartCoroutine("tickUpdate");
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    // Coroutine
    IEnumerator tickUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            Cull();
            Grow();
            if (age >= 0.03)
                Propogate();
            if (onFire)
                fireSpread();
=======
        // for grow and states
        ratioTotal = 0;
        animMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            animMarks.Add(ratioTotal);
        }

        //dirtMound
        if (dirtMound) {
            dirtMound = Instantiate(dirtMound);
            dirtMound.transform.SetParent(transform, false);
            dirtMound.transform.position += dirtMoundOffset;
            dirtMound.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            dirtMound.SetActive(false);
        }
    }

    void Start() {
        for (int i = 0; i < stateAnimationNames.Count; i++) {
            if (age / lifeSpan < (animMarks[i + 1]) / ratioTotal) {
                state = i;
                break;
            }
        }
        StartCoroutine("tickUpdate");
    }
	
	// Update is called once per frame
	void Update () {
        age += Globals.deltaTime / Globals.time_resolution;
        if (age > lifeSpan) {
            Destroy(gameObject);
            return;
        }

        Grow();
        if(dirtMound) dirtMound.SetActive(state == 0);
    }

    // Coroutine is called once per second
    IEnumerator tickUpdate() {
        while (true) {
            yield return new WaitForSeconds(1);
            stickToGround();
            Cull();
            if (propogateDuringState[state]) Propogate();
            if (onFire) fireSpread();
>>>>>>> brains2
        }
    }

    private void stickToGround(){
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x, 10000000, transform.position.z), Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
        else Destroy(gameObject);
    }

    // Creates seeds in a radius around this tree if it's ready to
    private void Propogate(){
        if (donePropogating) return;
        if ((Globals.time - lastSpawned) / Globals.time_resolution > spawn_delay) { // if(Random.value < spawn_chance) {
            float randAngle = Random.Range(0, Mathf.PI * 2);
            float randDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector2 randomPoint = new Vector2(Mathf.Cos(randAngle) * randDistance + transform.position.x, Mathf.Sin(randAngle) * randDistance + transform.position.z);

            RaycastHit hit;
            Ray rayDown = new Ray(new Vector3(randomPoint.x, 10000000, randomPoint.y), Vector3.down);
            if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))) {
                Collider[] objectsInRange = Physics.OverlapSphere(hit.point, cull_radius, treeAndSeedMask);
                if (objectsInRange.Length > seed_cull_max_density) return;
                GameObject seed = Instantiate(seed_object);
                seed.GetComponent<InteractableObject>().plant(hit.point);
                numSpawned++;
                lastSpawned = Globals.time + Random.Range(-spawn_variance, spawn_variance) * spawn_delay;
                if (spawn_limit > 0 && numSpawned >= spawn_limit) donePropogating = true;
            }
        }
    }

    // If there are too many or nearby trees for too long, destroy this one
    private void Cull(){
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cull_radius, treeMask);
        if (age > lifeSpan || objectsInRange.Length > cull_max_density)
        {
            health -= 1;
            if(health <= 0)
            {
                donePropogating = true;

                Destroy(gameObject);
            }    
        }
        else if (health < max_health)
        {
            health += 0.1f;
        }
        else if (health > max_health)
        {
            health = max_health;
        }
    }

    private void Grow() { //also updates state
        if (age / lifeSpan > (animMarks[state + 1] / ratioTotal)) {
            state++;
            Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]);
        }
        
<<<<<<< HEAD
        if(useNewAnimationSystem){
            if(age/life_span < growDieAnimationRatio){//growing
                if(!anim.IsPlaying("growing")) anim.Play("growing");
                anim["growing"].time = anim["growing"].length * age/(life_span*growDieAnimationRatio);
                puzzleObj.image = puzzleIcons[0];
                state = 0;
            }else if(age/life_span > 1 - growDieAnimationRatio){//dying
                if(!anim.IsPlaying("dying")) anim.Play("dying");
                anim["dying"].time = anim["dying"].length * (age - (1 - growDieAnimationRatio)*life_span) / (life_span*growDieAnimationRatio);
                puzzleObj.image = puzzleIcons[1];
                state = 1;
            }else{ //mature
                if(!anim.IsPlaying("growing")) anim.Play("growing");
                anim["growing"].time = anim["growing"].length;
                puzzleObj.image = puzzleIcons[2];
                state = 2;
            }
        }else{
            state = 1;
            foreach (AnimationState animState in anim){
                //state.speed = Globals.time_scale*grow_speed;
                animState.time = age;
                anim_progress = animState.time / animState.length;
            }
        }
        float growth = height_vs_time.Evaluate(anim_progress);
        bc.size = new Vector3(0.4f+0.6f*growth, target_scale*growth, 0.4f+0.6f*growth);
        bc.center = new Vector3(0, target_scale*growth*0.5f, 0);
=======
        //updateAnimation
        //if anim name is blank, have it freeze at the last frame of the closest animation before it that is not blank
        //this means that the first element in stateAnimationNames cannot be blank
        if (stateAnimationNames[state] != ""){
            if (!anim.IsPlaying(stateAnimationNames[state])) anim.Play(stateAnimationNames[state]);
            anim[stateAnimationNames[state]].time = anim[stateAnimationNames[state]].length * ((age / lifeSpan) - (animMarks[state] / ratioTotal)) / stateRatios[state];
        } else {
            for (int i = state-1; i >= 0; i--) { // look backwards for available animation
                if (stateAnimationNames[i] != ""){
                    if (!anim.IsPlaying(stateAnimationNames[i])) anim.Play(stateAnimationNames[i]);
                    anim[stateAnimationNames[i]].time = anim[stateAnimationNames[i]].length;
                    break;
                }
            }
        }
>>>>>>> brains2

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
