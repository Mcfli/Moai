using UnityEngine;
using System.Collections;

public class ShrineManager : MonoBehaviour {
    public GenerationManager gen_manager;
    public GameObject prefab;

	// Use this for initialization
	void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
		placeShrine ((int) gen_manager.cur_chunk.x, (int) gen_manager.cur_chunk.y);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void placeShrine(int x, int y)
    {
		string chunk_name = "chunk (" + gen_manager.cur_chunk.x + "," +  gen_manager.cur_chunk.y + ")";
		GameObject chunk = GameObject.Find(chunk_name);

        Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

		GameObject new_shrine = Instantiate(prefab, chunk.transform.position , RandomRotation) as GameObject;
    }
}
