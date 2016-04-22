using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkGenerator : MonoBehaviour {
    public Material landMaterial;
    public float XZDeviationRatio; //only deviates positively (sadly)
    public int XZDeviationSeed;
    public float detailDeviation;
    public int detailDeviationSeed;
    public int detailSubdivisions;

    private GameObject TerrainParent;

    //references
    private NoiseSynth synth;
    private GenerationManager genManager;
    private int seed;

    //finals
    private float chunk_size;
    private int chunk_resolution;

    private void Awake () {
        synth = GetComponent<NoiseSynth>();
        genManager = GetComponent<GenerationManager>();
        seed = GetComponent<Seed>().seed;
        TerrainParent = new GameObject("Terrain");
        TerrainParent.transform.parent = transform;
        chunk_size = genManager.chunk_size;
        chunk_resolution = genManager.chunk_resolution;
        synth.Init();
	}

	public GameObject generate (Vector2 coordinates) {
        GameObject chunk = new GameObject();
        chunk.layer = LayerMask.NameToLayer("Terrain");
        chunk.name = "chunk (" + coordinates.x + "," + coordinates.y + ")";
        ChunkMeshes chunkMeshes = chunk.AddComponent<ChunkMeshes>();
        chunk.transform.parent = TerrainParent.transform;
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        mr.material = landMaterial;
        MeshFilter mf = chunk.AddComponent<MeshFilter>();

		chunkMeshes.lowMesh = new Mesh();
        chunkMeshes.lowMesh.name = "chunk (" + coordinates.x + ","+ coordinates.y + ") [l]";

        Vector3 pos = new Vector3(coordinates.x * chunk_size, 0, coordinates.y * chunk_size);
        chunk.transform.position = pos;
        
        // Generate chunk_resolution^2 vertices
		Vector3[] vertices = new Vector3[(chunk_resolution*chunk_resolution)];

        int originalSeed = Random.seed;
        for (int iy = 0; iy < chunk_resolution; iy++) {
			for (int ix = 0; ix < chunk_resolution; ix++) {
                float x = ix * chunk_size / (chunk_resolution - 1);
                float y = iy * chunk_size / (chunk_resolution - 1);
                float origx = x; float origy = y;
                if(XZDeviationRatio != 0){
                    Random.seed = seed + XZDeviationSeed + ((origx + coordinates.x * chunk_size).ToString() + "," + (origy + coordinates.y * chunk_size).ToString()).GetHashCode();
                    x = (ix + Random.value * XZDeviationRatio) * chunk_size / (chunk_resolution - 1);
                    y = (iy + Random.value * XZDeviationRatio) * chunk_size / (chunk_resolution - 1);
                }
                // vertices[iy * chunk_resolution + ix] = EniromentMapper.heightAtPos(xpos,ypos);
                vertices[iy * chunk_resolution + ix] = new Vector3(x, synth.heightAt(origx + chunk.transform.position.x, origy + chunk.transform.position.z, 0), y); // replace 0 with Globals.time eventually
            }
        }
        Random.seed = originalSeed;

        chunkMeshes.lowMesh.vertices = vertices;

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
                    triangles[i] = v2;
                    triangles[i + 1] = v3;
                    triangles[i + 2] = v4;
                    triangles[i + 3] = v3;
                    triangles[i + 4] = v2;
                    triangles[i + 5] = v1;
                }

                i += 6;
            }
        }
		chunkMeshes.lowMesh.triangles = triangles;
        ReCalcTriangles(chunkMeshes.lowMesh);

        chunkMeshes.highMesh = Instantiate(chunkMeshes.lowMesh);
        chunkMeshes.highMesh.name = "chunk (" + coordinates.x + "," + coordinates.y + ") [h]";
        subDivide(chunkMeshes.highMesh, coordinates, detailSubdivisions);

        mf.mesh = chunkMeshes.lowMesh;
        chunkMeshes.mf = mf;

        MeshCollider collider = (MeshCollider)chunk.AddComponent(typeof(MeshCollider));
        collider.sharedMesh = chunkMeshes.highMesh;

        return chunk;
	}

    // Calculate vertex colors
    public void colorChunk(GameObject chunkObj, float chunk_size){
        ChunkMeshes cm = chunkObj.GetComponent<ChunkMeshes>();
        colorMesh(chunkObj, cm.lowMesh, chunk_size);
        colorMesh(chunkObj, cm.highMesh, chunk_size);
    }

    public void colorMesh(GameObject chunkObj, Mesh mesh, float chunk_size) {
        Vector2 chunk = GenerationManager.worldToChunk(chunkObj.transform.position);
        Biome curBiome = genManager.chooseBiome(chunk);
        Biome up = genManager.chooseBiome(chunk + Vector2.up);
        Biome down = genManager.chooseBiome(chunk + Vector2.down);
        Biome left = genManager.chooseBiome(chunk + Vector2.left);
        Biome right = genManager.chooseBiome(chunk + Vector2.right);

        Vector3[] verts = mesh.vertices;
        Color[] colors = new Color[verts.Length];
        for(int c = 0; c < verts.Length; c += 3) {
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

            if(h > 0.5)
                hcolor = Color.Lerp(biome_color, right_color, 0.5f * (h - 0.5f));
            else
                hcolor = Color.Lerp(left_color, biome_color, 0.5f * h);
            if(v > 0.5)
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
        mesh.colors = colors;
    }

    public void refresh(GameObject chunk){
        /*Vector3[] verts = chunk.GetComponent<MeshFilter>().mesh.vertices;
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
        */
        //update heights
        colorChunk(chunk, chunk_size);
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

    //assumes no shared vertices and returns no shared vertices
    private void subDivide(Mesh mesh, Vector2 coordinates, int numOfDivisions) {
        if(numOfDivisions < 1) return;
        Vector3[] oldVerts = mesh.vertices;
        Vector3[] vertices = new Vector3[oldVerts.Length * 4];
        int[] triangles = new int[oldVerts.Length * 4];

        int originalSeed = Random.seed;
        for (int i = 0; i < oldVerts.Length; i += 3) {
            Vector3 hypotMid = Vector3.Lerp(oldVerts[i], oldVerts[i + 1], 0.5f);
            Random.seed = seed + detailDeviationSeed + ((hypotMid.x + coordinates.x * chunk_size).ToString() + "," + (hypotMid.z + coordinates.y * chunk_size).ToString()).GetHashCode();
            hypotMid = new Vector3(hypotMid.x + Random.Range(-detailDeviation, detailDeviation), hypotMid.y + Random.Range(-detailDeviation, detailDeviation), hypotMid.z + Random.Range(-detailDeviation, detailDeviation));
            Vector3 midpoint1 = Vector3.Lerp(oldVerts[i+1], oldVerts[i+2], 0.5f);
            Vector3 midpoint2 = Vector3.Lerp(oldVerts[i+2], oldVerts[i], 0.5f);
            vertices[i * 4    ] = hypotMid;
            vertices[i * 4 + 1] = oldVerts[i + 1];
            vertices[i * 4 + 2] = midpoint1;

            vertices[i * 4 + 3] = oldVerts[i + 2];
            vertices[i * 4 + 4] = hypotMid;
            vertices[i * 4 + 5] = midpoint1;

            vertices[i * 4 + 6] = hypotMid;
            vertices[i * 4 + 7] = oldVerts[i + 2];
            vertices[i * 4 + 8] = midpoint2;

            vertices[i * 4 + 9] = oldVerts[i];
            vertices[i * 4 +10] = hypotMid;
            vertices[i * 4 +11] = midpoint2;
        }
        Random.seed = originalSeed;
        for(int i = 0; i < triangles.Length; i++) triangles[i] = i;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        if(numOfDivisions == 1) {
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }else subDivide(mesh, coordinates, numOfDivisions - 1);
    }
}