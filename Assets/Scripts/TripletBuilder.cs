using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripletBuilder : MonoBehaviour
{

    public int dimensions = 5;


    public GameObject voxelGridPrefab;
    public GameObject cellGridPrefab;

    public VoxelGrid voxelGrid;
    public CellGrid topCellGrid, sideCellGrid, frontCellGrid;

    // Start is called before the first frame update
    void Start()
    {
        GameObject VoxelGridObject = GameObject.Instantiate(voxelGridPrefab, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
        voxelGrid = VoxelGridObject.GetComponent<VoxelGrid>();

        GameObject topCellGridObject = GameObject.Instantiate(cellGridPrefab, new Vector3(0, dimensions + 2, 0), Quaternion.identity, this.transform);
        topCellGrid = topCellGridObject.GetComponent<CellGrid>();

        GameObject sideCellGridObject = GameObject.Instantiate(cellGridPrefab, new Vector3(dimensions + 2, 0, 0), Quaternion.identity, this.transform);
        sideCellGrid = sideCellGridObject.GetComponent<CellGrid>();

        GameObject frontCellGridGridObject = GameObject.Instantiate(cellGridPrefab, new Vector3(0, 0, dimensions + 2), Quaternion.identity, this.transform);
        frontCellGrid = frontCellGridGridObject.GetComponent<CellGrid>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
