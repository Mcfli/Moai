using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {
    //prefab
    public string prefabPath;
    public GameObject seed_object;
    public float baseLifeSpan = 657000; // base life span in seconds
    public float lifeSpanVariance = 0.3f; // in ratio
    public float scaleVariance = 0.25f; // in ratio

    //animating
    public float ratioAnimUpdates; // in ratio

    //animation, collision, bounds, states
    public List<string> stateAnimationNames; //names of animation, leave blank if no animation, currently unused
    public List<float> stateRatios; //ratio of each state, currently unused
    public List<PuzzleObject> statePuzzleObjects; //puzzleObject component for each state
    public List<bool> propogateDuringState;
    public char heightAxis;
    public Vector2 collisionRadiusMinMax;
    public List<AnimationCurve> collisionVSTime;

    //dirtMound
    public GameObject dirtMound;
    public Vector3 dirtMoundOffset;
    public float dirtMoundLifeRatio;

    ///-------PRIVATES-------///
    [HideInInspector] public GameObject prefab;
    [HideInInspector] public float age;
    [HideInInspector] public float lifeSpan;
    
    //References
    private Animation anim;
    private CapsuleCollider capCollider;
    private SkinnedMeshRenderer rend;
    private bool animateNext;
    private bool updateBoxes;

    //animation, collision, states
    private float ratioTotal;
    private List<float> stateMarks;
    private float lastGrowUpdate;
    private int state;

    private ForestScript forestParent;

    // Use this for initialization
    // will initialize with random scale and lifeSpan with age of 0
    void Awake(){
        // references
        anim = GetComponent<Animation>();
        capCollider = GetComponent<CapsuleCollider>();
        rend = GetComponent<SkinnedMeshRenderer>();

        prefab = Resources.Load(prefabPath) as GameObject;

        gameObject.transform.localScale += gameObject.transform.localScale * Random.Range(-scaleVariance, scaleVariance);

        age = 0;
        lifeSpan = baseLifeSpan + Random.Range(-lifeSpanVariance, lifeSpanVariance) * baseLifeSpan;
        state = 0;
        animateNext = true;
        updateBoxes = false;

        ratioTotal = 0;
        stateMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            stateMarks.Add(ratioTotal);
        }

        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching

        // dirtMound
        if (dirtMound) {
            dirtMound = Instantiate(dirtMound, transform.position, transform.rotation) as GameObject;
            dirtMound.transform.parent = transform;
            dirtMound.transform.localPosition += dirtMoundOffset;
        }
    }

    void Start() {
        grow();
    }

    void OnDestroy(){
        forestParent.removeTree(GetInstanceID());
    }

    // Update is called once per frame
    void Update () {
        age += Globals.deltaTime / Globals.time_resolution; // update age

        if (age > lifeSpan) { // kill if too old
            if(forestParent) forestParent.removeTree(GetInstanceID());
            Destroy(gameObject);
            return;
        }

        if (Globals.time > lastGrowUpdate + (lifeSpan * ratioAnimUpdates * Globals.time_resolution)) grow();

        if(updateBoxes) {
            updateCollision();
            updateBounds();
            updateBoxes = false;
        }

        if(animateNext && rend.isVisible) {
            anim.enabled = true;
            animateNext = false;
            updateBoxes = true;
        } else anim.enabled = false;
    }

    public void grow() {
        if(dirtMound) dirtMound.SetActive(age/lifeSpan < dirtMoundLifeRatio);
        updateState();
        updateAnimation();
        lastGrowUpdate = Globals.time;
        animateNext = true;
    }

    public void setForestParent(ForestScript forest) {
        forestParent = forest;
    }

    public bool canPropogate() {
        return propogateDuringState[state];
    }

    // returns true if forest found, and false if it created a forest
    public void findForest() {
        Collider[] col = Physics.OverlapSphere(transform.position, seed_object.GetComponent<InteractableObject>().cull_radius, LayerMask.GetMask("Forest"));
        if(col.Length > 0) {
            forestParent = col[0].gameObject.GetComponent<ForestScript>();
            if(forestParent.amountOfTrees() > forestParent.maxTrees) Destroy(gameObject);
            else forestParent.addTree(this);
        } else {
            if(!TreeManager.loadedForests.ContainsKey(GenerationManager.worldToChunk(transform.position))) { //outside of tree load dist
                Destroy(gameObject);
                return;
            }
            Biome biome = Globals.GenerationManagerScript.chooseBiome(GenerationManager.worldToChunk(transform.position));
            GameObject g = new GameObject("Forest");
            forestParent = g.AddComponent(typeof(ForestScript)) as ForestScript;
            forestParent.createForest(transform.position, biome.forestRadius, biome.forestMaxTrees);
            forestParent.addTree(this);
            TreeManager.loadedForests[GenerationManager.worldToChunk(transform.position)].Add(forestParent.GetInstanceID(), forestParent);
        }
    }

    private void updateState(){
        if (age / lifeSpan >= stateMarks[state + 1] / ratioTotal || age / lifeSpan < stateMarks[state] / ratioTotal){
            for (int i = 0; i < stateMarks.Count - 1; i++){
                if (age / lifeSpan < (stateMarks[i + 1]) / ratioTotal){
                    state = i;
                    Globals.CopyComponent<PuzzleObject>(gameObject, statePuzzleObjects[state]);
                    break;
                }
            }
        }
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

    private void updateCollision() {
        capCollider.center = new Vector3(capCollider.center.x, (rend.rootBone.position.y - transform.position.y) / 2 / transform.localScale.y, capCollider.center.z);
        capCollider.height = (rend.rootBone.position.y - transform.position.y) / transform.localScale.y;
        capCollider.radius = Mathf.Lerp(collisionRadiusMinMax.x, collisionRadiusMinMax.y, collisionVSTime[state].Evaluate((age / lifeSpan - stateMarks[state]) / stateRatios[state]));
    }

    private void updateBounds() {
        Vector3 center = rend.localBounds.center;
        Vector3 size = rend.localBounds.size;
        float height = (rend.rootBone.position.y - transform.position.y) / rend.rootBone.lossyScale.y;
        if(heightAxis == 'x' || heightAxis == 'X') {
            center.x = height / 2;
            size.x = height;
        } else if(heightAxis == 'z' || heightAxis == 'Z') {
            center.z = height / 2;
            size.z = height;
        } else {
            center.y = height / 2;
            size.y = height;
        }
        rend.localBounds = new Bounds(center, size);
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
