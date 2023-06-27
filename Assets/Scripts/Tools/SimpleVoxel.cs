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

    public int PropagateDFS(int propagationGraphId, ConnectedDegree connectedDegree)
    {
        if (graphId >= 0 || GetConnectedLevel().empty)
            return 0;

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
        return 1;
    }

    public List<SimpleVoxel> VolumeConnectedVoxels()
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();


        if ( y-1 >= 0 && GetConnectedLevel().bottomFace && parent.voxelGrid[x, y - 1, z].GetConnectedLevel().topFace)
        {
            result.Add(parent.voxelGrid[x, y - 1, z]);
        }
        if (y + 1 < parent.gridSize && GetConnectedLevel().topFace && parent.voxelGrid[x, y + 1, z].GetConnectedLevel().bottomFace)
        {
            result.Add(parent.voxelGrid[x, y + 1, z]);
        }
        if (x - 1 >= 0 && GetConnectedLevel().leftFace && parent.voxelGrid[x - 1, y, z].GetConnectedLevel().rightFace)
        {
            result.Add(parent.voxelGrid[x - 1, y, z]);
        }
        if (x + 1 < parent.gridSize && GetConnectedLevel().rightFace && parent.voxelGrid[x + 1, y, z].GetConnectedLevel().leftFace)
        {
            result.Add(parent.voxelGrid[x + 1, y, z]);
        }
        if (z - 1 >= 0 && GetConnectedLevel().frontFace && parent.voxelGrid[x, y, z - 1].GetConnectedLevel().backFace)
        {
            result.Add(parent.voxelGrid[x, y, z - 1]);
        }
        if (z + 1 < parent.gridSize && GetConnectedLevel().backFace && parent.voxelGrid[x, y, z + 1].GetConnectedLevel().frontFace)
        {
            result.Add(parent.voxelGrid[x, y, z + 1]);
        }


        return result;
    }

    public List<SimpleVoxel> EdgeConnectedVoxels()
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();

        bool[,,] addToResult = new bool[3, 3, 3];
        VoxelConnectedLevel localConnectedLevel = this.GetConnectedLevel();
        SimpleVoxel currentVoxel;
        VoxelConnectedLevel currentConnectedLevel;
        int xoffset, yoffset, zoffset;
        for (int offset1 = -1; offset1 <= 1; offset1++)
            for (int offset2 = -1; offset2 <= 1; offset2++)
                if (!(offset1 == 0 && offset2 == 0))
                {
                    //Depth edges
                    xoffset = offset1;
                    yoffset = offset2;
                    if(!(x + xoffset >= parent.gridSize || y + yoffset >= parent.gridSize ||
                        x + xoffset < 0 || y + yoffset < 0))
                    {
                        currentVoxel = parent.voxelGrid[x + xoffset, y + yoffset, z];
                        currentConnectedLevel = currentVoxel.GetConnectedLevel();
                        for (int i = Mathf.Max(0, xoffset); i <= Mathf.Min(1, 1 + xoffset); i++)
                            for (int j = Mathf.Max(0, yoffset); j <= Mathf.Min(1, 1 + yoffset); j++)
                            {
                                if (localConnectedLevel.depthEdges[i, j] && currentConnectedLevel.depthEdges[i - xoffset, j - yoffset])
                                    addToResult[xoffset + 1, yoffset + 1, 1] = true;
                            }
                    }
                    

                    //Horizontal edges
                    yoffset = offset1;
                    zoffset = offset2;
                    if (!(z + zoffset >= parent.gridSize || y + yoffset >= parent.gridSize ||
                        z + zoffset < 0 || y + yoffset < 0))
                    {
                        currentVoxel = parent.voxelGrid[x, y + yoffset, z + zoffset];
                        currentConnectedLevel = currentVoxel.GetConnectedLevel();
                        for (int i = Mathf.Max(0, yoffset); i <= Mathf.Min(1, 1 + yoffset); i++)
                            for (int j = Mathf.Max(0, zoffset); j <= Mathf.Min(1, 1 + zoffset); j++)
                            {
                                if (localConnectedLevel.horizontalEdges[i, j] && currentConnectedLevel.horizontalEdges[i - yoffset, j - zoffset])
                                    addToResult[1, yoffset + 1, zoffset + 1] = true;
                            }
                    }

                    //Vertical edges
                    xoffset = offset1;
                    zoffset = offset2;
                    if (!(x + xoffset >= parent.gridSize || z + zoffset >= parent.gridSize ||
                        x + xoffset < 0 || z + zoffset < 0))
                    {
                        currentVoxel = parent.voxelGrid[x + xoffset, y, z + zoffset];
                        currentConnectedLevel = currentVoxel.GetConnectedLevel();
                        for (int i = Mathf.Max(0, xoffset); i <= Mathf.Min(1, 1 + xoffset); i++)
                            for (int j = Mathf.Max(0, zoffset); j <= Mathf.Min(1, 1 + zoffset); j++)
                            {
                                if (localConnectedLevel.verticalEdges[i, j] && currentConnectedLevel.verticalEdges[i - xoffset, j - zoffset])
                                    addToResult[xoffset + 1, 1, zoffset + 1] = true;
                            }
                    }
                }
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                for (int k = 0; k < 3; k++)
                    if (addToResult[i, j, k])
                        result.Add(parent.voxelGrid[x - 1 + i, y - 1 + j, z - 1 + k]);

        return result;
    }

    public List<SimpleVoxel> VertexConnectedVoxels()
    {
        List<SimpleVoxel> result = new List<SimpleVoxel>();

        SimpleVoxel currentVoxel;
        bool breakBool;
        //Each voxel at most one away in all directions except itself
        for (int xoffset = -1; xoffset <= 1; xoffset++)
            for (int yoffset = -1; yoffset <= 1; yoffset++)
                for (int zoffset = -1; zoffset <= 1; zoffset++)
                    if (!(xoffset == 0 && yoffset == 0 && zoffset == 0) &&
                        (x + xoffset < parent.gridSize && y + yoffset < parent.gridSize && z + zoffset < parent.gridSize &&
                        x + xoffset >= 0 && y + yoffset >= 0 && z + zoffset >= 0))
                    {
                        breakBool = false;
                        currentVoxel = parent.voxelGrid[x + xoffset, y + yoffset, z + zoffset];
                        for (int i = Mathf.Max(0, xoffset); i <= Mathf.Min(1, 1 + xoffset); i++)
                        {
                            for (int j = Mathf.Max(0, yoffset); j <= Mathf.Min(1, 1 + yoffset); j++)
                            {
                                for (int k = Mathf.Max(0, zoffset); k <= Mathf.Min(1, 1 + zoffset); k++)
                                    if (this.GetConnectedLevel().hasVertices[i, j, k] && currentVoxel.GetConnectedLevel().hasVertices[i - xoffset, j - yoffset, k - zoffset])
                                    {
                                        result.Add(currentVoxel);
                                        breakBool = true;
                                        break;
                                    }
                                if (breakBool)
                                    break;
                            }
                            if (breakBool)
                                break;
                        }            
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
        hasVertices = new bool[2, 2, 2];
        depthEdges = new bool[2, 2];
        verticalEdges = new bool[2, 2];
        horizontalEdges = new bool[2, 2];
        topFace = bottomFace = leftFace = rightFace = frontFace = backFace = false;
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
            topFace = bottomFace = leftFace = rightFace = frontFace = backFace = false;
            return;
        }
        //Detect which faces exist by checking if the face exists in all intersecting shapes
        topFace = (frontFillValue == Cell.FillValue.TopLeft || frontFillValue == Cell.FillValue.TopRight || frontFillValue == Cell.FillValue.Full) && 
            (sideFillValue == Cell.FillValue.TopLeft || sideFillValue == Cell.FillValue.TopRight || sideFillValue == Cell.FillValue.Full);
        bottomFace = (frontFillValue == Cell.FillValue.BottomLeft || frontFillValue == Cell.FillValue.BottomRight || frontFillValue == Cell.FillValue.Full) && 
            (sideFillValue == Cell.FillValue.BottomLeft || sideFillValue == Cell.FillValue.BottomRight || sideFillValue == Cell.FillValue.Full);
        frontFace = (topFillValue == Cell.FillValue.BottomLeft || topFillValue == Cell.FillValue.BottomRight || topFillValue == Cell.FillValue.Full) &&
            (sideFillValue == Cell.FillValue.BottomLeft || sideFillValue == Cell.FillValue.TopLeft || sideFillValue == Cell.FillValue.Full);
        backFace = (topFillValue == Cell.FillValue.TopLeft || topFillValue == Cell.FillValue.TopRight || topFillValue == Cell.FillValue.Full) &&
            (sideFillValue == Cell.FillValue.BottomRight || sideFillValue == Cell.FillValue.TopRight || sideFillValue == Cell.FillValue.Full);
        leftFace = (topFillValue == Cell.FillValue.TopLeft || topFillValue == Cell.FillValue.BottomLeft || topFillValue == Cell.FillValue.Full) &&
            (frontFillValue == Cell.FillValue.BottomLeft || frontFillValue == Cell.FillValue.TopLeft || frontFillValue == Cell.FillValue.Full);
        rightFace = (topFillValue == Cell.FillValue.TopRight || topFillValue == Cell.FillValue.BottomRight || topFillValue == Cell.FillValue.Full) &&
            (frontFillValue == Cell.FillValue.BottomRight || frontFillValue == Cell.FillValue.TopRight || frontFillValue == Cell.FillValue.Full);
        //Check if the extreme corner vertices exist, since those are the only vertices that can exist on the outside of the voxel
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                for (int z = 0; z < 2; z++)
                {
                    hasVertices[x, y, z] = vertices.Contains(new Vector3(-0.5f + x, -0.5f + y, -0.5f + z));
                }
        //Edges are detected by checking if both of its endpoints exist.
        //This works because both vertices for the extreme edges of the voxel only exist when the edge exists.
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                depthEdges[i, j] = vertices.Contains(new Vector3(-0.5f + i, -0.5f + j, -0.5f)) && vertices.Contains(new Vector3(-0.5f + i, -0.5f + j, 0.5f));
                verticalEdges[i, j] = vertices.Contains(new Vector3(-0.5f + i, -0.5f, -0.5f + j)) && vertices.Contains(new Vector3(-0.5f + i, 0.5f, -0.5f + j));
                horizontalEdges[i, j] = vertices.Contains(new Vector3(-0.5f, -0.5f + i, -0.5f + j)) && vertices.Contains(new Vector3(0.5f, -0.5f + i, -0.5f + j));
            }
    }
}
