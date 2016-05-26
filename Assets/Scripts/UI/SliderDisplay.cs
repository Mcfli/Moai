using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SliderDisplay: MonoBehaviour {
    //attach to button (not text)
    public UnityEngine.UI.Text text;
    public string settingName;
    public List<float> exceptionValues;
    public List<string> exceptionStrings;
    private string head;
    private UnityEngine.UI.Slider slider;

	void Awake() {
        head = text.text;
        slider = GetComponent<UnityEngine.UI.Slider>();
    }

    void Start() {
        slider.value = (Globals.settings[settingName]);
        updateText();
    }
	
    void Update() {
        updateText();
    }

    private void updateText() {
        if(!slider.interactable) text.text = head;
        else {
            for(int i = 0; i < exceptionValues.Count; i++) {
                if(slider.value == exceptionValues[i]) {
                    if(head.Length < 1) text.text = exceptionStrings[i];
                    else text.text = head + ": " + exceptionStrings[i];
                    return;
                }
            }
            if(head.Length < 1) text.text = "" + slider.value;
            else text.text = head + ": " + slider.value;
        }
    }

    public void updateSetting(float val) {
        Globals.settings[settingName] = Mathf.FloorToInt(val);
    }

    public void updateSetting() {
        Globals.settings[settingName] = Mathf.FloorToInt(slider.value);
    }
}
