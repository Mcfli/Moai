using UnityEngine;
using System.Collections;

public class TitleAnimate : MonoBehaviour {
    public float animationDuration; // seconds it takes to fade out
    public Vector2 startingOffset;
    private UnityEngine.UI.Image image;
    private float enableTime = -1;

    void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
    }

    void Update() {
        if(enableTime < 0) return;
        if(Time.time - enableTime < animationDuration) {
            image.color = new Color(image.color.r, image.color.g, image.color.b, (Time.time - enableTime) / animationDuration);
        } else {

        }
    }

    public void animate() {
        enableTime = Time.time;
    }
}