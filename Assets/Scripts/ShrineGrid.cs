using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineGrid : MonoBehaviour {

    public bool debug;
    public bool isDone;
    public float size;
    public int resolution;
    public int maxSolItems; // max items for a solution

    private Dictionary<Vector2, List<GameObject>> curState;
    private Dictionary<Vector2, GameObject> targetState;

    private List<GameObject> validObjects;

	// Use this for initialization
	void Start () {
        curState = new Dictionary<Vector2, List<GameObject>>();
		targetState = new Dictionary<Vector2, GameObject>();
        validObjects = new List<GameObject>();
        genTargetState();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isDone)
        {
            updateCurState();
            checkDone();
        }
	}

    public void enablePlacementItem(GameObject item)
    {
		validObjects.Add (item);
    }

    private Vector2 realToGrid(Vector3 pos)
    {
        return new Vector2();
    }

    private void updateCurState()
    {

    }

    private void checkDone()
    {
        bool found = false;
        
		GameObject tarObj;
		foreach (var target in targetState.Keys)
        {
            found = false;
			tarObj = targetState[target];
			foreach (GameObject curObj in curState[target]) 
            {
                // check if this is the target object
				if (curObj == tarObj) 
                {
                    found = true;
				}
			}
            // if did not find target item
            if(found == false)
            {
                isDone = false;
                return;
            }
		}
        // looped through all and did not return false, so we found all of them
        isDone = true;
        return;
    }

    // Uses a random Vector2 based on the resolution size and uses a random index based on the count in validObjects to populate targetState dictionary.
    private void genTargetState()
    {
        int numItems = 0;
        numItems = Random.Range(1, maxSolItems);
        for (int i = 0; i < numItems; i++)
        {
            Vector2 place = new Vector2(Random.Range(0, resolution),Random.Range(0, resolution));
            int index = Random.Range(0, validObjects.Count);
            GameObject placeObj = validObjects[index];
            targetState[place] = placeObj;
        }
    }
}
