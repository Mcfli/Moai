using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeInOut : MonoBehaviour {

	public Image fadeImage;
	public float fadeSpeed = 1.5f;
	public bool fadingWhite = false;
	public bool fadingClear = false;
	// Use this for initialization
	void Start () {
		fadeImage.rectTransform.localScale = new Vector2 (Screen.width, Screen.height);
		fadeImage.color = Color.clear;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void fadeToWhite()
	{
		fadeImage.color = Color.Lerp (fadeImage.color, Color.white, fadeSpeed * Time.deltaTime);
	}

	public void fadeToClear()
	{
		fadeImage.color = Color.Lerp (fadeImage.color, Color.clear, (fadeSpeed - 1) * Time.deltaTime);
	}
}
