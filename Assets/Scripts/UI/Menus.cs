using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Menus : MonoBehaviour {
    public int mainMenu = 0;                    // -1 for no menu
    public int pauseMenu = 0;                   // -1 for no menu
    public List<GameObject> menuScreens;
    public List<RectTransform> menuBackdrops;   // menus can use the same backgrounds or have no background

    private int currentMenu = -2;

    void Update() {
        if(currentMenu >= 0) adjustScreen(currentMenu);
    }

    public void switchTo(int menuNum) {
        if(menuNum == currentMenu) return;
        else currentMenu = menuNum;

        foreach(GameObject g in menuScreens) g.SetActive(false);
        foreach(RectTransform r in menuBackdrops) if(r) r.gameObject.SetActive(false);

        if(currentMenu < 0) return;
        
        menuScreens[currentMenu].SetActive(true);
        if(menuBackdrops[currentMenu]) menuBackdrops[currentMenu].gameObject.SetActive(true);
        adjustScreen(currentMenu);
    }

    public void switchTo(GameObject menu) {
        int m = -1;
        for(int i = 0; i < menuScreens.Count; i++) if(menu == menuScreens[i]) m = i;
        switchTo(m);
    }

    public void switchToInitial() {
        if(Globals.mode == -1) switchTo(mainMenu);
        else switchTo(pauseMenu);
    }
    
    private void adjustScreen(int menuNum) {
        if(menuBackdrops[menuNum]) {
            float aspectRatio = menuBackdrops[menuNum].sizeDelta.x / menuBackdrops[menuNum].sizeDelta.y;
            if((float)Screen.width / (float)Screen.height > aspectRatio) menuBackdrops[menuNum].sizeDelta = new Vector2(Screen.width, Screen.width / aspectRatio);
            else menuBackdrops[menuNum].sizeDelta = new Vector2(Screen.height * aspectRatio, Screen.height);
        }
    }

    public int getCurrentMenu() {
        return currentMenu;
    }

    public void returnToTitle() {
        Globals.GenerationManagerScript.deleteWorld();
        Random.seed = (int)System.DateTime.Now.Ticks;
        Globals.MenusScript.GetComponent<MainMenu>().setupMain();
        Globals.mode = -1;
        switchToInitial();
    }

    public void quit() {
        Application.Quit();
    }

    //Player Settings
    public static void changeSetting(string setting, int value) {
        Globals.settings[setting] = value;
    }
}