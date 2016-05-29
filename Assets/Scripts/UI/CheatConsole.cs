using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CheatConsole : MonoBehaviour{
    public InputField console;
    public GameObject puzzleFinisher;
    private bool active = true;
    private bool speed = false;

    void Start(){
        deactivateConsole();
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.BackQuote)) toggleConsole();
        console.gameObject.SetActive(active);
    }

    void toggleConsole() {
        active = !active;

        if(!active) {
            console.DeactivateInputField();
        }
        console.gameObject.SetActive(active);
        if(active) {
            console.ActivateInputField();
        }
        console.text = "";
    }

    void deactivateConsole() {
        active = false;
        console.DeactivateInputField();
        console.gameObject.SetActive(false);
        console.text = "";
    }

    void parseCommand()
    {
        string text = console.text;

        if (text.Equals("stars"))
        {
            Debug.Log("Adding one of each elemental star");
            addStars();
        }
        else if (text.Equals("finish"))
        {
            Debug.Log("Creating puzzle finisher");
            finishShrine();
        }
        else if(text.Equals("fly"))
        {
            Debug.Log("Fly toggled");
            Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().flyCheat = !Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().flyCheat;
        }
        else if (text.Equals("flash"))
        {
            Debug.Log("Superspeed toggled");
            if (!speed)
            {
                speed = true;
                Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().setRunSpeed(100);
            }
            else
            {
                speed = false;
                Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().setRunSpeed(30);
            }
            
        }
        else
        {
            Debug.Log("Unrecognized Command");
        }

        toggleConsole();

    }

    // Cheats

    void addStars(){
        foreach(KeyValuePair<string, List<GameObject>> p in Globals.Stars)
            Globals.SkyScript.addStar(p.Key);
    }

    // Create a shrine finisher at player
    void finishShrine(){
        Instantiate(puzzleFinisher, Globals.Player.transform.position, Quaternion.identity);
    }
}