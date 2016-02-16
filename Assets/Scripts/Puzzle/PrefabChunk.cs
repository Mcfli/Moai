using UnityEngine;
using System.Collections;


// To create a prefab chunk:
// 1. Import your SQUARE blender terrain into unity. Don't include lamp/camera in import.
// 2. In model's import settings, set the normals to calculated, smoothing angle to max value
// 3. Create a prefab of your model, and attatch this script to it
// 4. Make sure your prefab's xyz scale is all 1
// 5. Add your prefab chunk to the prefab chunk[] in worldGen's GenerationManager component

public class PrefabChunk : MonoBehaviour {

    public MeshFilter mf;
    

	// Use this for initialization
	void Start ()
    {
        
        mf = GetComponent<MeshFilter>();
        
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void scaleToSettings(float chunk_size,int chunk_res)
    {
        transform.position += new Vector3(chunk_size*0.5f,0,chunk_size*0.5f);
        mf = GetComponent<MeshFilter>();
        meshBlendToUnityRot();
        Vector3[] new_verts;
        int o_chunk_res = Mathf.FloorToInt(Mathf.Sqrt(mf.mesh.vertexCount));
        float o_chunk_size = 1;
        float o_step_size = 1;

        float max_x = float.MinValue;
        float min_x = float.MaxValue;

        for (int i = 0; i < mf.mesh.vertices.Length; i++)
        {
            
            if (mf.mesh.vertices[i].x < min_x)
                min_x = mf.mesh.vertices[i].x;
            else if (mf.mesh.vertices[i].x > max_x)
                max_x = mf.mesh.vertices[i].x;
        }
        o_chunk_size = max_x - min_x;
        o_step_size = o_chunk_size/o_chunk_res;
        Debug.Log(o_chunk_size);

        new_verts = new Vector3[mf.mesh.vertices.Length];

        for(int i = 0; i < mf.mesh.vertices.Length; i++)
        {
            float x = mf.mesh.vertices[i].x;
            float y = mf.mesh.vertices[i].y;
            float z = mf.mesh.vertices[i].z;

            x *= chunk_size/o_chunk_size;
            y *= chunk_size/o_chunk_size;
            z *= chunk_size / o_chunk_size;

            Vector3 new_vert = new Vector3(x,y,z);
            new_verts[i] = new_vert;
        }
        mf.mesh.vertices = new_verts;
        ReCalcTriangles(mf.mesh);

        Color[] colors = new Color[mf.mesh.vertices.Length];
        for (int c = 0; c < mf.mesh.triangles.Length; c += 3)
        {
            float height = (mf.mesh.vertices[c].z + mf.mesh.vertices[c + 1].z + mf.mesh.vertices[c + 2].z) / 3;

            // colors[i] = environmentMapper.colorAtPos(xpos,vertices[c].y,ypos)
            Color color;
            if (height > 10)
                color = new Color(0.9f, 0.9f, 0.9f);
            else if (height > -30)
                color = new Color(0.1f, 0.4f, 0.2f);
            else
                color = new Color(0.7f, 0.7f, 0.3f);
            colors[c] = color;
            colors[c + 1] = color;
            colors[c + 2] = color;
        }
        mf.mesh.colors = colors;
        GetComponent<MeshCollider>().sharedMesh = mf.mesh;
    }
    private void meshBlendToUnityRot()
    {
        for (int i = 0; i < mf.mesh.vertices.Length; i++)
        {
            mf.mesh.vertices[i] = new Vector3(mf.mesh.vertices[i].x, mf.mesh.vertices[i].z, mf.mesh.vertices[i].y);
        }
    }
    private void ReCalcTriangles(Mesh mesh)
    {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];


        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];

            triangles[i] = i;

        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
