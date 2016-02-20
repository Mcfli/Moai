using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineGrid : MonoBehaviour {

    public bool debug;
    public bool isDone;
    public float size;
    public int resolution;

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
		bool valid = false;
		bool doneCheck = false;

		GameObject tarObj;
		foreach (var target in targetState.Keys) {
			tarObj = targetState[target];
			foreach (GameObject curObj in curState[target]) {
				if (curObj == tarObj) {
					valid = true;
				}

			
			}
		}
    }

    private void genTargetState()
    {

    }
}
