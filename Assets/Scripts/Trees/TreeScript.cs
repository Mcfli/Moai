using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {
    //prefab
    public GameObject prefab;
    public GameObject seed_object;
    public float baseLifeSpan = 657000; // base life span in seconds
    public float lifeSpanVariance = 0.3f; // in ratio
    public float scaleVariance = 0.25f; // in ratio

    //animating
    public float ratioAnimUpdates; // in ratio

    //animation, collision, states
    public List<string> stateAnimationNames; //names of animation, leave blank if no animation, currently unused
    public List<float> stateRatios; //ratio of each state, currently unused
    public List<PuzzleObject> statePuzzleObjects; //puzzleObject component for each state
    public float maxHeight;
    public List<AnimationCurve> heightVSTime; //collision

    //dirtMound
    public GameObject dirtMound;
    public Vector3 dirtMoundOffset;

    ///-------PRIVATES-------///

    [HideInInspector] public float age;
    [HideInInspector] public float lifeSpan;

    //References
    private Animation anim;
    private BoxCollider boxCollider;
    //private MeshRenderer meshRenderer;
    //private bool animateNext;

    //animation, collision, states
    private float ratioTotal;
    private List<float> stateMarks;
    private float lastGrowUpdate;
    private int state;

    // Use this for initialization
    // will initialize with random scale and lifeSpan with age of 0
    void Awake(){
        // references
        anim = GetComponent<Animation>();
        boxCollider = GetComponent<BoxCollider>();
        //meshRenderer = GetComponent<MeshRenderer>();

        gameObject.transform.localScale += gameObject.transform.localScale * Random.Range(-scaleVariance, scaleVariance);

        age = 0;
        lifeSpan = baseLifeSpan + Random.Range(-lifeSpanVariance, lifeSpanVariance) * baseLifeSpan;
        state = 0;
        //animateNext = true;

        ratioTotal = 0;
        stateMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            stateMarks.Add(ratioTotal);
        }

        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching

        // dirtMound
        if (dirtMound) {
            dirtMound = Instantiate(dirtMound);
            dirtMound.transform.SetParent(transform, false);
            dirtMound.transform.position += dirtMoundOffset;
            dirtMound.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
        }
    }

    void Start() {
        //anim.enabled = false;
        grow();
        RaycastHit hit;
        if(Physics.SphereCast(new Ray(transform.position, Vector3.down), seed_object.GetComponent<InteractableObject>().cull_radius, out hit, 0, LayerMask.GetMask("Forest"))) {
            hit.collider.gameObject.GetComponent<ForestScript>().addTree(this);
        } else {
            if(!TreeManager.loadedForests.ContainsKey(Globals.GenerationManagerScript.worldToChunk(transform.position))) Destroy(this);
            List<GameObject> types = new List<GameObject>();
            types.Add(prefab);
            GameObject g = new GameObject();
            ForestScript newForest = g.AddComponent(typeof(ForestScript)) as ForestScript;
            newForest.createForest(transform.position, 100, types, 16); //radius and max trees should be pulled from biome prefab
            TreeManager.loadedForests[Globals.GenerationManagerScript.worldToChunk(transform.position)].Add(newForest);
        }
    }
	
	// Update is called once per frame
	void Update () {
        age += Globals.deltaTime / Globals.time_resolution; // update age

        if (age > lifeSpan) { // kill if too old
            Destroy(gameObject);
            return;
        }

        if (Globals.time > lastGrowUpdate + (lifeSpan * ratioAnimUpdates * Globals.time_resolution)) grow();

        /*if (animateNext && meshRenderer.isVisible){
            anim.enabled = true;
            animateNext = false;
        }else anim.enabled = false;*/
    }

    private void grow() {
        updateState();
        updateAnimation();
        updateCollision();
        lastGrowUpdate = Globals.time;
        //animateNext = true;
    }

    private void updateState(){
        if (age / lifeSpan >= stateMarks[state + 1] / ratioTotal || age / lifeSpan < stateMarks[state] / ratioTotal){
            for (int i = 0; i < stateMarks.Count - 1; i++){
                if (age / lifeSpan < (stateMarks[i + 1]) / ratioTotal){
                    state = i;
                    if (dirtMound) dirtMound.SetActive(state == 0);
                    Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]);
                    break;
                }
            }
        }
    }

    //if anim name is blank, have it freeze at the last frame of the closest animation before it that is not blank
    //this means that the first element in stateAnimationNames cannot be blank
    private void updateAnimation() {
        //anim.enabled = true;
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

    private void updateCollision() {
        // update collision
        float size = heightVSTime[state].Evaluate((age / lifeSpan - stateMarks[state]) / stateRatios[state]);
        boxCollider.size = new Vector3(0.4f + 0.6f * size, maxHeight * size, 0.4f + 0.6f * size);
        boxCollider.center = new Vector3(0, maxHeight * size * 0.5f, 0);
    }
    
    public struct treeStruct {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public float age;
        public float lifeSpan;
        public GameObject prefab;

        public treeStruct(TreeScript t) {
            position = t.transform.position;
            rotation = t.transform.rotation;
            scale = t.transform.localScale;
            age = t.age;
            lifeSpan = t.lifeSpan;
            prefab = t.prefab;
        }
    }
}
