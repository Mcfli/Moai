using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressurePlateTrigger : MonoBehaviour {
	bool active = false;
	private List<GameObject> pressing = new List<GameObject>();
	private Vector3 origPosition;
	private GameObject parentObject;

	// Use this for initialization
	void Start () {
		parentObject = transform.parent.gameObject;
		transform.parent = parentObject.transform.parent;
		origPosition = parentObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(pressing.Count > 0) active = true;
		else active = false;
		
		if(active) parentObject.transform.position = origPosition + new Vector3(0.0f, -0.15f, 0.0f);
		else parentObject.transform.position = origPosition;
	}
	
	void OnTriggerEnter (Collider col){
		if(col.attachedRigidbody != null || col.gameObject.tag == "Player") pressing.Add(col.gameObject);
    }
	
	void OnTriggerExit (Collider col){
        if(col.attachedRigidbody != null || col.gameObject.tag == "Player") pressing.Remove(col.gameObject);
    }
}
