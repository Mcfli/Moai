using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour {
    public Color initialColor = new Color(1, 1, 1, 0.75f);
    public Color canGrabColor = new Color(0, 0.5f, 1, 0.75f);

    void Update () {
        if (Globals.Player.GetComponent<Player>().LookingAtGrabbable()) GetComponent<UnityEngine.UI.Image>().color = canGrabColor;
        else GetComponent<UnityEngine.UI.Image>().color = initialColor;
    }
}
