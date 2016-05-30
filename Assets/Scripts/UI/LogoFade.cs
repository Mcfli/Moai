using UnityEngine;
using System.Collections;

public class LogoFade : MonoBehaviour {
    public float animationDuration = 10.0f; // seconds it takes to fade out
    public float secondsOfFade = 1f;
    public float endingSizeRatio;
    private UnityEngine.UI.Image image;
    private float enableTime;
    private Vector2 originalSize;

    void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
        originalSize = image.rectTransform.sizeDelta;
    }

    void OnEnable() {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.rectTransform.sizeDelta = originalSize;
        enableTime = Time.time;
    }

    void Update() {
        if(Time.time - enableTime > animationDuration) {
            gameObject.SetActive(false);
            return;
        }

        image.rectTransform.sizeDelta = Vector2.Lerp(originalSize, originalSize * endingSizeRatio, (Time.time - enableTime)/animationDuration);

        if(Time.time - enableTime < secondsOfFade) image.color = new Color(image.color.r, image.color.g, image.color.b, (Time.time - enableTime) / secondsOfFade);
        else if(animationDuration - (Time.time - enableTime) < secondsOfFade) image.color = new Color(image.color.r, image.color.g, image.color.b, animationDuration - (Time.time - enableTime) / secondsOfFade);
        else image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }
}