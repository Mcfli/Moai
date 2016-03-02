using UnityEngine;
using System.Collections;

public class ShrineManager : MonoBehaviour {
    public GenerationManager gen_manager;
    public GameObject prefab;
    public float acceptable_heightDiff = 0.0f;
    public int max_tries = 1;

	// Use this for initialization
	void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void placeShrine(Vector3 chunk_pos)
    {
        Quaternion RandomRotation = Quaternion.Euler(-90, Random.Range(0, 360), 0);

        for (int tries = 0; tries <= max_tries; tries++)
        {
            Vector3 position = new Vector3(Random.Range(chunk_pos.x, chunk_pos.x + gen_manager.chunk_size) + 50, 0, Random.Range(chunk_pos.z, chunk_pos.z + gen_manager.chunk_size) + 50);
            if (checkHeights(position))
            {
                GameObject new_shrine = Instantiate(prefab, position, RandomRotation) as GameObject;
                break;
            }
        }

    }

    public bool checkHeights(Vector3 position)
    {
        bool acceptable = false;
        int terrain = LayerMask.GetMask("Terrain");

        // Top left corner raycast
        RaycastHit hitOne;
        Ray rayDownOne = new Ray(new Vector3(position.x, 10000000, position.z), Vector3.down);
        float distOne = 0.0f;
        if (Physics.Raycast(rayDownOne, out hitOne, Mathf.Infinity, terrain))
        {
            distOne = hitOne.distance;
        }

        // Top right corner raycast
        RaycastHit hitTwo;
        Ray rayDownTwo = new Ray(new Vector3(position.x + gen_manager.chunk_size, 10000000, position.z), Vector3.down);
        float distTwo = 0.0f;
        if (Physics.Raycast(rayDownTwo, out hitTwo, Mathf.Infinity, terrain))
        {
            distTwo = hitTwo.distance;
        }

        // Bottom left corner raycast
        RaycastHit hitThree;
        Ray rayDownThree = new Ray(new Vector3(position.x, 10000000, position.z + gen_manager.chunk_size), Vector3.down);
        float distThree = 0.0f;
        if (Physics.Raycast(rayDownThree, out hitThree, Mathf.Infinity, terrain))
        {
            distThree = hitThree.distance;
        }

        // Bottom right corner raycast
        RaycastHit hitFour;
        Ray rayDownFour = new Ray(new Vector3(position.x + gen_manager.chunk_size, 10000000, position.z + gen_manager.chunk_size), Vector3.down);
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

        if (total_diff <= acceptable_heightDiff)
        {
            acceptable = true;
        }
        else
        {
            acceptable = false;
        }

        return acceptable;
    }
}
