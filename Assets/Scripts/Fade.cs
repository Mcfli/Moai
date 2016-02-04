using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour {

    public ScreenFader fade;
    private bool pressed = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (!pressed)
        {
            if (Input.GetKey(KeyCode.E))
            {
                pressed = true;
                fade.fadeIn = !fade.fadeIn;
                if (SceneManager.GetActiveScene().buildIndex == 0)
                    SceneManager.LoadScene(1);
                else if (SceneManager.GetActiveScene().buildIndex == 1)
                    SceneManager.LoadScene(0);
                pressed = false;
            }
        }
	}
}
