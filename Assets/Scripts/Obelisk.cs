using UnityEngine;
using System.Collections;

public class Obelisk : MonoBehaviour {

    // Control lighting up
    private bool litUp = false; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseOver()
    {
        if (!litUp) litUp = true;
    }

    void OnMouseExit()
    {
        if (litUp) litUp = false;
    }

    void applyLitEffects()
    {

    }
}
