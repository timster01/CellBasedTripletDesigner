using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Voxel : MonoBehaviour
{
    public int x, y, z;
    public GameObject childShape;
    public VoxelGrid parent;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
    }

    private bool IsConnectedUp()
    {
        if (y == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y + 1, z);
        //TODO: check if adjacent faces have overlap

        return false;
    }

    private bool IsConnectedDown()
    {
        if (y == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y - 1, z);
        return adjacentVoxel.IsConnectedUp();
    }

    private bool IsConnectedRight()
    {
        if (x == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x + 1, y, z);
        //TODO: check if adjacent faces have overlap

        return false;
    }
    private bool IsConnectedLeft()
    {
        if (x == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x - 1, y, z);
        return adjacentVoxel.IsConnectedRight();
    }

    private bool IsConnectedBack()
    {
        if (z == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z + 1);
        //TODO: check if adjacent faces have overlap

        return false;
    }
    private bool IsConnectedFront()
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
        Cell.FillValue frontCellFillValue = parent.parent.frontCellGrid.GetCellAtCoords(x,y).currentFillValue;
        Cell.FillValue sideCellFillValue = parent.parent.sideCellGrid.GetCellAtCoords(z, y).currentFillValue;
        Cell.FillValue topCellFillValue = parent.parent.topCellGrid.GetCellAtCoords(x, z).currentFillValue;
        SetVoxel(frontCellFillValue, sideCellFillValue, topCellFillValue);
    }

    void SetVoxel(Cell.FillValue frontFillValue, Cell.FillValue sideFillValue, Cell.FillValue topFillValue)
    {
        if (frontFillValue == Cell.FillValue.Empty || sideFillValue == Cell.FillValue.Empty || topFillValue == Cell.FillValue.Empty)
            childShape.GetComponent<MeshFilter>().mesh.Clear();

        string objPath = $"{frontFillValue.ToString()}-{sideFillValue.ToString()}-{topFillValue.ToString()}";
        Mesh mesh = Resources.Load<Mesh>(objPath);
        childShape.GetComponent<MeshFilter>().mesh = mesh;
    }
}

public struct Triangle
{
    Vector3 coord1, coord2, coord3;

    public Triangle(Vector3 coord1, Vector3 coord2, Vector3 coord3)
    {
        this.coord1 = coord1;
        this.coord2 = coord2;
        this.coord3 = coord3;
    }

    public Triangle2D FlattenX()
    {
        return new Triangle2D(new Vector2(coord1.z, coord1.y), new Vector2(coord2.z, coord2.y), new Vector2(coord3.z, coord3.y));
    }
    public Triangle2D FlattenY()
    {
        return new Triangle2D(new Vector2(coord1.x, coord1.z), new Vector2(coord2.x, coord2.z), new Vector2(coord3.x, coord3.z));
    }
    public Triangle2D FlattenZ()
    {
        return new Triangle2D(new Vector2(coord1.x, coord1.y), new Vector2(coord2.x, coord2.y), new Vector2(coord3.x, coord3.y));
    }

}

public struct Triangle2D
{
    Vector2 coord1, coord2, coord3;

    public Triangle2D(Vector2 coord1, Vector2 coord2, Vector2 coord3)
    {
        this.coord1 = coord1;
        this.coord2 = coord2;
        this.coord3 = coord3;
    }

    //TODO Implement
    public bool TriangleOverlaps(Triangle2D otherTriangle)
    {
        
        return false;
    }
}
