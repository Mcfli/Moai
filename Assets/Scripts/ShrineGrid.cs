using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineGrid : MonoBehaviour
{

    public bool debug;
    public bool isDone;
    public float size;
    public int resolution;  // res X res Number of squares in the grid
    public int minSolItems; // min items for a solution
    public int maxSolItems; // max items for a solution
    public GameObject mural;
    public GameObject glow;
    public GameObject vertexPillar;

    public Vector2 WaterFireEarthAirChange; // The changes the shrine has to WaterFire/EarthAir map
    public float WaterFireEarthAirChangeMin = 15f;
    public float WaterFireEarthAirChangeMax = 50f;

    public GameObject completeGlow;
    public GameObject incompleteGlow;

    private List<GameObject> curState;
    private List<PuzzleObject> targetState;

    public List<PuzzleObject> validObjects;
    private LayerMask notTerrain;
    private LayerMask glowLayer;

    private Dictionary<int, bool> completedGlows;

    public Vector3 saved_position;
    public Quaternion saved_rotation;
    public Vector3 saved_scale;

    private GameObject shrineGlow;

    // Use this for initialization
    void Start()
    {
        curState = new List<GameObject>();
        targetState = new List<PuzzleObject>();
        completedGlows = new Dictionary<int, bool>();
        validObjects = new List<PuzzleObject>();
        notTerrain = ~(LayerMask.GetMask("Terrain"));
        glowLayer = LayerMask.GetMask("Glow");

        // For testing 
        populateValidItems();
        genTargetState();
        updateCurState();


        WaterFireEarthAirChange *= Random.Range(WaterFireEarthAirChangeMin, WaterFireEarthAirChangeMax);
        shrineGlow = Instantiate(incompleteGlow, transform.position + Vector3.up * 10, Quaternion.identity) as GameObject;
		shrineGlow.transform.parent = gameObject.transform;

		transform.position = snapToTerrain(transform.position);
		createMural();
		createPillars();
		killTrees();
    }

    // Update is called once per frame
    void Update()
    {
        if (debug)
            drawGrid();
        if (!isDone)
        {
            updateCurState();
            checkDone();
            drawGlows();
        }

    }

    public void enablePlacementItem(PuzzleObject item)
    {
        validObjects.Add(item);
    }

    private void populateValidItems()
    {
        // Find all biomes within our current WaterFire-EarthAir vector box
        List<Biome> allBiomes = GameObject.Find("WorldGen").GetComponent<GenerationManager>().biomes;

        // Calculate the bounding edges of our WaterFire-EarthAir box
        float EarthAirMin = Globals.WaterFireEarthAirVector.y > 0 ?
            Globals.WaterFireEarthAirOrigin.y - Globals.WaterFireEarthAirMin :
            Globals.WaterFireEarthAirOrigin.y + Globals.WaterFireEarthAirVector.y - Globals.WaterFireEarthAirMin;
        float EarthAirMax = Globals.WaterFireEarthAirVector.y > 0 ?
            Globals.WaterFireEarthAirOrigin.y + Globals.WaterFireEarthAirVector.y + Globals.WaterFireEarthAirMin :
            Globals.WaterFireEarthAirOrigin.y + Globals.WaterFireEarthAirMin;
        float WaterFireMin = Globals.WaterFireEarthAirVector.x > 0 ?
           Globals.WaterFireEarthAirOrigin.x - Globals.WaterFireEarthAirMin :
           Globals.WaterFireEarthAirOrigin.x + Globals.WaterFireEarthAirVector.x - Globals.WaterFireEarthAirMin;
        float WaterFireMax = Globals.WaterFireEarthAirVector.x > 0 ?
            Globals.WaterFireEarthAirOrigin.x + Globals.WaterFireEarthAirVector.x + Globals.WaterFireEarthAirMin :
            Globals.WaterFireEarthAirOrigin.x + Globals.WaterFireEarthAirMin;

        // Calculate max distance
        float maxDist = Vector2.Distance(new Vector2(WaterFireMax, EarthAirMax), Globals.WaterFireEarthAirOrigin);

        foreach (Biome b in allBiomes)
        {
            // If the biome is in our box, add it as a valid biome
            if (b.WaterFire <= WaterFireMax && b.WaterFire >= WaterFireMin &&
                b.EarthAir <= EarthAirMax && b.EarthAir >= EarthAirMin)
            {
                float distance = Vector2.Distance(Globals.WaterFireEarthAirOrigin, new Vector2(b.WaterFire, b.EarthAir));
                float weight = distance / maxDist;
                weight = Mathf.Sqrt(weight);

                if (weight > Globals.WaterFireEarthAirDistGuaranteed && Random.value <= weight) continue;

                // Add all trees with puzzleObjects associated with the current biome
				foreach (GameObject tree in b.treeTypes)
				{
					foreach (PuzzleObject po in tree.GetComponent<TreeScript>().statePuzzleObjects)
					{
						if (po != null)
							enablePlacementItem(po);
					}

				}
                

                // Add all doodads with puzzleObjects associated with the current biome
                foreach (GameObject doodad in b.smallDoodads)
                {
                    PuzzleObject po = doodad.GetComponent<PuzzleObject>();
                    if (po != null)
                        enablePlacementItem(po);
                }
            }
        }
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
        Vector3 offset = new Vector3(grid.x * squareSize, 0, grid.y * squareSize);
        return corner + offset;
    }

    private void updateCurState()
    {
        // Clear old state
        curState.Clear();

        // Find all objects in our square range
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(size * 0.5f, 10, size * 0.5f), Quaternion.identity, notTerrain);
        foreach (Collider collider in colliders)
        {
            GameObject go = collider.gameObject;
            if(go.GetComponent<PuzzleObject>() != null)
                curState.Add(go);
        }
    }

    private void checkDone()
    {
        List<GameObject> itemsCounted= new List<GameObject>();
        // Look at each requirement in targetState
        foreach (PuzzleObject tarObj in targetState)
        {
            // if we don't currently have the desired item in the box, it can't be complete   
            bool found = false;
            
            for(int i = 0; i < curState.Count; i++)
            {
                GameObject gameObj = curState[i];
                if (itemsCounted.IndexOf(gameObj) != -1) continue;
                PuzzleObject po = gameObj.GetComponent<PuzzleObject>();

                if (po == null)
                {
                    continue;
                }

                if (po.Equals( tarObj))
                {
                    itemsCounted.Add(gameObj);

                    found = true;
                }
            }
            if (!found) {
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
            int index = Random.Range(0, validObjects.Count - 1);
            PuzzleObject placeObj = validObjects[index];
            if (placeObj != null)
                targetState.Add(placeObj);
        }
    }

    private void createMural()
    {
        GetComponent<Mural>().generateTexture(targetState);
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
                GameObject pillar = Instantiate(vertexPillar, snapToTerrain(cur), vertexPillar.transform.rotation) as GameObject;
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
        drawSquare(Color.white);
        foreach(GameObject gameObj in curState)
        {
            if(gameObj == null)
            {
                continue;
            }
            PuzzleObject po = gameObj.GetComponent<PuzzleObject>();
            if(po != null)
            {
                if (targetState.Contains(po))
                {
                    Debug.DrawRay(gameObj.transform.position, Vector3.up, Color.green);
                }
                else
                {
                    Debug.DrawRay(gameObj.transform.position, Vector3.up, Color.white);
                }
            }
        }
    }
    private void drawSquare(Color color)
    {
        Vector3 topleft = gridToReal(Vector2.zero + 3* Vector2.up);
        Vector3 topright = gridToReal(3 * Vector2.right + 3 * Vector2.up);
        Vector3 botleft = gridToReal(Vector2.zero);
        Vector3 botright = gridToReal(Vector2.zero + 3* Vector2.right);

        // Top
        Debug.DrawLine(topleft, topright, color);
        // Bottom
        Debug.DrawLine(botleft, botright, color);
        // Left
        Debug.DrawLine(botleft, topleft, color);
        // Right
        Debug.DrawLine(botright, topright, color);
    }

    private void drawGlows()
    {

        // Look through the items in targetState
        for (int i = 0; i < targetState.Count; i++)
        {
            PuzzleObject targetObj = targetState[i];
            bool targetObjComplete = false;
            // Look through the stuff in current state to see if this item was found

            foreach (GameObject curObj in curState)
            {
                PuzzleObject curPuzzleObj = curObj.GetComponent<PuzzleObject>();
                if (curPuzzleObj == null) continue;
                // If this object matches the one in our target state, and there isn't already a glow for that item, make a glow, 
                // then move on to next item in target state
                if(curPuzzleObj.Equals(targetObj))
                {
                    targetObjComplete = true;
                    if(!completedGlows.ContainsKey(i) || completedGlows[i] == false)
                    {
                        GameObject glowInstance = Instantiate(glow, curObj.transform.position, Quaternion.identity) as GameObject;
                        glowInstance.name = "Glow " + gameObject.GetHashCode() + targetObj.GetHashCode();
                        completedGlows[i] = true;
                    }
                }
            }

            // If this object has a glow on it but isn't complete, destroy the glow
            if (completedGlows.ContainsKey(i) && completedGlows[i] == true && !targetObjComplete)
            {
                GameObject glowInstance = GameObject.Find("Glow " + gameObject.GetHashCode() + targetObj.GetHashCode());
                Destroy(glowInstance);
                completedGlows[i] = false;
            }
        } 
    }

    private void complete()
    {
        Destroy(shrineGlow);
        shrineGlow = Instantiate(completeGlow, transform.position + Vector3.up * 10, Quaternion.identity) as GameObject;
        // Stop the cells from glowing
        drawGlows();
        // Change the chunk
        GameObject.Find("WorldGen").GetComponent<GenerationManager>().modifyChunk(transform.position, WaterFireEarthAirChange);
        // Add a star
        GameObject.Find("Sky").GetComponent<Sky>().addStar();
        // Update puzzle complexity
        Globals.WaterFireEarthAirVector += WaterFireEarthAirChange;
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
                ret = new Vector3(pos.x, hit.point.y + 0.1f, pos.z);
            }
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
        completedGlows = shrine.completedGlows;
        saved_position = shrine.saved_position;
        saved_rotation = shrine.saved_rotation;
        saved_scale = shrine.saved_scale;
    }

}
