using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkMeshes : MonoBehaviour{
    public Mesh lowMesh;
    public Mesh highMesh;
    public MeshFilter mf;

    //
    public Vector2 coordinates;

    // Generation Checkpoints 
    public bool doneBase = false;
    public bool doneObjects = false;
    public bool loadingBase = false;
    public bool loadingObjects = false;
    public bool unloadedBase = false;
    public bool unloadedObjects = false;
    public bool detailed = false;
    public bool meshGenerated = false;
    public bool colliderGenerated = false;
    public bool treesGenerated = false;
    public bool doodadsGenerated = false;
    public bool shrinesGenerated = false;
    public bool obelisksGenerated = false;
    public bool lakesGenerated = false;

    //references
    private NoiseSynth synth;
    private GenerationManager genManager;
    private ChunkGenerator chunkGen;
    private TreeManager treeManager;
    private ShrineManager shrineManager;
    private DoodadManager doodadManager;
    private WaterManager waterManager;

    // private
    private List<lakeStruct> lakes;
    private Biome biome;

    void Update()
    {
        if (loadingBase)
        {
            loadBase();
        }
        else if (loadingObjects)
        {
            loadObjects();
        }
    }

    public void setReferences(NoiseSynth sy,GenerationManager g, ChunkGenerator c, TreeManager t,ShrineManager s,DoodadManager d,WaterManager w)
    {
        synth = sy;
        genManager = g;
        chunkGen = c;
        treeManager = t;
        shrineManager = s;
        doodadManager = d;
        waterManager = w;
    }

    public void loadBase()
    {
        //if (System.DateTime.Now >= genManager.endTime) return;
        unloadedBase = false;
        loadingBase = true;
        if (!meshGenerated) generateMesh();
        if (meshGenerated && !colliderGenerated) generateCollider();
        if (meshGenerated && colliderGenerated && !lakesGenerated) generateLakes();
        if (meshGenerated && colliderGenerated && lakesGenerated && !obelisksGenerated) generateObelisks();
        if (meshGenerated && colliderGenerated && lakesGenerated && obelisksGenerated)
        {
            doneBase = true;
            loadingBase = false;
        }
    }

    public void loadObjects()
    {
        loadingObjects = true;
        if (System.DateTime.Now >= genManager.endTime) return;
        if (loadingBase) return;
       // unloadedObjects = false;
        
        if (!doodadsGenerated) generateDoodads();
        if (doodadsGenerated && !treesGenerated) generateTrees();
        if (doodadsGenerated && treesGenerated && !shrinesGenerated) generateShrines();
        if (doodadsGenerated && treesGenerated && shrinesGenerated)
        {
            unloadedObjects = false;
            doneObjects = true;
            loadingObjects = false;
        }
    }

    public void unloadBase()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        if (lakesGenerated) unloadLakes();
        if (!lakesGenerated && obelisksGenerated) unloadObelisks();
        if (!lakesGenerated && !obelisksGenerated)
        {
            unloadedBase = true;
            doneBase = false;
        }
    }

    public void unloadObjects()
    {
        if (unloadedObjects || System.DateTime.Now >= genManager.endTime) return;
        if (treesGenerated) unloadTrees();
        if (!treesGenerated && doodadsGenerated) unloadDoodads();
        if (!treesGenerated && !doodadsGenerated && shrinesGenerated) unloadShrines();
        if (!treesGenerated && !doodadsGenerated && !shrinesGenerated)
        {
            unloadedObjects = true;
            doneObjects = false;
        }
    }

    void generateMesh()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        biome = genManager.chooseBiome(coordinates);
        lakes = new List<lakeStruct>();

        lowMesh = new Mesh();
        lowMesh.name = "chunk (" + coordinates.x + "," + coordinates.y + ") [l]";

        Vector3 pos = new Vector3(coordinates.x * genManager.chunk_size, 0, coordinates.y * genManager.chunk_size);
        transform.position = pos;

        int originalSeed = Random.seed;

        // Generate lakes for this chunk
        Random.seed = Globals.SeedScript.seed + new Vector3((int)pos.x, (int)pos.y, (int)pos.z).ToString().GetHashCode();

        for (int il = 0; il < biome.lakeCount; il++)
        {
            if (Random.value < biome.lakeChance)
            {
                Vector3 lakeSize = biome.lakeSize;
                Vector3 lakePos = pos;
                lakePos.x += Random.Range(0, genManager.chunk_size);
                lakePos.z += Random.Range(0, genManager.chunk_size);
                lakePos.x = Mathf.Clamp(lakePos.x, pos.x + lakeSize.x + 20, pos.x + genManager.chunk_size - lakeSize.x - 20);
                lakePos.z = Mathf.Clamp(lakePos.z, pos.z + lakeSize.z + 20, pos.z + genManager.chunk_size - lakeSize.z - 20);

                lakes.Add(new lakeStruct(lakePos, lakeSize));
            }
        }
        Random.seed = originalSeed;

        // Generate chunk_resolution^2 vertices
        Vector3[] vertices = new Vector3[(genManager.chunk_resolution * genManager.chunk_resolution)];

        for (int iy = 0; iy < genManager.chunk_resolution; iy++)
        {
            for (int ix = 0; ix < genManager.chunk_resolution; ix++)
            {
                float x = ix * genManager.chunk_size / (genManager.chunk_resolution - 1);
                float y = iy * genManager.chunk_size / (genManager.chunk_resolution - 1);
                float origx = x; float origy = y;

                float lakeOffset = 0;
                int lakeIntersections = 0;

                if (genManager.XZDeviationRatio != 0)
                {
                    Random.seed = Globals.SeedScript.seed + genManager.XZDeviationSeed + 
                        ((origx + coordinates.x * genManager.chunk_size).ToString() + "," + 
                        (origy + coordinates.y * genManager.chunk_size).ToString()).GetHashCode();
                    x = (ix + Random.value * genManager.XZDeviationRatio) *
                        genManager.chunk_size / (genManager.chunk_resolution - 1);
                    y = (iy + Random.value * genManager.XZDeviationRatio) *
                        genManager.chunk_size / (genManager.chunk_resolution - 1);
                }

                float xpos = x + pos.x;
                float zpos = y + pos.z;
                Vector3 vertexWorld = new Vector3(xpos, 0, zpos);

                // Take lakes into account when determining height,
                // BUt only if the vertex is not on the edge
                if (ix != 0 && iy != 0 && ix != genManager.chunk_resolution - 1 &&
                    iy != genManager.chunk_resolution - 1)
                {
                    foreach (lakeStruct lake in lakes)
                    {
                        // See if this vertex is in range of this lake
                        if (Vector3.Distance(vertexWorld, lake.position) <= lake.size.x)
                        {
                            lakeOffset += lake.size.y * Mathf.Max(0, Mathf.Pow((1 - (Vector3.Distance(vertexWorld, lake.position) / lake.size.x)), 3));
                            lakeIntersections++;
                        }
                    }
                }

                vertices[iy * genManager.chunk_resolution + ix] = 
                    new Vector3(x, synth.heightAt(origx + transform.position.x, origy + transform.position.z, 0) - lakeOffset, y);
            }
        }
        Random.seed = originalSeed;

        lowMesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(genManager.chunk_resolution - 1) * (genManager.chunk_resolution - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < genManager.chunk_resolution - 1; y++)
        {
            for (int x = 0; x < genManager.chunk_resolution - 1; x++)
            {
                // Specify quad edges as vertex indices
                //v1 ________ v2
                //  |        |
                //  |        |
                //  |        |
                //v3 _________  v4
                int v1 = x + y * genManager.chunk_resolution;
                int v2 = (x + 1) + y * genManager.chunk_resolution;
                int v3 = x + (y + 1) * genManager.chunk_resolution;
                int v4 = (x + 1) + (y + 1) * genManager.chunk_resolution;

                // Create two triangles from the quad
                if (Mathf.Repeat(x + y, 2) == 1)
                { //top left to bottom right
                    triangles[i] = v4;
                    triangles[i + 1] = v1;
                    triangles[i + 2] = v3;
                    triangles[i + 3] = v1;
                    triangles[i + 4] = v4;
                    triangles[i + 5] = v2;
                }
                else
                { //top right to bottom left
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
        lowMesh.triangles = triangles;
        ReCalcTriangles(lowMesh);

        highMesh = Instantiate(lowMesh);
        highMesh.name = "chunk (" + coordinates.x + "," + coordinates.y + ") [h]";
        subDivide(highMesh, coordinates, chunkGen.detailSubdivisions);

        mf.mesh = lowMesh;
        meshGenerated = true;
        

        colorMesh(lowMesh);
        colorMesh(highMesh);
    }


    void generateCollider()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        MeshCollider collider = (MeshCollider)gameObject.AddComponent(typeof(MeshCollider));
        collider.sharedMesh = highMesh;
        
        colliderGenerated = true;
    }

    void generateTrees()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        treeManager.loadTrees(coordinates, biome);
          
        treesGenerated = true;
    }

    void generateDoodads()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        doodadManager.loadDoodads(coordinates, biome);
        doodadsGenerated = true;
    }

    void generateShrines()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        shrineManager.loadShrines((int)coordinates.x, (int)coordinates.y);
        shrinesGenerated = true;
    }

    void generateObelisks()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        shrineManager.loadObelisks((int)coordinates.x, (int)coordinates.y);
        obelisksGenerated = true;
    }

    void generateLakes()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        foreach (lakeStruct lake in lakes)
        {
            waterManager.createWater(coordinates, lake.position, lake.size, biome);
        }
        lakesGenerated = true;
    }

    void unloadMesh()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        meshGenerated = false;
    }    
         
    void unloadTrees()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        treeManager.unloadTrees(coordinates);
        treesGenerated = false;
    }    
         
    void unloadDoodads()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        doodadManager.unloadDoodads(coordinates);
        doodadsGenerated = false;
    }

    void unloadShrines()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        shrineManager.unloadShrines((int)coordinates.x, (int)coordinates.y);
        shrinesGenerated = false;
    }
    void unloadObelisks()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        shrineManager.unloadObelisks((int)coordinates.x, (int)coordinates.y);
        obelisksGenerated = false;
    }    
         
    void unloadLakes()
    {
        if (System.DateTime.Now >= genManager.endTime) return;
        waterManager.unloadWater(coordinates);
        lakesGenerated = false;
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

    //assumes no shared vertices and returns no shared vertices
    private void subDivide(Mesh mesh, Vector2 coordinates, int numOfDivisions)
    {
        if (numOfDivisions < 1) return;
        Vector3[] oldVerts = mesh.vertices;
        Vector3[] vertices = new Vector3[oldVerts.Length * 4];
        int[] triangles = new int[oldVerts.Length * 4];

        int originalSeed = Random.seed;
        for (int i = 0; i < oldVerts.Length; i += 3)
        {
            Vector3 hypotMid = Vector3.Lerp(oldVerts[i], oldVerts[i + 1], 0.5f);
            Random.seed = Globals.SeedScript.seed + chunkGen.detailDeviationSeed + ((hypotMid.x + coordinates.x * genManager.chunk_size).ToString() + ","
                + (hypotMid.z + coordinates.y * genManager.chunk_size).ToString()).GetHashCode();
            hypotMid = new Vector3(hypotMid.x + Random.Range(-chunkGen.detailDeviation, chunkGen.detailDeviation), 
                hypotMid.y + Random.Range(-chunkGen.detailDeviation, chunkGen.detailDeviation), 
                hypotMid.z + Random.Range(-chunkGen.detailDeviation, chunkGen.detailDeviation));
            Vector3 midpoint1 = Vector3.Lerp(oldVerts[i + 1], oldVerts[i + 2], 0.5f);
            Vector3 midpoint2 = Vector3.Lerp(oldVerts[i + 2], oldVerts[i], 0.5f);
            vertices[i * 4] = hypotMid;
            vertices[i * 4 + 1] = oldVerts[i + 1];
            vertices[i * 4 + 2] = midpoint1;

            vertices[i * 4 + 3] = oldVerts[i + 2];
            vertices[i * 4 + 4] = hypotMid;
            vertices[i * 4 + 5] = midpoint1;

            vertices[i * 4 + 6] = hypotMid;
            vertices[i * 4 + 7] = oldVerts[i + 2];
            vertices[i * 4 + 8] = midpoint2;

            vertices[i * 4 + 9] = oldVerts[i];
            vertices[i * 4 + 10] = hypotMid;
            vertices[i * 4 + 11] = midpoint2;
        }
        Random.seed = originalSeed;
        for (int i = 0; i < triangles.Length; i++) triangles[i] = i;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        if (numOfDivisions == 1)
        {
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
        else subDivide(mesh, coordinates, numOfDivisions - 1);
    }

    public void colorMesh(Mesh mesh)
    {
        Vector2 chunk = GenerationManager.worldToChunk(transform.position);
        Biome curBiome = biome;
        Biome up = genManager.chooseBiome(chunk + Vector2.up);
        Biome down = genManager.chooseBiome(chunk + Vector2.down);
        Biome left = genManager.chooseBiome(chunk + Vector2.left);
        Biome right = genManager.chooseBiome(chunk + Vector2.right);

        Vector3[] verts = mesh.vertices;
        Color[] colors = new Color[verts.Length];
        for (int c = 0; c < verts.Length; c += 3)
        {
            float height = (verts[c].y + verts[c + 1].y + verts[c + 2].y) / 3;
            float h = verts[c].x / genManager.chunk_size;
            float v = verts[c].z / genManager.chunk_size;

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
                hcolor = Color.Lerp(biome_color, right_color, 0.5f * (h - 0.5f));
            else
                hcolor = Color.Lerp(left_color, biome_color, 0.5f * h);
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
        mesh.colors = colors;
    }

    private struct lakeStruct
    {
        public Vector3 position;
        public Vector3 size;

        public lakeStruct(Vector3 pos, Vector3 siz)
        {
            position = pos;
            size = siz;
        }
    }
}
