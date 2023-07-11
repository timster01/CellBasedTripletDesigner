using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;

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

    public TextMeshPro textField;

    SimpleTripletBuilder tripletBuilderDuplicate;

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

        tripletBuilderDuplicate = new SimpleTripletBuilder(dimensions);
    }

    // Update is called once per frame
    void Update()
    {
        if (!test)
            return;

        if (AreSilhouettesValid())
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

    public void FindBestShapeOrientation()
    {
        
        tripletBuilderDuplicate.loadShapeFromFillValueGrid(tripletBuilderDuplicate.frontCellGrid, frontCellGrid.GridFillValues());
        tripletBuilderDuplicate.loadShapeFromFillValueGrid(tripletBuilderDuplicate.sideCellGrid, sideCellGrid.GridFillValues());
        tripletBuilderDuplicate.loadShapeFromFillValueGrid(tripletBuilderDuplicate.topCellGrid, topCellGrid.GridFillValues());

        int previousGraphCount;
        int rotFrontBest, rotSideBest, rotTopBest, mirFrontBest, mirSideBest, mirTopBest;
        rotFrontBest = rotSideBest = rotTopBest = mirFrontBest = mirSideBest = mirTopBest = 0;
        bool volumeConnectedValidGraphFound = false;
        bool volumeConnectedValidSubgraphFound = false;
        bool edgeConnectedValidGraphFound = false;
        bool edgeConnectedValidSubgraphFound = false;
        bool vertexConnectedValidGraphFound = false;
        bool vertexConnectedValidSubgraphFound = false;
        bool unconnectedValidGraphFound = false;

        for (int rotFront = 0; rotFront < 4; rotFront++)
        {
            for (int rotSide = 0; rotSide < 4; rotSide++)
            {
                for (int rotTop = 0; rotTop < 4; rotTop++)
                {
                    for (int mirFront = 0; mirFront < 2; mirFront++)
                    {
                        for (int mirSide = 0; mirSide < 2; mirSide++)
                        {
                            for (int mirTop = 0; mirTop < 2; mirTop++)
                            {
                                //Handle this situation for the current shapeset
                                while (true)
                                {

                                    //Volume connected
                                    if (volumeConnectedValidGraphFound)
                                        break;

                                    tripletBuilderDuplicate.MarkGraphId(SimpleVoxel.ConnectedDegree.volume);
                                    if (tripletBuilderDuplicate.graphCount == 1)
                                    {
                                        if (tripletBuilderDuplicate.AreSilhouettesValid())
                                        {
                                            volumeConnectedValidGraphFound = true;
                                            rotFrontBest = rotFront;
                                            rotSideBest = rotSide;
                                            rotTopBest = rotTop;
                                            mirFrontBest = mirFront;
                                            mirSideBest = mirSide;
                                            mirTopBest = mirTop;
                                            break;
                                        }
                                    }
                                    if (volumeConnectedValidSubgraphFound)
                                        break;
                                    if (tripletBuilderDuplicate.graphCount > 1)
                                    {
                                        for (int i = 0; i < tripletBuilderDuplicate.graphCount; i++)
                                        {
                                            if (tripletBuilderDuplicate.AreSilhouettesValid(i))
                                            {
                                                volumeConnectedValidSubgraphFound = true;
                                                rotFrontBest = rotFront;
                                                rotSideBest = rotSide;
                                                rotTopBest = rotTop;
                                                mirFrontBest = mirFront;
                                                mirSideBest = mirSide;
                                                mirTopBest = mirTop;
                                                break;
                                            }
                                        }
                                        if (volumeConnectedValidSubgraphFound)
                                            break;
                                    }
                                    previousGraphCount = tripletBuilderDuplicate.graphCount;

                                    //Edge connected
                                    if (edgeConnectedValidGraphFound)
                                        break;

                                    tripletBuilderDuplicate.MarkGraphId(SimpleVoxel.ConnectedDegree.edge);

                                    if (tripletBuilderDuplicate.graphCount < previousGraphCount)
                                    {
                                        if (tripletBuilderDuplicate.graphCount == 1)
                                        {
                                            if (tripletBuilderDuplicate.AreSilhouettesValid())
                                            {
                                                edgeConnectedValidGraphFound = true;
                                                rotFrontBest = rotFront;
                                                rotSideBest = rotSide;
                                                rotTopBest = rotTop;
                                                mirFrontBest = mirFront;
                                                mirSideBest = mirSide;
                                                mirTopBest = mirTop;
                                                break;
                                            }
                                        }
                                        if (edgeConnectedValidSubgraphFound)
                                            break;
                                        if (tripletBuilderDuplicate.graphCount > 1)
                                        {
                                            for (int i = 0; i < tripletBuilderDuplicate.graphCount; i++)
                                            {
                                                if (tripletBuilderDuplicate.AreSilhouettesValid(i))
                                                {
                                                    edgeConnectedValidSubgraphFound = true;
                                                    rotFrontBest = rotFront;
                                                    rotSideBest = rotSide;
                                                    rotTopBest = rotTop;
                                                    mirFrontBest = mirFront;
                                                    mirSideBest = mirSide;
                                                    mirTopBest = mirTop;
                                                    break;
                                                }
                                                if (edgeConnectedValidSubgraphFound)
                                                    break;
                                            }
                                        }
                                        previousGraphCount = tripletBuilderDuplicate.graphCount;
                                    }

                                    //Vertex connected
                                    if (vertexConnectedValidGraphFound)
                                        break;

                                    tripletBuilderDuplicate.MarkGraphId(SimpleVoxel.ConnectedDegree.vertex);

                                    if (tripletBuilderDuplicate.graphCount < previousGraphCount)
                                    {
                                        if (tripletBuilderDuplicate.graphCount == 1)
                                        {
                                            if (tripletBuilderDuplicate.AreSilhouettesValid())
                                            {
                                                vertexConnectedValidGraphFound = true;
                                                rotFrontBest = rotFront;
                                                rotSideBest = rotSide;
                                                rotTopBest = rotTop;
                                                mirFrontBest = mirFront;
                                                mirSideBest = mirSide;
                                                mirTopBest = mirTop;
                                                break;
                                            }
                                        }
                                        if (vertexConnectedValidSubgraphFound)
                                            break;
                                        if (tripletBuilderDuplicate.graphCount > 1)
                                        {
                                            for (int i = 0; i < tripletBuilderDuplicate.graphCount; i++)
                                            {
                                                if (tripletBuilderDuplicate.AreSilhouettesValid(i))
                                                {
                                                    vertexConnectedValidSubgraphFound = true;
                                                    rotFrontBest = rotFront;
                                                    rotSideBest = rotSide;
                                                    rotTopBest = rotTop;
                                                    mirFrontBest = mirFront;
                                                    mirSideBest = mirSide;
                                                    mirTopBest = mirTop;
                                                    break;
                                                }
                                                if (vertexConnectedValidSubgraphFound)
                                                    break;
                                            }
                                        }
                                    }

                                    //Unconnected but valid
                                    if (unconnectedValidGraphFound)
                                        break;

                                    if (tripletBuilderDuplicate.graphCount > 1 && tripletBuilderDuplicate.AreSilhouettesValid())
                                    {
                                        unconnectedValidGraphFound = true;
                                        rotFrontBest = rotFront;
                                        rotSideBest = rotSide;
                                        rotTopBest = rotTop;
                                        mirFrontBest = mirFront;
                                        mirSideBest = mirSide;
                                        mirTopBest = mirTop;
                                    }
                                        

                                    //Not valid at all
                                    break;
                                }
                                //Finished this situation for the current shapesets

                                if (volumeConnectedValidGraphFound)
                                    volumeConnectedValidSubgraphFound = true;

                                if (volumeConnectedValidSubgraphFound)
                                    edgeConnectedValidGraphFound = true;

                                if (edgeConnectedValidGraphFound)
                                    edgeConnectedValidSubgraphFound = true;

                                if (edgeConnectedValidSubgraphFound)
                                    vertexConnectedValidGraphFound = true;

                                if (vertexConnectedValidGraphFound)
                                    vertexConnectedValidSubgraphFound = true;

                                if (vertexConnectedValidSubgraphFound)
                                    unconnectedValidGraphFound = true;

                                //Mirrored after first unmirrored cycle and reverses mirroring after the second mirrored cycle
                                tripletBuilderDuplicate.MirrorLeftRight(tripletBuilderDuplicate.topCellGrid);
                            }
                            tripletBuilderDuplicate.MirrorLeftRight(tripletBuilderDuplicate.sideCellGrid);
                        }
                        tripletBuilderDuplicate.MirrorLeftRight(tripletBuilderDuplicate.frontCellGrid);
                    }
                    tripletBuilderDuplicate.RotateClockWise(tripletBuilderDuplicate.topCellGrid);
                }
                tripletBuilderDuplicate.RotateClockWise(tripletBuilderDuplicate.sideCellGrid);
            }
            tripletBuilderDuplicate.RotateClockWise(tripletBuilderDuplicate.frontCellGrid);
        }

        //Orient like best scenario or does nothing if no valid scenario was found since all best values are 0
        for (int rotFront = 0; rotFront < rotFrontBest; rotFront++)
            this.frontCellGrid.RotateClockWise();
        for (int rotSide = 0; rotSide < rotSideBest; rotSide++)
            this.sideCellGrid.RotateClockWise();
        for (int rotTop = 0; rotTop < rotTopBest; rotTop++)
            this.topCellGrid.RotateClockWise();
        for (int mirFront = 0; mirFront < mirFrontBest; mirFront++)
            this.frontCellGrid.MirrorLeftRight();
        for (int mirSide = 0; mirSide < mirSideBest; mirSide++)
            this.sideCellGrid.MirrorLeftRight();
        for (int mirTop = 0; mirTop < mirTopBest; mirTop++)
            this.topCellGrid.MirrorLeftRight();

        if (volumeConnectedValidGraphFound)
        {
            textField.text = "volume connected graph";
        }
        else if (volumeConnectedValidSubgraphFound)
        {
            textField.text = "volume connected subgraph";
        }
        else if (edgeConnectedValidGraphFound)
        {
            textField.text = "edge connected graph";
        }
        else if (edgeConnectedValidSubgraphFound)
        {
            textField.text = "edge connected subgraph";
        }
        else if (vertexConnectedValidGraphFound)
        {
            textField.text = "vertex connected graph";
        }
        else if (vertexConnectedValidSubgraphFound)
        {
            textField.text = "vertex connected subgraph";
        }
        else
        {
            textField.text = "invalid";
        }


    }

    public bool AreSilhouettesValid(int graphId = -1)
    {
        return frontCellGrid.IsSilhouetteValid(graphId) && topCellGrid.IsSilhouetteValid(graphId) && sideCellGrid.IsSilhouetteValid(graphId) ;
    }

    public bool IsTripletConnected()
    {
        return voxelGrid.IsTripletConnected();
    }
}