using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    public string pauseButton = "PauseButton";
    public string crosshairToggle = "CrosshairToggle";
    public GameObject HUDParent;
    public UnityEngine.UI.Text worldWallText;
    public Color initialColor = new Color(1, 1, 1, 0.75f);
    public Color canGrabColor = new Color(0, 0.5f, 1, 0.75f);
    public Color canUseColor = new Color(0, 0, 1, 0.75f);
    public List<UnityEngine.UI.Image> crosshairList;

    //references
    private Menus menus;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;

    void Awake() {
        firstPersonCont = Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        menus = GetComponent<Menus>();
    }

    void Start() {
        if(Globals.mode == -1) //mainmenu
            menus.switchTo(menus.mainMenu);
        else if(Globals.mode == 1) //paused
            menus.switchTo(menus.pauseMenu);
        else if(Globals.mode == 0) //playing
            menus.switchTo(-1);
    }

    void Update() {
        HUDParent.SetActive(Globals.settings["ShowHUD"] == 1 && Globals.mode == 0);

        //crosshair color
        foreach(UnityEngine.UI.Image crosshair in crosshairList) {
            if(!Globals.PlayerScript.isInCinematic() && Globals.settings["Crosshair"] == 1) {
                crosshair.gameObject.SetActive(true);
                if(Globals.PlayerScript.LookingAtGrabbable() && Globals.PlayerScript.getHeldObj() == null) crosshair.color = canGrabColor;
                else if(Globals.PlayerScript.canUse()) crosshair.color = canUseColor;
                else crosshair.color = initialColor;
            } else crosshair.gameObject.SetActive(false);
        }

        //toggle HUD
        if(Input.GetButtonDown(crosshairToggle) && Globals.mode == 0)
            Globals.settings["ShowHUD"] = (Globals.settings["ShowHUD"] == 0) ? 1 : 0;

        //pause button
        if(Input.GetButtonDown(pauseButton)) {
            if(Globals.mode == 1) resumeGame();
            else if(Globals.mode == 0) pauseGame();
        }

        /*
        if(Input.GetKeyDown(KeyCode.P)) {
            //UnityEditor.AssetDatabase.CreateAsset(GameObject.Find("chunk (0,0)").GetComponent<ChunkMeshes>().highMesh, "Assets/00.asset");
            for(int x = -19; x <= -15; x++)
                for(int y = -8; y <= -5; y++)
                    UnityEditor.AssetDatabase.CreateAsset(GameObject.Find("chunk (" + x + "," + y + ")").GetComponent<ChunkMeshes>().lowMesh, "Assets/" + "chunk (" + x + ", " + y + ")" + ".asset");
        }
        */
    }

    public void pauseGame() {
        Globals.mode = 1;
        menus.switchTo(menus.pauseMenu);
        firstPersonCont.getMouseLook().SetCursorLock(false);
    }

    public void resumeGame() {
        Globals.mode = 0;
        menus.switchTo(-1);
        firstPersonCont.getMouseLook().SetCursorLock(true);
    }
}
