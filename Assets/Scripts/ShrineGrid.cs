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
    public Mural mural;
    public GameObject glow;
    public GameObject vertexPillar;

    public float WaterFireEarthAirChangeMin = 15f;
    public float WaterFireEarthAirChangeMax = 50f;

    public GameObject completeGlow;
    public GameObject incompleteGlow;

    public GameObject fireStar;
    public GameObject waterStar;
    public GameObject earthStar;
    public GameObject airStar;

    // The element that the shrine will check curState against targetState... for
    private string curElement;

    private List<GameObject> curState;
    private List<PuzzleObject> targetStateFire;
    private List<PuzzleObject> targetStateWater;
    private List<PuzzleObject> targetStateEarth;
    private List<PuzzleObject> targetStateAir;

    public List<PuzzleObject> validObjects;
    private LayerMask notTerrain;
    private LayerMask glowLayer;

    //private Dictionary<int, bool> completedGlows;

    public Vector3 saved_position;
    public Quaternion saved_rotation;
    public Vector3 saved_scale;

    private GameObject shrineGlow;

    // Use this for initialization
    void Start()
    {
        curState = new List<GameObject>();
        targetStateFire = new List<PuzzleObject>();
        targetStateWater = new List<PuzzleObject>();
        targetStateEarth = new List<PuzzleObject>();
        targetStateAir = new List<PuzzleObject>();
        validObjects = new List<PuzzleObject>();
        notTerrain = ~(LayerMask.GetMask("Terrain"));
        glowLayer = LayerMask.GetMask("Glow");
        mural = GetComponent<Mural>();

        curElement = "";

        populateValidItems();
        genTargetState();
        updateCurState();
        
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
        if (!isDone)
        {
            updateCurState();
            checkDone();
            //drawGlows();
        }

    }

    public void enablePlacementItem(PuzzleObject item)
    {
        validObjects.Add(item);
    }

    public void changeElement(string element)
    {
        curElement = element;
        mural.lightIcon(element);

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
        List<GameObject> itemsCounted = new List<GameObject>();
        Dictionary<int, bool> completed = new Dictionary<int, bool>();
        
        bool done = true;
        // Look at each requirement in each targetState
        int index = 0;
        List<PuzzleObject> targetState;
        if (curElement == "fire") targetState = targetStateFire;
        else if (curElement == "water") targetState = targetStateWater;
        else if (curElement == "earth") targetState = targetStateEarth;
        else if (curElement == "air") targetState = targetStateAir;
        else return;
        
        foreach (PuzzleObject tarObj in targetState)
        {
            // if we don't currently have the desired item in the box, it can't be complete   
            bool found = false;

            for (int i = 0; i < curState.Count; i++)
            {
                GameObject gameObj = curState[i];
                if (itemsCounted.IndexOf(gameObj) != -1) continue;
                PuzzleObject po = gameObj.GetComponent<PuzzleObject>();

                if (po == null)
                {
                    continue;
                }

                if (po.Equals(tarObj))
                {
                    itemsCounted.Add(gameObj);
                    found = true;
                    completed[index] = true;
                }
            }
            if (!found)
            {
                done = false;
            }
            index++;
        }
        isDone = done;
        if(isDone)
            complete(curElement);
        mural.genMurals(targetStateFire, targetStateWater, targetStateEarth, targetStateAir);
        mural.glowPuzzleObjects(completed, targetState, curElement);
    }

    // Uses a random Vector2 based on the resolution size and uses a random index based on the count in validObjects to populate targetState dictionary.
    private void genTargetState()
    {
        int numItems = 0;

        // Fire
        numItems = Random.Range(minSolItems, maxSolItems);
        for (int i = 0; i < numItems; i++)
        {
            int index = Random.Range(0, validObjects.Count - 1);
            PuzzleObject placeObj = validObjects[index];
            if (placeObj != null)
                targetStateFire.Add(placeObj);
        }

        // Water
        numItems = Random.Range(minSolItems, maxSolItems);
        for (int i = 0; i < numItems; i++)
        {
            int index = Random.Range(0, validObjects.Count - 1);
            PuzzleObject placeObj = validObjects[index];
            if (placeObj != null)
                targetStateWater.Add(placeObj);
        }

        // Earth
        numItems = Random.Range(minSolItems, maxSolItems);
        for (int i = 0; i < numItems; i++)
        {
            int index = Random.Range(0, validObjects.Count - 1);
            PuzzleObject placeObj = validObjects[index];
            if (placeObj != null)
                targetStateEarth.Add(placeObj);
        }
        //Air
        numItems = Random.Range(minSolItems, maxSolItems);
        for (int i = 0; i < numItems; i++)
        {
            int index = Random.Range(0, validObjects.Count - 1);
            PuzzleObject placeObj = validObjects[index];
            if (placeObj != null)
                targetStateAir.Add(placeObj);
        }
    }

    private void createMural()
    {
        mural.genMurals(targetStateFire, targetStateWater, targetStateEarth, targetStateAir);
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


    // element - "earth", "fire", "water", "air"
    private void complete(string element)
    {
        Destroy(shrineGlow);
        shrineGlow = Instantiate(completeGlow, transform.position + Vector3.up * 10, Quaternion.identity) as GameObject;
        // Stop the cells from glowing
        //drawGlows();

        Vector2 WaterFireEarthAirChange;
        if (element == "fire") WaterFireEarthAirChange = Vector2.right; //(1,0)
        else if (element == "water") WaterFireEarthAirChange = Vector2.left; //(-1,0)
        else if (element == "earth") WaterFireEarthAirChange = Vector2.down; //(0,-1)
        else WaterFireEarthAirChange = Vector2.up; //(0,1)

        WaterFireEarthAirChange *= Random.Range(WaterFireEarthAirChangeMin,WaterFireEarthAirChangeMax);
        // Change the chunk
        GameObject.Find("WorldGen").GetComponent<GenerationManager>().modifyChunk(transform.position, WaterFireEarthAirChange);
        // Add a star
        GameObject star;
        Vector3 target = Globals.Player.transform.position + Vector3.up * 10000;
        if (element.Equals("fire")){
            star = Instantiate(fireStar, target, Quaternion.identity) as GameObject;
        }
        else if (element.Equals("water"))
        {
            star = Instantiate(waterStar, target, Quaternion.identity) as GameObject;
        }
        else if (element.Equals("earth"))
        {
            star = Instantiate(earthStar, target, Quaternion.identity) as GameObject;
        }
        else if (element.Equals("air"))
        {
            star = Instantiate(airStar, target, Quaternion.identity) as GameObject;
        }

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

                ret = new Vector3(pos.x, hit.point.y, pos.z);
            
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
        targetStateFire = shrine.targetStateFire;
        targetStateWater = shrine.targetStateWater;
        targetStateEarth = shrine.targetStateEarth;
        targetStateAir = shrine.targetStateAir;
        validObjects = shrine.validObjects;
        notTerrain = shrine.notTerrain;
        glowLayer = shrine.glowLayer;
        //completedGlows = shrine.completedGlows;
        saved_position = shrine.saved_position;
        saved_rotation = shrine.saved_rotation;
        saved_scale = shrine.saved_scale;
    }

}
