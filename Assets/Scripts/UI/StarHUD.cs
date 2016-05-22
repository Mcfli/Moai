using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarHUD : MonoBehaviour {
    public GameObject StarHUDParent;
    public GameObject fireStarHUD;
    public GameObject waterStarHUD;
    public GameObject airStarHUD;
    public GameObject earthStarHUD;
    public float distanceBetweenIcons;
    public Vector2 firstIconOffset;

    private Dictionary<string, List<GameObject>> StarIcons = new Dictionary<string, List<GameObject>>() {
        { "fire",  new List<GameObject>() },
        { "water", new List<GameObject>() },
        { "air",   new List<GameObject>() },
        { "earth", new List<GameObject>() },
    };

    public void Update() {
        StarHUDParent.SetActive((Globals.settings["StarIcons"] == 1) && Globals.SkyScript.getNumberOfStars() > 0);
    }

    public void addStar(string element) {
        GameObject icon;
        if(element.Equals("fire")) {
            icon = Instantiate(fireStarHUD);
            icon.transform.SetParent(StarHUDParent.transform);
            RectTransform t = icon.transform as RectTransform;
            t.anchoredPosition = new Vector2(-firstIconOffset.x - StarIcons[element].Count * distanceBetweenIcons, firstIconOffset.y);
        } else if(element.Equals("water")) {
            icon = Instantiate(waterStarHUD);
            icon.transform.SetParent(StarHUDParent.transform);
            RectTransform t = icon.transform as RectTransform;
            t.anchoredPosition = new Vector2(-firstIconOffset.x - StarIcons[element].Count * distanceBetweenIcons, -firstIconOffset.y);
        } else if(element.Equals("air")) {
            icon = Instantiate(airStarHUD);
            icon.transform.SetParent(StarHUDParent.transform);
            RectTransform t = icon.transform as RectTransform;
            t.anchoredPosition = new Vector2(firstIconOffset.x + StarIcons[element].Count * distanceBetweenIcons, firstIconOffset.y);
        } else if(element.Equals("earth")) {
            icon = Instantiate(earthStarHUD);
            icon.transform.SetParent(StarHUDParent.transform);
            RectTransform t = icon.transform as RectTransform;
            t.anchoredPosition = new Vector2(firstIconOffset.x + StarIcons[element].Count * distanceBetweenIcons, -firstIconOffset.y);
        } else return;
        StarIcons[element].Add(icon);
    }

    public void removeStar(string element) {
        if(!StarIcons.ContainsKey(element)) return;
        if(StarIcons[element].Count < 1) return;
        Destroy(StarIcons[element][StarIcons[element].Count - 1]);
        StarIcons[element].RemoveAt(StarIcons[element].Count - 1);
    }

    public void clearStars() {
        foreach(KeyValuePair<string, List<GameObject>> p in StarIcons) foreach(GameObject g in p.Value) Destroy(g);
        StarIcons = new Dictionary<string, List<GameObject>>() {
            { "fire",  new List<GameObject>() },
            { "water", new List<GameObject>() },
            { "air",   new List<GameObject>() },
            { "earth", new List<GameObject>() },
        };
    }
}
