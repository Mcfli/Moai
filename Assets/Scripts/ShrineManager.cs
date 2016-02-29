using UnityEngine;
using System.Collections;

public class ShrineManager : MonoBehaviour {
    public GenerationManager gen_manager;
    public GameObject prefab;

	// Use this for initialization
	void Start () {
        gen_manager = gameObject.GetComponent<GenerationManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void placeShrine(int x, int y)
    {
        Vector3 position = new Vector3(x, 0, y);
        Quaternion RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        GameObject new_shrine = Instantiate(prefab, position, RandomRotation) as GameObject;
    }
}
