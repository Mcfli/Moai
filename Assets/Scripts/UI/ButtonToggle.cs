using UnityEngine;
using System.Collections;

public class ButtonToggle: MonoBehaviour {
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
        Globals.settings["Crosshair"] = (toggle.isOn) ? 1 : 0;
        updateText();
    }
	
    void Update() {
        updateText();
    }

    private void updateText() {
        if(toggle.isOn) text.text = head + ": " + onText;
        else text.text = head + ": " + offText;
    }

    public void updateSetting(bool val) {
        Globals.settings[settingName] = (val) ? 1 : 0;
    }
}
