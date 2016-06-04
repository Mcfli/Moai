using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TooltipSystem : MonoBehaviour {
    public GameObject tutorialParent;
    public List<TooltipDisplay> tooltips;

    public static TooltipDisplay activeTooltip;

	void Update () {
        tutorialParent.SetActive(Globals.settings["Tooltip"] == 1);
        if(Globals.mode == -1) {
            foreach(TooltipDisplay t in tooltips) t.reset();
            activeTooltip = null;
        }
	}

    public void reset() {
        foreach(TooltipDisplay t in tooltips) t.reset();
        activeTooltip = null;
        ShrineActivator.firstActivate = false;
    }
}
