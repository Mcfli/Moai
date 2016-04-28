using UnityEngine;
using System.Collections;

public class Prop : MonoBehaviour {
    public string animationName;
    public float animationTime;

	// Use this for initialization
	void Start () {
        Animation anim = GetComponent<Animation>();
        anim.Play(animationName);
        anim[animationName].time = anim[animationName].length * animationTime;
        anim[animationName].speed = 0;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
