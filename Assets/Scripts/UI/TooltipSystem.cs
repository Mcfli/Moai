using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TooltipSystem : MonoBehaviour {
    public GameObject tutorialParent;
    public List<TooltipDisplay> tooltips;
    public int firstTooltip;

    private List<TooltipDisplay> activeTooltip;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
