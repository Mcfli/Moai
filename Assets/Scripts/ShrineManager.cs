using UnityEngine;
using System.Collections;

public class ShrineManager : MonoBehaviour {
    public GenerationManager gen_manager;
    public GameObject prefab;
    public Rigidbody rb;

	// Use this for initialization
	void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void placeShrine(Vector3 chunk_pos)
    {
        Vector3 position = new Vector3(Random.Range(chunk_pos.x, chunk_pos.x + gen_manager.chunk_size) + 50, 0, Random.Range(chunk_pos.z, chunk_pos.z + gen_manager.chunk_size) + 50);
        Quaternion RandomRotation = Quaternion.Euler(-90, Random.Range(0, 360), 0);

		GameObject new_shrine = Instantiate(prefab, position , RandomRotation) as GameObject;
    }

    public bool checkHeights(Vector3 position)
    {
        bool acceptable = false;
        int terrain = LayerMask.GetMask("Terrain");

        // Top left corner raycast
        RaycastHit hitOne;
        Ray rayDownOne = new Ray(new Vector3(position.x, position.y, position.z), Vector3.down);
        float distOne = 0.0f;
        if (Physics.Raycast(rayDownOne, out hitOne, Mathf.Infinity, terrain))
        {
            distOne = hitOne.distance;
        }

        // Top right corner raycast
        RaycastHit hitTwo;
        Ray rayDownTwo = new Ray(new Vector3(position.x, position.y, position.z), Vector3.down);
        float distTwo = 0.0f;
        if (Physics.Raycast(rayDownOne, out hitTwo, Mathf.Infinity, terrain))
        {
            distTwo = hitTwo.distance;
        }


        return acceptable;
    }
}
