using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

    public float chunk_size = 10;           // The size of each chunk in world coordinates
    public int chunk_resolution = 10;     // The number of vertices on one side if the chunk
    public Material landMaterial;
	private Vector3[] vertices;
    

    private void Awake () {
	}

	public void generate (int chunk_x,int chunk_y, float amplitude) {
        GameObject chunk = new GameObject();
        chunk.name = "chunk (" + chunk_x + "," + chunk_y + ")";
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        mr.material = landMaterial;
        MeshFilter mf = chunk.AddComponent<MeshFilter>();
		mf.mesh = new Mesh();
		mf.mesh.name = "chunk (" + chunk_x+","+chunk_y+")";

        Vector3 pos = new Vector3(chunk_x * chunk_size, 0, chunk_y * chunk_size);

        chunk.transform.position = pos;
        
        // Generate chunk_resolution^2 vertices
		vertices = new Vector3[(chunk_resolution*chunk_resolution)];
        
        for (int iy = 0; iy < chunk_resolution; iy++) {
			for (int ix = 0; ix < chunk_resolution; ix++) {
                float x = ix * chunk_size/(chunk_resolution-1);
                float y = iy * chunk_size / (chunk_resolution-1);
                float xpos = chunk.transform.position.x + x;
                float ypos = chunk.transform.position.z + y;

                // vertices[iy * chunk_resolution + ix] = EniromentMapper.heightAtPos(xpos,ypos);
                vertices[iy * chunk_resolution + ix] = new Vector3(x, amplitude * NoiseGen.genPerlin(xpos, ypos), y);

            }
		}


		mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(chunk_resolution-1)*(chunk_resolution-1) * 6];
        int i = 0;
        // iterate through each quad in vertices
		for(int y = 0; y < chunk_resolution-1; y++)
        {
            for(int x = 0; x < chunk_resolution-1; x++)
            {
                // Specify quad edges as vertex indices
                //v1 ________ v2
                //  |        |
                //  |        |
                //  |        |
                //v3 _________  v4
                int v1 = x + y * chunk_resolution;
                int v2 = (x + 1) + y * chunk_resolution;
                int v3 = x + (y+1) * chunk_resolution;
                int v4 = (x+1) + (y + 1) * chunk_resolution;

                // Create two triangles from the quad
                triangles[i] = v4;
                triangles[i + 1] = v1;
                triangles[i + 2] = v3;
                triangles[i + 3] = v1;
                triangles[i + 4] = v4;
                triangles[i + 5] = v2;

                i += 6;
            }
        }
		mf.mesh.triangles = triangles;
        ReCalcTriangles(mf.mesh);

        // Calculate vertex colors

        Color[] colors = new Color[mf.mesh.vertices.Length];
        for (int c = 0; c < mf.mesh.triangles.Length; c+=3)
        {
            float height = (mf.mesh.vertices[c].y+ mf.mesh.vertices[c+1].y+ mf.mesh.vertices[c+2].y)/3;

            // colors[i] = environmentMapper.colorAtPos(xpos,vertices[c].y,ypos)
            Color color;
            if (height > 80)
                color = new Color(0.9f, 0.9f, 0.9f);
            else if (height > -150)
                color = new Color(0.1f,0.4f,0.2f);
            else
                color = new Color(0.7f, 0.7f, 0.3f);
            colors[c] = color;
            colors[c+1] = color;
            colors[c+2] = color;
        }
        mf.mesh.colors = colors;
        MeshCollider meshc = chunk.AddComponent(typeof(MeshCollider)) as MeshCollider;
	}

    private void ReCalcTriangles(Mesh mesh) {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];


        for (int i = 0; i < triangles.Length; i++){
            vertices[i] = oldVerts[triangles[i]];
            
            triangles[i] = i;
            
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}