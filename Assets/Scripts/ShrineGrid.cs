using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineGrid : MonoBehaviour {

    public bool debug;
    public bool isDone;
    public float size;
    public int resolution;  // res X res Number of squares in the grid
    public int minSolItems; // max items for a solution
    public int maxSolItems; // max items for a solution
    public GameObject mural;
    public GameObject glow;
    public GameObject vertexPillar;

    private Dictionary<Vector2, List<PuzzleObject>> curState;
    private Dictionary<Vector2, PuzzleObject> targetState;

    public List<GameObject> validObjects;
    private LayerMask notTerrain;
    private LayerMask glowLayer;

    private Dictionary<Vector2,bool> glowGrid;

	public Vector3 saved_position;
	public Quaternion saved_rotation;
	public Vector3 saved_scale;

	// Use this for initialization
	void Start () {
        curState = new Dictionary<Vector2, List<PuzzleObject>>();
		targetState = new Dictionary<Vector2, PuzzleObject>();
        glowGrid = new Dictionary<Vector2, bool>();
        //validObjects = new List<GameObject>();
        notTerrain = ~(LayerMask.GetMask("Terrain"));
        glowLayer = LayerMask.GetMask("Glow");

        // For testing      
        genTargetState();
        updateCurState();

        transform.position = snapToTerrain(transform.position);
        createMural();
        createPillars();
        killTrees();
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
        drawGlows();
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
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(size*0.5f,10, size * 0.5f),Quaternion.identity,notTerrain);
        foreach(Collider collider in colliders)
        {
            GameObject go = collider.gameObject;
            Vector2 gridPos = realToGrid(go.transform.position);

            // Init list if needed
            if (!curState.ContainsKey(gridPos)||curState[gridPos] == null)
                curState[gridPos] = new List<PuzzleObject>();
            // Make sure it's a valid object, Make sure we don't add the same object twice
            PuzzleObject po = go.GetComponent<PuzzleObject>();
            if(po != null)
                curState[gridPos].Add(po);
        }
    }

    private void checkDone()
    {
		PuzzleObject tarObj;

        // Look at each requirement in targetState
		foreach (Vector2 cell in targetState.Keys)
        {
            // If curState doesn't have anything in this cell, it can't be complete
            if (!curState.ContainsKey(cell)) {
                isDone = false;
                return;
            }
            tarObj = targetState[cell].GetComponent<PuzzleObject>();
            
            // if we don't currently have the desired item in the cell, it can't be complete    
            if (curState[cell].IndexOf(tarObj) == -1)
            {
                //Debug.Log("Some object here, but not goal");
                isDone = false;
                return;
            }
            
            // if there are other puzzle objects in the cell, it can't be complete
            if (curState[cell].Count > 1)
            {
                isDone = false;
                return;
            }
		}
        // looped through all and did not return false, so we found all of them
        isDone = true;
        complete();
    }

    // Uses a random Vector2 based on the resolution size and uses a random index based on the count in validObjects to populate targetState dictionary.
    private void genTargetState()
    {
        int numItems = 0;
        numItems = Random.Range(minSolItems, maxSolItems);
        for (int i = 0; i < numItems; i++)
        {
            Vector2 centerSquare = new Vector2(Mathf.Floor(resolution/2), Mathf.Floor(resolution / 2));
            Vector2 place = new Vector2(Random.Range(0, resolution),Random.Range(0, resolution));
            while(place == centerSquare)
                place = new Vector2(Random.Range(0, resolution), Random.Range(0, resolution));
            int index = Random.Range(0, validObjects.Count-1);
            GameObject placeObj = validObjects[index];
            targetState[place] = placeObj.GetComponent<PuzzleObject>();
        }
    }

    private void createMural()
    {
        Vector3 offset = new Vector3(10,0,5);
        GameObject localMural = Instantiate(mural,transform.position + offset, mural.transform.rotation) as GameObject;
        localMural.GetComponent<Mural>().generateTexture(targetState);
        localMural.transform.parent = gameObject.transform;
    }

    private void createPillars()
    {
        for (int i = 0; i <= resolution; i++)
        {
            for (int j = 0; j <= resolution; j++)
            {
                // Skip non-edge points
                if (i != 0 && i != resolution && j != 0 && j != resolution) continue;
                // Skip the square corners
                if ((i == 0 || i == resolution) && (j == 0 || j == resolution)) continue;

                Vector2 curGrid = new Vector2(j, i);
                Vector3 cur = gridToReal(curGrid);
                GameObject pillar = Instantiate(vertexPillar,snapToTerrain(cur),vertexPillar.transform.rotation) as GameObject;
                pillar.transform.parent = gameObject.transform;
            }
        }
    }

    private void killTrees()
    {
        Vector3 half_extents = new Vector3(size, 100000, size);
        LayerMask tree_mask = LayerMask.GetMask("Tree");

        Collider[] colliders = Physics.OverlapBox(transform.position, half_extents, Quaternion.identity, tree_mask);
        for (int i = 0; i < colliders.Length; i++)
        {

            GameObject tree = colliders[i].gameObject;
            Destroy(tree);
        }
    }

    private void drawGrid()
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Vector2 curGrid = new Vector2(i, j);
                //Vector3 cur = gridToReal(curGrid);
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

    private void drawGlows()
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Vector2 curGrid = new Vector2(i, j);
                Vector3 cur = gridToReal(curGrid);

                if (!curState.ContainsKey(curGrid)) continue;
                if (!targetState.ContainsKey(curGrid)) continue;
                List<PuzzleObject> inCell = curState[curGrid];
                PuzzleObject targetItem = targetState[curGrid];

                if (inCell != null && inCell.Count == 1 && inCell.Contains(targetItem))
                {
                    if (!glowGrid.ContainsKey(curGrid))
                    {
                        glowGrid[curGrid] = false;
                    }
                    if(!glowGrid[curGrid])
                    {
                        
                        glowGrid[curGrid] = true;
                        GameObject glowInstance = Instantiate(glow, cur + new Vector3(size/resolution*0.5f,0, size / resolution * 0.5f),
                            Quaternion.identity) as GameObject;
                        glowInstance.name = "Glow "+curGrid;
                    }
                }
                else if (inCell != null && (inCell.Count != 1 || !inCell.Contains(targetItem)))
                {
                    if (!glowGrid.ContainsKey(curGrid))
                    {
                        glowGrid[curGrid] = false;
                    }
                    if (glowGrid[curGrid])
                    {
                        glowGrid[curGrid] = false;
                        GameObject glowInstance = GameObject.Find("Glow " + curGrid);
                        Destroy(glowInstance);
                    }
                }
            }
        }
    }

    private void complete()
    {
        /*GameObject glowInstance = */Instantiate(glow,transform.position+Vector3.up*10,Quaternion.identity); //as GameObject;
    }

    private Vector3 snapToTerrain(Vector3 pos)
    {
        Vector3 ret = pos;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(pos.x, 10000000, pos.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {
            if (hit.point.y > Globals.water_level)
            {
                ret = new Vector3(pos.x, hit.point.y + 0.5f, pos.z);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
        return ret;
    }

	public void saveTransforms()
	{
		saved_position = transform.position;
		saved_rotation = transform.rotation;
		saved_scale = transform.localScale;
	}

	public void copyFrom(ShrineGrid shrine)
	{
		debug = shrine.debug;
		isDone = shrine.isDone;
		size = shrine.size;
		resolution = shrine.resolution;
		minSolItems = shrine.minSolItems;
		maxSolItems = shrine.maxSolItems;
		mural = shrine.mural;
		glow = shrine.glow;
		vertexPillar = shrine.vertexPillar;
		curState = shrine.curState;
		targetState = shrine.targetState;
		validObjects = shrine.validObjects;
		notTerrain = shrine.notTerrain;
		glowLayer = shrine.glowLayer;
		glowGrid = shrine.glowGrid;
		saved_position = shrine.saved_position;
		saved_rotation = shrine.saved_rotation;
		saved_scale = shrine.saved_scale;
	}
		
}
