using UnityEngine;
using System.Collections;

public class ToggleDisplay: MonoBehaviour {
    //attach to button (not text)
    public UnityEngine.UI.Text text;
    public string settingName;
    public string onText = "ON";
    public string offText = "OFF";
    private string head;
    private UnityEngine.UI.Toggle toggle;

	void Awake() {
        head = text.text;
        toggle = GetComponent<UnityEngine.UI.Toggle>();
    }

    void Start() {
        toggle.isOn = (Globals.settings[settingName] == 1);
        updateText();
    }
	
    void Update() {
        updateText();
    }

    private void updateText() {
        if(!toggle.interactable) text.text = head;
        if(head.Length < 1) {
            if(toggle.isOn) text.text = onText;
            else text.text = offText;
        } else {
            if(toggle.isOn) text.text = head + ": " + onText;
            else text.text = head + ": " + offText;
        }
    }

    public void updateSetting(bool val) {
        Globals.settings[settingName] = (val) ? 1 : 0;
    }

    public void updateSetting() {
        Globals.settings[settingName] = (toggle.isOn) ? 1 : 0;
    }
}
