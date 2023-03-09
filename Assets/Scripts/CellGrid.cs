using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{

    public GameObject CellPrefab;
    public int dimension = 5;
    public TripletBuilder parent;
    public enum CellGridAngle { Front, Side, Top}
    public CellGridAngle cellGridAngle;
    public bool test = false;

    List<List<Cell>> grid;

    List<Color> graphColors;

    // Start is called before the first frame update
    void Start()
    {
        graphColors = new List<Color>();
        grid = new List<List<Cell>>();
        GameObject cellObject;
        Cell cell;
        for (int x = 0; x < dimension; x++)
        {
            List<Cell> column = new List<Cell>();
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
            }
            grid.Add(column);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!test)
            return;

        if (IsSilhouetteConnected())
            Debug.Log("Silhouette is connected");
        else
            Debug.Log("Silhouette is not connected");

        
        test = false;
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
                    if (cell.currentFillValue != Cell.FillValue.Empty)
                    {
                        foreach(List<Cell> foundCells in graphs)
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
        foreach(List<Cell> column in grid)
        {
            foreach(Cell cell in column)
            {
                if(cell.currentFillValue != Cell.FillValue.Empty)
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
        while(i < graph.Count)
        {
            foreach(Cell cell in graph[i].ConnectedCells())
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
                if (cell.currentFillValue != Cell.FillValue.Empty && !graph.Contains(cell))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void CellUpdated(int x, int y)
    {
        if(cellGridAngle == CellGridAngle.Front)
        {
            parent.voxelGrid.UpdateVoxelColumnZ(x, y);
        }
        if (cellGridAngle == CellGridAngle.Side)
        {
            parent.voxelGrid.UpdateVoxelColumnX(x, y);
        }
        if (cellGridAngle == CellGridAngle.Top)
        {
            parent.voxelGrid.UpdateVoxelColumnY(x, y);
        }
        MarkGraphId();
    }

    public bool IsSilhouetteValid()
    {
        foreach(List<Cell> column in grid)
        {
            foreach(Cell cell in column)
            {
                if (!cell.IsSilhouetteValid())
                    return false;
            }
        }
        return true;
    }
}
