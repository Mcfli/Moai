using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkGenerator : MonoBehaviour {
    public Material landMaterial;
    public NoiseGen DeviationMap;
    public float XZDeviationRatio = 0.5f;
    
    private GameObject TerrainParent;

    //references
    private NoiseSynth synth;
    private GenerationManager genManager;

    private void Awake () {
        synth = GetComponent<NoiseSynth>();
        genManager = GetComponent<GenerationManager>();
        TerrainParent = new GameObject("Terrain");
        TerrainParent.transform.parent = transform;
        synth.Init();
	}

	public GameObject generate (Vector2 coordinates, float chunk_size, int chunk_resolution) {
        GameObject chunk = new GameObject();
        chunk.layer = LayerMask.NameToLayer("Terrain");
        chunk.name = "chunk (" + coordinates.x + "," + coordinates.y + ")";
        chunk.transform.parent = TerrainParent.transform;
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        mr.material = landMaterial;
        MeshFilter mf = chunk.AddComponent<MeshFilter>();
		mf.mesh = new Mesh();
		mf.mesh.name = "chunk (" + coordinates.x + ","+ coordinates.y + ")";

        Vector3 pos = new Vector3(coordinates.x * chunk_size, 0, coordinates.y * chunk_size);
        chunk.transform.position = pos;
        
        // Generate chunk_resolution^2 vertices
		Vector3[] vertices = new Vector3[(chunk_resolution*chunk_resolution)];
        
        for (int iy = 0; iy < chunk_resolution; iy++) {
			for (int ix = 0; ix < chunk_resolution; ix++) {
                float x = ix * chunk_size / (chunk_resolution - 1);
                float y = iy * chunk_size / (chunk_resolution - 1);
                float origx = x; float origy = y;
                if(XZDeviationRatio != 0) {
                    x += Mathf.Repeat(DeviationMap.genPerlin((origx + coordinates.x * chunk_size) * 2, (origy + coordinates.y * chunk_size) * 2+1, 0), XZDeviationRatio); // replace 0 with Globals.time eventually
                    y += Mathf.Repeat(DeviationMap.genPerlin((origx + coordinates.x * chunk_size) * 2+1, (origy + coordinates.y * chunk_size) * 2, 0), XZDeviationRatio); // replace 0 with Globals.time eventually
                    //x = (ix + Random.Range(-XZDeviationRatio, XZDeviationRatio)) * chunk_size / (chunk_resolution - 1); // problem with this is that when the chunks are regenerated,
                    //y = (iy + Random.Range(-XZDeviationRatio, XZDeviationRatio)) * chunk_size / (chunk_resolution - 1); // the deviations are randomized again; also, the chunks don't line up
                }
                // vertices[iy * chunk_resolution + ix] = EniromentMapper.heightAtPos(xpos,ypos);
                vertices[iy * chunk_resolution + ix] = new Vector3(x, synth.heightAt(origx + chunk.transform.position.x, origy + chunk.transform.position.z, 0), y); // replace 0 with Globals.time eventually

            }
		}

		mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(chunk_resolution-1)*(chunk_resolution-1) * 6];
        int i = 0;
        // iterate through each quad in vertices
		for(int y = 0; y < chunk_resolution-1; y++){
            for(int x = 0; x < chunk_resolution-1; x++){
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
                if(Mathf.Repeat(x + y, 2) == 1) { //top left to bottom right
                    triangles[i] = v4;
                    triangles[i + 1] = v1;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v1;
                    triangles[i + 4] = v4;
                    triangles[i + 5] = v2;
                } else { //top right to bottom left
                    triangles[i] = v4;
                    triangles[i + 1] = v2;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v2;
                    triangles[i + 4] = v1;
                    triangles[i + 5] = v3;
                }

                i += 6;
            }
        }
		mf.mesh.triangles = triangles;
        ReCalcTriangles(mf.mesh);

        chunk.AddComponent(typeof(MeshCollider));

        return chunk;
	}

    // Calculate vertex colors
    public void colorChunk(GameObject chunkObj, float chunk_size)
    {
        Vector2 chunk = genManager.worldToChunk(chunkObj.transform.position);
        Biome curBiome = genManager.chooseBiome(chunk);
        Biome up = genManager.chooseBiome(chunk + Vector2.up);
        Biome down = genManager.chooseBiome(chunk + Vector2.down);
        Biome left = genManager.chooseBiome(chunk + Vector2.left);
        Biome right = genManager.chooseBiome(chunk + Vector2.right);

        Vector3[] verts = chunkObj.GetComponent<MeshFilter>().mesh.vertices;
        Color[] colors = new Color[verts.Length];
        for (int c = 0; c < verts.Length; c += 3)
        {
            float height = (verts[c].y + verts[c + 1].y + verts[c + 2].y) / 3;
            float h = verts[c].x / chunk_size;
            float v = verts[c].z / chunk_size;

            h = Mathf.Sqrt(h);
            v = Mathf.Sqrt(v);

            Color biome_color = curBiome.colorAt(height);
            Color left_color = left.colorAt(height);
            Color right_color = right.colorAt(height);
            Color up_color = up.colorAt(height);
            Color down_color = down.colorAt(height);

            Color color = biome_color;
            Color hcolor, vcolor;

            if (h > 0.5)
                hcolor = Color.Lerp(biome_color,right_color,0.5f*(h-0.5f));
            else
                hcolor = Color.Lerp(left_color,biome_color,  0.5f*h);
            if (v > 0.5)
                vcolor = Color.Lerp(biome_color, up_color, 0.5f * (v - 0.5f));
            else
                vcolor = Color.Lerp(down_color, biome_color, 0.5f * v);

            float hm = Mathf.Abs(h - 0.5f);
            float vm = Mathf.Abs(v - 0.5f);
            float interp = Mathf.Max(vm / (hm + vm), 0f);

            color = Color.Lerp(hcolor, vcolor, interp);

            colors[c] = color;
            colors[c + 1] = color;
            colors[c + 2] = color;
        }
        chunkObj.GetComponent<MeshFilter>().mesh.colors = colors;
    }

    public void refresh(GameObject chunk)
    {
        Vector3[] verts = chunk.GetComponent<MeshFilter>().mesh.vertices;
        for (int j = 0; j < verts.Length; j++)
        {
            float x = verts[j].x;
            float y = verts[j].z;
            float xpos = chunk.transform.position.x + x;
            float ypos = chunk.transform.position.z + y;

            verts[j] = new Vector3(x, synth.heightAt(xpos, ypos, 0), y); //replace 0 with Globals.time eventually
        }
        chunk.GetComponent<MeshFilter>().mesh.vertices = verts;
        chunk.GetComponent<MeshCollider>().sharedMesh = chunk.GetComponent<MeshFilter>().mesh;

        Color[] colors = new Color[verts.Length];
        for (int c = 0; c < verts.Length; c += 3)
        {
            float height = (verts[c].y + verts[c + 1].y + verts[c + 2].y) / 3;

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
        chunk.GetComponent<MeshFilter>().mesh.colors = colors;
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