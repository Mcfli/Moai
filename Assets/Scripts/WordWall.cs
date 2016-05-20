using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordWall : MonoBehaviour {

    public static List<WordWallText> texts;

    private bool generated = false;
    private Renderer rend;
    private UnityEngine.UI.Text screenText;

    // Use this for initialization
    void Start () {
        screenText = GameObject.Find("WordWallText").GetComponent<UnityEngine.UI.Text>();
        screenText.color = new Color(0,0,0,0);
    }
	
	// Update is called once per frame
	void Update () {
        if (!generated && Vector3.Distance(transform.position, Globals.Player.transform.position) < 100)
        {
            generateText();
        }
	}

    void OnMouseDown()
    {

    }

    private void generateText()
    {
        
        generated = true;
        
    }

    private IEnumerator displayText()
    {
        //WordWallText wwt = texts[Word];
        yield return null;
    }

    
}
