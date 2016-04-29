using UnityEngine;
using System.Collections;

public class WaterBody : MonoBehaviour {

    public Vector3 size;
    public Vector3 center;
    public Biome biome;
    public float waterResolution;

    //private Mesh mesh;
    private float stepLength = 0.1f;
    private int terrain;
    private int maxTries = 2;
    private int maxRays = 1;
    private bool settled = false;
    private bool removedForests = false;
    private bool expanded = false;
    private bool setBelow = false;
    

    private int edgeIndex = 0; // keeps track of where in the search of vertices for
                                // setting the edges below the terrain

    // References
    private MeshFilter mf;

	// Use this for initialization
	void Start () {
        //mesh = GetComponent<MeshFilter>().mesh;
        terrain = LayerMask.GetMask("Terrain");
        center.y = Mathf.Infinity;
        mf = GetComponent<MeshFilter>();
        size = Vector3.right + Vector3.forward;
    }
	
	// Update is called once per frame
	void Update () {
        if (!settled || !expanded ||!setBelow) fitToTerrain();
	}

    // Returns whether this body overlaps another
    public bool overlaps(WaterBody other)
    {
        if (Vector3.Distance(center,other.center)<size.x+other.size.x)
            return true;
        return false;
    }

    public void fitToTerrain()
    {
        // Stick water to ground
        if (!settled)
            moveToLocalMinimum();
        else if (!expanded)
            moveToMaxFillHeight();
        else if (!removedForests)
            destroyOverlappingTrees();
        else if (!setBelow)
            moveCornersDown();
    }

    // Drops the water body to the lwoest nearby point
    private void moveToLocalMinimum()
    {
        float minHeight = -1;
        int tries = 0;
        RaycastHit hit;
        Vector3 topLeft = new Vector3(center.x - stepLength, 10000000, center.z - stepLength);
        Vector3 topRight = new Vector3(center.x + stepLength, 10000000, center.z - stepLength);
        Vector3 bottomLeft = new Vector3(center.x - stepLength, 10000000, center.z + stepLength);
        Vector3 bottomRight = new Vector3(center.x + stepLength, 10000000, center.z + stepLength);


        while(tries < maxTries)
        {
            // Cast rays around current center
            topLeft = new Vector3(center.x - stepLength, 10000000, center.z - stepLength);
            topRight = new Vector3(center.x + stepLength, 10000000, center.z - stepLength);
            bottomLeft = new Vector3(center.x - stepLength, 10000000, center.z + stepLength);
            bottomRight = new Vector3(center.x + stepLength, 10000000, center.z + stepLength);

            Ray rayTopLeft      = new Ray(topLeft, Vector3.down);
            Ray rayTopRight     = new Ray(topRight, Vector3.down);
            Ray rayBottomLeft   = new Ray(bottomLeft, Vector3.down);
            Ray rayBottomRight  = new Ray(bottomRight, Vector3.down);

            if (Physics.Raycast(rayTopLeft, out hit, Mathf.Infinity, terrain))
            {
                topLeft = hit.point;
            }
                
            if (Physics.Raycast(rayTopRight, out hit, Mathf.Infinity, terrain))
            {
                topRight = hit.point;
            }
                
            if (Physics.Raycast(rayBottomLeft, out hit, Mathf.Infinity, terrain))
            {
                bottomLeft = hit.point;
            }
                
            if (Physics.Raycast(rayBottomRight, out hit, Mathf.Infinity, terrain))
            {
                bottomRight = hit.point;
            }

            minHeight = Mathf.Min(topLeft.y,topRight.y,bottomLeft.y,bottomRight.y);

            // If we have found a local minimum, exit the loop
            if (center.y <= minHeight)
            {
                settled = true;
                break;
            }
            // Otherwise, move to the new lowest point and continue searching
            else
            {
                if (minHeight == topLeft.y) center = topLeft;
                else if (minHeight == topRight.y) center = topRight;
                else if (minHeight == bottomLeft.y) center = bottomLeft;
                else center = bottomRight;
                tries++;
            }
        }
        transform.position = center;
    }

    // Right now acually destroys all overlapping forests
    private void destroyOverlappingTrees()
    {
        Collider[] colliders = Physics.OverlapBox(center,size+ Vector3.up*1000f,Quaternion.identity);
       
        foreach(Collider collider in colliders)
        {
            if (collider.gameObject == null|| collider.gameObject.transform.parent == null) continue;
            TreeScript tree = collider.gameObject.transform.parent.GetComponent<TreeScript>();
            if (tree == null) continue;
            Destroy(tree);
        }
        
        removedForests = true;
    }

    // Gradually expands the water body to the highest it could fill this point
    private void moveToMaxFillHeight()
    {

        int tries = 0;
        RaycastHit hit;

        // Contact positions
        Vector3 topPoint,bottomPoint,leftPoint,rightPoint;

        while (tries < maxTries)
        {
            topPoint = Vector3.zero;
            bottomPoint = Vector3.zero;
            leftPoint = Vector3.zero;
            rightPoint = Vector3.zero;

            tries++;

            Ray rayTop = new Ray(center + Vector3.up * stepLength, Vector3.forward);
            Ray rayBottom = new Ray(center + Vector3.up * stepLength, Vector3.back);
            Ray rayLeft = new Ray(center + Vector3.up * stepLength, Vector3.left);
            Ray rayRight = new Ray(center + Vector3.up * stepLength, Vector3.right);

            // Cast rays from center to see if we have lakeable terrain
            if (Physics.Raycast(rayBottom, out hit, biome.lakeMaxLength, terrain))
            {
                bottomPoint = hit.point;
            }
            if (Physics.Raycast(rayTop, out hit, biome.lakeMaxLength, terrain))
            {
                topPoint = hit.point;
            }
            if (Physics.Raycast(rayLeft, out hit, biome.lakeMaxLength, terrain))
            {
                leftPoint = hit.point;
            }
            if (Physics.Raycast(rayRight, out hit, biome.lakeMaxLength, terrain))
            {
                rightPoint = hit.point;
            }

            // If any of those casts didn't find ANY terrain, this height is not lakeable
            if (leftPoint == Vector3.zero || rightPoint == Vector3.zero ||
                topPoint == Vector3.zero || bottomPoint == Vector3.zero)
            {
                expanded = true;   
                break;
            }
                
            // Otherwise move up to this position, update size, and keep searching
            else
            {
                float lDis = Vector3.Distance(center, leftPoint);
                float rDis = Vector3.Distance(center, rightPoint);
                float tDis = Vector3.Distance(center, topPoint);
                float bDis = Vector3.Distance(center, bottomPoint);

                size.x = 1.5f*(lDis + rDis);
                size.z = 1.5f*(bDis + tDis);
                center.y += stepLength;
              
            }
        }
        transform.position = center;
        if (expanded)
        {
            calculateVertices();
        }
    }

    private void moveCornersDown()
    {
        if (setBelow) return;
        int tries = 0;
        RaycastHit hit;
        float minHeight = center.y;
        float xMin = -size.x * 0.5f;
        float xMax = size.x * 0.5f;
        float zMin = -size.z * 0.5f;
        float zMax = size.z * 0.5f;

        
        while (edgeIndex < mf.mesh.vertices.Length && tries < maxRays)
        {

            if (mf.mesh.vertices[edgeIndex].x == xMin ||
                mf.mesh.vertices[edgeIndex].x == xMax ||
                mf.mesh.vertices[edgeIndex].z == zMin ||
                mf.mesh.vertices[edgeIndex].z == zMax)
            {
                Vector3 vert = center + mf.mesh.vertices[edgeIndex];
                
                Ray ray = new Ray(vert, Vector3.down);
                 
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrain))
                {
                    
                    if (hit.point.y < minHeight)
                    {
                        minHeight = hit.point.y;
                    }
                }
            
            }
            tries++;
            edgeIndex++;

            center.y = minHeight ;
            transform.position = center - Vector3.up * 10f;
        }

        if(edgeIndex >= mf.mesh.vertices.Length-1)
        {
            setBelow = true;
        }
    }

    private void calculateVertices()
    {
        if (size.Equals(Vector3.zero)) return;

        int xRes = Mathf.CeilToInt(size.x * waterResolution);
        int yRes = Mathf.CeilToInt(size.z * waterResolution);
        float xStepSize = size.x / (xRes - 1);
        float yStepSize = size.z / (yRes - 1);

        // Generate verticies
        Vector3[] vertices = new Vector3[(xRes * yRes)];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int iy = 0; iy < yRes; iy++)
        {
            for (int ix = 0; ix < xRes; ix++)
            {
                float x = ix * xStepSize - size.x * 0.5f;
                float y = iy * yStepSize - size.z * 0.5f;
                vertices[iy * xRes + ix] = new Vector3(x, 0, y);
                uvs[iy * xRes + ix] = new Vector2((float)ix/xRes,(float)iy/yRes);
            }
        }
        mf.mesh.vertices = vertices;
        mf.mesh.uv = uvs;

        // Generate triangles using these vertices
        int[] triangles = new int[(xRes - 1) * (yRes - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < yRes - 1; y++)
        {
            for (int x = 0; x < xRes - 1; x++)
            {
                int v1 = x + y * xRes;
                int v2 = (x + 1) + y * xRes;
                int v3 = x + (y + 1) * xRes;
                int v4 = (x + 1) + (y + 1) * xRes;
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
        mf.mesh.triangles = triangles;
        ReCalcTriangles(mf.mesh);
    }

    private void ReCalcTriangles(Mesh mesh)
    {
        Vector3[] oldVerts = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = new Vector3[triangles.Length];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i] = oldVerts[triangles[i]];
            uvs[i] = mesh.uv[triangles[i]];
            triangles[i] = i;

        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
