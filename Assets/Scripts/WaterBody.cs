using UnityEngine;
using System.Collections;

public class WaterBody : MonoBehaviour {

    public Vector3 size;
    public Vector3 center;
    public Biome biome;
    public float waterResolution;

    private Mesh mesh;
    private float stepLength = 0.1f;
    private int terrain;
    private int maxTries = 1;
    private bool settled = false;
    private bool expanded = false;


    // References
    private MeshFilter mf;

	// Use this for initialization
	void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        terrain = LayerMask.GetMask("Terrain");
        center.y = Mathf.Infinity;
        mf = GetComponent<MeshFilter>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!settled || !expanded) fitToTerrain();
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
        if(!settled)
            moveToLocalMinimum();
        else if (!expanded)
            moveToMaxFillHeight();
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

    // Gradually expands the water body to the highest it could fill this point
    private void moveToMaxFillHeight()
    {
        int tries = 0;
        RaycastHit hit;
        // Direction rays
        Vector3 topLeft = Vector3.left + Vector3.forward;
        Vector3 topRight = Vector3.right + Vector3.forward;
        Vector3 bottomLeft = Vector3.left + Vector3.back;
        Vector3 bottomRight = Vector3.right + Vector3.back;

        // Contact positions
        Vector3 topLeftPoint, topRightPoint, bottomLeftPoint, bottomRightPoint;

        while (tries < maxTries)
        {
            topLeftPoint = Vector3.zero;
            topRightPoint = Vector3.zero;
            bottomLeftPoint = Vector3.zero;
            bottomRightPoint = Vector3.zero;

            tries++;
            Ray rayTopLeft = new Ray(center + Vector3.up * stepLength, topLeft);
            Ray rayTopRight = new Ray(center + Vector3.up * stepLength, topRight);
            Ray rayBottomLeft = new Ray(center + Vector3.up * stepLength, bottomLeft);
            Ray rayBottomRight = new Ray(center + Vector3.up * stepLength, bottomRight);

            // Cast rays from center to see if we have lakeable terrain
            if (Physics.Raycast(rayTopLeft, out hit, Mathf.Infinity, terrain))
            {
                topLeftPoint = hit.point;
            }

            if (Physics.Raycast(rayTopRight, out hit, Mathf.Infinity, terrain))
            {
                topRightPoint = hit.point;
            }

            if (Physics.Raycast(rayBottomLeft, out hit, Mathf.Infinity, terrain))
            {
                bottomLeftPoint = hit.point;
            }

            if (Physics.Raycast(rayBottomRight, out hit, Mathf.Infinity, terrain))
            {
                bottomRightPoint = hit.point;
            }

            // If any of those casts didn't find ANY terrain, this height is not lakeable
            if(topLeftPoint == Vector3.zero || topRightPoint == Vector3.zero ||
                bottomLeftPoint == Vector3.zero || bottomRightPoint == Vector3.zero)
            {
                expanded = true;   
                break;
            }

            // If we have a lakeable height here, advance the center to this point
            else if ((topLeftPoint.y == topRightPoint.y) && (topRight.y == bottomRight.y)
                && (bottomRight.y == bottomLeft.y))
            {
                center.y = topLeft.y;
            }
            // Otherwise we won't be able to fill higher, so quit
            else
            {
                expanded = true;
                break;
            }
        }
        transform.position = center;
        if (expanded)
        {
            calculateVertices();
        }
    }

    private void calculateVertices()
    {
        float sideLength = 1.5f * size.x;
        int resolution = Mathf.CeilToInt(size.x * waterResolution);
        float stepSize = sideLength / (resolution - 1);

        // Generate verticies
        Vector3[] vertices = new Vector3[(resolution * resolution)];
        for (int iy = 0; iy < resolution; iy++)
        {
            for (int ix = 0; ix < resolution; ix++)
            {
                float x = ix * stepSize - sideLength * 0.5f;
                float y = iy * stepSize - sideLength * 0.5f;
                vertices[iy * resolution + ix] = new Vector3(x, 0, y);
            }
        }
        mf.mesh.vertices = vertices;

        // Generate triangles using these vertices
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int i = 0;
        // iterate through each quad in vertices
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int v1 = x + y * resolution;
                int v2 = (x + 1) + y * resolution;
                int v3 = x + (y + 1) * resolution;
                int v4 = (x + 1) + (y + 1) * resolution;

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
