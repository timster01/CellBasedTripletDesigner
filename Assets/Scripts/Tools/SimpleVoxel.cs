using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVoxel
{

    public SimpleTripletBuilder parent;
    int x, y, z;
    public int graphId = -1;
    

    public enum ConnectedDegree { volume, edge, vertex};

    public SimpleVoxel(SimpleTripletBuilder parent, int x, int y, int z)
    {
        this.parent = parent;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SimpleCell GetFrontCell()
    {
        return parent.frontCellGrid[x, y];
    }

    public SimpleCell GetSideCell()
    {
        return parent.sideCellGrid[z, y];
    }

    public SimpleCell GetTopCell()
    {
        return parent.topCellGrid[x, z];
    }

    public VoxelConnectedLevel GetConnectedLevel()
    {
        return parent.connectedLevels[(int)GetFrontCell().fillValue, (int)GetSideCell().fillValue, (int)GetTopCell().fillValue];
    }

    public void PropagateDFS(int propagationGraphId, ConnectedDegree connectedDegree)
    {
        if (graphId < 0)
            return;

        graphId = propagationGraphId;
        List<SimpleVoxel> connectedVoxels;
        if (connectedDegree == ConnectedDegree.volume)
            connectedVoxels = VolumeConnectedVoxels();
        else if (connectedDegree == ConnectedDegree.edge)
            connectedVoxels = EdgeConnectedVoxels();
        else
            connectedVoxels = VertexConnectedVoxels();

        foreach (SimpleVoxel voxel in connectedVoxels)
            voxel.PropagateDFS(propagationGraphId, connectedDegree);
    }

    public List<SimpleVoxel> VolumeConnectedVoxels()
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();


        //TODO: add logic
        if (GetConnectedLevel().bottomFace && parent.voxelGrid[x, y - 1, z].GetConnectedLevel().topFace)
        {
            result.Add(parent.voxelGrid[x, y - 1, z]);
        }
        if (GetConnectedLevel().topFace && parent.voxelGrid[x - 1, y, z].GetConnectedLevel().bottomFace)
        {
            result.Add(parent.voxelGrid[x, y + 1, z]);
        }
        if (GetConnectedLevel().leftFace && parent.voxelGrid[x - 1, y, z].GetConnectedLevel().rightFace)
        {
            result.Add(parent.voxelGrid[x - 1, y, z]);
        }
        if (GetConnectedLevel().rightFace && parent.voxelGrid[x + 1, y, z].GetConnectedLevel().leftFace)
        {
            result.Add(parent.voxelGrid[x + 1, y, z]);
        }
        if (GetConnectedLevel().frontFace && parent.voxelGrid[x, y, z - 1].GetConnectedLevel().backFace)
        {
            result.Add(parent.voxelGrid[x, y, z - 1]);
        }
        if (GetConnectedLevel().backFace && parent.voxelGrid[x, y, z + 1].GetConnectedLevel().frontFace)
        {
            result.Add(parent.voxelGrid[x, y, z + 1]);
        }


        return result;
    }

    public List<SimpleVoxel> EdgeConnectedVoxels()
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();


        SimpleVoxel currentVoxel;
        //Each voxel at most one away in 2 directions except itself
        for (int xoffset = -1; xoffset <= 1; xoffset++)
            for (int yoffset = -1; yoffset <= 1; yoffset++)
                for (int zoffset = -1; zoffset <= 1; zoffset++)
                    if ((xoffset == 0 || yoffset == 0 || zoffset == 0) && !(xoffset == 0 && yoffset == 0 && zoffset == 0))
                    {
                        currentVoxel = parent.voxelGrid[x + xoffset, y + yoffset, z + zoffset];
                        //TODO: compare connected degree edges here
                    }


        return result;
    }

    public List<SimpleVoxel> VertexConnectedVoxels()
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();

        SimpleVoxel currentVoxel;
        Vector3 offsetVector;
        //Each voxel at most one away in all directions except itself
        for (int xoffset = -1; xoffset <= 1; xoffset++)
            for (int yoffset = -1; yoffset <= 1; yoffset++)
                for (int zoffset = -1; zoffset <= 1; zoffset++)
                    if (!(xoffset == 0 && yoffset == 0 && zoffset == 0))
                    {
                        currentVoxel = parent.voxelGrid[x + xoffset, y + yoffset, z + zoffset];
                        //TODO: compare connected degree vertices here
                    }



        return result;
    }

}

public class VoxelConnectedLevel{
    public bool empty;
    public bool topFace, bottomFace, leftFace, rightFace, frontFace, backFace;
    public bool[,] depthEdges, verticalEdges, horizontalEdges;
    public bool[,,] hasVertices;

    public VoxelConnectedLevel()
    {
        empty = true;
    }

    public VoxelConnectedLevel(Mesh mesh, Cell.FillValue frontFillValue, Cell.FillValue sideFillValue, Cell.FillValue topFillValue)
    {
        hasVertices = new bool[2, 2, 2];
        depthEdges = new bool[2, 2];
        verticalEdges = new bool[2, 2];
        horizontalEdges = new bool[2, 2];
        if(frontFillValue == Cell.FillValue.Empty || sideFillValue == Cell.FillValue.Empty || topFillValue == Cell.FillValue.Empty || mesh.vertexCount == 0)
        {
            empty = true;
            return;
        }
        topFace = (frontFillValue == Cell.FillValue.TopLeft || frontFillValue == Cell.FillValue.TopRight) && 
            (sideFillValue == Cell.FillValue.TopLeft || sideFillValue == Cell.FillValue.TopRight);
        bottomFace = (frontFillValue == Cell.FillValue.BottomLeft || frontFillValue == Cell.FillValue.BottomRight) && 
            (sideFillValue == Cell.FillValue.BottomLeft || sideFillValue == Cell.FillValue.BottomRight);
        frontFace = (topFillValue == Cell.FillValue.BottomLeft || topFillValue == Cell.FillValue.BottomRight) &&
            (sideFillValue == Cell.FillValue.BottomLeft || sideFillValue == Cell.FillValue.TopLeft);
        backFace = (topFillValue == Cell.FillValue.TopLeft || topFillValue == Cell.FillValue.TopRight) &&
            (sideFillValue == Cell.FillValue.BottomRight || sideFillValue == Cell.FillValue.TopRight);
        leftFace = (topFillValue == Cell.FillValue.TopLeft || topFillValue == Cell.FillValue.BottomLeft) &&
            (frontFillValue == Cell.FillValue.BottomLeft || frontFillValue == Cell.FillValue.TopLeft);
        rightFace = (topFillValue == Cell.FillValue.TopRight || topFillValue == Cell.FillValue.BottomRight) &&
            (frontFillValue == Cell.FillValue.BottomRight || frontFillValue == Cell.FillValue.TopRight);
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                for (int z = 0; z < 2; z++)
                {
                    hasVertices[x, y, z] = vertices.Contains(new Vector3(-0.5f + x, -0.5f + y, -0.5f + z));
                }

        //TODO: set for edges
    }
}
