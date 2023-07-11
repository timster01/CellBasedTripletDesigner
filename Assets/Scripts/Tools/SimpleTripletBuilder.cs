using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;
using SimpleFileBrowser;

public class SimpleTripletBuilder
{

    public SimpleCell[,] frontCellGrid;
    public SimpleCell[,] sideCellGrid;
    public SimpleCell[,] topCellGrid;

    public SimpleVoxel[,,] voxelGrid;

    public string[,,] shapeSilhouettesFront;
    public string[,,] shapeSilhouettesSide;
    public string[,,] shapeSilhouettesTop;
    public VoxelConnectedLevel[,,] connectedLevels;

    public int graphCount = 0;

    public int gridSize;

    public SimpleTripletBuilder(int gridSize = 5)
    {
        this.gridSize = gridSize;
        shapeSilhouettesFront = new string[6, 6, 6];
        shapeSilhouettesSide = new string[6, 6, 6];
        shapeSilhouettesTop = new string[6, 6, 6];
        connectedLevels = new VoxelConnectedLevel[6,6,6];
        VoxelConnectedLevel emptyLevel = new VoxelConnectedLevel();
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
            {
                shapeSilhouettesFront[0, i, j] = "Empty";
                shapeSilhouettesFront[i, 0, j] = "Empty";
                shapeSilhouettesFront[i, j, 0] = "Empty";

                shapeSilhouettesSide[0, i, j] = "Empty";
                shapeSilhouettesSide[i, 0, j] = "Empty";
                shapeSilhouettesSide[i, j, 0] = "Empty";

                shapeSilhouettesTop[0, i, j] = "Empty";
                shapeSilhouettesTop[i, 0, j] = "Empty";
                shapeSilhouettesTop[i, j, 0] = "Empty";

                connectedLevels[0, i, j] = emptyLevel;
                connectedLevels[i, 0, j] = emptyLevel;
                connectedLevels[i, j, 0] = emptyLevel;
            }
        DefineConnectedLevels();
        DefineSilhouetteType();
        frontCellGrid = new SimpleCell[gridSize, gridSize];
        sideCellGrid = new SimpleCell[gridSize, gridSize];
        topCellGrid = new SimpleCell[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
            {
                frontCellGrid[i, j] = new SimpleCell(this, i, j, CellGrid.CellGridAngle.Front);
                sideCellGrid[i, j] = new SimpleCell(this, i, j, CellGrid.CellGridAngle.Side);
                topCellGrid[i, j] = new SimpleCell(this, i, j, CellGrid.CellGridAngle.Top);
            }
        voxelGrid = new SimpleVoxel[gridSize, gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
                for (int k = 0; k < gridSize; k++)
                {
                    voxelGrid[i, j, k] = new SimpleVoxel(this, i, j, k);
                }
    }

    public List<SimpleVoxel> GetVoxelColumnFront(int graphId = -1, int x = 0, int y = 0)
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();
        for (int z = 0; z < gridSize; z++)
        {
            if (graphId == -1 || voxelGrid[x, y, z].graphId == graphId)
                result.Add(voxelGrid[x, y, z]);
        }
        return result;
    }

    public List<SimpleVoxel> GetVoxelColumnSide(int graphId = -1, int z = 0, int y = 0)
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();
        for (int x = 0; x < gridSize; x++)
        {
            if (graphId == -1 || voxelGrid[x, y, z].graphId == graphId)
                result.Add(voxelGrid[x, y, z]);
        }
        return result;
    }

    public List<SimpleVoxel> GetVoxelColumnTop(int graphId = -1, int x = 0, int z = 0)
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();
        for (int y = 0; y < gridSize; y++)
        {
            if (graphId == -1 || voxelGrid[x, y, z].graphId == graphId)
                result.Add(voxelGrid[x, y, z]);
        }
        return result;
    }

    public bool AreSilhouettesValid(int graphId = -1)
    {
        foreach (SimpleCell cell in frontCellGrid)
            if (!cell.IsValid(graphId))
                return false;

        foreach (SimpleCell cell in sideCellGrid)
            if (!cell.IsValid(graphId))
                return false;

        foreach (SimpleCell cell in topCellGrid)
            if (!cell.IsValid(graphId))
                return false;

        //Return true if non of the cells had an invalid silhouette
        return true;
    }

    public void loadShapeFromFile(SimpleCell[,] cellGrid, string path)
    {
        string serialized = FileBrowserHelpers.ReadTextFromFile(path);
        List<List<Cell.FillValue>> fillValues = DeserializeGridFillValues(serialized);
        loadShapeFromFillValueGrid(cellGrid, fillValues);
    }

    public void loadShapeFromFillValueGrid(SimpleCell[,] cellGrid, List<List<Cell.FillValue>> fillValues)
    {
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                cellGrid[x, y].fillValue = fillValues[x][y];
    }

    public void RotateClockWise(SimpleCell[,] cellGrid)
    {
        Cell.FillValue[,] fillGrid = GridFillValues(cellGrid);
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Cell.FillValue value = fillGrid[gridSize - 1 - y, x];
                if (value == Cell.FillValue.Full || value == Cell.FillValue.Empty)
                    cellGrid[x, y].fillValue = value;
                if (value == Cell.FillValue.TopLeft)
                    cellGrid[x, y].fillValue = Cell.FillValue.TopRight;
                if (value == Cell.FillValue.TopRight)
                    cellGrid[x, y].fillValue = Cell.FillValue.BottomRight;
                if (value == Cell.FillValue.BottomLeft)
                    cellGrid[x, y].fillValue = Cell.FillValue.TopLeft;
                if (value == Cell.FillValue.BottomRight)
                    cellGrid[x, y].fillValue = Cell.FillValue.BottomLeft;
            }
        }
    }

    public void MirrorLeftRight(SimpleCell[,] cellGrid)
    {
        Cell.FillValue[,] fillGrid = GridFillValues(cellGrid);
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Cell.FillValue value = fillGrid[gridSize - 1 - x, y];
                if (value == Cell.FillValue.Full || value == Cell.FillValue.Empty)
                    cellGrid[x, y].fillValue = value;
                if (value == Cell.FillValue.TopLeft)
                    cellGrid[x, y].fillValue = Cell.FillValue.TopRight;
                if (value == Cell.FillValue.TopRight)
                    cellGrid[x, y].fillValue = Cell.FillValue.TopLeft;
                if (value == Cell.FillValue.BottomLeft)
                    cellGrid[x, y].fillValue = Cell.FillValue.BottomRight;
                if (value == Cell.FillValue.BottomRight)
                    cellGrid[x, y].fillValue = Cell.FillValue.BottomLeft;
            }
        }
    }

    public Cell.FillValue[,] GridFillValues(SimpleCell[,] cellGrid){
        Cell.FillValue[,] fillGrid = new Cell.FillValue[gridSize,gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
            {
                fillGrid[x, y] = cellGrid[x, y].fillValue;
            }
        return fillGrid;
    }

    public List<List<Cell.FillValue>> DeserializeGridFillValues(string serialized)
    {
        List<List<Cell.FillValue>> result = new List<List<Cell.FillValue>>();
        List<Cell.FillValue> column;
        string[] lines = serialized.Split(System.Environment.NewLine);
        List<string[]> values = new List<string[]>();
        foreach (string line in lines)
            values.Add(line.Split(" "));
        for (int x = 0; x < gridSize; x++)
        {
            column = new List<Cell.FillValue>();
            for (int y = 0; y < gridSize; y++)
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

    public void MarkGraphId(SimpleVoxel.ConnectedDegree connectedDegree)
    {
        graphCount = 0;
        foreach (SimpleVoxel voxel in voxelGrid)
            voxel.graphId = -1;
        foreach (SimpleVoxel voxel in voxelGrid)
            if (voxel.graphId < 0 && !voxel.GetConnectedLevel().empty)
                graphCount += voxel.PropagateDFS(graphCount, connectedDegree);
    }

    //TODO: Write these results to a file and use it for validity checking. Possibly unneccesary
    void DefineSilhouetteType()
    {
        Mesh mesh;
        PathsD polygons;
        string resultSide;
        string resultTop;
        string resultFront;
        for (Cell.FillValue frontFillValue = (Cell.FillValue)1; (int)frontFillValue <= 5; frontFillValue++)
            for (Cell.FillValue sideFillValue = (Cell.FillValue)1; (int)sideFillValue <= 5; sideFillValue++)
                for (Cell.FillValue topFillValue = (Cell.FillValue)1; (int)topFillValue <= 5; topFillValue++)
                {
                    string objPath = $"{frontFillValue.ToString()}-{sideFillValue.ToString()}-{topFillValue.ToString()}";
                    mesh = Resources.Load<Mesh>(objPath);

                    if (mesh != null)
                    {
                        polygons = new PathsD();
                        foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
                        {
                            Triangle2D tri2D = tri.FlattenX();
                            if (tri2D.IsInLine())
                                continue;
                            if (tri2D.IsCounterClockWise())
                                tri2D = tri2D.FlipCoordOrder();
                            polygons.Add(tri2D.ToPathD());
                        }
                        polygons = Clipper.Union(polygons, FillRule.NonZero);
                        polygons = Clipper.SimplifyPaths(polygons, 0.0);
                        resultSide = polygons.Count > 0 ? ReturnSilhouetteShape(polygons[0]) : "Empty";

                        polygons = new PathsD();
                        foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
                        {
                            Triangle2D tri2D = tri.FlattenY();
                            if (tri2D.IsInLine())
                                continue;
                            if (tri2D.IsCounterClockWise())
                                tri2D = tri2D.FlipCoordOrder();
                            polygons.Add(tri2D.ToPathD());
                        }
                        polygons = Clipper.Union(polygons, FillRule.NonZero);
                        polygons = Clipper.SimplifyPaths(polygons, 0.0);

                        resultTop = polygons.Count > 0 ? ReturnSilhouetteShape(polygons[0]) : "Empty";

                        polygons = new PathsD();
                        foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
                        {
                            Triangle2D tri2D = tri.FlattenZ();
                            if (tri2D.IsInLine())
                                continue;
                            if (tri2D.IsCounterClockWise())
                                tri2D = tri2D.FlipCoordOrder();
                            polygons.Add(tri2D.ToPathD());
                        }
                        polygons = Clipper.Union(polygons, FillRule.NonZero);
                        polygons = Clipper.SimplifyPaths(polygons, 0.0);

                        resultFront = polygons.Count > 0 ? ReturnSilhouetteShape(polygons[0]) : "Empty";
                    }
                    else
                        resultFront = resultSide = resultTop = "Empty";


                    shapeSilhouettesFront[(int)frontFillValue, (int)sideFillValue, (int)topFillValue] = resultFront;
                    shapeSilhouettesSide[(int)frontFillValue, (int)sideFillValue, (int)topFillValue] = resultSide;
                    shapeSilhouettesTop[(int)frontFillValue, (int)sideFillValue, (int)topFillValue] = resultTop;

                }

    }

    public string ReturnSilhouetteShape(PathD polygon)
    {
        PointD middle = new PointD(0.0, 0.0);
        PointD bottomleft = new PointD(-0.5, -0.5);
        PointD bottomright = new PointD(0.5, -0.5);
        PointD topleft = new PointD(-0.5, 0.5);
        PointD topright = new PointD(0.5, 0.5);
        if (polygon.Contains(bottomleft) && polygon.Contains(bottomright) && polygon.Contains(topright) && polygon.Contains(topleft))
            return "Full";
        if (polygon.Contains(bottomleft) && polygon.Contains(bottomright) && polygon.Contains(topright))
            return "BottomRight";
        if (polygon.Contains(bottomleft) && polygon.Contains(bottomright) && polygon.Contains(topleft))
            return "BottomLeft";
        if (polygon.Contains(topleft) && polygon.Contains(bottomright) && polygon.Contains(topright))
            return "TopRight";
        if (polygon.Contains(bottomleft) && polygon.Contains(topleft) && polygon.Contains(topright))
            return "TopLeft";
        if (polygon.Contains(bottomleft) && polygon.Contains(bottomright) && polygon.Contains(middle))
            return "Bottom";
        if (polygon.Contains(topleft) && polygon.Contains(topright) && polygon.Contains(middle))
            return "Top";
        if (polygon.Contains(bottomleft) && polygon.Contains(topleft) && polygon.Contains(middle))
            return "Left";
        if (polygon.Contains(topright) && polygon.Contains(bottomright) && polygon.Contains(middle))
            return "Right";

        return "wrong";
    }

    void DefineConnectedLevels()
    {
        Mesh mesh;
        for (Cell.FillValue frontFillValue = (Cell.FillValue)1; (int)frontFillValue <= 5; frontFillValue++)
            for (Cell.FillValue sideFillValue = (Cell.FillValue)1; (int)sideFillValue <= 5; sideFillValue++)
                for (Cell.FillValue topFillValue = (Cell.FillValue)1; (int)topFillValue <= 5; topFillValue++)
                {
                    string objPath = $"{frontFillValue.ToString()}-{sideFillValue.ToString()}-{topFillValue.ToString()}";
                    mesh = Resources.Load<Mesh>(objPath);
                    VoxelConnectedLevel result;
                    if (mesh != null)
                    {
                        result = new VoxelConnectedLevel(mesh, frontFillValue, sideFillValue, topFillValue);
                    }
                    else
                        result = new VoxelConnectedLevel();
                    
                    connectedLevels[(int)frontFillValue, (int)sideFillValue, (int)topFillValue] = result;
                }

    }

}
