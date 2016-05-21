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

   

    //references
    private NoiseSynth synth;
    private GenerationManager genManager;
    //private WaterManager waterManager;

    //finals
    private float chunk_size;

    private void Awake () {
        synth = GetComponent<NoiseSynth>();
        genManager = GetComponent<GenerationManager>();
        //waterManager = GetComponent<WaterManager>();
        
        chunk_size = genManager.chunk_size;
        synth.Init();
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

    
}