using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Menus : MonoBehaviour {
    public int mainMenu = 0;                    // -1 for no menu
    public int pauseMenu = 0;                   // -1 for no menu
    public List<GameObject> menuScreens;        // first one should be the initial pause screen
    public List<Vector2> menuPositions;         // -1 to 1. 0 is middle of screen
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
        menuScreens[menuNum].GetComponent<RectTransform>().anchoredPosition = new Vector2(menuPositions[menuNum].x * Screen.width, menuPositions[menuNum].y * Screen.height) / 2;
        if(menuBackdrops[menuNum]) menuBackdrops[menuNum].sizeDelta = new Vector2(Screen.width, Screen.height);
    }

    public int getCurrentMenu() {
        return currentMenu;
    }

    public void loadScene(string sceneName) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void quit() {
        Application.Quit();
    }

    //Player Settings
    public static void changeSetting(string setting, int value) {
        Globals.settings[setting] = value;
    }
}