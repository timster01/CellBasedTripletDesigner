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
        List<Vector2> polyPoints = new List<Vector2>();
        foreach(PointD point in polygon)
        {
            polyPoints.Add(new Vector2((float)point.x, (float)point.y));
        }
        Vector2[] verts2D = polyPoints.ToArray();
        Sebastian.Geometry.Polygon triangulatePoly = new Sebastian.Geometry.Polygon(verts2D);
        Sebastian.Geometry.Triangulator triangulator = new Sebastian.Geometry.Triangulator(triangulatePoly);
        int[] tris = triangulator.Triangulate();
        List<int> trisList = new List<int>();
        for (int i = 0; i < tris.Length; i++)
            trisList.Add(tris[i]);
        for (int i = tris.Length - 1; i >= 0; i--)
            trisList.Add(tris[i]);
        List<Vector3> verts = new List<Vector3>();
        foreach (Vector2 vert2d in verts2D)
            verts.Add(vert2d);
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = trisList.ToArray();
        return mesh;
    }

}
