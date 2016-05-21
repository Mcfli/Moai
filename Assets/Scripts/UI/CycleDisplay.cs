using UnityEngine;
using System.Collections;

public class CycleDisplay: MonoBehaviour {
    //attach to button (not text)
    public UnityEngine.UI.Text text;
    public string settingName;
    private string head;
    private UnityEngine.UI.Button button;

	void Awake() {
        head = text.text;
        button = GetComponent<UnityEngine.UI.Button>();
    }

    void Start() {
        //button.isOn = (Globals.settings[settingName] == 1);
        updateText();
    }
	
    void Update() {
        if(Screen.resolutions.Length < 1) button.interactable = false;
        updateText();
    }

    private void updateText() {
        if(!button.interactable) text.text = head;
        text.text = head + ": " + Screen.currentResolution.width + "x" + Screen.currentResolution.height;
    }

    public void updateSetting(bool val) {
        Globals.settings[settingName] = (val) ? 1 : 0;
    }

    public void updateSetting() {
        //Globals.settings[settingName] = (toggle.isOn) ? 1 : 0;
    }
}
