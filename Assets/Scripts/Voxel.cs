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
