using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrineManager : MonoBehaviour {
    public GenerationManager gen_manager;
    public GameObject prefab;
    public float acceptable_heightDiff = 0.0f;
    public int max_tries = 1;
    public int shrine_probability = 1;

	private static Dictionary<Vector2, List<ShrineGrid>> shrines;

	// Use this for initialization
	void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
		shrines = new Dictionary<Vector2, List<ShrineGrid>>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void placeShrine(Vector3 chunk_pos, Vector2 chunk)
    {
        int val = Random.Range(1, shrine_probability);
        if (val == 1)
        {
            for (int tries = 0; tries <= max_tries; tries++)
            {
                Vector3 position = new Vector3(Random.Range(chunk_pos.x, chunk_pos.x + gen_manager.chunk_size) + 50, 0, Random.Range(chunk_pos.z, chunk_pos.z + gen_manager.chunk_size) + 50);
                if (checkHeights(position))
                {
                    GameObject new_shrine = Instantiate(prefab, position, Quaternion.Euler(-90,0,0)) as GameObject;
                    shrines[chunk] = new List<ShrineGrid>();
                    break;
                }
            }
        }
        

    }

    public bool checkHeights(Vector3 position)
    {
        bool acceptable = false;
        int terrain = LayerMask.GetMask("Terrain");

        // Top left corner raycast
        RaycastHit hitOne;
        Ray rayDownOne = new Ray(new Vector3(position.x - 25, 10000000, position.z - 25), Vector3.down);
        float distOne = 0.0f;
        if (Physics.Raycast(rayDownOne, out hitOne, Mathf.Infinity, terrain))
        {
            distOne = hitOne.distance;
        }

        // Top right corner raycast
        RaycastHit hitTwo;
        Ray rayDownTwo = new Ray(new Vector3(position.x + 25, 10000000, position.z - 25), Vector3.down);
        float distTwo = 0.0f;
        if (Physics.Raycast(rayDownTwo, out hitTwo, Mathf.Infinity, terrain))
        {
            distTwo = hitTwo.distance;
        }

        // Bottom left corner raycast
        RaycastHit hitThree;
        Ray rayDownThree = new Ray(new Vector3(position.x - 25, 10000000, position.z + 25), Vector3.down);
        float distThree = 0.0f;
        if (Physics.Raycast(rayDownThree, out hitThree, Mathf.Infinity, terrain))
        {
            distThree = hitThree.distance;
        }

        // Bottom right corner raycast
        RaycastHit hitFour;
        Ray rayDownFour = new Ray(new Vector3(position.x + 25, 10000000, position.z + 25), Vector3.down);
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
		if(!shrines.ContainsKey(chunk) || shrines[chunk] == null)
		{
			shrines[chunk] = new List<ShrineGrid>();
		}
		shrines[chunk].Add(shrine.GetComponent<ShrineGrid>());
	}

	public void unloadShrines(int x, int y)
	{
		Vector3 center = new Vector3(x * gen_manager.chunk_size + gen_manager.chunk_size*0.5f,0, y * gen_manager.chunk_size + gen_manager.chunk_size * 0.5f);
		Vector3 half_extents = new Vector3(gen_manager.chunk_size*0.5f,100000, gen_manager.chunk_size*0.5f );
		LayerMask shrine_mask = LayerMask.GetMask("Shrine");
		Vector2 chunk = new Vector2(x, y);

		Collider[] colliders = Physics.OverlapBox(center, half_extents,Quaternion.identity,shrine_mask);
		for (int i = 0;i < colliders.Length; i++)
		{
			GameObject shrine = colliders[i].gameObject;
			shrine.GetComponent<ShrineGrid>().saveTransforms();
			saveShrine(chunk, shrine);

			Destroy(shrine);
		}
	}

	public void loadShrines(int x, int y)
	{
		Vector2 key = new Vector2(x, y);

		if (shrines.ContainsKey(key))
		{
			List<ShrineGrid> shrines_in_chunk = shrines[key];

			for (int i = shrines_in_chunk.Count-1; i >= 0; i--)
			{
				ShrineGrid shrine = shrines_in_chunk[i];
				GameObject new_shrine = Instantiate(prefab, shrine.saved_position, shrine.saved_rotation) as GameObject;
				new_shrine.GetComponent<ShrineGrid>().copyFrom(shrine);
				shrines[key].Remove(shrine);  
			}
		}
		else
		{
			placeShrine(gen_manager.chunkToWorld(key), key);
		}
	}
}
