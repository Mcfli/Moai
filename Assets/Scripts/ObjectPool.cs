using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

    private static List<GameObject> pool = new List<GameObject>();

    // Creates an empty game object
    public static GameObject Create(GameObject original, Vector3 position, Quaternion rotation)
    {
        GameObject obj;
        if (pool.Count < 1)
        {
            obj = Instantiate(original, position, rotation) as GameObject;
        }
        else
        {
            obj = pool[0];
            obj.SetActive(true);
            pool.RemoveAt(0);
            Component[] comps = original.GetComponents<Component>();
            foreach (Component c in comps)
            {
                Globals.CopyComponent(obj, c);
            }
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    // Creates a game object specified
    public static GameObject Create()
    {
        GameObject obj;
        if (pool.Count < 1)
        {
            obj = new GameObject();
        }
        else
        {
            
            obj = pool[0];
            obj.name = "recycledObject";
            obj.SetActive(true);
            pool.RemoveAt(0);
        }
        return obj;
    }

    public static void Erase(GameObject obj)
    {
        if(pool.Count >= 1000)
        {
            Destroy(obj);
        }
        else
        {
            obj.name = "pooledObject";
            // Erase children
            for(int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                Erase(obj.transform.GetChild(i).gameObject);
            }
            Component[] comps = obj.GetComponents<Component>();
            foreach (Component c in comps)
            {
                if (c == obj.transform) continue;
                Destroy(c);
            }
            if (obj.GetComponent<MeshRenderer>() != null) Destroy(obj.GetComponent<MeshRenderer>());
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
}
