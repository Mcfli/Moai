using UnityEngine;
using System.Collections;

public class CreatePlane : MonoBehaviour
{
    void Awake()
    {
        GameObject plane = new GameObject("Plane");
        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = CreateMesh(30, 30);
        MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material.shader = Shader.Find("Particles/Additive");
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.green);
        tex.Apply();
        renderer.material.mainTexture = tex;
        renderer.material.color = Color.green;
		MeshCollider mc = (MeshCollider)plane.AddComponent(typeof(MeshCollider));
    }

    Mesh CreateMesh(float width, float height)
    {
        Mesh m = new Mesh();
        m.name = "ScriptedMesh";
        m.vertices = new Vector3[] {
       		new Vector3(-width, 0.01f, -height),
			new Vector3(-width, 1.0f, -height/2),
         	new Vector3(width, 0.01f, -height),
         	new Vector3(width, 0.01f, height),
        	new Vector3(-width, 0.01f, height)
     };
        m.uv = new Vector2[] {
         	new Vector2 (0, 0),
         	new Vector2 (0, 1),
         	new Vector2(1, 1),
         	new Vector2 (1, 0)
     };
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateNormals();

        return m;
    }
}
