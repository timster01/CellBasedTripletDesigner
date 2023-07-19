using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class Cell : MonoBehaviour
{

    public int x, y;
    public CellGrid parent;
    public GameObject childShape;
    public int graphId;


    public enum FillValue { Empty, Full, BottomLeft, BottomRight, TopLeft, TopRight}



    private FillValue currentFillValue = FillValue.Empty;

    public FillValue CurrentFillValue { 
        get => currentFillValue; 
        set {
            if (currentFillValue == FillValue.Empty)
                parent.emptyCount--;
            if (value == FillValue.Empty)
                parent.emptyCount++;
            currentFillValue = value;
            UpdateShape();
        } 
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        childShape.GetComponent<MeshRenderer>().material.color = parent.GetGraphColor(graphId);
    }

    public void CycleFillValue()
    {
        CurrentFillValue = (FillValue)(((int)CurrentFillValue + 1) % 6);
    }

    public void UpdateShape()
    {
        if (CurrentFillValue == Cell.FillValue.Empty)
            childShape.GetComponent<MeshFilter>().mesh.Clear();

        string objPath = $"BaseShapes/{CurrentFillValue.ToString()}";
        Mesh mesh = Resources.Load<Mesh>(objPath);
        childShape.GetComponent<MeshFilter>().mesh = mesh;
        parent.CellUpdated(x, y);
    }

    public bool IsSilhouetteValid(int graphId = -1)
    {
        PathsD polygons = new PathsD();
        if (parent.cellGridAngle == CellGrid.CellGridAngle.Front)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnZ(x, y, graphId);
        }

        if (parent.cellGridAngle == CellGrid.CellGridAngle.Side)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnX(x, y, graphId);
        }

        if (parent.cellGridAngle == CellGrid.CellGridAngle.Top)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnY(x, y, graphId);

        }

        if (CurrentFillValue == FillValue.Empty && polygons.Count == 0)
        {
            return true;
        }

        if (polygons.Count != 1)
            return false;

        

        if (CurrentFillValue == FillValue.TopLeft)
        {
            return polygons[0].Count == 3 && 
                polygons[0].Contains(new PointD(-0.5, -0.5)) && polygons[0].Contains(new PointD(-0.5, 0.5)) && 
                polygons[0].Contains(new PointD(0.5, 0.5));
        }
        if (CurrentFillValue == FillValue.TopRight)
        {
            return polygons[0].Count == 3 && 
                polygons[0].Contains(new PointD(0.5, -0.5)) && polygons[0].Contains(new PointD(-0.5, 0.5)) && 
                polygons[0].Contains(new PointD(0.5, 0.5));
        }
        if (CurrentFillValue == FillValue.BottomLeft)
        {
            return polygons[0].Count == 3 && 
                polygons[0].Contains(new PointD(-0.5, -0.5)) && polygons[0].Contains(new PointD(-0.5, 0.5)) && 
                polygons[0].Contains(new PointD(0.5, -0.5));
        }
        if (CurrentFillValue == FillValue.BottomRight)
        {
            return polygons[0].Count == 3 && 
                polygons[0].Contains(new PointD(-0.5, -0.5)) && polygons[0].Contains(new PointD(0.5, 0.5)) && 
                polygons[0].Contains(new PointD(0.5, -0.5));
        }
        if (CurrentFillValue == FillValue.Full)
        {
            return polygons[0].Count == 4 && 
                polygons[0].Contains(new PointD(-0.5, -0.5)) && polygons[0].Contains(new PointD(-0.5, 0.5)) && 
                polygons[0].Contains(new PointD(0.5, 0.5)) && polygons[0].Contains(new PointD(0.5, -0.5));
        }

        return false;
    }

    private bool IsConnectedUp()
    {
        if (y == parent.dimensions - 1)
            return false;
        Cell adjacentCell = parent.GetCellAtCoords(x, y + 1);
        return ((CurrentFillValue == FillValue.Full || CurrentFillValue == FillValue.TopLeft || CurrentFillValue == FillValue.TopRight) && (adjacentCell.CurrentFillValue == FillValue.Full || adjacentCell.CurrentFillValue == FillValue.BottomLeft || adjacentCell.CurrentFillValue == FillValue.BottomRight));
    }

    private bool IsConnectedDown()
    {
        if (y == 0)
            return false;
        Cell adjacentCell = parent.GetCellAtCoords(x, y - 1);
        return adjacentCell.IsConnectedUp();
    }

    private bool IsConnectedRight()
    {
        if (x == parent.dimensions - 1)
            return false;
        Cell adjacentCell = parent.GetCellAtCoords(x + 1, y);
        return ((CurrentFillValue == FillValue.Full || CurrentFillValue == FillValue.TopRight || CurrentFillValue == FillValue.BottomRight) && (adjacentCell.CurrentFillValue == FillValue.Full || adjacentCell.CurrentFillValue == FillValue.TopLeft || adjacentCell.CurrentFillValue == FillValue.BottomLeft));
    }
    private bool IsConnectedLeft()
    {
        if (x == 0)
            return false;
        Cell adjacentCell = parent.GetCellAtCoords(x-1, y);
        return adjacentCell.IsConnectedRight();
    }

    public List<Cell> ConnectedCells()
    {
        List<Cell> result = new List<Cell>();
        if (IsConnectedDown())
        {
            result.Add(parent.GetCellAtCoords(x, y - 1));
        }
        if (IsConnectedUp())
        {
            result.Add(parent.GetCellAtCoords(x, y + 1));
        }
        if (IsConnectedLeft())
        {
            result.Add(parent.GetCellAtCoords(x - 1, y));
        }
        if (IsConnectedRight())
        {
            result.Add(parent.GetCellAtCoords(x + 1, y));
        }
        return result;
    }

}
