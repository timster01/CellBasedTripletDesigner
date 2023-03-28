using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class SilhouetteCell : MonoBehaviour
{
    public int x, y;
    public CellGrid parent;
    public GameObject childShape;

    private PathsD polygons;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Make this flatten the column and triangulate the polygon
    public void UpdateShape()
    {
        polygons = new PathsD();
        if (parent.cellGridAngle == CellGrid.CellGridAngle.Front)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnZ(x, y);
        }

        if (parent.cellGridAngle == CellGrid.CellGridAngle.Side)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnX(x, y);
        }

        if (parent.cellGridAngle == CellGrid.CellGridAngle.Top)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnY(x, y);

        }
    
        childShape.GetComponent<MeshFilter>().mesh = TriangulatePolygons();
    }

    Mesh TriangulatePolygons()
    {
        Mesh combinedMesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[polygons.Count];
        int c = 0;
        foreach (PathD polygon in polygons)
        {
            combine[c].mesh = TriangulatePolygon(polygon);
            combine[c].transform = Matrix4x4.identity;
            c++;
        }
        combinedMesh.CombineMeshes(combine);
        return combinedMesh;
    }

    Mesh TriangulatePolygon(PathD polygon)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        LinkedList<Vector2> polygonLinked = new LinkedList<Vector2>();
        foreach (PointD point in polygon)
            polygonLinked.AddLast(new Vector2((float)point.x, (float)point.y));
        LinkedListNode<Vector2> firstNode = polygonLinked.First;
        LinkedListNode<Vector2> currentNode = polygonLinked.First;
        LinkedListNode<Vector2> lastNode = polygonLinked.Last;
        while (polygonLinked.Count > 2)
        {
            firstNode = polygonLinked.First;
            currentNode = polygonLinked.First;
            lastNode = polygonLinked.Last;
            bool failedToFindEar = true;
            while(currentNode != lastNode)
            {
                bool isEar = false;
                if (currentNode == firstNode)
                    isEar = IsEar(lastNode.Value, currentNode.Value, currentNode.Next.Value, polygonLinked);
                else if (currentNode == lastNode)
                    isEar = IsEar(currentNode.Previous.Value, currentNode.Value, firstNode.Value, polygonLinked);
                else
                    isEar = IsEar(currentNode.Previous.Value, currentNode.Value, currentNode.Next.Value, polygonLinked);
                if (isEar)
                {
                    if (vertices.Contains(currentNode.Previous.Value))
                    {
                        triangles.Add(vertices.IndexOf(currentNode.Previous.Value));
                    }
                    else
                    {
                        vertices.Add(currentNode.Previous.Value);
                        triangles.Add(vertices.Count - 1);
                    }

                    if (vertices.Contains(currentNode.Value))
                    {
                        triangles.Add(vertices.IndexOf(currentNode.Value));
                    }
                    else
                    {
                        vertices.Add(currentNode.Value);
                        triangles.Add(vertices.Count - 1);
                    }

                    if (vertices.Contains(currentNode.Next.Value))
                    {
                        triangles.Add(vertices.IndexOf(currentNode.Next.Value));
                    }
                    else
                    {
                        vertices.Add(currentNode.Next.Value);
                        triangles.Add(vertices.Count - 1);
                    }

                    polygonLinked.Remove(currentNode);
                    failedToFindEar = false;
                    break;
                }
                currentNode = currentNode.Next;
            }
            if (failedToFindEar)
            {
                Debug.LogError($"Failed to find an ear in silhouette cell {x}, {y}");
                return new Mesh();
            }
                
        }

        return mesh;
    }

    bool IsEar(Vector2 vPrev, Vector2 v, Vector2 vNext, LinkedList<Vector2> vertices)
    {
        //TODO: test this angle formula
        float angle = Mathf.Acos(((vPrev - v).sqrMagnitude + (vNext - v).sqrMagnitude + (vPrev - vNext).sqrMagnitude) / 2 * (vPrev - v).sqrMagnitude * (vNext - v).sqrMagnitude);
        if (!(angle < Mathf.PI))
            return false;

        Triangle2D triangle = new Triangle2D(vPrev, v, vNext);
        foreach (Vector2 vertex in vertices)
        {
            if ((vertex != vPrev || vertex != v || vertex != vNext) && triangle.IsPointInTriangle(vertex))
                return false;
        }

        return true;
    }
}
