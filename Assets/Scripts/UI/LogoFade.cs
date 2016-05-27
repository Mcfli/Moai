using UnityEngine;
using System.Collections;

public class LogoFade : MonoBehaviour {
    public float animationDuration = 10.0f; // seconds it takes to fade out
    public float secondsOfFade = 1f;
    private UnityEngine.UI.Image image;
    private float enableTime;

    void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
    }

    void OnEnable() {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        enableTime = Time.time;
    }

    void Update() {
        if(Time.time - enableTime> animationDuration) gameObject.SetActive(false);

        if(Time.time - enableTime < secondsOfFade) image.color = new Color(image.color.r, image.color.g, image.color.b, (Time.time - enableTime) / secondsOfFade);
        else if(animationDuration - (Time.time - enableTime) < secondsOfFade) image.color = new Color(image.color.r, image.color.g, image.color.b, animationDuration - (Time.time - enableTime) / secondsOfFade);
        else image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
    }
}