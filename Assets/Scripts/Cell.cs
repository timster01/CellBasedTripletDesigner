using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public int x, y;
    public CellGrid parent;
    public GameObject childShape;

    public enum FillValue { Empty, Full, BottomLeft, BottomRight, TopLeft, TopRight}



    public FillValue currentFillValue = FillValue.Empty;

    void Start()
    {
    }

    public void CycleFillValue()
    {
        currentFillValue = (FillValue)(((int)currentFillValue + 1) % 6);
        UpdateShape();
    }

    public void UpdateShape()
    {
        if (currentFillValue == Cell.FillValue.Empty)
            childShape.GetComponent<MeshFilter>().mesh.Clear();

        string objPath = $"BaseShapes/{currentFillValue.ToString()}";
        Mesh mesh = Resources.Load<Mesh>(objPath);
        childShape.GetComponent<MeshFilter>().mesh = mesh;
        parent.CellUpdated(x, y);
    }

    public bool IsSilhouetteValid()
    {
        return false;
    }

    public bool IsConnectedUp()
    {
        if (y == parent.dimension - 1)
            return true;
        Cell adjacentCell = parent.GetCellAtCoords(x, y + 1);
        return ((currentFillValue == FillValue.Full || currentFillValue == FillValue.TopLeft || currentFillValue == FillValue.TopRight) && (adjacentCell.currentFillValue == FillValue.Full || adjacentCell.currentFillValue == FillValue.BottomLeft || adjacentCell.currentFillValue == FillValue.BottomRight));
    }

    public bool IsConnectedDown()
    {
        if (y == 0)
            return true;
        Cell adjacentCell = parent.GetCellAtCoords(x, y - 1);
        return adjacentCell.IsConnectedUp();
    }

    public bool IsConnectedRight()
    {
        if (x == parent.dimension - 1)
            return true;
        Cell adjacentCell = parent.GetCellAtCoords(x + 1, y);
        return ((currentFillValue == FillValue.Full || currentFillValue == FillValue.TopRight || currentFillValue == FillValue.BottomRight) && (adjacentCell.currentFillValue == FillValue.Full || adjacentCell.currentFillValue == FillValue.TopLeft || adjacentCell.currentFillValue == FillValue.BottomLeft));
    }
    public bool IsConnectedLeft()
    {
        if (x == 0)
            return true;
        Cell adjacentCell = parent.GetCellAtCoords(x-1, y);
        return adjacentCell.IsConnectedRight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
