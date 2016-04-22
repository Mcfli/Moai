using UnityEngine;
using System.Collections;

public class WaterBody : MonoBehaviour {

    public Vector3 size;
    public Vector3 center;
    public Biome biome;

    private Mesh mesh;


	// Use this for initialization
	void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // Returns whether this body overlaps another
    public bool overlaps(WaterBody other)
    {
        float deltaX = Mathf.Abs(center.x - other.center.x);
        float deltaY = Mathf.Abs(center.z - other.center.z);

        if (deltaX < size.x + other.size.x || deltaY < size.x + other.size.x)
            return true;
        return false;
    }

    /*
    private void updateVertices(Vector3 waveHeight, Vector3 waveSpeed, Vector3 waveLength, Vector3 waveOffset)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        float baseX = transform.position.x;
        float baseY = transform.position.y;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            vertex.y = Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.x) * waveSpeed.x + (baseX + vertex.x) * waveLength.x) * waveHeight.x;
            vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.y) * waveSpeed.y + ((chunkCoordinates.x + chunkCoordinates.y) * chunk_size + vertex.x + vertex.z) * waveLength.y) * waveHeight.y;
            vertex.y += Mathf.Sin((Globals.time / Globals.time_resolution + waveOffset.z) * waveSpeed.z + (chunkCoordinates.y * chunk_size + vertex.z) * waveLength.z) * waveHeight.z;
            if (Mathf.Repeat(Mathf.Round((vertex.x + vertex.z) / chunk_size * (resolution - 1)) + chunkCoordinates.x + chunkCoordinates.y, 2) == 1 && invertEveryOther) vertex.y *= -1;
            vertices[i] = vertex;
            if (Mathf.Repeat(i, 3) == 2)
            {
                Vector3 side1 = vertices[i - 2] - vertices[i];
                Vector3 side2 = vertices[i - 1] - vertices[i];
                Vector3 perp = Vector3.Cross(side1, side2);
                normals[i] = perp.normalized;
                normals[i - 1] = perp.normalized;
                normals[i - 2] = perp.normalized;
            }
        }
        m.vertices = vertices;
        m.normals = normals;
    }*/


}
