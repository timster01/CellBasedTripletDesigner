using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{

    public GameObject CellPrefab;
    public GameObject SilhouetteCellPrefab;
    public int dimension = 5;
    public TripletBuilder parent;
    public enum CellGridAngle { Front, Side, Top }
    public CellGridAngle cellGridAngle;
    public bool testRot = false;
    public bool testTopBot = false;
    public bool testLR = false;

    List<List<Cell>> grid;
    List<List<SilhouetteCell>> silhouetteGrid;

    List<Color> graphColors;

    public int emptyCount;
    private bool runningBatchOperation = false;

    // Start is called before the first frame update
    void Start()
    {
        graphColors = new List<Color>();
        grid = new List<List<Cell>>();
        silhouetteGrid = new List<List<SilhouetteCell>>();
        GameObject cellObject;
        Cell cell;
        GameObject silhouetteCellObject;
        SilhouetteCell silhouetteCell;
        for (int x = 0; x < dimension; x++)
        {
            List<Cell> column = new List<Cell>();
            List<SilhouetteCell> silhouetteColumn = new List<SilhouetteCell>();
            for (int y = 0; y < dimension; y++)
            {
                cellObject = GameObject.Instantiate(CellPrefab, this.transform);
                cellObject.transform.localPosition = new Vector3(x, y, 0);
                cellObject.transform.localRotation = Quaternion.identity;
                cell = cellObject.GetComponent<Cell>();
                cell.x = x;
                cell.y = y;
                cell.parent = this;
                column.Add(cell);
                silhouetteCellObject = GameObject.Instantiate(SilhouetteCellPrefab, this.transform);
                silhouetteCellObject.transform.localPosition = new Vector3(x, y, 0.5f);
                silhouetteCellObject.transform.localRotation = Quaternion.identity;
                silhouetteCell = silhouetteCellObject.GetComponent<SilhouetteCell>();
                silhouetteCell.x = x;
                silhouetteCell.y = y;
                silhouetteCell.parent = this;
                silhouetteColumn.Add(silhouetteCell);
            }
            grid.Add(column);
            silhouetteGrid.Add(silhouetteColumn);
        }
        emptyCount = dimension * dimension;
    }

    public bool IsEmpty()
    {
        return emptyCount == dimension * dimension;
    }

    // Update is called once per frame
    void Update()
    {
        if (testRot)
        {
            RotateClockWise();
            testRot = false;
        }

        if (testTopBot)
        {
            MirrorTopBottom();
            testTopBot = false;
        }

        if (testLR)
        {
            MirrorLeftRight();
            testLR = false;
        }
    }

    public Color GetGraphColor(int graphId)
    {
        for (int i = graphColors.Count; i <= graphId; i++)
        {
            Color newColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0f, 1f, 0.3f, 0.3f);
            graphColors.Add(newColor);
        }
        return graphColors[graphId];
    }

    public Cell GetCellAtCoords(int x, int y)
    {
        return grid[x][y];
    }

    public List<List<Cell>> GetConnectedGraphs()
    {
        List<List<Cell>> graphs = new List<List<Cell>>();
        bool shouldBreak = false;
        bool notInAGraph;
        List<Cell> graph;
        bool foundCell;
        while (true)
        {
            graph = new List<Cell>();
            foundCell = false;
            foreach (List<Cell> column in grid)
            {
                foreach (Cell cell in column)
                {
                    notInAGraph = true;
                    if (cell.CurrentFillValue != Cell.FillValue.Empty)
                    {
                        foreach (List<Cell> foundCells in graphs)
                        {
                            if (foundCells.Contains(cell))
                            {
                                notInAGraph = false;
                                break;
                            }
                        }

                        if (notInAGraph)
                        {
                            graph.Add(cell);
                            graph.AddRange(cell.ConnectedCells());
                            shouldBreak = true;
                            foundCell = true;
                            break;
                        }
                    }
                }
                if (shouldBreak)
                    break;
            }
            shouldBreak = false;
            if (!foundCell)
            {
                break;
            }
            int i = 1;
            while (i < graph.Count)
            {
                foreach (Cell cell in graph[i].ConnectedCells())
                {
                    if (!graph.Contains(cell))
                        graph.Add(cell);
                }
                i++;
            }
            graphs.Add(graph);
        }
        return graphs;
    }

    public void MarkGraphId()
    {
        int i = 0;
        foreach (List<Cell> graph in GetConnectedGraphs())
        {
            foreach (Cell cell in graph)
            {
                cell.graphId = i;
            }
            i++;
        }
    }

    public bool IsSilhouetteConnected()
    {
        List<Cell> graph = new List<Cell>();
        bool shouldBreak = false;
        foreach (List<Cell> column in grid)
        {
            foreach (Cell cell in column)
            {
                if (cell.CurrentFillValue != Cell.FillValue.Empty)
                {
                    graph.Add(cell);
                    graph.AddRange(cell.ConnectedCells());
                    shouldBreak = true;
                    break;
                }
            }
            if (shouldBreak)
                break;
        }
        if (graph.Count == 0)
            return true;
        int i = 1;
        while (i < graph.Count)
        {
            foreach (Cell cell in graph[i].ConnectedCells())
            {
                if (!graph.Contains(cell))
                    graph.Add(cell);
            }
            i++;
        }
        foreach (List<Cell> column in grid)
        {
            foreach (Cell cell in column)
            {
                if (cell.CurrentFillValue != Cell.FillValue.Empty && !graph.Contains(cell))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void UpdateSilhouetteCell(int x, int y)
    {
        silhouetteGrid[x][y].UpdateShape();
    }

    public void UpdateAllSilhouetteCells()
    {
        foreach (List<SilhouetteCell> column in silhouetteGrid)
            foreach (SilhouetteCell silhouetteCell in column)
                silhouetteCell.UpdateShape();
    }

    public void StartBatchOperation()
    {
        runningBatchOperation = true;
    }
    public void FinishBatchOperation()
    {
        runningBatchOperation = false;
        parent.voxelGrid.UpdateAllVoxels();
        MarkGraphId();
    }

    public void CellUpdated(int x, int y)
    {
        if (runningBatchOperation)
            return;

        if (emptyCount >= dimension * dimension - 1)
            parent.voxelGrid.UpdateAllVoxels();
        else if (cellGridAngle == CellGridAngle.Front)
        {
            parent.voxelGrid.UpdateVoxelColumnZ(x, y);
        }
        else if (cellGridAngle == CellGridAngle.Side)
        {
            parent.voxelGrid.UpdateVoxelColumnX(x, y);
        }
        else if (cellGridAngle == CellGridAngle.Top)
        {
            parent.voxelGrid.UpdateVoxelColumnY(x, y);
        }
        MarkGraphId();
    }

    public bool IsSilhouetteValid()
    {
        if (IsEmpty())
            return true;
        foreach (List<Cell> column in grid)
        {
            foreach (Cell cell in column)
            {
                if (!cell.IsSilhouetteValid())
                    return false;
            }
        }
        return true;
    }

    public List<List<Cell.FillValue>> GridFillValues(){
        List<List<Cell.FillValue>> fillGrid = new List<List<Cell.FillValue>>();

        foreach(List<Cell> column in grid)
        {
            List<Cell.FillValue> fillColumn = new List<Cell.FillValue>();
            foreach (Cell cell in column)
            {
                fillColumn.Add(cell.CurrentFillValue);
            }
            fillGrid.Add(fillColumn);
        }
        return fillGrid;
    }

    public void RotateClockWise()
    {
        StartBatchOperation();
        List<List<Cell.FillValue>> fillGrid = GridFillValues();
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                Cell.FillValue value = fillGrid[dimension - 1 - y][x];
                if (value == Cell.FillValue.Full || value == Cell.FillValue.Empty)
                    grid[x][y].CurrentFillValue = value;
                if (value == Cell.FillValue.TopLeft)
                    grid[x][y].CurrentFillValue = Cell.FillValue.TopRight;
                if (value == Cell.FillValue.TopRight)
                    grid[x][y].CurrentFillValue = Cell.FillValue.BottomRight;
                if (value == Cell.FillValue.BottomLeft)
                    grid[x][y].CurrentFillValue = Cell.FillValue.TopLeft;
                if (value == Cell.FillValue.BottomRight)
                    grid[x][y].CurrentFillValue = Cell.FillValue.BottomLeft;
            }
        }
        FinishBatchOperation();
    }

    public void MirrorTopBottom()
    {
        StartBatchOperation();
        List<List<Cell.FillValue>> fillGrid = GridFillValues();
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                Cell.FillValue value = fillGrid[x][dimension - 1 - y];
                if (value == Cell.FillValue.Full || value == Cell.FillValue.Empty)
                    grid[x][y].CurrentFillValue = value;
                if (value == Cell.FillValue.TopLeft)
                    grid[x][y].CurrentFillValue = Cell.FillValue.BottomLeft;
                if (value == Cell.FillValue.TopRight)
                    grid[x][y].CurrentFillValue = Cell.FillValue.BottomRight;
                if (value == Cell.FillValue.BottomLeft)
                    grid[x][y].CurrentFillValue = Cell.FillValue.TopLeft;
                if (value == Cell.FillValue.BottomRight)
                    grid[x][y].CurrentFillValue = Cell.FillValue.TopRight;
            }
        }
        FinishBatchOperation();
    }

    public void MirrorLeftRight()
    {
        StartBatchOperation();
        List<List<Cell.FillValue>> fillGrid = GridFillValues();
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                Cell.FillValue value = fillGrid[dimension - 1 - x][y];
                if (value == Cell.FillValue.Full || value == Cell.FillValue.Empty)
                    grid[x][y].CurrentFillValue = value;
                if (value == Cell.FillValue.TopLeft)
                    grid[x][y].CurrentFillValue = Cell.FillValue.TopRight;
                if (value == Cell.FillValue.TopRight)
                    grid[x][y].CurrentFillValue = Cell.FillValue.TopLeft;
                if (value == Cell.FillValue.BottomLeft)
                    grid[x][y].CurrentFillValue = Cell.FillValue.BottomRight;
                if (value == Cell.FillValue.BottomRight)
                    grid[x][y].CurrentFillValue = Cell.FillValue.BottomLeft;
            }
        }
        FinishBatchOperation();
    }
}
