using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using SimpleFileBrowser;

public class TripletBuilder : MonoBehaviour
{

    public int dimensions = 5;


    public GameObject voxelGridPrefab;
    public GameObject cellGridPrefab;

    public VoxelGrid voxelGrid;
    public CellGrid topCellGrid, sideCellGrid, frontCellGrid;

    public bool test = false;

    //TODO: make toggleable using a button, maybe
    public bool autoDisplaySilhouetteCells = true;
    public bool autoDisplayCombinedMesh = true;

    // Start is called before the first frame update
    void Start()
    {
        //xz
        GameObject topCellGridObject = GameObject.Instantiate(cellGridPrefab, this.transform);
        topCellGridObject.transform.localPosition = new Vector3(0, dimensions + 4, 0);
        topCellGridObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        topCellGrid = topCellGridObject.GetComponent<CellGrid>();
        topCellGrid.parent = this;
        topCellGrid.cellGridAngle = CellGrid.CellGridAngle.Top;

        //zy
        GameObject sideCellGridObject = GameObject.Instantiate(cellGridPrefab, this.transform);
        sideCellGridObject.transform.localPosition = new Vector3(dimensions + 4, 0, 0);
        sideCellGridObject.transform.localRotation = Quaternion.Euler(0, -90, 0);
        sideCellGrid = sideCellGridObject.GetComponent<CellGrid>();
        sideCellGrid.parent = this;
        sideCellGrid.cellGridAngle = CellGrid.CellGridAngle.Side;

        //xy
        GameObject frontCellGridGridObject = GameObject.Instantiate(cellGridPrefab, this.transform);
        frontCellGridGridObject.transform.localPosition = new Vector3(0, 0, -4);
        frontCellGridGridObject.transform.localRotation = Quaternion.identity;
        frontCellGrid = frontCellGridGridObject.GetComponent<CellGrid>();
        frontCellGrid.parent = this;
        frontCellGrid.cellGridAngle = CellGrid.CellGridAngle.Front;

        //VoxelGrid
        GameObject VoxelGridObject = GameObject.Instantiate(voxelGridPrefab, this.transform);
        VoxelGridObject.transform.localPosition = new Vector3(0, 0, 0);
        VoxelGridObject.transform.localRotation = Quaternion.identity;
        voxelGrid = VoxelGridObject.GetComponent<VoxelGrid>();
        voxelGrid.parent = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!test)
            return;

        if (IsSilhouetteValid())
            Debug.Log($"Silhouette is valid{(frontCellGrid.IsEmpty() ? ", front has no active silhouette" : "")}" +
                $"{(topCellGrid.IsEmpty() ? ", top has no active silhouette" : "")}" +
                $"{(sideCellGrid.IsEmpty() ? ", side has no active silhouette" : "")}");
        else
        {
            if (frontCellGrid.IsSilhouetteValid())
                Debug.Log($"Front silhouette is valid{(frontCellGrid.IsEmpty() ? ", Because it has no active silhouette" :"" )}");
            if (topCellGrid.IsSilhouetteValid())
                Debug.Log($"Top silhouette is valid{(topCellGrid.IsEmpty() ? ", Because it has no active silhouette" : "")}");
            if (sideCellGrid.IsSilhouetteValid())
                Debug.Log($"Side silhouette is valid{(sideCellGrid.IsEmpty() ? ", Because it has no active silhouette" : "")}");
        }

        if (IsTripletConnected())
            Debug.Log("Triplet is connected");

        test = false;
    }

    public bool IsSilhouetteValid(int graphId = -1)
    {
        return frontCellGrid.IsSilhouetteValid(graphId) && topCellGrid.IsSilhouetteValid(graphId) && sideCellGrid.IsSilhouetteValid(graphId) ;
    }

    public bool IsTripletConnected()
    {
        return voxelGrid.IsTripletConnected();
    }
}