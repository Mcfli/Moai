using UnityEngine;
using System.Collections;

public class AnimationTimeScale : MonoBehaviour {
    private Animation anim;

	// Use this for initialization
	void Awake () {
        anim = GetComponent<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
        foreach(AnimationState animState in anim) animState.speed = Globals.time_scale;
    }
}
