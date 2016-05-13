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

    void Start() {
        if(Globals.mode == -1) //mainmenu
            menus.switchTo(menus.mainMenu);
        else if(Globals.mode == 1) //paused
            menus.switchTo(menus.pauseMenu);
        else if(Globals.mode == 0) //playing
            menus.switchTo(-1);
    }

    void Update() {
        //crosshair color
        if(Globals.settings["Crosshair"] == 1 && !Globals.PlayerScript.isInCinematic() && Globals.mode == 0) {
            crosshair.gameObject.SetActive(true);
            if(Globals.PlayerScript.LookingAtGrabbable() && Globals.PlayerScript.getHeldObj() == null) crosshair.color = canGrabColor;
            else if(Globals.PlayerScript.canUse()) crosshair.color = canUseColor;
            else crosshair.color = initialColor;
        } else crosshair.gameObject.SetActive(false);

        //toggle crosshair
        if(Input.GetButtonDown(crosshairToggle) && Globals.mode == 0)
            Globals.settings["Crosshair"] = (Globals.settings["Crosshair"] == 0) ? 1 : 0;

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
