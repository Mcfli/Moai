using UnityEngine;
using System.Collections;

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
        mf = GetComponent<MeshFilter>();
        Vector3[] new_verts;
        int o_chunk_res = Mathf.FloorToInt(Mathf.Sqrt(mf.mesh.vertexCount));
        float o_chunk_size = Vector3.Distance(mf.mesh.vertices[o_chunk_res], mf.mesh.vertices[0]);
        float o_step_size = o_chunk_size / o_chunk_res;

        Debug.Log(o_chunk_size);

        new_verts = new Vector3[mf.mesh.vertices.Length];

        for(int i = 0; i < mf.mesh.vertices.Length; i++)
        {
            float x = mf.mesh.vertices[i].x;
            float y = mf.mesh.vertices[i].y;
            float z = mf.mesh.vertices[i].z;

            x *= chunk_size/o_chunk_size;
            y *= chunk_size / o_chunk_size;

            Vector3 new_vert = new Vector3(x,y,z);
            new_verts[i] = new_vert;
        }
        mf.mesh.vertices = new_verts;
        GetComponent<MeshCollider>().sharedMesh = mf.mesh;
    }
}
