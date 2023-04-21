using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SimpleFileBrowser;

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
    public bool testLoad = false;
    public bool testSave = false;

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
                silhouetteCellObject.transform.localPosition = new Vector3(x, y, dimension + 8);
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

        if (testLoad)
        {
            LoadFromFileDialog();
            testLoad = false;
        }
        if (testSave)
        {
            SaveToFileDialog();
            testSave = false;
        }
    }

    public bool IsEmpty()
    {
        return emptyCount == dimension * dimension;
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

    public void SaveToFileDialog()
    {
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.DisableCamControl();
        FileBrowser.SetFilters(false, new string[] { ".shape" });
        FileBrowser.SetDefaultFilter(".shape");
        FileBrowser.ShowSaveDialog(SaveToFile, CancelFileDialog, FileBrowser.PickMode.Files);
        
    }

    public void SaveToFile(string[] paths)
    {
        string serialized = SerializeGridFillValues();
        FileBrowserHelpers.WriteTextToFile(paths[0], serialized);
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.EnableCamControl();
    }

    public void LoadFromFileDialog()
    {
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.DisableCamControl();
        FileBrowser.SetFilters(false, new string[] { ".shape" });
        FileBrowser.SetDefaultFilter(".shape");
        FileBrowser.ShowLoadDialog(LoadFromFile, CancelFileDialog, FileBrowser.PickMode.Files);
    }

    public void LoadFromFile( string[] paths)
    {
        string serialized = FileBrowserHelpers.ReadTextFromFile(paths[0]);
        LoadFillValueList(DeserializeGridFillValues(serialized));
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.EnableCamControl();
    }

    public void CancelFileDialog()
    {
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.EnableCamControl();
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

    public bool IsSilhouetteValid(int graphId = -1)
    {
        if (IsEmpty())
            return true;
        foreach (List<Cell> column in grid)
        {
            foreach (Cell cell in column)
            {
                if (!cell.IsSilhouetteValid(graphId))
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

    public string SerializeGridFillValues()
    {
        List<List<Cell.FillValue>> values = GridFillValues();
        StringBuilder result = new StringBuilder("", dimension*(dimension*3 + System.Environment.NewLine.Length));
        string fillValueString = "";
        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {
                if (values[x][y] == Cell.FillValue.Full)
                    fillValueString = "fu";
                if (values[x][y] == Cell.FillValue.Empty)
                    fillValueString = "em";
                if (values[x][y] == Cell.FillValue.TopLeft)
                    fillValueString = "tl";
                if (values[x][y] == Cell.FillValue.TopRight)
                    fillValueString = "tr";
                if (values[x][y] == Cell.FillValue.BottomLeft)
                    fillValueString = "bl";
                if (values[x][y] == Cell.FillValue.BottomRight)
                    fillValueString = "br";
                if(x == dimension - 1)
                    result.Append(fillValueString);
                else
                    result.Append(fillValueString + " ");
            }
            if(y < dimension - 1)
                result.Append(System.Environment.NewLine);
        }
        return result.ToString();
    }

    public List<List<Cell.FillValue>> DeserializeGridFillValues(string serialized)
    {
        List<List<Cell.FillValue>> result = new List<List<Cell.FillValue>>();
        List<Cell.FillValue> column;
        string[] lines = serialized.Split(System.Environment.NewLine);
        List<string[]> values = new List<string[]>();
        foreach (string line in lines)
            values.Add(line.Split(" "));
        for (int x = 0; x < dimension; x++)
        {
            column = new List<Cell.FillValue>();
            for (int y = 0; y < dimension; y++)
            {
                if (values[y][x] == "fu")
                    column.Add(Cell.FillValue.Full);
                if (values[y][x] == "em")
                    column.Add(Cell.FillValue.Empty);
                if (values[y][x] == "tl")
                    column.Add(Cell.FillValue.TopLeft);
                if (values[y][x] == "tr")
                    column.Add(Cell.FillValue.TopRight);
                if (values[y][x] == "bl")
                    column.Add(Cell.FillValue.BottomLeft);
                if (values[y][x] == "br")
                    column.Add(Cell.FillValue.BottomRight);
            }
            result.Add(column);
        }
        return result;
    }

    public void LoadFillValueList(List<List<Cell.FillValue>> list)
    {
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                grid[x][y].CurrentFillValue = list[x][y];
            }
        }
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
