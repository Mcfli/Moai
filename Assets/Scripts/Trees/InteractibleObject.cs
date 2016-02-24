using UnityEngine;
using System.Collections;

public class InteractibleObject: MonoBehaviour
{

    public GameObject spawn_object;
    public LayerMask cull_layer;
    public float cull_radius;        // How far this must be from other seeds and trees in order to grow.
    public float life_length;        // How long before the seed sprouts into  a tree (or dies)

    private float spawned_time;     // Stores what the game time was when this was spawned. 

    // Use this for initialization
    void Start()
    {
        spawned_time = Globals.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.time - spawned_time > life_length*Globals.time_resolution)
        {
            Collider[] close_trees = Physics.OverlapSphere(transform.position, cull_radius, cull_layer);
            if(close_trees.Length < 1)
            {
                var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Instantiate(spawn_object, transform.position, RandomRotation);
            }
            Destroy(gameObject);
        }
    }
}