using UnityEngine;
using System.Collections;

public class TitleAnimate : MonoBehaviour {
    public float animationDuration;
    public float startingRatio;

    private UnityEngine.UI.Image image;
    private Vector2 originalSize;
    private Vector2 startingSize;
    private float startTime = -1;

    // Use this for initialization
    void Awake () {
        image = GetComponent<UnityEngine.UI.Image>();
    }
	
	// Update is called once per frame
	void Update () {
        if(startTime < 0) return;
	    if(Time.time - startTime < animationDuration) {
            image.rectTransform.sizeDelta = Vector2.Lerp(startingSize, originalSize, Mathf.Pow(((Time.time - startTime) / animationDuration), 0.25f)); //linear
            image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Pow(((Time.time - startTime) / animationDuration), 0.25f));
        } else {
            startTime = -1;
            image.rectTransform.sizeDelta = originalSize;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
	}

    public void animate() {
        startTime = Time.time;
        originalSize = image.rectTransform.sizeDelta;
        startingSize = originalSize * startingRatio;
        image.rectTransform.sizeDelta = startingSize;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
    }
}
