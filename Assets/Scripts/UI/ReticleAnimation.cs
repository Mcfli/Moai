using UnityEngine;
using System.Collections;

public class ReticleAnimation : MonoBehaviour {
    public float secondsToFadeOut = 1.0f; // seconds it takes to fade out
    public Vector2 endingSize;
    private UnityEngine.UI.Image image;
    private RectTransform rectTrans;
    private float initialOpacity;
    private Vector2 initialSize;

    void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
        rectTrans = transform as RectTransform;
        initialOpacity = image.color.a;
        initialSize = rectTrans.sizeDelta;
    }

	void OnEnable () {
        image.color = new Color(image.color.r, image.color.g, image.color.b, initialOpacity);
        rectTrans.sizeDelta = initialSize;
    }
	
	void Update () {
        if(image.color.a - initialOpacity / secondsToFadeOut * Time.deltaTime < 0) gameObject.SetActive(false);
        rectTrans.sizeDelta = rectTrans.sizeDelta + (endingSize - initialSize) / secondsToFadeOut * Time.deltaTime;
        image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - initialOpacity / secondsToFadeOut * Time.deltaTime);
    }
}
