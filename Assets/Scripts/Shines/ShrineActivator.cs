using UnityEngine;
using System.Collections;

public class ShrineActivator : MonoBehaviour {

    public string element;
    public static bool firstActivate = false;

    private ShrineGrid parentShrine;

    // Click this to set parent shrine's element to this prefab's element
	// Use this for initialization
	void Start () {
        parentShrine = transform.parent.gameObject.GetComponent<ShrineGrid>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool activate()
    {
        if (Time.timeScale > 0 && parentShrine != null && !parentShrine.isDone)
        {
            parentShrine.changeElement(element);

            firstActivate = true;

            return true;
        }
        return false;
        
    }

    public bool active() {
        return parentShrine.getElement() == element;
    }
}
