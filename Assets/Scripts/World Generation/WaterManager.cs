using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterManager : MonoBehaviour {

    private static Dictionary<Vector2, List<GameObject>> waterBodies;

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // Creates a water body game object of specified size at chunk specified by key
    public void createWater(Vector2 key, Vector3 size)
    {

    }

    // unloads all water bodies in chunk specified by key
    public void unloadWater(Vector2 key)
    {

    }
}
