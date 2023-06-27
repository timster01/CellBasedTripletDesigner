using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public int x, y, z;
    public GameObject childShape;
    public VoxelGrid parent;
    public int graphId;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if(graphId != -1)
            childShape.GetComponent<MeshRenderer>().material.color = parent.GetGraphColor(graphId);
    }

    public bool IsConnectedUp()
    {
        if (y == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y + 1, z);
        List<Triangle> ownTriangles = Triangle.MeshToTriangles(childShape.GetComponent<MeshFilter>().mesh);
        List<Triangle> adjacentTriangles = Triangle.MeshToTriangles(adjacentVoxel.childShape.GetComponent<MeshFilter>().mesh);
        if (ownTriangles.Count == 0 || adjacentTriangles.Count == 0)
            return false;
        List<Triangle2D> ownTriangles2D = new List<Triangle2D>();
        List<Triangle2D> adjacentTriangles2D = new List<Triangle2D>();
        foreach (Triangle triangle in ownTriangles)
        {
            if (triangle.coord1.y == 0.5f && triangle.coord2.y == 0.5f && triangle.coord3.y == 0.5f)
                ownTriangles2D.Add(triangle.FlattenY());
        }
        foreach (Triangle triangle in adjacentTriangles)
        {
            if (triangle.coord1.y == -0.5f && triangle.coord2.y == -0.5f && triangle.coord3.y == -0.5f)
                adjacentTriangles2D.Add(triangle.FlattenY());
        }
        if (ownTriangles2D.Count == 0 || adjacentTriangles2D.Count == 0)
            return false;

        foreach (Triangle2D t1 in ownTriangles2D)
        {
            foreach (Triangle2D t2 in adjacentTriangles2D)
            {
                if (t1.TriangleOverlaps(t2))
                    return true;
            }
        }

        return false;
    }

    public bool IsConnectedDown()
    {
        if (y == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y - 1, z);
        return adjacentVoxel.IsConnectedUp();
    }

    public bool IsConnectedRight()
    {
        if (x == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x + 1, y, z);
        List<Triangle> ownTriangles = Triangle.MeshToTriangles(childShape.GetComponent<MeshFilter>().mesh);
        List<Triangle> adjacentTriangles = Triangle.MeshToTriangles(adjacentVoxel.childShape.GetComponent<MeshFilter>().mesh);
        if (ownTriangles.Count == 0 || adjacentTriangles.Count == 0)
            return false;
        List<Triangle2D> ownTriangles2D = new List<Triangle2D>();
        List<Triangle2D> adjacentTriangles2D = new List<Triangle2D>();
        foreach(Triangle triangle in ownTriangles)
        {
            if (triangle.coord1.x == 0.5f && triangle.coord2.x == 0.5f && triangle.coord3.x == 0.5f)
                ownTriangles2D.Add(triangle.FlattenX());
        }
        foreach (Triangle triangle in adjacentTriangles)
        {
            if (triangle.coord1.x == -0.5f && triangle.coord2.x == -0.5f && triangle.coord3.x == -0.5f)
                adjacentTriangles2D.Add(triangle.FlattenX());
        }
        if (ownTriangles2D.Count == 0 || adjacentTriangles2D.Count == 0)
            return false;

        foreach(Triangle2D t1 in ownTriangles2D)
        {
            foreach(Triangle2D t2 in adjacentTriangles2D)
            {
                if (t1.TriangleOverlaps(t2))
                    return true;
            }
        }

        return false;
    }
    public bool IsConnectedLeft()
    {
        if (x == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x - 1, y, z);
        return adjacentVoxel.IsConnectedRight();
    }

    public bool IsConnectedBack()
    {
        if (z == parent.dimension - 1)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z + 1);
        List<Triangle> ownTriangles = Triangle.MeshToTriangles(childShape.GetComponent<MeshFilter>().mesh);
        List<Triangle> adjacentTriangles = Triangle.MeshToTriangles(adjacentVoxel.childShape.GetComponent<MeshFilter>().mesh);
        if (ownTriangles.Count == 0 || adjacentTriangles.Count == 0)
            return false;
        List<Triangle2D> ownTriangles2D = new List<Triangle2D>();
        List<Triangle2D> adjacentTriangles2D = new List<Triangle2D>();
        foreach (Triangle triangle in ownTriangles)
        {
            if (triangle.coord1.z == 0.5f && triangle.coord2.z == 0.5f && triangle.coord3.z == 0.5f)
                ownTriangles2D.Add(triangle.FlattenZ());
        }
        foreach (Triangle triangle in adjacentTriangles)
        {
            if (triangle.coord1.z == -0.5f && triangle.coord2.z == -0.5f && triangle.coord3.z == -0.5f)
                adjacentTriangles2D.Add(triangle.FlattenZ());
        }
        if (ownTriangles2D.Count == 0 || adjacentTriangles2D.Count == 0)
            return false;

        foreach (Triangle2D t1 in ownTriangles2D)
        {
            foreach (Triangle2D t2 in adjacentTriangles2D)
            {
                if (t1.TriangleOverlaps(t2))
                    return true;
            }
        }

        return false;
    }
    public bool IsConnectedFront()
    {
        if (z == 0)
            return false;
        Voxel adjacentVoxel = parent.GetVoxelAtCoords(x, y, z - 1);
        return adjacentVoxel.IsConnectedBack();
    }

    public List<Voxel> ConnectedVoxels(bool volume = true, bool edge = false, bool vertex = false)
    {
        if (vertex)
            return VertexConnectedVoxels();
        if (edge)
            return EdgeConnectedVoxels();
        return VolumeConnectedVoxels();
    }

    public List<Voxel> VolumeConnectedVoxels()
    {
        List<Voxel> result = new List<Voxel>();
        if (IsConnectedDown())
        {
            result.Add(parent.GetVoxelAtCoords(x, y - 1, z));
        }
        if (IsConnectedUp())
        {
            result.Add(parent.GetVoxelAtCoords(x, y + 1, z));
        }
        if (IsConnectedLeft())
        {
            result.Add(parent.GetVoxelAtCoords(x - 1, y, z));
        }
        if (IsConnectedRight())
        {
            result.Add(parent.GetVoxelAtCoords(x + 1, y, z));
        }
        if (IsConnectedFront())
        {
            result.Add(parent.GetVoxelAtCoords(x, y, z - 1));
        }
        if (IsConnectedBack())
        {
            result.Add(parent.GetVoxelAtCoords(x, y, z + 1));
        }
        return result;
    }

    public List<Voxel> EdgeConnectedVoxels()
    {
        List<Voxel> result = new List<Voxel>();
        Voxel currentVoxel;
        Mesh thisMesh = childShape.GetComponent<MeshFilter>().mesh;
        List<(Vector3, Vector3)> thisEdges = new List<(Vector3, Vector3)>();
        for (int i = 0; i < thisMesh.triangles.Length; i += 3)
        {
            thisEdges.Add((thisMesh.vertices[thisMesh.triangles[0]], thisMesh.vertices[thisMesh.triangles[1]]));
            thisEdges.Add((thisMesh.vertices[thisMesh.triangles[1]], thisMesh.vertices[thisMesh.triangles[2]]));
            thisEdges.Add((thisMesh.vertices[thisMesh.triangles[2]], thisMesh.vertices[thisMesh.triangles[0]]));
        }
        Mesh currentMesh;
        List<(Vector3, Vector3)> currentEdges;
        Vector3 offsetVector;
        //Each voxel at most one away in 2 directions except itself
        for (int xoffset = -1; xoffset <= 1; xoffset++)
            for (int yoffset = -1; yoffset <= 1; yoffset++)
                for (int zoffset = -1; zoffset <= 1; zoffset++)
                    if((xoffset == 0 || yoffset == 0 || zoffset == 0) && ! (xoffset == 0 && yoffset == 0 && zoffset == 0))
                    {
                        offsetVector = new Vector3(xoffset, yoffset, zoffset);
                        currentVoxel = parent.GetVoxelAtCoords(x + xoffset, y + yoffset, z + zoffset);
                        currentMesh = currentVoxel.childShape.GetComponent<MeshFilter>().mesh;
                        currentEdges = new List<(Vector3, Vector3)>();
                        for (int i = 0; i < currentMesh.triangles.Length; i+=3)
                        {
                            currentEdges.Add((currentMesh.vertices[currentMesh.triangles[0]] + offsetVector, currentMesh.vertices[currentMesh.triangles[1]] + offsetVector));
                            currentEdges.Add((currentMesh.vertices[currentMesh.triangles[1]] + offsetVector, currentMesh.vertices[currentMesh.triangles[2]] + offsetVector));
                            currentEdges.Add((currentMesh.vertices[currentMesh.triangles[2]] + offsetVector, currentMesh.vertices[currentMesh.triangles[0]] + offsetVector));
                        }
                        foreach((Vector3,Vector3)edge in currentEdges)
                        {
                            if(thisEdges.Contains(edge) || thisEdges.Contains((edge.Item2, edge.Item1)))
                            {
                                result.Add(currentVoxel);
                                break;
                            }
                        }
                    }
        return result;
    }

    public List<Voxel> VertexConnectedVoxels()
    {
        List<Voxel> result = new List<Voxel>();
        Voxel currentVoxel;
        Mesh thisMesh = childShape.GetComponent<MeshFilter>().mesh;
        List<Vector3> thisVertices = new List<Vector3>(thisMesh.vertices);
        Mesh currentMesh;
        Vector3 offsetVector;
        //Each voxel at most one away in all directions except itself
        for (int xoffset = -1; xoffset <= 1; xoffset++)
            for (int yoffset = -1; yoffset <= 1; yoffset++)
                for (int zoffset = -1; zoffset <= 1; zoffset++)
                    if (!(xoffset == 0 && yoffset == 0 && zoffset == 0))
                    {
                        offsetVector = new Vector3(xoffset, yoffset, zoffset);
                        currentVoxel = parent.GetVoxelAtCoords(x + xoffset, y + yoffset, z + zoffset);
                        currentMesh = currentVoxel.childShape.GetComponent<MeshFilter>().mesh;
                        foreach(Vector3 vertex in currentMesh.vertices)
                        {
                            if (thisVertices.Contains(vertex + offsetVector))
                            {
                                result.Add(currentVoxel);
                                break;
                            }
                        }
                    }
        return result;
    }

    public void UpdateVoxel()
    {
        Cell.FillValue frontCellFillValue = parent.parent.frontCellGrid.GetCellAtCoords(x,y).CurrentFillValue;
        if (parent.parent.frontCellGrid.IsEmpty())
            frontCellFillValue = Cell.FillValue.Full;
        Cell.FillValue sideCellFillValue = parent.parent.sideCellGrid.GetCellAtCoords(z, y).CurrentFillValue;
        if (parent.parent.sideCellGrid.IsEmpty())
            sideCellFillValue = Cell.FillValue.Full;
        Cell.FillValue topCellFillValue = parent.parent.topCellGrid.GetCellAtCoords(x, z).CurrentFillValue;
        if (parent.parent.topCellGrid.IsEmpty())
            topCellFillValue = Cell.FillValue.Full;
        SetVoxel(frontCellFillValue, sideCellFillValue, topCellFillValue);
    }

    void SetVoxel(Cell.FillValue frontFillValue, Cell.FillValue sideFillValue, Cell.FillValue topFillValue)
    {
        if (frontFillValue == Cell.FillValue.Empty || sideFillValue == Cell.FillValue.Empty || topFillValue == Cell.FillValue.Empty)
        {
            childShape.GetComponent<MeshFilter>().mesh.Clear();
            graphId = -1;
        }
        string objPath = $"{frontFillValue.ToString()}-{sideFillValue.ToString()}-{topFillValue.ToString()}";
        Mesh mesh = Resources.Load<Mesh>(objPath);
        childShape.GetComponent<MeshFilter>().mesh = mesh;
    }
}