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

    //TODO: implement actual check
    public bool IsConnectedUp()
    {
        if (y == parent.dimension - 1)
            return true;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y + 1, z);
        return false;
    }

    public bool IsConnectedDown()
    {
        if (y == 0)
            return true;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y - 1, z);
        return adjacentVoxel.IsConnectedUp();
    }

    public bool IsConnectedRight()
    {
        if (x == parent.dimension - 1)
            return true;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x + 1, y, z);
        return false;
    }
    public bool IsConnectedLeft()
    {
        if (x == 0)
            return true;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x - 1, y, z);
        return adjacentVoxel.IsConnectedRight();
    }

    public bool IsConnectedBack()
    {
        if (z == parent.dimension - 1)
            return true;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z + 1);
        return false;
    }
    public bool IsConnectedFront()
    {
        if (z == 0)
            return true;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z - 1);
        return adjacentVoxel.IsConnectedBack();
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
