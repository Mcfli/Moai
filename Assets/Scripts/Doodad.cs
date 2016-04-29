using UnityEngine;
using System.Collections;

public class Doodad : MonoBehaviour {
    public float radius;
    public float secondsToRelocate;
    public Vector3 pivotPoint;
    
    private float lastMoved;

	// Use this for initialization
	void Start () {
        lastMoved = Globals.time - secondsToRelocate * Random.value * Globals.time_resolution;
	}
	
	// Update is called once per frame
	void Update () {
	    //if(lastMoved + secondsToRelocate * Globals.time_resolution == Globals.time) 
    }
}
