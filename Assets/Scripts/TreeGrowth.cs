using UnityEngine;
using System.Collections;

public class TreeGrowth : MonoBehaviour {

	public string buttonName;
    public float targetScale;
    public float growSpeed;
    private Vector3 v3Scale;

	private void Update(){
        Grow();
		if(Input.GetButton(buttonName)){
            //Grow();
		}
		else {
			// do not change
		}
	}

    private void Grow() {
        v3Scale = new Vector3(targetScale, targetScale, targetScale);
        transform.localScale = Vector3.Lerp(transform.localScale, v3Scale, Time.deltaTime * growSpeed);
    }
}
