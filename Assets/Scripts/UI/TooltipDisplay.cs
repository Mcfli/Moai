using UnityEngine;
using System.Collections;

public class TooltipDisplay : MonoBehaviour {
    public string inputName;
    public bool isButton = true; //axis if false
    public float secondsToHold; // 0 for tap
    public float disappearDelay;
    public float timeUntilDisplay;
    public float secondsToGiveUp; //0 for indefinite

    public enum Cause { GameStart, AnotherTooltipFinish, PlantingSeedInGround, CollectivePatienceAmount, HoveredGameObjectHasComponent };
    public Cause trigger;
    public TooltipDisplay anotherTooltip;
    public float patienceAmount;
    public string componentType;

    [HideInInspector] public bool finished = false;
    [HideInInspector] public bool triggered;
    private float timeWaited;
    private bool holdingSeed;

    private float timePressed;

    // Use this for initialization
    void Start () {
	    
	}

    // Update is called once per frame
    void Update() {
        if(finished) return;
        if(!triggerCheck()) return;
        //

    }

    public void finish() {
        finished = true;
    }

    private bool triggerCheck() {
        if(triggered) return true;
        switch(trigger) {
                case Cause.GameStart:
                    if(Globals.mode == 0) triggered = true;
            break;
                case Cause.AnotherTooltipFinish:
                    if(anotherTooltip.finished) triggered = true;
            break;
                case Cause.PlantingSeedInGround:
                    if(!holdingSeed && Globals.PlayerScript.holding().Contains("seed")) holdingSeed = true;
            if(holdingSeed && !Globals.PlayerScript.holding().Contains("seed")) triggered = true;
            break;
                case Cause.CollectivePatienceAmount:
                    if(Input.GetButton("Patience") && Globals.mode == 0) timeWaited += Time.deltaTime;
            if(timeWaited > patienceAmount) triggered = true;
            break;
                case Cause.HoveredGameObjectHasComponent:
                    if(Globals.PlayerScript.GetHover().collider.gameObject.GetComponent(componentType) != null) triggered = true;
            break;
        }
        return triggered;
    }

    //Look Around - no mouse movement for 15 seconds (prompt will disappear after 2 seconds)
    //Move - not move(hold w for 2 seconds) for 15 seconds
    //Run - no run(hold shift w 2 seconds) for 15 seconds

    //Patience - after planting seed in shrine
    //WaitFaster - after waiting for a collective of 30 seconds
    //Interact - when mousing over shrine
    //Pick Up - mousing over interactableobject
    //Place Waypoint - leaving shrine
}
