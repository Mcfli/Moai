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

	// Use this for initialization
	void Start () {
        muralTex = new Texture2D(imageRes*numSquares,imageRes*numSquares);
        //snapToTerrain();
	}

    // Generate mural texture based on a target state
    public void generateTexture(List<PuzzleObject> state)
    {
       
        muralTex = new Texture2D(imageRes * numSquares, imageRes * numSquares,TextureFormat.ARGB32,false);
        muralTex.name = "Mural Texture";
        muralTex.filterMode = FilterMode.Point;
        
        Color[] newPixels = new Color[imageRes * numSquares * imageRes * numSquares];
        for(int i = 0; i< newPixels.Length; i++)
        {
            newPixels[i] = muralColor;
        }

        Vector2 coordIndex = Vector2.zero;

        foreach (PuzzleObject obj in state) {
            Texture2D image = obj.image;

            Vector2 coord = coordIndex;
            // The coordinates for texturing are mirrored, so we need to flip the cord values
            coord.x = (numSquares - 1) - coord.x;
            coord.y = (numSquares - 1) - coord.y;

            // Calculate where to start adding these pixels to new
            int offx = (int)coord.x * imageRes;
            int offy = (int)coord.y * imageRes * imageRes * numSquares;

            Color[] pixels = image.GetPixels();
            // Add image pixels to newPixels
            for (int i = 0; i < imageRes; i++) {
                for (int j = 0; j < imageRes; j++) {
                    Color newColor = new Color();
                    Color imgColor = pixels[j + i * imageRes];
                    Color bgColor = newPixels[j + i * numSquares * imageRes + offx + offy];

                    newColor.a = 1 - (1 - newColor.a) * (1 - bgColor.a);        // alpha 
                    newColor.r = imgColor.r * imgColor.a / newColor.a + bgColor.r * bgColor.a * (1 - imgColor.a) / newColor.a; // Red
                    newColor.g = imgColor.g * imgColor.a / newColor.a + bgColor.g * bgColor.a * (1 - imgColor.a) / newColor.a; // Green
                    newColor.b = imgColor.b * imgColor.a / newColor.a + bgColor.b * bgColor.a * (1 - imgColor.a) / newColor.a; // Blue
                    newPixels[j + i * numSquares * imageRes + offx + offy] = newColor;
                }
            }

            // Increment picture position
            coordIndex += Vector2.right;
            // If too far horizontal, move to nex row
            if (coordIndex.x > 2)
            {
                coordIndex.x = 0;
                coordIndex.y += 1;
            }
            // If on center square, move right
            if(coordIndex.x == 1 && coordIndex.y == 1)
                coordIndex += Vector2.right;
        }

        //TEMP SHRINE ICON
        Vector2 shrineCoord = new Vector2(1,1);

        // Calculate where to start adding these pixels to new
        int shrineOffx = (int)shrineCoord.x * imageRes;
        int shrineOffy = (int)shrineCoord.y * imageRes * imageRes * numSquares;

        Color[] shrinePixels = shrineTexture.GetPixels();
        // Add image pixels to newPixels
        for (int i = 0; i < imageRes; i++) {
            for (int j = 0; j < imageRes; j++) {
                Color newColor = new Color();
                Color imgColor = shrinePixels[j + i * imageRes];
                Color bgColor = newPixels[j + i * numSquares * imageRes + shrineOffx + shrineOffy];

                newColor.a = 1 - (1 - newColor.a) * (1 - bgColor.a);        // alpha 
                newColor.r = imgColor.r * imgColor.a / newColor.a + bgColor.r * bgColor.a * (1 - imgColor.a) / newColor.a; // Red
                newColor.g = imgColor.g * imgColor.a / newColor.a + bgColor.g * bgColor.a * (1 - imgColor.a) / newColor.a; // Green
                newColor.b = imgColor.b * imgColor.a / newColor.a + bgColor.b * bgColor.a * (1 - imgColor.a) / newColor.a; // Blue
                newPixels[j + i * numSquares * imageRes + shrineOffx + shrineOffy] = newColor;
            }
        }

        muralTex.SetPixels(newPixels);
        muralTex.Apply();
        gameObject.GetComponent<Renderer>().materials[4].mainTexture = muralTex;
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
