using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour {
    // Access variables
    public float placement_radius;
    public float height_variation;
    public float height_base;

    // Tuning variables
    public float max_opacity;
    public float fade_speed;

    // Internal variables
    private float cur_opacity;
    public bool dissipating;
    private Renderer rend;
    private GameObject player;
    private Vector3 pos_offset;

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        pos_offset = new Vector3(2*Random.value*placement_radius-placement_radius,
            height_base+2*Random.value*height_variation-height_variation,
            2*Random.value*placement_radius - placement_radius);
        cur_opacity = 0.0f;
        dissipating = false;
        rend = GetComponent<Renderer>();
        rend.material.color *= new Color (1,1,1,0.0f);
	}

    public void dissipate()
    {
        dissipating = true;
    }
	
	// Update is called once per frame
	void Update () {
        handleOpacity();
        moveWithPlayer();
	}
    
    void handleOpacity()
    {
        if (!dissipating)
        {
            if (cur_opacity < max_opacity)
            {
                cur_opacity += fade_speed;
                if (cur_opacity > max_opacity) cur_opacity = max_opacity;
                rend.material.color += new Color(0, 0, 0, fade_speed);
            }
        }
        else
        {
            if (cur_opacity > 0)
            {
                cur_opacity -= fade_speed;
                rend.material.color -= new Color(0, 0, 0, fade_speed);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    void moveWithPlayer()
    {
        transform.position = player.transform.position + pos_offset;
    }
}
