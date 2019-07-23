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


	private void ProcessTriangle(int[] triangles, int ti, int[] pointCount, Vector3[] vertices, int[] sides, bool[] hasMoved, List<int> newTriangles)
	{
		int vi0 = triangles[ti  ];
		int vi1 = triangles[ti+1];
		int vi2 = triangles[ti+2];
		// if all the three points are under the plane, keep it
		if( pointCount[ti] == 0 )
		{
			newTriangles.Add( vi0 );
			newTriangles.Add( vi1 );
			newTriangles.Add( vi2 );
		}
		else if ( pointCount[ti] == 1 )	// only one point is above
		{
			
		}
		// otherwise, delete it
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
		bool[] hasMoved = new bool[vertices.Length]; // true if the vertices has moved

		int[] triangles = mesh.triangles;
		int[] pointCount = new int[triangles.Length]; // number of points of a triangle [above, under, 0]

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
			hasMoved[i] = false;
		}

		Vector3[] newVertices = vertices.Clone() as Vector3[];
		for(int i = 0, cp = 0; i < triangles.Length; )
		{
			if( sides[ triangles[i] ] > 0 )
				pointCount[cp]++;
			else if ( sides[ triangles[i] ] < 0 )
				pointCount[cp+1]++;
			if( ++i % 3 == 0 )
				cp = i;
		}

		List<int> newTriangles = new List<int>();
		for(var i = 0; i < triangles.Length; i+=3)
		{
			ProcessTriangle(triangles, i, pointCount, vertices, sides, hasMoved, newTriangles);
		}

		// https://stackoverflow.com/questions/1367504/converting-listint-to-int
		// update the triangle array
		mesh.triangles = newTriangles.ToArray();
	}
}
