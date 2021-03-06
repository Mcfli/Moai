﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TooltipDisplay : MonoBehaviour {
    public GameObject tooltip;
    public float timeUntilDisplay;
    public AudioClip activateSound;
    public float secondsToGiveUp;   //0 for indefinite

    public enum Cause { GameStart, AnotherTooltipFinish, PlantingSeedInGround, CollectivePatienceAmount, HoveredGameObjectHasComponent, Never };
    public Cause trigger;
    public TooltipDisplay anotherTooltip;
    public float patienceAmount;
    public string componentType;
    public float secondsOfHover;

    public enum Finish { ButtonPress, PickUpObject, ShrineActivate, WaypointActive, Never };
    public Finish finisher;
    public List<string> inputNames;
    public bool AND = true;         // OR if false
    public bool isButton = true;    // axis if false
    public float secondsToHold;     // 0 for tap

    [HideInInspector] public bool finished;
    [HideInInspector] public bool triggered;
    private float timeHovered;
    private float timeTriggered; // seconds from time triggered
    private float timeWaited;
    private float timePressed;
    private bool holdingSeed;
    private AudioSource audioSource;

    void Awake() {
        audioSource = Globals.MenusScript.GetComponent<AudioSource>();
    }

    void Start() {
        reset();
    }

    void Update() {
        if(finishCheck()) finish();
        else if(triggerCheck()) {
            if(timeTriggered < 0) timeTriggered = 0;
            timeTriggered += Time.deltaTime;
            if(timeTriggered >= timeUntilDisplay) {
                if(TooltipSystem.activeTooltip && TooltipSystem.activeTooltip != this) TooltipSystem.activeTooltip.finish();
                else if(!tooltip.activeSelf) activate();
            }else if(secondsToGiveUp > 0 && timeTriggered >= timeUntilDisplay + secondsToGiveUp) finish();
        }
    }

    public void reset() {
        gameObject.SetActive(true);
        tooltip.SetActive(false);
        finished = false;
        triggered = false;
        timeHovered = -1;
        timeTriggered = -1;
        timeWaited = 0;
        timePressed = -1;
        holdingSeed = false;
    }

    public void activate() {
        tooltip.SetActive(true);
        TooltipSystem.activeTooltip = this;
        audioSource.PlayOneShot(activateSound);
    }

    public void finish() {
        finished = true;
        tooltip.SetActive(false);
        gameObject.SetActive(false);
        TooltipSystem.activeTooltip = null;
    }

    private bool finishCheck() {
        if(finished) return true;
        if(Globals.mode != 0 || Globals.loading) return false;
        switch(finisher) {
            case Finish.ButtonPress:
                bool buttonPressed = AND;
                foreach(string s in inputNames) {
                    bool input;
                    if(isButton) input = Input.GetButton(s);
                    else input = Input.GetAxis(s) != 0;

                    if(input != buttonPressed) {
                        buttonPressed = input;
                        break;
                    }
                }

                if(buttonPressed) {
                    if(timePressed < 0) timePressed = 0;
                    timePressed += Time.deltaTime;
                } else timePressed = -1;
                if(timePressed >= secondsToHold) return true;
                return false;
            case Finish.PickUpObject:
                return Globals.PlayerScript.getHeldObj();
            case Finish.ShrineActivate:
                return ShrineActivator.firstActivate;
            case Finish.WaypointActive:
                return Globals.PlayerScript.waypoint.activeInHierarchy;
        }
        return false;
    }

    private bool triggerCheck() {
        if(triggered) return true;
        if(Globals.mode != 0 || Globals.loading) return false;
        switch(trigger) {
            case Cause.GameStart:
                if(Globals.mode == 0 && !Globals.loading) triggered = true;
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
                if(Globals.PlayerScript.GetHover().collider) {
                    if(Globals.PlayerScript.GetHover().collider.gameObject.GetComponent(componentType) != null) {
                        if(timeHovered < 0) timeHovered = 0;
                        timeHovered += Time.deltaTime;
                    } else timeHovered = -1;
                } else timeHovered = -1;
                if(timeHovered >= secondsOfHover) triggered = true;
                break;
        }
        return triggered;
    }
}

//Look Around - no mouse movement for 15 seconds (prompt will disappear after 2 seconds)
//Move - not move(hold w for 2 seconds) for 15 seconds
//Run - no run(hold shift w 2 seconds) for 15 seconds

//Patience - after planting seed in shrine
//WaitFaster - after waiting for a collective of 30 seconds
//Interact - when mousing over shrine
//Pick Up - mousing over interactableobject
//Place Waypoint - leaving shrine