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
            if(!fader.isFading() && fader.targetColor == Color.white)
            {
				Globals.Player.transform.position = telePos + new Vector3(-5, 140,-5);
                Globals.PlayerScript.warpToGround(Globals.PlayerScript.transform.position.y, true);
                fader.fade(Color.clear, 1.5f);
            } 
			else if(!fader.isFading() && fader.targetColor == Color.clear)
			{
                fromIsland = false;
            }
		}
	}

    void OnMouseDown()
    {
        float dist = Vector3.Distance(Globals.Player.transform.position, transform.position);
        if (dist < lightUpDistance && Globals.time_scale > 0 && Time.timeScale > 0 && !fromIsland)
        {
            fader.fade(Color.white, 1.5f);
            fromIsland = true;
        }
    }
}
