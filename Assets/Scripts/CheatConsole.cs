using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CheatConsole : MonoBehaviour {

    private InputField console;

    private GameObject airStar;
    private GameObject earthStar ;
    private GameObject fireStar  ;
    private GameObject waterStar ;

    private bool active = true;
    void Start()
    {
        airStar = Resources.Load("Prefabs/Stars/star_air") as GameObject;
        earthStar = Resources.Load("Prefabs/Stars/star_earth") as GameObject;
        fireStar = Resources.Load("Prefabs/Stars/star_fire") as GameObject;
        waterStar = Resources.Load("Prefabs/Stars/star_water") as GameObject;
        console = GameObject.Find("Console").GetComponent<InputField>();
        toggleConsole();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            toggleConsole();
        }
        console.gameObject.SetActive(active);
    }

    void toggleConsole()
    {
        active = !active;

        if (!active)
        {
            console.DeactivateInputField();
        }
        console.gameObject.SetActive(active);
        if (active)
        {
            console.ActivateInputField();
        }
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
        else
        {
            Debug.Log("Unrecognized Command");
        }

        toggleConsole();

    }

    // Cheats

    void addStars()
    {
        GameObject aStar = Instantiate(airStar, transform.position, Quaternion.identity) as GameObject;
        GameObject eStar = Instantiate(earthStar, transform.position, Quaternion.identity) as GameObject;
        GameObject fStar = Instantiate(fireStar, transform.position, Quaternion.identity) as GameObject;
        GameObject wStar = Instantiate(waterStar, transform.position, Quaternion.identity) as GameObject;

        Vector3 target = Globals.Player.transform.position + Vector3.up * 10000;

        aStar.GetComponent<StarEffect>().setTarget(target);
        eStar.GetComponent<StarEffect>().setTarget(target);
        fStar.GetComponent<StarEffect>().setTarget(target);
        wStar.GetComponent<StarEffect>().setTarget(target);
        Globals.airStars.Add(aStar);
        Globals.airStars.Add(eStar);
        Globals.airStars.Add(fStar);
        Globals.airStars.Add(wStar);
    }
}
