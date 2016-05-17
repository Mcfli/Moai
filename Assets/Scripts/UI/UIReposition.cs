using UnityEngine;
using System.Collections;

public class UIReposition : MonoBehaviour {
    public Vector2 position = new Vector2(0, 0.25f);
    private RectTransform trans;
    
	void Awake () {
        trans = GetComponent<RectTransform>();
    }

    void OnEnable() {
        trans.anchoredPosition = new Vector2(position.x * Screen.width, position.y * Screen.height) / 2;
    }
	
	// Will not call when disabled
	void Update () {
        trans.anchoredPosition = new Vector2(position.x * Screen.width, position.y * Screen.height) / 2;
    }
}
