using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class SimpleTripletBuilder
{

    public SimpleCell[,] frontCellGrid;
    public SimpleCell[,] sideCellGrid;
    public SimpleCell[,] topCellGrid;

    public SimpleVoxel[,,] voxelGrid;

    public string[,,] shapeSilhouettesFront;
    public string[,,] shapeSilhouettesSide;
    public string[,,] shapeSilhouettesTop;

    int gridSize;

    public SimpleTripletBuilder(int gridSize = 5)
    {
        this.gridSize = gridSize;
        shapeSilhouettesFront = new string[6, 6, 6];
        shapeSilhouettesSide = new string[6, 6, 6];
        shapeSilhouettesTop = new string[6, 6, 6];
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
            }
        DefineSilhouetteType();
        frontCellGrid = new SimpleCell[5, 5];
        sideCellGrid = new SimpleCell[5, 5];
        topCellGrid = new SimpleCell[5, 5];
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
            {
                frontCellGrid[i, j] = new SimpleCell(this, i, j, CellGrid.CellGridAngle.Front);
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

    public bool IsValid(int graphId = -1)
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

    //TODO: Write these results to a file and use it for validity checking.
    //TODO2: Try something similar for connectedness if possible.
    void DefineSilhouetteType()
    {
        Mesh mesh;
        PathsD polygons = new PathsD();
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
}
