using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineManager : MonoBehaviour {
    public GameObject shrine_prefab;
    public GameObject obelisk_prefab;
    public float acceptable_heightDiff = 0.0f;
    public int max_tries = 1;
    public int shrinePlacementSeed;
    public float shrine_probability = 0.70f;
    public int shrine_min_dist = 2;
    public int obeliskPlacementSeed;
    public float obelisk_probability = 0.50f;
    public int obelisk_min_dist = 2;

    private GenerationManager gen_manager;
    public static Dictionary<Vector2, ShrineGrid> shrines;
    public static Dictionary<Vector2, Obelisk> obelisks;

    public static List<Vector2> failedShrines;
    public static List<Vector2> failedObelisks;

    // Use this for initialization
    void Awake () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
		shrines = new Dictionary<Vector2, ShrineGrid>();
        obelisks = new Dictionary<Vector2, Obelisk>();
        failedShrines = new List<Vector2>();
        failedObelisks = new List<Vector2>();
    }
	
	// Update is called once per frame
	void Update () {

	}

	public void placeShrine(Vector3 chunk_pos, Vector2 chunk)
    {
        for (int i = -shrine_min_dist; i <= shrine_min_dist; i++)
        {
            for (int j = -shrine_min_dist; j <= shrine_min_dist; j++)
            {
                Vector2 temp = chunk + Vector2.right * i + Vector2.up * j;
                if (shrines.ContainsKey(temp) || obelisks.ContainsKey(temp))
                {
                    return;
                }
            }
        }
        if (Random.value < shrine_probability && !failedShrines.Contains(chunk))
        {
            for (int tries = 0; tries <= max_tries; tries++)
            {
                Vector3 position = new Vector3(Random.Range(chunk_pos.x, chunk_pos.x + gen_manager.chunk_size) + 50, 0, Random.Range(chunk_pos.z, chunk_pos.z + gen_manager.chunk_size) + 50);
                if (checkHeights(position))
                {
                    GameObject shrine = Instantiate(shrine_prefab, position, Quaternion.Euler(0, 0, 0)) as GameObject;
                    saveShrine(chunk, shrine);
                    return;
                }
            }
            failedShrines.Add(chunk);
        }
        else
        {
            failedShrines.Add(chunk);
        }
    }

    public void placeObelisk(Vector3 chunk_pos, Vector2 chunk)
    {
        for(int i = -obelisk_min_dist; i <= obelisk_min_dist; i++)
        {
            for (int j = -obelisk_min_dist; j <= obelisk_min_dist; j++)
            {
                Vector2 temp = chunk + Vector2.right * i + Vector2.up * j;
                if (obelisks.ContainsKey(temp) && obelisks[temp] != null || shrines.ContainsKey(temp) && shrines[temp] != null)
                {
                    return;
                }
            }
        }
        if (Random.value < obelisk_probability && !failedObelisks.Contains(chunk))
        {
            for (int tries = 0; tries <= max_tries; tries++)
            {
                Vector3 position = new Vector3(Random.Range(chunk_pos.x, chunk_pos.x + gen_manager.chunk_size) + 50, 0, Random.Range(chunk_pos.z, chunk_pos.z + gen_manager.chunk_size) + 50);
                if (checkHeights(position))
                {
                    GameObject obelisk  = Instantiate(obelisk_prefab, position, Quaternion.Euler(0, 0, 0)) as GameObject;
                    saveObelisk(chunk, obelisk);
                    break;
                }
            }
            failedObelisks.Add(chunk);
        }
        else
        {
            failedObelisks.Add(chunk);
        }
    }

    public bool checkHeights(Vector3 position)
    {
        bool acceptable = false;
        int terrain = LayerMask.GetMask("Terrain");

        // Top left corner raycast
        RaycastHit hitOne;
        Ray rayDownOne = new Ray(new Vector3(position.x - 50, 10000000, position.z - 50), Vector3.down);
        float distOne = 0.0f;
        if (Physics.Raycast(rayDownOne, out hitOne, Mathf.Infinity, terrain))
        {
            distOne = hitOne.distance;
        }

        // Top right corner raycast
        RaycastHit hitTwo;
        Ray rayDownTwo = new Ray(new Vector3(position.x + 50, 10000000, position.z - 50), Vector3.down);
        float distTwo = 0.0f;
        if (Physics.Raycast(rayDownTwo, out hitTwo, Mathf.Infinity, terrain))
        {
            distTwo = hitTwo.distance;
        }

        // Bottom left corner raycast
        RaycastHit hitThree;
        Ray rayDownThree = new Ray(new Vector3(position.x - 50, 10000000, position.z + 50), Vector3.down);
        float distThree = 0.0f;
        if (Physics.Raycast(rayDownThree, out hitThree, Mathf.Infinity, terrain))
        {
            distThree = hitThree.distance;
        }

        // Bottom right corner raycast
        RaycastHit hitFour;
        Ray rayDownFour = new Ray(new Vector3(position.x + 50, 10000000, position.z + 50), Vector3.down);
        float distFour = 0.0f;
        if (Physics.Raycast(rayDownFour, out hitFour, Mathf.Infinity, terrain))
        {
            distFour = hitFour.distance;
        }



        float avg_distance = (distOne + distTwo + distThree + distFour) / 4;

        float diff_from_avg_One = Mathf.Abs(distOne - avg_distance);
        float diff_from_avg_Two = Mathf.Abs(distTwo - avg_distance);
        float diff_from_avg_Three = Mathf.Abs(distThree - avg_distance);
        float diff_from_avg_Four = Mathf.Abs(distFour - avg_distance);

        float total_diff = diff_from_avg_One + diff_from_avg_Two + diff_from_avg_Three + diff_from_avg_Four;

        if (total_diff < acceptable_heightDiff)
        {
            acceptable = true;
        }
        else
        {
            acceptable = false;
        }

        return acceptable;
    }

	public void saveShrine(Vector2 chunk, GameObject shrine)
	{
		shrine.GetComponent<ShrineGrid>().saveTransforms();
        shrines[chunk] = shrine.GetComponent<ShrineGrid>();
	}

    public void saveObelisk(Vector2 chunk, GameObject obelisk)
    {
        obelisk.GetComponent<Obelisk>().saveTransforms();
        obelisks[chunk] = obelisk.GetComponent<Obelisk>();
    }

	public void unloadShrines(int x, int y)
	{
		Vector2 chunk = new Vector2(x, y);
        if (shrines.ContainsKey(chunk))
        {
            if(shrines[chunk].gameObject != null)
			    Destroy(shrines[chunk].gameObject);
        }
	}

    public void unloadObelisks(int x, int y)
    {
        Vector2 chunk = new Vector2(x, y);
        if (obelisks.ContainsKey(chunk))
        {
            if(obelisks[chunk] != null)
                Destroy(obelisks[chunk].gameObject);
        }
            
    }

	public void loadShrines(int x, int y)
	{
        Vector2 key = new Vector2(x, y);

        int originalSeed = Random.seed;
        Random.seed = Globals.SeedScript.seed + obeliskPlacementSeed + key.GetHashCode();
        if (shrines.ContainsKey(key))
        {
            ShrineGrid shrine = shrines[key];
            GameObject new_shrine = Instantiate(shrine_prefab, shrine.saved_position, shrine.saved_rotation) as GameObject;
            new_shrine.GetComponent<ShrineGrid>().copyFrom(shrine);
            saveShrine(key, new_shrine);
        }
        else if(!failedShrines.Contains(key))
        {
            placeShrine(GenerationManager.chunkToWorld(key), key);
        }

        Random.seed = originalSeed;
	}

    public void loadObelisks(int x, int y)
    {
        Vector2 key = new Vector2(x, y);

        int originalSeed = Random.seed;
        Random.seed = Globals.SeedScript.seed + obeliskPlacementSeed + key.GetHashCode();

        if (obelisks.ContainsKey(key))
        {
            Obelisk obelisk = obelisks[key];
            GameObject new_obelisk = Instantiate(obelisk_prefab, obelisk.saved_position, obelisk.saved_rotation) as GameObject;
            new_obelisk.GetComponent<Obelisk>().copyFrom(obelisk);
        }
        else
        {
            placeObelisk(GenerationManager.chunkToWorld(key), key);
        }

        Random.seed = originalSeed;
    }
}
