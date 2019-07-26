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
		if( Input.GetKeyDown("space") )
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

	private void ProcessTriangleB1(int[] triangles, int ti, int[] pointCount, Vector3[] vertices, Vector3 p, Vector3 n, int[] sides, int[] hasMovedTo, List<int> newTriangles, List<Vector3> newVertices)
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
			newVertices[v0] = k * vertices[v1] + (1-k) * vertices[v0];
			hasMovedTo[v0] = v1;
		}
		else if( hasMovedTo[v0] != v1 ) // v0 has not been moved to v1, should create a new point and a new triangle
		{
			k = MoveOntoPlane( vertices[v0], vertices[v1], p, n);
			newVertices.Add( k * vertices[v1] + (1-k) * vertices[v0] );
			v0 = newVertices.Count-1;
		}

		if( hasMovedTo[v2] == -1)
		{
			k = MoveOntoPlane( vertices[v2], vertices[v1], p, n);
			newVertices[v2] = k * vertices[v1] + (1-k) * vertices[v2];
			hasMovedTo[v2] = v1;
		}
		else if( hasMovedTo[v2] != v1 ) // v2 has not been moved to v1, should create a new point and a new triangle
		{
			k = MoveOntoPlane( vertices[v2], vertices[v1], p, n);
			newVertices.Add( k * vertices[v1] + (1-k) * vertices[v2] );
			v2 = newVertices.Count-1;
		}

		newTriangles.Add( v0 );
		newTriangles.Add( v1 );
		newTriangles.Add( v2 );
	}

/*
	private void ProcessTriangleB2(int[] triangles, int ti, int[] pointCount, Vector3[] vertices, Vector3 p, Vector3 n, int[] sides, int[] hasMovedTo, List<int> newTriangles, List<Vector3> newVertices)
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

		if( hasMovedTo[v1] == -1 )		// v1 has not been moved
		{
			k = MoveOntoPlane( vertices[v1], vertices[v0], p, n);
			newVertices[v1] = k * vertices[v0] + (1-k) * vertices[v1];
			hasMovedTo[v1] = v0;



			k = MoveOntoPlane( vertices[v1], vertices[v2], p, n);
			newVertices.Add(k * vertices[v2] + (1-k) * vertices[v1]);

		}
		else if( hasMovedTo[v1] == v0 )	// v1 has been moved toward v0
		{
			k = MoveOntoPlane( vertices[v1], vertices[v0], p, n);
			Vector3 newPoint =
			Debug.Log("Already Moved");
		}
		else if( hasMovedTo[v1] == v2 )	// v1 has been moved toward v2
		{
			Debug.Log("Already Moved");
		}
		else							// v1 has been moved toward unknown point
		{
			Debug.Log("Uncovered Mesh");
		}

		newTriangles.Add( v0 );
		newTriangles.Add( v1 );
		newTriangles.Add( v2 );
	}
*/

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
		newVertices.AddRange(vertices);
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

		List<int> newTriangles = new List<int>();
		for(var i = 0; i < triangles.Length; i+=3)
		{
			if(pointCount[i] == 0 || pointCount[i+1] == 2 )
				AddTriangle(triangles, i, newTriangles);
			// process the triangle with one point below the plane.
			else if(pointCount[i+1] == 1)
				ProcessTriangleB1(triangles, i, pointCount, vertices, p, n, sides, hasMovedTo, newTriangles, newVertices);
		}

/*
		for(var i = 0; i < triangles.Length; i+=3)
		{
			if(pointCount[i+1] == 2)
				ProcessTriangleB2(triangles, i, pointCount, vertices, p, n, sides, hasMovedTo, newTriangles, newVertices);
		}
*/
		// https://stackoverflow.com/questions/1367504/converting-listint-to-int
		// update the triangle array
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
	}
}
