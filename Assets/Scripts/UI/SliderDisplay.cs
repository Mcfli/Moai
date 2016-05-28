using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SliderDisplay: MonoBehaviour {
    //attach to button (not text)
    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Text exceptionText;
    public string settingName;
    public List<Vector2> exceptionValues;
    public List<string> exceptionStrings;
    private UnityEngine.UI.Slider slider;

	void Awake() {
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
        text.text = slider.value.ToString();
        exceptionText.text = "";
        for(int i = 0; i < exceptionValues.Count; i++)
            if(slider.value >= exceptionValues[i].x && slider.value <= exceptionValues[i].y)
                exceptionText.text = exceptionStrings[i];
    }

    public void updateSetting(float val) {
        Globals.settings[settingName] = Mathf.FloorToInt(val);
    }

    public void updateSetting() {
        Globals.settings[settingName] = Mathf.FloorToInt(slider.value);
    }
}
