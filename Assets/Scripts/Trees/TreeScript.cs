using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {

    public GameObject seed_object;
    public GameObject prefab;
    public float max_health = 100;
    public float radius;
    public float spawn_delay;
    public float spawn_delay_variance;
    public int spawn_limit;
    public float cull_radius;
    public int cull_max_density;
    public float target_scale;
    public float lifeSpan = 657000; // base life span, in seconds, will be modified by lifeSpanVariance when instantiated
    public float lifeSpanVariance = 0.1f; // in ratio
    
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
    public float time_unloaded;
    public float age;

    private LayerMask treeMask;
    private bool done = false;
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


    public void saveTransforms(){
        saved_position = transform.position;
        saved_rotation = transform.rotation;
        time_unloaded = Globals.time;
    }

    // Use this for initialization
    void Awake(){
        anim = GetComponent<Animation>();
        boxCollider = GetComponent<BoxCollider>();

        fire = Resources.Load("fire") as GameObject;
        state = -1;
        age = 0.0f;
        health = max_health;
        treeMask = LayerMask.GetMask("Tree");
        spawn_delay += Random.value * 2 * spawn_delay_variance - spawn_delay_variance;
        //Collider[] hitColiders = Physics.OverlapSphere(Vector3.zero, radius);
        numSpawned = 0;
        lastSpawned = Time.time;
        lifeSpan += Random.Range(-lifeSpanVariance, lifeSpanVariance) * lifeSpan;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
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

        // for grow and states
        ratioTotal = 0;
        animMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            animMarks.Add(ratioTotal);
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
    }

    // Coroutine is called once per second
    IEnumerator tickUpdate() {
        while (true) {
            yield return new WaitForSeconds(1);
            stickToGround();
            Cull();
            Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]); // change puzzle element
            if (propogateDuringState[state]) Propogate();
            if (onFire) fireSpread();
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
        if (!done) {
            if (Time.time - lastSpawned > spawn_delay) {
                lastSpawned = Time.time;
                Vector2 randomPoint = Random.insideUnitCircle * radius + new Vector2(transform.position.x, transform.position.z);

                RaycastHit hit;
                Ray rayDown = new Ray(new Vector3(randomPoint.x, 10000000, randomPoint.y), Vector3.down);
                if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))) {
                    GameObject seed = Instantiate(seed_object);
                    seed.GetComponent<InteractableObject>().plant(hit.point);
                    numSpawned++;
                    if (spawn_limit > 0 && numSpawned >= spawn_limit) {
                        done = true;
                    }
                }
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
                done = true;

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
        if (age / lifeSpan > (animMarks[state + 1] / ratioTotal)) state++;
        
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
