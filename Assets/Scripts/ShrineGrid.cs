using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineGrid : MonoBehaviour {

    public bool debug;
    public bool isDone;
    public float size;
    public int resolution;  // res X res Number of squares in the grid
    public int maxSolItems; // max items for a solution

    private Dictionary<Vector2, List<GameObject>> curState;
    private Dictionary<Vector2, GameObject> targetState;

    public List<GameObject> validObjects;
    private LayerMask not_terrain;

	// Use this for initialization
	void Start () {
        curState = new Dictionary<Vector2, List<GameObject>>();
		targetState = new Dictionary<Vector2, GameObject>();
        //validObjects = new List<GameObject>();
        not_terrain = ~(LayerMask.GetMask("Terrain"));

        // For testing
        
        
        genTargetState();
	}
	
	// Update is called once per frame
	void Update () {
        if (debug)
            drawGrid();
        if (!isDone)
        {
            updateCurState();
            //checkDone();
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

        float squareSize = size / resolution;

        // Round the relative position to grid coordinates
        int x = Mathf.FloorToInt(offset.x / squareSize);
        int y = Mathf.FloorToInt(offset.z / squareSize);
        return new Vector2(x, y);
    }

    // Returns point in center of grid square
    private Vector3 gridToReal(Vector2 grid)
    {
  
        float squareSize = size / resolution;
        Vector3 corner = transform.position - new Vector3(0.5f * size, 0, 0.5f * size);
        Vector3 offset = new Vector3(grid.x * squareSize,0,grid.y*squareSize);
        return corner + offset;
    }

    private void updateCurState()
    {
        // Clear old state
        curState.Clear();
        
        // Find all objects in our square range
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(size*0.5f,10, size * 0.5f),Quaternion.identity,not_terrain);
        foreach(Collider collider in colliders)
        {
            GameObject go = collider.gameObject;
            Vector2 gridPos = realToGrid(go.transform.position);

            // Init list if needed
            if (!curState.ContainsKey(gridPos)||curState[gridPos] == null)
                curState[gridPos] = new List<GameObject>();
            // Make sure we don't add the same object twice
           // if (validObjects.IndexOf(go) !=-1 && curState[gridPos].IndexOf(go)==-1)
           // {
                curState[gridPos].Add(go);
           // }
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
                Vector2 curGrid = new Vector2(i, j);
                Vector3 cur = gridToReal(curGrid);
                if (targetState.ContainsKey(curGrid) && !curState.ContainsKey(curGrid))
                    drawSquare(curGrid,Color.red);
                else if (targetState.ContainsKey(curGrid) && curState.ContainsKey(curGrid))
                    drawSquare(curGrid, Color.green);
                else if (curState.ContainsKey(curGrid))
                    drawSquare(curGrid, Color.white);
                else
                    drawSquare(curGrid, Color.grey);
            }
        }
    }
    private void drawSquare(Vector2 pos, Color color)
    {
        Vector3 topleft = gridToReal(pos + Vector2.up);
        Vector3 topright= gridToReal(pos + Vector2.right + Vector2.up);
        Vector3 botleft = gridToReal(pos);
        Vector3 botright = gridToReal(pos + Vector2.right);

        // Top
        Debug.DrawLine(topleft,topright,color);
        // Bottom
        Debug.DrawLine(botleft, botright, color);
        // Left
        Debug.DrawLine(botleft, topleft, color);
        // Right
        Debug.DrawLine(botright, topright, color);
    }
}
