using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public struct Triangle
{
    public Vector3 coord1, coord2, coord3;

    public Triangle(Vector3 coord1, Vector3 coord2, Vector3 coord3)
    {
        this.coord1 = coord1;
        this.coord2 = coord2;
        this.coord3 = coord3;
    }

    public Triangle2D FlattenX()
    {
        return new Triangle2D(new Vector2(coord1.z, coord1.y), new Vector2(coord2.z, coord2.y), new Vector2(coord3.z, coord3.y));
    }
    public Triangle2D FlattenY()
    {
        return new Triangle2D(new Vector2(coord1.x, coord1.z), new Vector2(coord2.x, coord2.z), new Vector2(coord3.x, coord3.z));
    }
    public Triangle2D FlattenZ()
    {
        return new Triangle2D(new Vector2(coord1.x, coord1.y), new Vector2(coord2.x, coord2.y), new Vector2(coord3.x, coord3.y));
    }

    public static List<Triangle> MeshToTriangles(Mesh mesh)
    {
        List<Triangle> result = new List<Triangle>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
            result.Add(new Triangle(mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]], mesh.vertices[mesh.triangles[i + 2]]));

        return result;
    }

}

public struct Triangle2D
{
    public Vector2 coord1, coord2, coord3;

    public Triangle2D(Vector2 coord1, Vector2 coord2, Vector2 coord3)
    {
        this.coord1 = coord1;
        this.coord2 = coord2;
        this.coord3 = coord3;
    }

    public List<Vector2> GetCoordsList()
    {
        return new List<Vector2>() { coord1, coord2, coord3 };
    }

    //Adapted from https://www.habrador.com/tutorials/math/6-triangle-triangle-intersection/
    public bool TriangleOverlaps(Triangle2D otherTriangle)
    {
        Triangle2D t1 = this;
        Triangle2D t2 = otherTriangle;

        float t1_minX = Mathf.Min(t1.coord1.x, Mathf.Min(t1.coord2.x, t1.coord3.x));
        float t1_maxX = Mathf.Max(t1.coord1.x, Mathf.Max(t1.coord2.x, t1.coord3.x));
        float t1_minY = Mathf.Min(t1.coord1.y, Mathf.Min(t1.coord2.y, t1.coord3.y));
        float t1_maxY = Mathf.Max(t1.coord1.y, Mathf.Max(t1.coord2.y, t1.coord3.y));

        //Triangle 2
        float t2_minX = Mathf.Min(t2.coord1.x, Mathf.Min(t2.coord2.x, t2.coord3.x));
        float t2_maxX = Mathf.Max(t2.coord1.x, Mathf.Max(t2.coord2.x, t2.coord3.x));
        float t2_minY = Mathf.Min(t2.coord1.y, Mathf.Min(t2.coord2.y, t2.coord3.y));
        float t2_maxY = Mathf.Max(t2.coord1.y, Mathf.Max(t2.coord2.y, t2.coord3.y));


        //Are the rectangles intersecting?
        //If the min of one box in one dimension is greater than the max of another box then the boxes are not intersecting
        //They have to intersect in 2 dimensions. We have to test if box 1 is to the left or box 2 and vice versa
        bool isAABBIntersecting = true;

        //X axis
        if (t1_minX > t2_maxX)
        {
            isAABBIntersecting = false;
        }
        else if (t2_minX > t1_maxX)
        {
            isAABBIntersecting = false;
        }
        //Y axis
        else if (t1_minY > t2_maxY)
        {
            isAABBIntersecting = false;
        }
        else if (t2_minY > t1_maxY)
        {
            isAABBIntersecting = false;
        }

        //Step 1. AABB intersection
        if (isAABBIntersecting)
        {
            //Step 2. Line segment - triangle intersection
            bool isAnyLineSegmentIntersecting = false;
            List<Vector2> t1coords = t1.GetCoordsList();
            List<Vector2> t2coords = t2.GetCoordsList();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //The start/end coordinates of the current line segments
                    Vector2 t1_p1 = t1coords[i];
                    Vector2 t1_p2 = t1coords[(i + 1) % 3];
                    Vector2 t2_p1 = t2coords[j];
                    Vector2 t2_p2 = t2coords[(j + 1) % 3];

                    //Are they intersecting?
                    if (AreLineSegmentsIntersecting(t1_p1, t1_p2, t2_p1, t2_p2))
                    {
                        isAnyLineSegmentIntersecting = true;

                        //To stop the outer for loop
                        i = int.MaxValue - 1;

                        break;
                    }
                }
            }

            if (isAnyLineSegmentIntersecting)
            {
                return true;
            }

            bool IsAnyCornerInsideTriangle = false;

            //We only have to test one corner from each triangle
            //Triangle 1 in triangle 2
            if (t1.IsPointInTriangle(t2.coord1))
            {
                IsAnyCornerInsideTriangle = true;
            }
            //Triangle 2 in triangle 1
            else if (t2.IsPointInTriangle(t1.coord1))
            {
                IsAnyCornerInsideTriangle = true;
            }

            //Step 3. Point in triangle intersection - if one of the triangles is inside the other
            if (IsAnyCornerInsideTriangle)
            {
                return true;
            }
        }

        return false;
    }

    //Check if 2 line segments are intersecting in 2d space
    //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
    //p1 and p2 belong to line 1, p3 and p4 belong to line 2
    private bool AreLineSegmentsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        bool isIntersecting = false;

        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        //Make sure the denominator is != 0, if 0 the lines are parallel
        if (denominator != 0)
        {
            float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            //TODO: check if this happens when triangles only touch but don't overlap, probably but doesn't seem to matter for this use case
            //Is intersecting if u_a and u_b are between 0 and 1
            if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

    public bool IsPointInTriangle(Vector2 p)
    {
        if (IsClockwise())
            return this.FlipCoordOrder().IsPointInTriangle(p);
        bool isWithinTriangle = false;

        float denominator = ((this.coord2.y - this.coord3.y) * (this.coord1.x - this.coord3.x) + (this.coord3.x - this.coord2.x) * (this.coord1.y - this.coord3.y));

        float a = ((this.coord2.y - this.coord3.y) * (p.x - this.coord3.x) + (this.coord3.x - this.coord2.x) * (p.y - this.coord3.y)) / denominator;
        float b = ((this.coord3.y - this.coord1.y) * (p.x - this.coord3.x) + (this.coord1.x - this.coord3.x) * (p.y - this.coord3.y)) / denominator;
        float c = 1 - a - b;

        //TODO: check if this happens when triangles only touch but don't overlap, probably but doesn't seem to matter for this use case
        //The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }


    private float matrixDet()
    {
        float a, b, c, d, e, f, g, h, i;
        a = coord1.x;
        b = coord1.y;
        c = 1;
        d = coord2.x;
        e = coord2.y;
        f = 1;
        g = coord3.x;
        h = coord3.y;
        i = 1;
        float det = a * e * i - a * f * h - b * d * i + b * f * g + c * d * h - c * e * g;
        return det;
    }
    public bool IsCounterClockWise()
    {
        return this.matrixDet() > 0;
    }

    public bool IsInLine()
    {
        return this.matrixDet() == 0;
    }

    public bool IsClockwise()
    {

        return this.matrixDet() < 0;
    }

    public Triangle2D FlipCoordOrder()
    {
        return new Triangle2D(coord3, coord2, coord1);
    }

    public PathD ToPathD()
    {
        PathD polygon = new PathD();
        polygon.Add(new PointD((double)coord1.x, (double)coord1.y));
        polygon.Add(new PointD((double)coord2.x, (double)coord2.y));
        polygon.Add(new PointD((double)coord3.x, (double)coord3.y));
        return polygon;
    }
}
