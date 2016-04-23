using UnityEngine;
using System.Collections;

public class ButtonToggleText : MonoBehaviour {
    //attach to button (not text)
    public UnityEngine.UI.Text text;
    public string onText = "ON";
    public string offText = "OFF";
    private string head;
    private UnityEngine.UI.Toggle toggle;

	void Awake() {
        head = text.text;
        toggle = GetComponent<UnityEngine.UI.Toggle>();
    }

    void Start() {
        toggle.isOn = Menus.showCrosshair;
        updateText();
    }
	
    void Update() {
        updateText();
    }

    private void updateText() {
        if(toggle.isOn) text.text = head + ": " + onText;
        else text.text = head + ": " + offText;
    }
}
