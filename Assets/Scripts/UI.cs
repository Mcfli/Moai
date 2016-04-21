using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI : MonoBehaviour {
    public string crosshairToggle = "CrosshairToggle";
    public Color initialColor = new Color(1, 1, 1, 0.75f);
    public Color canGrabColor = new Color(0, 0.5f, 1, 0.75f);
    public Color canUseColor = new Color(0, 0, 1, 0.75f);
    public UnityEngine.UI.Image crosshair;

    //pause menu
    public string pauseButton = "PauseButton";
    public RectTransform backdrop;
    public List<GameObject> menuScreens; //first one should be the initial pause screen
    private bool pauseScreen;
    
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;

    void Awake() {
        foreach(GameObject g in menuScreens) g.SetActive(false);
        backdrop.gameObject.SetActive(false);
        firstPersonCont = Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        pauseScreen = false;
    }

    void Update() {
        if(Input.GetButtonDown(crosshairToggle) && !Globals.paused) {
            Globals.showCrosshair = !Globals.showCrosshair;
        }

        if(Globals.showCrosshair && !Globals.PlayerScript.isInCinematic() && !Globals.paused) {
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
            backdrop.sizeDelta = new Vector2(Screen.width, Screen.height);
            showPauseScreen();
        } else {
            Time.timeScale = 1;
            hidePauseScreen();
        }

        //if(Input.GetKeyDown(KeyCode.P)) UnityEditor.AssetDatabase.CreateAsset(GameObject.Find("chunk (0,0)").GetComponent<ChunkMeshes>().highMesh,"Assets/00.asset");
    }

    private void showPauseScreen() {
        if(pauseScreen) return;
        pauseScreen = true;
        menuScreens[0].SetActive(true);
        backdrop.gameObject.SetActive(true);
        firstPersonCont.getMouseLook().SetCursorLock(false);
        backdrop.sizeDelta = new Vector2(Screen.width, Screen.height);
    }

    private void hidePauseScreen() {
        if(!pauseScreen) return;
        pauseScreen = false;
        foreach(GameObject g in menuScreens) g.SetActive(false);
        backdrop.gameObject.SetActive(false);
        firstPersonCont.getMouseLook().SetCursorLock(true);
    }

    public void switchTo(GameObject screen) {
        foreach(GameObject g in menuScreens) g.SetActive(false);
        screen.SetActive(true);
    }

    public void pauseGame() { Globals.paused = true; }
    public void resumeGame() { Globals.paused = false; }
}
