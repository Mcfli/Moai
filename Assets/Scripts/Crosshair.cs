using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour {
    public Color initialColor = new Color(1, 1, 1, 0.75f);
    public Color canGrabColor = new Color(0, 0.5f, 1, 0.75f);
    public Color canUseColor = new Color(0, 0, 1, 0.75f);
    public string crosshairToggleInput = "CrosshairToggle";
    public UnityEngine.UI.Image LeftTip;
    public UnityEngine.UI.Image RightTip;

    private UnityEngine.UI.Image crosshair;
    private bool visible;

    void Start() {
        visible = true;
        crosshair = GetComponent<UnityEngine.UI.Image>();
    }

    void Update () {
        if (Input.GetButtonDown(crosshairToggleInput)) visible = !visible;

        if (visible) {
            crosshair.color = (Globals.PlayerScript.LookingAtGrabbable()) ? canGrabColor : initialColor;
            LeftTip.color = (Globals.PlayerScript.canUse()[0]) ? canUseColor : new Color(0, 0, 0, 0);
            RightTip.color = (Globals.PlayerScript.canUse()[1]) ? canUseColor : new Color(0, 0, 0, 0);
        } else crosshair.color = new Color(0,0,0,0);
    }
}
