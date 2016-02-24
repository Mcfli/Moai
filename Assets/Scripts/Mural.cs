using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Mural : MonoBehaviour {
    public int imageRes;  // Number of pixels per side in 
    public int numSquares; // Number of cells per side
    public Color muralColor;

    Texture2D muralTex;

	// Use this for initialization
	void Start () {
        muralTex = new Texture2D(imageRes*numSquares,imageRes*numSquares);
	}

    // Generate mural texture based on a target state
    public void generateTexture(Dictionary<Vector2,PuzzleObject> state)
    {
        Color[] newPixels = new Color[imageRes*imageRes];
        for(int i = 0; i< newPixels.Length; i++)
        {
            newPixels[i] = muralColor;
        }

        foreach(KeyValuePair<Vector2,PuzzleObject> pair in state)
        {
            Vector2 coord = pair.Key;
            PuzzleObject obj = pair.Value;
            Texture2D image = obj.image;

            // Rescale image to correct size
            image.Resize(imageRes,imageRes);
            image.Apply();

            // Calculate where to start adding these pixels to new
            int offset = (int)coord.x * imageRes + (int)coord.y * imageRes * numSquares;
            
            // Add image pixels to newPixels
            Array.Copy(image.GetPixels(),0,newPixels, offset, imageRes *imageRes);   
        }

        muralTex.SetPixels(newPixels);
        muralTex.Apply();
        gameObject.GetComponent<Renderer>().material.mainTexture = muralTex;
    }

    
}
