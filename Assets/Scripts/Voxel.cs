using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public int x, y, z;
    public GameObject childShape;
    public VoxelGrid parent;
    public int graphId;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if(graphId != -1)
            childShape.GetComponent<MeshRenderer>().material.color = parent.GetGraphColor(graphId);
    }

    public bool IsConnectedUp()
    {
        if (y == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y + 1, z);
        List<Triangle> ownTriangles = Triangle.MeshToTriangles(childShape.GetComponent<MeshFilter>().mesh);
        List<Triangle> adjacentTriangles = Triangle.MeshToTriangles(adjacentVoxel.childShape.GetComponent<MeshFilter>().mesh);
        if (ownTriangles.Count == 0 || adjacentTriangles.Count == 0)
            return false;
        List<Triangle2D> ownTriangles2D = new List<Triangle2D>();
        List<Triangle2D> adjacentTriangles2D = new List<Triangle2D>();
        foreach (Triangle triangle in ownTriangles)
        {
            if (triangle.coord1.y == 0.5f && triangle.coord2.y == 0.5f && triangle.coord3.y == 0.5f)
                ownTriangles2D.Add(triangle.FlattenY());
        }
        foreach (Triangle triangle in adjacentTriangles)
        {
            if (triangle.coord1.y == -0.5f && triangle.coord2.y == -0.5f && triangle.coord3.y == -0.5f)
                adjacentTriangles2D.Add(triangle.FlattenY());
        }
        if (ownTriangles2D.Count == 0 || adjacentTriangles2D.Count == 0)
            return false;

        foreach (Triangle2D t1 in ownTriangles2D)
        {
            foreach (Triangle2D t2 in adjacentTriangles2D)
            {
                if (t1.TriangleOverlaps(t2))
                    return true;
            }
        }

        return false;
    }

    public bool IsConnectedDown()
    {
        if (y == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y - 1, z);
        return adjacentVoxel.IsConnectedUp();
    }

    public bool IsConnectedRight()
    {
        if (x == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x + 1, y, z);
        List<Triangle> ownTriangles = Triangle.MeshToTriangles(childShape.GetComponent<MeshFilter>().mesh);
        List<Triangle> adjacentTriangles = Triangle.MeshToTriangles(adjacentVoxel.childShape.GetComponent<MeshFilter>().mesh);
        if (ownTriangles.Count == 0 || adjacentTriangles.Count == 0)
            return false;
        List<Triangle2D> ownTriangles2D = new List<Triangle2D>();
        List<Triangle2D> adjacentTriangles2D = new List<Triangle2D>();
        foreach(Triangle triangle in ownTriangles)
        {
            if (triangle.coord1.x == 0.5f && triangle.coord2.x == 0.5f && triangle.coord3.x == 0.5f)
                ownTriangles2D.Add(triangle.FlattenX());
        }
        foreach (Triangle triangle in adjacentTriangles)
        {
            if (triangle.coord1.x == -0.5f && triangle.coord2.x == -0.5f && triangle.coord3.x == -0.5f)
                adjacentTriangles2D.Add(triangle.FlattenX());
        }
        if (ownTriangles2D.Count == 0 || adjacentTriangles2D.Count == 0)
            return false;

        foreach(Triangle2D t1 in ownTriangles2D)
        {
            foreach(Triangle2D t2 in adjacentTriangles2D)
            {
                if (t1.TriangleOverlaps(t2))
                    return true;
            }
        }

        return false;
    }
    public bool IsConnectedLeft()
    {
        if (x == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x - 1, y, z);
        return adjacentVoxel.IsConnectedRight();
    }

    public bool IsConnectedBack()
    {
        if (z == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z + 1);
        List<Triangle> ownTriangles = Triangle.MeshToTriangles(childShape.GetComponent<MeshFilter>().mesh);
        List<Triangle> adjacentTriangles = Triangle.MeshToTriangles(adjacentVoxel.childShape.GetComponent<MeshFilter>().mesh);
        if (ownTriangles.Count == 0 || adjacentTriangles.Count == 0)
            return false;
        List<Triangle2D> ownTriangles2D = new List<Triangle2D>();
        List<Triangle2D> adjacentTriangles2D = new List<Triangle2D>();
        foreach (Triangle triangle in ownTriangles)
        {
            if (triangle.coord1.z == 0.5f && triangle.coord2.z == 0.5f && triangle.coord3.z == 0.5f)
                ownTriangles2D.Add(triangle.FlattenZ());
        }
        foreach (Triangle triangle in adjacentTriangles)
        {
            if (triangle.coord1.z == -0.5f && triangle.coord2.z == -0.5f && triangle.coord3.z == -0.5f)
                adjacentTriangles2D.Add(triangle.FlattenZ());
        }
        if (ownTriangles2D.Count == 0 || adjacentTriangles2D.Count == 0)
            return false;

        foreach (Triangle2D t1 in ownTriangles2D)
        {
            foreach (Triangle2D t2 in adjacentTriangles2D)
            {
                if (t1.TriangleOverlaps(t2))
                    return true;
            }
        }

        return false;
    }
    public bool IsConnectedFront()
    {
        if (z == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z - 1);
        return adjacentVoxel.IsConnectedBack();
    }

    public List<Voxel> ConnectedVoxels()
    {
        List<Voxel> result = new List<Voxel>();
        if (IsConnectedDown())
        {
            result.Add(parent.GetVoxelAtCoords(x, y - 1, z));
        }
        if (IsConnectedUp())
        {
            result.Add(parent.GetVoxelAtCoords(x, y + 1, z));
        }
        if (IsConnectedLeft())
        {
            result.Add(parent.GetVoxelAtCoords(x - 1, y, z));
        }
        if (IsConnectedRight())
        {
            result.Add(parent.GetVoxelAtCoords(x + 1, y, z));
        }
        if (IsConnectedFront())
        {
            result.Add(parent.GetVoxelAtCoords(x, y, z - 1));
        }
        if (IsConnectedBack())
        {
            result.Add(parent.GetVoxelAtCoords(x, y, z + 1));
        }
        return result;
    }

    public void UpdateVoxel()
    {
        Cell.FillValue frontCellFillValue = parent.parent.frontCellGrid.GetCellAtCoords(x,y).CurrentFillValue;
        if (parent.parent.frontCellGrid.IsEmpty())
            frontCellFillValue = Cell.FillValue.Full;
        Cell.FillValue sideCellFillValue = parent.parent.sideCellGrid.GetCellAtCoords(z, y).CurrentFillValue;
        if (parent.parent.sideCellGrid.IsEmpty())
            sideCellFillValue = Cell.FillValue.Full;
        Cell.FillValue topCellFillValue = parent.parent.topCellGrid.GetCellAtCoords(x, z).CurrentFillValue;
        if (parent.parent.topCellGrid.IsEmpty())
            topCellFillValue = Cell.FillValue.Full;
        SetVoxel(frontCellFillValue, sideCellFillValue, topCellFillValue);
    }

    void SetVoxel(Cell.FillValue frontFillValue, Cell.FillValue sideFillValue, Cell.FillValue topFillValue)
    {
        if (frontFillValue == Cell.FillValue.Empty || sideFillValue == Cell.FillValue.Empty || topFillValue == Cell.FillValue.Empty)
        {
            childShape.GetComponent<MeshFilter>().mesh.Clear();
            graphId = -1;
        }
        string objPath = $"{frontFillValue.ToString()}-{sideFillValue.ToString()}-{topFillValue.ToString()}";
        Mesh mesh = Resources.Load<Mesh>(objPath);
        childShape.GetComponent<MeshFilter>().mesh = mesh;
    }
}