using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class menuScript : MonoBehaviour {

	public Canvas quitMenu;
	public Button startText;
	public Button exitText;

	// Use this for initialization
	void Start () {
		quitMenu = quitMenu.GetComponent<Canvas> ();
		startText = startText.GetComponent<Button> ();
		exitText = exitText.GetComponent<Button> ();
		quitMenu.enabled = false;
	}

	// quit yes button pressed
	public void ExitPress(){
		quitMenu.enabled = true;
		startText.enabled = false;
		exitText.enabled = false;
	}

	// quit no button pressed 
	public void NoPress(){
		quitMenu.enabled = false;
		startText.enabled = true;
		exitText.enabled = true;
	}

	public void StartLevel(){
		Application.LoadLevel (0);
	}

	public void ResumeLevel() {
		Application.LoadLevel(Application.loadedLevel);
	}


	// exit game pressed
	public void ExitGame(){
		Application.Quit ();
	}
}
