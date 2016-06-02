using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
    public string pauseButton = "PauseButton";
    public string crosshairToggle = "CrosshairToggle";
    public GameObject HUDParent;
    public UnityEngine.UI.Text worldWallText;
    public Color initialColor = new Color(1, 1, 1, 0.4f);
    public Color canGrabColor = new Color(0, 0.5f, 1, 0.75f);
    public Color canUseColor = new Color(0, 0, 1, 0.75f);
    public float idleOpacity;
    public float activeOpacity;
    public GameObject crosshairParent;
    public List<UnityEngine.UI.Image> crosshairList;
    public List<UnityEngine.UI.Image> crosshairOutlines;

    private int activeCrosshair = 0;
    private bool pingNext = false;

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
        //pause button
        if(!Globals.loading && Input.GetButtonDown(pauseButton)) {
            if(Globals.mode == 1) resumeGame();
            else if(Globals.mode == 0) pauseGame();
        }

        if (!crosshairParent.activeInHierarchy) foreach (UnityEngine.UI.Image i in crosshairOutlines) i.gameObject.SetActive(false);

        //toggle HUD
        if (Input.GetButtonDown(crosshairToggle) && Globals.mode == 0 && !Globals.MenusScript.GetComponent<CheatConsole>().isActive()) Globals.settings["ShowHUD"] = (Globals.settings["ShowHUD"] == 0) ? 1 : 0;
        HUDParent.SetActive(Globals.settings["ShowHUD"] == 1 && Globals.mode == 0);
        if(!HUDParent.activeInHierarchy) return;

        //crosshair
        crosshairParent.SetActive(!Globals.PlayerScript.isInCinematic() && Globals.settings["Crosshair"] == 1);
        if (!crosshairParent.activeInHierarchy) return;

        //active crosshair - square, pentagon, triangle, octogon; blank (or anything else) is diamond
        string heldID = "";
        if(Globals.PlayerScript.getHeldObj()) {
            PuzzleObject po = Globals.PlayerScript.getHeldObj().GetComponent<PuzzleObject>();
            if(po) heldID = po.ID;
        }

        activeCrosshair = 0;
        for(int i = 0; i < crosshairList.Count; i++) {
            if(crosshairList[i].gameObject.name.Equals(heldID)) activeCrosshair = i;
            else crosshairList[i].gameObject.SetActive(false);
        }
        crosshairList[activeCrosshair].gameObject.SetActive(true);

        //crosshair color
        if(Globals.PlayerScript.LookingAtGrabbable() && Globals.PlayerScript.getHeldObj() == null) crosshairList[activeCrosshair].color = new Color(canGrabColor.r, canGrabColor.g, canGrabColor.b, activeOpacity);
        else if(Globals.PlayerScript.canUse()) crosshairList[activeCrosshair].color = new Color(canUseColor.r, canUseColor.g, canUseColor.b, activeOpacity);
        else if(activeCrosshair > 0) crosshairList[activeCrosshair].color = new Color(initialColor.r, initialColor.g, initialColor.b, activeOpacity);
        else crosshairList[activeCrosshair].color = new Color(initialColor.r, initialColor.g, initialColor.b, idleOpacity);

        if(pingNext) {
            pingCrosshair(true);
            pingNext = false;
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

    public void ping() {
        if(crosshairParent.activeInHierarchy) pingNext = true;
    }

    private void pingCrosshair(bool pingDefaultCrosshair = false) {
        if(!crosshairParent.activeInHierarchy) return;
        if(!pingDefaultCrosshair && activeCrosshair == 0) return;
        crosshairOutlines[activeCrosshair].gameObject.SetActive(true);
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
