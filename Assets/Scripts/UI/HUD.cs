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
        if(Input.GetButtonDown(crosshairToggle) && Globals.mode == 0) Globals.settings["Crosshair"] = (Globals.settings["Crosshair"] == 0) ? 1 : 0;

        if(Globals.settings["Crosshair"] == 1 && !Globals.PlayerScript.isInCinematic()) {
            crosshair.enabled = true;
            crosshair.color = (Globals.PlayerScript.canUse()) ? canUseColor : new Color(0, 0, 0, 0);
            crosshair.color = (Globals.PlayerScript.LookingAtGrabbable()) ? canGrabColor : initialColor;
        } else crosshair.enabled = false;

        if(Input.GetButtonDown(pauseButton)) {
            if(Globals.mode == 1) Globals.mode = 0;
            else if(Globals.mode == 0) Globals.mode = 1;
        }

        if(Globals.mode == 1) {
            Time.timeScale = 0;
            if(menus.getCurrentMenu() < 0) {
                menus.switchTo(menus.initialMenu);
                firstPersonCont.getMouseLook().SetCursorLock(false);
            }
        } else if(Globals.mode == 0) {
            Time.timeScale = 1;
            if(menus.getCurrentMenu() >= 0) {
                menus.switchTo(-1);
                firstPersonCont.getMouseLook().SetCursorLock(true);
            }
        }

        //if(Input.GetKeyDown(KeyCode.P)) UnityEditor.AssetDatabase.CreateAsset(GameObject.Find("chunk (0,0)").GetComponent<ChunkMeshes>().highMesh,"Assets/00.asset");
    }

    public void startGame() {
        Globals.mode = 0;
        Globals.GenerationManagerScript.initiateWorld();
        Random.seed = Globals.SeedScript.seed;
    }

    public void pauseGame() { Globals.mode = 1; }
    public void resumeGame() { Globals.mode = 0; }
}
