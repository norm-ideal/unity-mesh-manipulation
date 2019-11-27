using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CutObject : MonoBehaviour
{
	public GameObject cutPlane;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Cut(gameObject, cutPlane);
        }        
    }

    private float MoveOntoPlane(Vector3 p1, Vector3 p2, Vector3 p0, Vector3 n)
	{
		return Vector3.Dot( p0-p1, n ) / Vector3.Dot(p2-p1, n);
	}

	private void AddTriangle(int[] triangles, int start, List<int> newTriangles)
	{
		newTriangles.Add(triangles[start++]);
		newTriangles.Add(triangles[start++]);
		newTriangles.Add(triangles[start++]);
	}

	private void ProcessTriangleB1(int[] triangles, int ti, int[] pointCount, Vector3[] vertices, Vector3[] normals, Vector3 p, Vector3 n, int[] sides, int[] hasMovedTo, List<int> newTriangles, List<Vector3> newVertices, List<Vector3> newNormals,int[] rightIndex)
	{
		int[] vi = new int[5];
		vi[0] = vi[3] = triangles[ti  ];
		vi[1] = vi[4] = triangles[ti+1];
		vi[2]         = triangles[ti+2];

		float k;
		int v0, v1, v2;
		// vb is index-index of lower point
		int vb = ((pointCount[ti+2] & 0b100000) != 0) ? 3 : ((pointCount[ti+2] & 0b001000)!= 0) ? 1 : 2 ;
		v0 = vi[vb-1];
		v1 = vi[vb];	// lower point
		v2 = vi[vb+1];
        
		if( hasMovedTo[v0] == -1) // v0 has not been moved
		{
			k = MoveOntoPlane( vertices[v0], vertices[v1], p, n);
			newVertices[v0] = Vector3.Lerp(vertices[v0], vertices[v1], k);
			newNormals[v0] = Vector3.Lerp(normals[v0], normals[v1], k);
			hasMovedTo[v0] = v1;
		}
		else if( hasMovedTo[v0] != v1 ) // v0 has not been moved to v1, should create a new point and a new triangle
		{
			k = MoveOntoPlane( vertices[v0], vertices[v1], p, n);
			newVertices.Add( Vector3.Lerp(vertices[v0], vertices[v1], k) );
			newNormals.Add( Vector3.Lerp(normals[v0], normals[v1], k) );
			v0 = newVertices.Count-1;
			Debug.Log("new vertex");
		}

		if( hasMovedTo[v2] == -1)
		{
			k = MoveOntoPlane( vertices[v2], vertices[v1], p, n);
			newVertices[v2] = Vector3.Lerp(vertices[v2], vertices[v1], k);
			newNormals[v2] = Vector3.Lerp(normals[v2], normals[v1], k);
			hasMovedTo[v2] = v1;
		}
		else if( hasMovedTo[v2] != v1 ) // v2 has not been moved to v1, should create a new point and a new triangle
		{
			k = MoveOntoPlane( vertices[v2], vertices[v1], p, n);
			newVertices.Add( Vector3.Lerp(vertices[v2], vertices[v1], k) );
			newNormals.Add( Vector3.Lerp(normals[v2], normals[v1], k) );
			v2 = newVertices.Count-1;
			Debug.Log("new vertex");
		}

		newTriangles.Add( v0 );
		newTriangles.Add( v1 );
		newTriangles.Add( v2 );
        rightIndex[v0] = v2;
	}


	private void ProcessTriangleB2(int[] triangles, int ti, int[] pointCount, Vector3[] vertices, Vector3[] normals, Vector3 p, Vector3 n, int[] sides, int[] hasMovedTo, List<int> newTriangles, List<Vector3> newVertices, List<Vector3> newNormals, int[] rightIndex)
	{
		int[] vi = new int[5];
		vi[0] = vi[3] = triangles[ti  ];
		vi[1] = vi[4] = triangles[ti+1];
		vi[2]         = triangles[ti+2];

		float k;
		int v0, v1, v2;
		// vb is the index-index of upper point
		int vb = ((pointCount[ti+2] & 0b010000) != 0) ? 3 : ((pointCount[ti+2] & 0b000100)!= 0) ? 1 : 2 ;
		v0 = vi[vb-1];
		v1 = vi[vb];	// upper point
		v2 = vi[vb+1];

		int v0n, v2n;	// two points where v1 moved toward v0 and v2, accordingly
		v0n = v2n = -1;

		// if the point is moved toward v0 or v2, reuse it. Do not create.
		if( hasMovedTo[v1] == v0 )
			v0n = v1;
		else if ( hasMovedTo[v1] == v2 )
			v2n = v1;
        
        if (hasMovedTo[v1] == -1)
        {
            k = MoveOntoPlane(vertices[v1], vertices[v0], p, n);
            newVertices[v1] = Vector3.Lerp(vertices[v1], vertices[v0], k);
            newNormals[v1] = Vector3.Lerp(normals[v1], normals[v0], k);
            hasMovedTo[v1] = v0;
        }
        else if ( v0n == -1 )// v1 has not been moved toward v0, create it and store the index in v0n
		{
			k = MoveOntoPlane( vertices[v1], vertices[v0], p, n);
			newVertices.Add( Vector3.Lerp(vertices[v1], vertices[v0], k) );
			newNormals.Add( Vector3.Lerp(normals[v1], normals[v0], k) );
			v0n = newVertices.Count - 1;
			Debug.Log("new vertex");
		}

        if (hasMovedTo[v1] == -1)
        {
            k = MoveOntoPlane(vertices[v1], vertices[v2], p, n);
            newVertices[v1] = Vector3.Lerp(vertices[v1], vertices[v2], k);
            newNormals[v1] = Vector3.Lerp(normals[v1], normals[v2], k);
            hasMovedTo[v1] = v2;
        }
        else if ( v2n == -1 )// v1 has not been moved toward v2, create it and store the index in v2n
		{
			k = MoveOntoPlane( vertices[v1], vertices[v2], p, n);
			newVertices.Add( Vector3.Lerp(vertices[v1], vertices[v2], k) );
			newNormals.Add( Vector3.Lerp(normals[v1], normals[v2], k) );
			v2n = newVertices.Count - 1;
			Debug.Log("new vertex");
		}
        
		newTriangles.Add( v0 );
		newTriangles.Add( v2n );
		newTriangles.Add( v2 );

		newTriangles.Add( v0 );
		newTriangles.Add( v0n );
		newTriangles.Add( v2n );
        rightIndex[v2n] = v0n;
		Debug.Log("new triangle");
	}

// https://docs.unity3d.com/ScriptReference/Mesh.html
// Cuts the victim GameObject v with the cutting plane cutPlane,
// Keeps the triangle that is under the cutting plane.
	void Cut(GameObject v, GameObject cutPlane)
	{
		// get the mesh data from GameObject v
		Mesh mesh = v.GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		int[] sides = new int[vertices.Length]; // 1 : above, 0 : on the plane, -1 : under
		int[] hasMovedTo = new int[vertices.Length]; // true if the vertices has moved

		int[] triangles = mesh.triangles;
		int[] pointCount = new int[triangles.Length]; // number of points of a triangle [above, below, 0]

		// get the information from cutting plane (global)
		Vector3 n = cutPlane.transform.TransformVector(cutPlane.GetComponent<MeshFilter>().mesh.normals[0]);
		Vector3 p = cutPlane.transform.position;
		// convert it to model local. note that the normal vector should keep its length, thus it is converted with Direction
		n = v.transform.InverseTransformDirection(n);
		p = v.transform.InverseTransformPoint(p);

		// check which side the point is. Set the point as "unmoved"
		// calculations are in Model Local
		for(int i = 0; i < vertices.Length; i++ )
		{
			sides[i] = Math.Sign( Vector3.Dot( vertices[i]-p, n) );
			hasMovedTo[i] = -1;
		}

		List<Vector3> newVertices = new List<Vector3>();
		List<Vector3> newNormals = new List<Vector3>();
		newVertices.AddRange(vertices);
		newNormals.AddRange(normals);

		for(int i = 0, cp = 0, code = 0; i < triangles.Length; )
		{
			if( sides[ triangles[i] ] > 0 )
			{
				pointCount[cp]++;
				code = (code << 2) | 0b01;
			}
			else if ( sides[ triangles[i] ] < 0 )
			{
				pointCount[cp+1]++;
				code = (code << 2) | 0b10;
			}
			else
			{
				code = (code << 2) | 0b00;
			}
			if( ++i % 3 == 0 )
			{
				pointCount[cp+2] = code;
				code = 0;
				cp = i;
			}
		}

        int[] rightIndex = new int[vertices.Length * 3 / 2];
        for (int i = 0; i < rightIndex.Length; i++)
            rightIndex[i] = -1;

        List<int> newTriangles = new List<int>();
		for(var i = 0; i < triangles.Length; i+=3)
		{
			if(pointCount[i] == 0 )
				AddTriangle(triangles, i, newTriangles);
			// process the triangle with one point below the plane.
			else if(pointCount[i+1] == 1)
				ProcessTriangleB1(triangles, i, pointCount, vertices, normals, p, n, sides, hasMovedTo, newTriangles, newVertices, newNormals,rightIndex);
		}

		for(var i = 0; i < triangles.Length; i+=3)
		{
			if(pointCount[i+1] == 2)
				ProcessTriangleB2(triangles, i, pointCount, vertices, normals, p, n, sides, hasMovedTo, newTriangles, newVertices, newNormals,rightIndex);
		}

        int futa1, futa2, futa3;

        for(int i = 0; i < rightIndex.Length; i++)
        {
            if (rightIndex[i] != -1)
            {
                futa1 = i;
                futa2 = rightIndex[futa1];
                futa3 = rightIndex[futa2];

                while (futa1 != futa3 && futa3 != -1) 
                {
                    Debug.Log(futa1);
                    Debug.Log(futa2);
                    Debug.Log(futa3);
                    Debug.Log(newTriangles.Count);
                    Debug.Log("---");

                    newTriangles.Add(futa1);
                    newTriangles.Add(futa2);
                    newTriangles.Add(futa3);

                    newTriangles.Add(futa1);
                    newTriangles.Add(futa3);
                    newTriangles.Add(futa2);

                    futa2 = futa3;
                    futa3 = rightIndex[futa2];
                }

//                break;
            }
        }
        // https://stackoverflow.com/questions/1367504/converting-listint-to-int
        // update the triangle array
        mesh.vertices = newVertices.ToArray();
		mesh.normals = newNormals.ToArray();
		mesh.triangles = newTriangles.ToArray();
	}
}
