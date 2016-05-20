using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordWall : MonoBehaviour {

    public List<WordWallText> texts;

    private bool generated = false;
    private Renderer rend;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!generated && Vector3.Distance(transform.position, Globals.Player.transform.position) < 100)
        {
            generateText();
        }
	}

    private void generateText()
    {
        
        generated = true;
        
    }
}
