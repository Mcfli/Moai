using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour {
    // Access variables

    // Tuning variables
    public float max_opacity;
    public float fade_speed;

    // Internal variables
    private float cur_opacity;
    public bool dissipating;
    private Renderer rend;

	// Use this for initialization
	void Start () {
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
    
}
