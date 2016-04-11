using UnityEngine;
using System.Collections;

public class Seed : MonoBehaviour {
    public bool setSeed = true;
    public int seed = 0; //this will change if setSeed is false;

	// Use this for initialization
	void Awake () {
        if(setSeed) Random.seed = seed;
        else {
            Random.seed = (int)System.DateTime.Now.Ticks;
            seed = Random.seed;
        }
        Globals.seed = seed;
    }
}
