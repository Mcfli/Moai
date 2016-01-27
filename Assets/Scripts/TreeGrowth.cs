using UnityEngine;
using System.Collections;

public class TreeGrowth : MonoBehaviour {

	public string buttonName;
	public float rate;
	private float growthRate;
	private float currScaleX;
	private float currScaleY;
	private float currScaleZ;

	private void Start(){
		currScaleX = transform.localScale.x;
		currScaleY = transform.localScale.y;
		currScaleZ = transform.localScale.z;
	}

	private void Update(){
		currScaleX = transform.localScale.x;
		currScaleY = transform.localScale.y;
		currScaleZ = transform.localScale.z;
		if(Input.GetButton(buttonName)){
			transform.localScale += new Vector3 (currScaleX + rate/1000000, currScaleY + rate/1000000, currScaleZ + rate/1000000);
		}
		else {
			// do not change
		}
	}
}
