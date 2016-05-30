using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeInOut : MonoBehaviour {
	public Image fadeImage;
    public Color targetColor;
    private float fadeSpeed;
    private Color fromColor;
    private bool fading;
    private float fadeProgress;

    // Use this for initialization
    void Start () {
		fadeImage.rectTransform.localScale = new Vector2 (Screen.width, Screen.height);
        fadeImage.color = Color.clear;
        fadeImage.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        fadeImage.rectTransform.localScale = new Vector2(Screen.width, Screen.height);
        if(fadeSpeed <= 0) fadeProgress = 1;
        else fadeProgress += Time.deltaTime / fadeSpeed;
        if(fadeProgress < 1) fadeImage.color = Color.Lerp(fromColor, targetColor, fadeProgress);
        else {
            fadeImage.color = targetColor;
            if(targetColor == Color.clear) fadeImage.gameObject.SetActive(false);
            fading = false;
        }
    }

    public void fade(Color target, float speed) {
        if(speed <= 0) {
            fadeImage.color = target;
            targetColor = target;
            fadeProgress = 1;
            fading = false;
            return;
        }
        fadeImage.gameObject.SetActive(true);
        targetColor = target;
        fadeSpeed = speed;
        fromColor = fadeImage.color;
        fadeProgress = 0;
        fading = true;
    }

    public bool isFading() {
        return fading;
    }
}
