using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshGeneration : MonoBehaviour
{
	public Vector3[] polygonVertices;

	private void Start()
    {
		//CreateMeshObject(polygonVertices);
	}

	public static GameObject CreateMeshObject(Vector3[] polygonVerticesArray)
    {
		List<Vector3> polygonVerticesList = polygonVerticesArray.ToList();
		List<Triangle> triangles = TriangulateConcavePolygon(polygonVerticesList);

		// Triangulate the polygon
		List<int> trianglesIndices = new List<int>();

		foreach (Triangle triangle in triangles)
		{
			trianglesIndices.Add(polygonVerticesList.IndexOf(triangle.p1));
			trianglesIndices.Add(polygonVerticesList.IndexOf(triangle.p2));
			trianglesIndices.Add(polygonVerticesList.IndexOf(triangle.p3));

			Debug.Log(polygonVerticesList.IndexOf(triangle.p1) + " " +
				polygonVerticesList.IndexOf(triangle.p2) + " " +
				polygonVerticesList.IndexOf(triangle.p3));
		}

		// Create a mesh and set its vertices and triangles
		Mesh mesh = new Mesh();
		mesh.SetVertices(polygonVerticesArray);
		mesh.triangles = trianglesIndices.ToArray();

		// Ensure correct normals for correct lighting
		mesh.RecalculateNormals();

		// Create a game object with a MeshFilter and MeshRenderer to display the mesh
		GameObject polygonMesh = new GameObject("PolygonMesh");
		MeshFilter meshFilter = polygonMesh.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = polygonMesh.AddComponent<MeshRenderer>();
		meshFilter.mesh = mesh;

		return polygonMesh; 
	}

    public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points)
	{
		//The list with triangles the method returns
		List<Triangle> triangles = new List<Triangle>();

		//If we just have three points, then we dont have to do all calculations
		if (points.Count == 3)
		{
			triangles.Add(new Triangle(points[0], points[1], points[2]));

			return triangles;	
		}



		//Step 1. Store the vertices in a list and we also need to know the next and prev vertex
		List<Vertex> vertices = new List<Vertex>();

		for (int i = 0; i < points.Count; i++)
		{
			vertices.Add(new Vertex(points[i]));
		}

		//Find the next and previous vertex
		for (int i = 0; i < vertices.Count; i++)
		{
			int nextPos = ClampListIndex(i + 1, vertices.Count);

			int prevPos = ClampListIndex(i - 1, vertices.Count);

			vertices[i].prevVertex = vertices[prevPos];

			vertices[i].nextVertex = vertices[nextPos];
		}



		//Step 2. Find the reflex (concave) and convex vertices, and ear vertices
		for (int i = 0; i < vertices.Count; i++)
		{
			CheckIfReflexOrConvex(vertices[i]);
		}

		//Have to find the ears after we have found if the vertex is reflex or convex
		List<Vertex> earVertices = new List<Vertex>();

		for (int i = 0; i < vertices.Count; i++)
		{
			earVertices = IsVertexEar(vertices[i], vertices, earVertices);
		}



		//Step 3. Triangulate!
		while (true)
		{
			//This means we have just one triangle left
			if (vertices.Count == 3)
			{
				//The final triangle
				triangles.Add(new Triangle(vertices[0].position, vertices[0].prevVertex.position, vertices[0].nextVertex.position));

				break;
			}

			//Make a triangle of the first ear
			Vertex earVertex = earVertices[0];

			Vertex earVertexPrev = earVertex.prevVertex;
			Vertex earVertexNext = earVertex.nextVertex;

			Triangle newTriangle = new Triangle(earVertex.position, earVertexPrev.position, earVertexNext.position);

			triangles.Add(newTriangle);

			//Remove the vertex from the lists
			earVertices.Remove(earVertex);

			vertices.Remove(earVertex);

			//Update the previous vertex and next vertex
			earVertexPrev.nextVertex = earVertexNext;
			earVertexNext.prevVertex = earVertexPrev;

			//...see if we have found a new ear by investigating the two vertices that was part of the ear
			CheckIfReflexOrConvex(earVertexPrev);
			CheckIfReflexOrConvex(earVertexNext);

			earVertices.Remove(earVertexPrev);
			earVertices.Remove(earVertexNext);

			IsVertexEar(earVertexPrev, vertices, earVertices);
			IsVertexEar(earVertexNext, vertices, earVertices);
		}

		//Debug.Log(triangles.Count);

		return triangles;
	}



	//Check if a vertex if reflex or convex, and add to appropriate list
	private static void CheckIfReflexOrConvex(Vertex v)
	{
		v.isReflex = false;
		v.isConvex = false;

		//This is a reflex vertex if its triangle is oriented clockwise
		Vector2 a = v.prevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.nextVertex.GetPos2D_XZ();

		Debug.Log(a + " " + b + " " + c);

		if (IsTriangleOrientedClockwise(a, b, c))
		{
			v.isReflex = true;
		}
		else
		{
			v.isConvex = true;
		}
	}



	//Check if a vertex is an ear
	private static List<Vertex> IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
	{
		//A reflex vertex cant be an ear!
		if (v.isReflex)
		{
			return earVertices;
		}

		//This triangle to check point in triangle
		Vector2 a = v.prevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.nextVertex.GetPos2D_XZ();

		bool hasPointInside = false;

		for (int i = 0; i < vertices.Count; i++)
		{
			//We only need to check if a reflex vertex is inside of the triangle
			if (vertices[i].isReflex)
			{
				Vector2 p = vertices[i].GetPos2D_XZ();

				//This means inside and not on the hull
				if (IsPointInTriangle(a, b, c, p))
				{
					hasPointInside = true;

					break;
				}
			}
		}

		Debug.Log("Hari" + hasPointInside);
		if (!hasPointInside)
		{
			earVertices.Add(v);
		}

		return earVertices; 
	}

	//Clamp list indices
	//Will even work if index is larger/smaller than listSize, so can loop multiple times
	public static int ClampListIndex(int index, int listSize)
	{
		index = ((index % listSize) + listSize) % listSize;

		return index;
	}

	//From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
	//p is the testpoint, and the other points are corners in the triangle
	public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
	{
		bool isWithinTriangle = false;

		//Based on Barycentric coordinates
		float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

		float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
		float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
		float c = 1 - a - b;

		//The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
		//if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
		//{
		//    isWithinTriangle = true;
		//}

		//The point is within the triangle
		if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
		{
			isWithinTriangle = true;
		}

		return isWithinTriangle;
	}

	//Is a triangle in 2d space oriented clockwise or counter-clockwise
	//https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
	//https://en.wikipedia.org/wiki/Curve_orientation
	public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		bool isClockWise = true;

		float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

		if (determinant > 0f)
		{
			isClockWise = false;
		}

		return isClockWise;
	}

	//To store triangle data to get cleaner code
	public struct Triangle
	{
		//Corners of the triangle
		public Vector3 p1, p2, p3;
		//The 3 line segments that make up this triangle
		public LineSegment[] lineSegments;

		public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;

			lineSegments = new LineSegment[3];

			lineSegments[0] = new LineSegment(p1, p2);
			lineSegments[1] = new LineSegment(p2, p3);
			lineSegments[2] = new LineSegment(p3, p1);
		}
	}

	//To create a line segment
	public struct LineSegment
	{
		//Start/end coordinates
		public Vector3 p1, p2;

		public LineSegment(Vector3 p1, Vector3 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}
	}

	public class Vertex
	{
		public Vector3 position;

		//The outgoing halfedge (a halfedge that starts at this vertex)
		//Doesnt matter which edge we connect to it
		public HalfEdge halfEdge;

		//Which triangle is this vertex a part of?
		public Triangle triangle;

		//The previous and next vertex this vertex is attached to
		public Vertex prevVertex;
		public Vertex nextVertex;

		//Properties this vertex may have
		//Reflex is concave
		public bool isReflex;
		public bool isConvex;
		public bool isEar;

		public Vertex(Vector3 position)
		{
			this.position = position;
		}

		//Get 2d pos of this vertex
		public Vector2 GetPos2D_XZ()
		{
			Vector2 pos_2d_xz = new Vector2(position.x, position.z);

			return pos_2d_xz;
		}
	}

	public class HalfEdge
	{
		//The vertex the edge points to
		public Vertex v;

		//The face this edge is a part of
		public Triangle t;

		//The next edge
		public HalfEdge nextEdge;
		//The previous
		public HalfEdge prevEdge;
		//The edge going in the opposite direction
		public HalfEdge oppositeEdge;

		//This structure assumes we have a vertex class with a reference to a half edge going from that vertex
		//and a face (triangle) class with a reference to a half edge which is a part of this face 
		public HalfEdge(Vertex v)
		{
			this.v = v;
		}
	}
}
