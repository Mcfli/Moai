using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Mural : MonoBehaviour {
    public int imageRes;  // Number of pixels per side in 
    public int numSquares; // Number of cells per side
    public Color muralColor;
    public Texture2D shrineTexture;

    Texture2D muralTex;

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
     * Earth11 ---- 5
     * Earth12 ---- 3
     * Earth20 ---- 7
     * Earth21 ---- 6
     * Earth22 ---- 8
     * 
     * Air00 ---- 16
     * Air01 ---- 18
     * Air02 ---- 17
     * Air10 ---- 15
     * Air11 ---- 13
     * Air12 ---- 14
     * Air20 ---- 10
     * Air21 ---- 12
     * Air22 ---- 11
     * 
     * Water00 ---- 21
     * Water01 ---- 19
     * Water02 ---- 20
     * Water10 ---- 22
     * Water11 ---- 24
     * Water12 ---- 23
     * Water20 ---- 27
     * Water21 ---- 25
     * Water22 ---- 26
     * 
     * Fire00 ---- 34
     * Fire01 ---- 35
     * Fire02 ---- 33
     * Fire10 ---- 32
     * Fire11 ---- 31
     * Fire12 ---- 36
     * Fire20 ---- 29
     * Fire21 ---- 30
     * Fire22 ---- 28
     * 
     */

	// Use this for initialization
	void Start () {
        muralTex = new Texture2D(imageRes*numSquares,imageRes*numSquares);
        //snapToTerrain();
	}

    public void genMurals(List<PuzzleObject> targetState)
    {

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
