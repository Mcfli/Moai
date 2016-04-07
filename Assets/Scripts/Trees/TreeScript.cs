using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : MonoBehaviour {
    //prefab
    public GameObject prefab;
    public float baseLifeSpan = 657000; // base life span in seconds
    public float lifeSpanVariance = 0.3f; // in ratio

    //animating
    public float ratioAnimUpdates; // in ratio

    //animation, collision, states
    public List<string> stateAnimationNames; //names of animation, leave blank if no animation, currently unused
    public List<float> stateRatios; //ratio of each state, currently unused
    public List<PuzzleObject> statePuzzleObjects; //puzzleObject component for each state
    public float maxHeight;
    public List<AnimationCurve> heightVSTime; //collision

    //sapling
    public GameObject sapling;
    public Vector3 saplingOffset;

    ///-------PRIVATES-------///
    
    private float age;
    private float deathAge;

    //References
    private Animation anim;
    private BoxCollider boxCollider;
    private bool firstAnim;

    //animation, collision, states
    private float ratioTotal;
    private List<float> stateMarks;
    private float lastGrowUpdate;
    private int state;

    // Use this for initialization
    void Awake(){
        // references
        anim = GetComponent<Animation>();
        boxCollider = GetComponent<BoxCollider>();

        age = 0;
        deathAge = baseLifeSpan + Random.Range(-lifeSpanVariance, lifeSpanVariance) * baseLifeSpan;
        state = 0;
        firstAnim = false;

        ratioTotal = 0;
        stateMarks = new List<float> { 0 };
        foreach (float f in stateRatios) {
            ratioTotal += f;
            stateMarks.Add(ratioTotal);
        }

        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching

        // sapling
        if (sapling) {
            sapling = Instantiate(sapling);
            sapling.transform.SetParent(transform, false);
            sapling.transform.position += saplingOffset;
            sapling.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
        }
    }

    void Start() {
        grow();
    }
	
	// Update is called once per frame
	void Update () {
        age += Globals.deltaTime / Globals.time_resolution; // update age

        if (age > deathAge) { // kill if too old
            kill();
            return;
        }

        if (Globals.time > lastGrowUpdate + (deathAge * ratioAnimUpdates * Globals.time_resolution)) grow();
        //else if (!firstAnim) firstAnim = true;
        //else anim.enabled = false;
    }

    private void grow() {
        updateState();
        updateAnimation();
        updateCollision();
        lastGrowUpdate = Globals.time;
    }

    private void updateState(){
        if (age / deathAge >= stateMarks[state + 1] / ratioTotal || age / deathAge < stateMarks[state] / ratioTotal){
            for (int i = 0; i < stateMarks.Count - 1; i++){
                if (age / deathAge < (stateMarks[i + 1]) / ratioTotal){
                    state = i;
                    if (sapling) sapling.SetActive(state == 0);
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
            anim[stateAnimationNames[state]].time = anim[stateAnimationNames[state]].length * ((age / deathAge) - (stateMarks[state] / ratioTotal)) / stateRatios[state];
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
        float size = heightVSTime[state].Evaluate((age / deathAge - stateMarks[state]) / stateRatios[state]);
        boxCollider.size = new Vector3(0.4f + 0.6f * size, maxHeight * size, 0.4f + 0.6f * size);
        boxCollider.center = new Vector3(0, maxHeight * size * 0.5f, 0);
    }

    private void kill() {
        Destroy(gameObject);
    }

    public float getAge(){
        return age;
    }

    public float getDeathAge(){
        return deathAge;
    }

    public void setAge(float age, float deathAge){
        this.age = age; this.deathAge = deathAge;
    }
}
