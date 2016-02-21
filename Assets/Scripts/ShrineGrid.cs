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

    public List<GameObject> validObjects;

	// Use this for initialization
	void Start () {
        curState = new Dictionary<Vector2, List<GameObject>>();
		targetState = new Dictionary<Vector2, GameObject>();
        //validObjects = new List<GameObject>();

        // For testing
        
        
        genTargetState();
        Debug.Log(targetState.Count);
	}
	
	// Update is called once per frame
	void Update () {
        if (debug)
            drawGrid();
        if (!isDone)
        {
            updateCurState();
            checkDone();
        }
	}

    public void enablePlacementItem(GameObject item)
    {
		validObjects.Add(item);
    }

    private Vector2 realToGrid(Vector3 pos)
    {
        // Find the top left corner of our square
        Vector3 corner = transform.position - new Vector3(0.5f * size, 0, 0.5f * size);
        // Find the relative position to that corner
        Vector3 offset = pos - corner;

        // Round the relative position to grid coordinates
        int x = Mathf.FloorToInt((resolution-1f) * offset.x / size);
        int y = Mathf.FloorToInt((resolution-1f) * offset.z / size);
        return new Vector2(x, y);
    }

    private void updateCurState()
    {
        // Clear old state
        curState.Clear();
        
        // Find all objects in our square range
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(size*0.5f,10, size * 0.5f));
        foreach(Collider collider in colliders)
        {
            GameObject go = collider.gameObject;
            Vector2 gridPos = realToGrid(go.transform.position);

            // Init list if needed
            if (!curState.ContainsKey(gridPos)||curState[gridPos] == null)
                curState[gridPos] = new List<GameObject>();
            // Make sure we don't add the same object twice
            if (go.tag == "Object" && curState[gridPos].IndexOf(go)==-1)
            {
                curState[gridPos].Add(go);
            }
        }
    }

    private void checkDone()
    {
        bool found = false;
        
		GameObject tarObj;
		foreach (Vector2 target in targetState.Keys)
        {
            if (!curState.ContainsKey(target)) continue;
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
            int index = Random.Range(0, validObjects.Count-1);
            GameObject placeObj = validObjects[index];
            targetState[place] = placeObj;
        }
    }

    private void drawGrid()
    {
        float stepInterval = size / (resolution-1f);
        Vector3 corner = transform.position - new Vector3(0.5f * size, 0, 0.5f * size);
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Debug.DrawRay(corner+new Vector3(i*stepInterval,0,j*stepInterval),Vector3.up,Color.red,10);
            }
        }
    }
}
