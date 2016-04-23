using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    public string pauseButton = "PauseButton";
    public string crosshairToggle = "CrosshairToggle";
    public Color initialColor = new Color(1, 1, 1, 0.75f);
    public Color canGrabColor = new Color(0, 0.5f, 1, 0.75f);
    public Color canUseColor = new Color(0, 0, 1, 0.75f);
    public UnityEngine.UI.Image crosshair;

    //references
    private Menus menus;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;

    void Awake() {
        firstPersonCont = Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        menus = GetComponent<Menus>();
    }

    void Update() {
        if(Input.GetButtonDown(crosshairToggle) && !Globals.paused) menus.changeCrosshair();

        if(Menus.showCrosshair && !Globals.PlayerScript.isInCinematic()) {
            crosshair.enabled = true;
            crosshair.color = (Globals.PlayerScript.canUse()) ? canUseColor : new Color(0, 0, 0, 0);
            crosshair.color = (Globals.PlayerScript.LookingAtGrabbable()) ? canGrabColor : initialColor;
        } else crosshair.enabled = false;

        if(Input.GetButtonDown(pauseButton)) {
            if(Globals.paused) Globals.paused = false;
            else Globals.paused = true;
        }

        if(Globals.paused) {
            Time.timeScale = 0;
            if(menus.getCurrentMenu() < 0) {
                menus.switchTo(menus.initialMenu);
                firstPersonCont.getMouseLook().SetCursorLock(false);
            }
        } else {
            Time.timeScale = 1;
            if(menus.getCurrentMenu() >= 0) {
                menus.switchTo(-1);
                firstPersonCont.getMouseLook().SetCursorLock(true);
            }
        }

        //if(Input.GetKeyDown(KeyCode.P)) UnityEditor.AssetDatabase.CreateAsset(GameObject.Find("chunk (0,0)").GetComponent<ChunkMeshes>().highMesh,"Assets/00.asset");
    }

    public void pauseGame() { Globals.paused = true; }
    public void resumeGame() { Globals.paused = false; }
}
