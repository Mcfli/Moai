using UnityEngine;
using System.Collections;

public class TreeGrowth : MonoBehaviour {

	public string buttonName = "Wait";
    //public float targetScale;
    //public float growSpeed;
    
	private Vector3 v3Scale;
	private Animation anim;

	private void Start(){
		anim = GetComponent<Animation>();
		foreach (AnimationState state in anim) {
			state.speed = 0.0F;
		}
	}

	private void Update(){
		if(Input.GetButton(buttonName)){
			//v3Scale = new Vector3(targetScale, targetScale, targetScale);
			//transform.localScale = Vector3.Lerp(transform.localScale, v3Scale, Time.deltaTime * growSpeed);
			foreach (AnimationState state in anim) {
				state.speed = 1.0F;
			}
		}
		else {
			foreach (AnimationState state in anim) {
				state.speed = 0.0F;
			}
		}
	}
}
