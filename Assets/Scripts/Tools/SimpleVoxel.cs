using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVoxel
{

    public SimpleTripletBuilder parent;
    int x, y, z;
    public int graphId;

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
}
