using UnityEngine;
using System.Collections;

public class TeleportStone : MonoBehaviour {

    public GameObject linkedObelisk;
    public float lightUpDistance = 10f;

	// Screen Fader
	private FadeInOut fader;

	private Vector3 telePos;

	private bool fromIsland = false;

	// Use this for initialization
	void Start () {
		fader = GameObject.Find("UI").GetComponent<FadeInOut> ();
		telePos = linkedObelisk.transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(fromIsland)
		{
			if (fader.fadingWhite) 
			{
				fader.fadeToWhite ();
			}
			if (fader.fadeImage.color.a >= 0.95f && fader.fadingWhite)
			{
				fader.fadeImage.color = Color.white;
				Globals.Player.transform.position = telePos + new Vector3(-5, 140,-5);
				fader.fadingClear = true;
				fader.fadingWhite = false;
			} 
			else if(fader.fadingClear)
			{
				fader.fadeToClear ();
				if(fader.fadeImage.color.a <= 0.05f)
				{
					fader.fadeImage.color = Color.clear;
					fader.fadingClear = false;
                    fromIsland = false;
				}
			}
		}
	}

    void OnMouseDown()
    {
        float dist = Vector3.Distance(Globals.Player.transform.position, transform.position);
        if (dist < lightUpDistance && Globals.time_scale > 0 && Time.timeScale > 0 && !fader.fadingWhite && !fader.fadingClear)
        {
			fader.fadingWhite = true;
			fromIsland = true;
        }
    }
}
