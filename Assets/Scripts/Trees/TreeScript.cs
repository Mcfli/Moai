using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {
    public GameObject prefab;

    //lifeSpan
    public float age;
    public float lifeSpan = 657000; // base life span, in seconds, will be modified by lifeSpanVariance when instantiated
    public float lifeSpanVariance = 0.1f; // in ratio

    //animating
    public float ratioAnimUpdates = 0.01f; // in ratio

    //animation, collision, states
    public List<string> stateAnimationNames; //names of animation, leave blank if no animation, currently unused
    public List<float> stateRatios; //ratio of each state, currently unused
    public List<PuzzleObject> statePuzzleObjects; //puzzleObject component for each state
    public float maxHeight;
    public List<AnimationCurve> heightVSTime;

    //fire
    public float cul_spread;
    public bool onFire;

    //sapling
    public GameObject sapling;
    public Vector3 saplingOffset;

    ///-------PRIVATES-------///

    //References
    private Animation anim;
    private BoxCollider boxCollider;
    private LayerMask treeMask;
    private LayerMask treeAndSeedMask;
    //private Renderer thisRenderer; // unused

    //animation, collision, states
    //private bool firstAnim;
    private float ratioTotal;
    private List<float> stateMarks;
    private float lastAnimUpdate;
    private int state;

    //fire
    private GameObject fire;
    private GameObject Torch;

    // Use this for initialization
    void Awake(){
        // finals
        anim = GetComponent<Animation>();
        boxCollider = GetComponent<BoxCollider>();
        fire = Resources.Load("fire") as GameObject;
        treeMask = LayerMask.GetMask("Tree");
        treeAndSeedMask = LayerMask.GetMask("Tree", "Seed");
        lifeSpan += Random.Range(-lifeSpanVariance, lifeSpanVariance) * lifeSpan;

        ratioTotal = 0;
        stateMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            stateMarks.Add(ratioTotal);
        }

        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching
        
        state = 0;
        age = 0.0f;


        // tries to place on terrain. destroys otherwise.
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)){
            if (hit.point.y < Globals.water_level)
                Destroy(gameObject);
            else transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
        }else Destroy(gameObject);

        // pre-growth
        if (sapling) {
            sapling = Instantiate(sapling);
            sapling.transform.SetParent(transform, false);
            sapling.transform.position += saplingOffset;
            sapling.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            sapling.SetActive(false);
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
        
        lastAnimUpdate = Globals.time;
        StartCoroutine("tickUpdate");
    }
	
	// Update is called once per frame
	void Update () {
        age += Globals.deltaTime / Globals.time_resolution; // update age
        if (age > lifeSpan) { // kill if too old
            kill();
            return;
        }
        
        if (Globals.time > lastAnimUpdate + (lifeSpan * ratioAnimUpdates * Globals.time_resolution)) {
            if(age / lifeSpan > (stateMarks[state + 1] / ratioTotal)) { //update state
                state++;
                Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]);
            }
            updateAnimation();
            updateCollision();
            lastAnimUpdate = Globals.time;
        }
    }

    // Coroutine is called once per second
    IEnumerator tickUpdate() {
        while (true) {
            yield return new WaitForSeconds(1);
            //if (Globals.time_scale <= 1) updateAnimation();
            stickToGround();
            if (sapling) sapling.SetActive(state == 0);
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
        boxCollider.size = new Vector3(0.4f + 0.6f * growth, maxHeight * growth, 0.4f + 0.6f * growth);
        boxCollider.center = new Vector3(0, maxHeight * growth * 0.5f, 0);
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

    private void kill() {
        Destroy(gameObject);
    }
    
    public int getState(){ //0:growing,1:mature,2:dying,3:burnt
        if(onFire) return 3;
        else return state;
    }
}
