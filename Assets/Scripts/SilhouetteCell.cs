using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class SilhouetteCell : MonoBehaviour
{
    public int x, y;
    public CellGrid parent;
    public GameObject childShape;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Make this flatten the column and triangulate the polygon
    public void UpdateShape()
    {
        PathsD polygons = new PathsD();
        if (parent.cellGridAngle == CellGrid.CellGridAngle.Front)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnZ(x, y);
        }

        if (parent.cellGridAngle == CellGrid.CellGridAngle.Side)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnX(x, y);
        }

        if (parent.cellGridAngle == CellGrid.CellGridAngle.Top)
        {
            polygons = parent.parent.voxelGrid.FlattenVoxelColumnY(x, y);

        }

        Mesh mesh = new Mesh();

        foreach (PathD polygon in polygons)
        {

        }

        childShape.GetComponent<MeshFilter>().mesh = mesh;
        parent.CellUpdated(x, y);
    }
}
