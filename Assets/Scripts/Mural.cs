﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Mural : MonoBehaviour {
    public Material fireIconDull;
    public Material waterIconDull;
    public Material earthIconDull;
    public Material airIconDull;

    public Material fireIconLit;
    public Material waterIconLit;
    public Material earthIconLit;
    public Material airIconLit;

    //Texture2D muralTex;
    private Renderer rend;

    /* Shrine mural material mapping
     * 
     * Material Name ---- Index
     * 
     * shrine ---- 9
     * 
     * FireSide ---- 37
     * EarthSide ---- 38
     * WaterSide ----- 39
     * AirSide ---- 40
     * 
     * Earth00 ---- 1
     * Earth01 ---- 0
     * Earth02 ---- 2
     * Earth10 ---- 4
     * Earth12 ---- 3
     * Earth20 ---- 7
     * Earth21 ---- 6
     * Earth22 ---- 8
     * 
     * Air00 ---- 16
     * Air01 ---- 18
     * Air02 ---- 17
     * Air10 ---- 15
     * Air12 ---- 14
     * Air20 ---- 10
     * Air21 ---- 12
     * Air22 ---- 11
     * 
     * Water00 ---- 21
     * Water01 ---- 19
     * Water02 ---- 20
     * Water10 ---- 22
     * Water12 ---- 23
     * Water20 ---- 27
     * Water21 ---- 25
     * Water22 ---- 26
     * 
     * Fire00 ---- 34
     * Fire01 ---- 35
     * Fire02 ---- 33
     * Fire10 ---- 32
     * Fire12 ---- 36
     * Fire20 ---- 29
     * Fire21 ---- 30
     * Fire22 ---- 28
     * 
     */

	// Use this for initialization
	void Start () {
        rend = GetComponent<Renderer>();
        //snapToTerrain();
	}

    public void genMurals(List<PuzzleObject> targetStateFire, List<PuzzleObject> targetStateWater, List<PuzzleObject> targetStateEarth, List<PuzzleObject> targetStateAir)
    {
        if(rend == null) rend = GetComponent<Renderer>();
        Material[] tempMats = rend.sharedMaterials;
        // Generate Fire mural
        int index = 0;
        foreach(PuzzleObject po in targetStateFire)
        {
            if (index == 0)      tempMats[34] = po.image;
            else if (index == 1) tempMats[35] = po.image;
            else if (index == 2) tempMats[33] = po.image;
            else if (index == 3) tempMats[32] = po.image;
            else if (index == 4) tempMats[36] = po.image;
            else if (index == 5) tempMats[29] = po.image;
            else if (index == 6) tempMats[30] = po.image;
            else                 tempMats[28] = po.image;
            index++;
        }

        // Generate Water mural
        index = 0;
        foreach (PuzzleObject po in targetStateWater)
        {
            if (index == 0)      tempMats[21] = po.image;
            else if (index == 1) tempMats[19] = po.image;
            else if (index == 2) tempMats[20] = po.image;
            else if (index == 3) tempMats[22] = po.image;
            else if (index == 4) tempMats[23] = po.image;
            else if (index == 5) tempMats[27] = po.image;
            else if (index == 6) tempMats[25] = po.image;
            else                 tempMats[26] = po.image;
            index++;
        }

        // Generate Air mural
        index = 0;
        foreach (PuzzleObject po in targetStateAir)
        {
            if (index == 0)      tempMats[16] = po.image;
            else if (index == 1) tempMats[18] = po.image;
            else if (index == 2) tempMats[17] = po.image;
            else if (index == 3) tempMats[15] = po.image;
            else if (index == 4) tempMats[14] = po.image;
            else if (index == 5) tempMats[10] = po.image;
            else if (index == 6) tempMats[12] = po.image;
            else                 tempMats[11] = po.image;
            index++;
        }

        // Generate Earth mural
        index = 0;
        foreach (PuzzleObject po in targetStateEarth)
        {
            if (index == 0)      tempMats[1] = po.image;
            else if (index == 1) tempMats[0] = po.image;
            else if (index == 2) tempMats[2] = po.image;
            else if (index == 3) tempMats[4] = po.image;
            else if (index == 4) tempMats[3] = po.image;
            else if (index == 5) tempMats[7] = po.image;
            else if (index == 6) tempMats[6] = po.image;
            else                 tempMats[8] = po.image;
            index++;
        }

        // Assign materials
        rend.sharedMaterials = tempMats;
    }

    public void glowPuzzleObjects(Dictionary<int,bool> completed,List<PuzzleObject> targetState, string element)
    {
        List<GameObject> itemsCounted = new List<GameObject>();
        int index = 0;
        if (rend == null) rend = GetComponent<Renderer>();
        Material[] tempMats = rend.sharedMaterials;

        // Update fire side
        if (element.Equals("fire"))
        {
            foreach (PuzzleObject po in targetState)
            {
                bool objectDone = (completed.ContainsKey(index) && completed[index]);
                if (index == 0) tempMats[34]      = objectDone ? po.imageGlowing:po.image;
                else if (index == 1) tempMats[35] = objectDone ? po.imageGlowing : po.image;
                else if (index == 2) tempMats[33] = objectDone ? po.imageGlowing : po.image;
                else if (index == 3) tempMats[32] = objectDone ? po.imageGlowing:po.image;
                else if (index == 4) tempMats[36] = objectDone ? po.imageGlowing:po.image;
                else if (index == 5) tempMats[29] = objectDone ? po.imageGlowing:po.image;
                else if (index == 6) tempMats[30] = objectDone ? po.imageGlowing:po.image;
                else tempMats[28]                 = objectDone ? po.imageGlowing : po.image;
                index++;
            }
        }


        // Update water side
        else if (element.Equals("water"))
        {
            foreach (PuzzleObject po in targetState)
            {
                bool objectDone = (completed.ContainsKey(index) && completed[index]);
                if (index == 0) tempMats[21]      = objectDone ? po.imageGlowing : po.image;
                else if (index == 1) tempMats[19] = objectDone ? po.imageGlowing : po.image;
                else if (index == 2) tempMats[20] = objectDone ? po.imageGlowing : po.image;
                else if (index == 3) tempMats[22] = objectDone ? po.imageGlowing : po.image;
                else if (index == 4) tempMats[23] = objectDone ? po.imageGlowing : po.image;
                else if (index == 5) tempMats[27] = objectDone ? po.imageGlowing : po.image;
                else if (index == 6) tempMats[25] = objectDone ? po.imageGlowing : po.image;
                else tempMats[26]                 = objectDone ? po.imageGlowing : po.image;
                index++;
            }
        }

        // Update air side
        if (element.Equals("air"))
        {
            bool objectDone = (completed.ContainsKey(index) && completed[index]);
            foreach (PuzzleObject po in targetState)
            {
                if (index == 0) tempMats[16]      = objectDone ? po.imageGlowing : po.image;
                else if (index == 1) tempMats[18] = objectDone ? po.imageGlowing : po.image;
                else if (index == 2) tempMats[17] = objectDone ? po.imageGlowing : po.image;
                else if (index == 3) tempMats[15] = objectDone ? po.imageGlowing : po.image;
                else if (index == 4) tempMats[14] = objectDone ? po.imageGlowing : po.image;
                else if (index == 5) tempMats[10] = objectDone ? po.imageGlowing : po.image;
                else if (index == 6) tempMats[12] = objectDone ? po.imageGlowing : po.image;
                else tempMats[11]                 = objectDone ? po.imageGlowing : po.image;
                index++;
            }
        }

        // Update earth side
        if (element.Equals("earth"))
        {
            bool objectDone = (completed.ContainsKey(index) && completed[index]);
            foreach (PuzzleObject po in targetState)
            {
                if (index == 0) tempMats[1]      = objectDone ? po.imageGlowing : po.image;
                else if (index == 1) tempMats[0] = objectDone ? po.imageGlowing : po.image;
                else if (index == 2) tempMats[2] = objectDone ? po.imageGlowing : po.image;
                else if (index == 3) tempMats[4] = objectDone ? po.imageGlowing : po.image;
                else if (index == 4) tempMats[3] = objectDone ? po.imageGlowing : po.image;
                else if (index == 5) tempMats[7] = objectDone ? po.imageGlowing : po.image;
                else if (index == 6) tempMats[6] = objectDone ? po.imageGlowing : po.image;
                else tempMats[8]                 = objectDone ? po.imageGlowing : po.image;
                index++;
            }
        }

        // Assign materials
        rend.sharedMaterials = tempMats;

    }

    public void lightIcon(string element)
    {
        Material[] tempMats = rend.sharedMaterials;
        tempMats[37] = fireIconDull;
        tempMats[38] = earthIconDull;
        tempMats[39] = waterIconDull;
        tempMats[40] = airIconDull;
        if (element.Equals( "fire"))          tempMats[37] = fireIconLit;
        else if (element.Equals("water"))    tempMats[39] = waterIconLit;
        else if (element.Equals("earth"))    tempMats[38] = earthIconLit;
        else if (element.Equals("air"))    tempMats[40] = airIconLit;

        // Assign materials
        rend.sharedMaterials = tempMats;
    }

    private void snapToTerrain()
    {
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x, 10000000, transform.position.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {

            if (hit.point.y < Globals.water_level)
                Destroy(gameObject);
            else
                transform.position = new Vector3(transform.position.x, hit.point.y + 0.5f, transform.position.z);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
