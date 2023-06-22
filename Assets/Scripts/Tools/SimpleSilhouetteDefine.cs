using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class SimpleSilhouetteDefine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DefineSilhouetteType();
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    

                    Debug.Log($"{objPath}:{resultFront}:{resultSide}:{resultTop}");
                    Debug.Log(polygons[0]);
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
