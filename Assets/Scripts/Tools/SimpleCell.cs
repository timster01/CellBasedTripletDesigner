using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCell
{

    public SimpleTripletBuilder parent;
    int x, y;
    CellGrid.CellGridAngle CellGridAngle;
    public Cell.FillValue fillValue;

    public SimpleCell(SimpleTripletBuilder parent, int x, int y, CellGrid.CellGridAngle cellGridAngle)
    {
        this.parent = parent;
        this.x = x;
        this.y = y;
        this.CellGridAngle = cellGridAngle;
    }

    public bool IsValid(int graphId = -1)
    {
        if (fillValue == Cell.FillValue.Empty)
            return true;
        string[,,] silhouetteShapes;
        List<SimpleVoxel> voxels;
        if (CellGridAngle == CellGrid.CellGridAngle.Front)
        {
            silhouetteShapes = parent.shapeSilhouettesFront;
            voxels = parent.GetVoxelColumnFront(graphId, x, y);
        } 
        else if (CellGridAngle == CellGrid.CellGridAngle.Side)
        {
            silhouetteShapes = parent.shapeSilhouettesSide;
            voxels = parent.GetVoxelColumnSide(graphId, x, y);
        }
        else
        {
            silhouetteShapes = parent.shapeSilhouettesTop;
            voxels = parent.GetVoxelColumnTop(graphId, x, y);
        }
            
        bool left, right, top, bottom;
        left = right = top = bottom = false;
        foreach (SimpleVoxel voxel in voxels)
        {
            string silhouetteShape = silhouetteShapes[(int)voxel.GetFrontCell().fillValue, (int)voxel.GetSideCell().fillValue, (int)voxel.GetTopCell().fillValue];
            if(silhouetteShape == "Full")
            {
                left = right = top = bottom = true;
                continue;
            }
            if (silhouetteShape == "Top")
            {
                top = true;
                continue;
            }
            if (silhouetteShape == "Bottom")
            {
                bottom = true;
                continue;
            }
            if (silhouetteShape == "Left")
            {
                left = true;
                continue;
            }
            if (silhouetteShape == "Right")
            {
                right = true;
                continue;
            }
            if (silhouetteShape == "BottomLeft")
            {
                left = bottom = true;
                continue;
            }
            if (silhouetteShape == "BottomRight")
            {
                right= bottom = true;
                continue;
            }
            if (silhouetteShape == "TopLeft")
            {
                left= top = true;
                continue;
            }
            if (silhouetteShape == "TopRight")
            {
                right = top = true;
                continue;
            }
        }
        if (fillValue == Cell.FillValue.Full && top && bottom && left && right)
            return true;
        if (fillValue == Cell.FillValue.BottomLeft && bottom && left)
            return true;
        if (fillValue == Cell.FillValue.BottomRight && bottom && right)
            return true;
        if (fillValue == Cell.FillValue.TopLeft && top && left)
            return true;
        if (fillValue == Cell.FillValue.TopRight && top && right)
            return true;

        //Not valid if it isn't covered for the parts relevant to the fillvalue
        return false;

    }
}
