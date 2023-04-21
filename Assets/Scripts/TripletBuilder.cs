using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class TripletBuilder : MonoBehaviour
{

    public int dimensions = 5;


    public GameObject voxelGridPrefab;
    public GameObject cellGridPrefab;

    public VoxelGrid voxelGrid;
    public CellGrid topCellGrid, sideCellGrid, frontCellGrid;

    public bool test = false;

    string datasetPath = "";
    string saveResultPath = "";
    int results = 0;
    int validResults = 0;
    int connectedResults = 0;
    int validConnectedResults = 0;
    int shapeSets = 0;
    int validAndConnectedShapeSets = 0;

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
        if ("" != datasetPath && "" != saveResultPath)
        {
            RunTestSet(datasetPath, saveResultPath);
            datasetPath = "";
            saveResultPath = "";
        }    

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

    public void TestSet()
    {
        StartCoroutine(GetPaths());
    }


    public void RunTestSet(string dataPath, string savePath)
    {
        results = 0;
        validResults = 0;
        connectedResults = 0;
        validConnectedResults = 0;
        shapeSets = 0;
        validAndConnectedShapeSets = 0;
        List<List<Cell.FillValue>> frontBackup = frontCellGrid.GridFillValues();
        List<List<Cell.FillValue>> sideBackup = sideCellGrid.GridFillValues();
        List<List<Cell.FillValue>> topBackup = topCellGrid.GridFillValues();
        FileSystemEntry[] files = FileBrowserHelpers.GetEntriesInDirectory(dataPath, false);
        int rdegsFront, rdegsSide, rdegsTop;
        bool xmirFront, xmirSide, xmirTop;
        bool ymirFront, ymirSide, ymirTop;
        string filename;
        foreach (FileSystemEntry fileFront in files)
        {
            if (fileFront.Extension != ".shape")
                continue;
            foreach (FileSystemEntry fileSide in files)
            {
                if (fileSide.Extension != ".shape")
                    continue;
                foreach (FileSystemEntry fileTop in files)
                {
                    if (fileTop.Extension != ".shape")
                        continue;
                    frontCellGrid.LoadFromFile(new string[] { fileFront.Path });
                    sideCellGrid.LoadFromFile(new string[] { fileSide.Path });
                    topCellGrid.LoadFromFile(new string[] { fileTop.Path });
                    rdegsFront = 0;
                    xmirFront = xmirSide = xmirTop = false;
                    ymirFront = ymirSide = ymirTop = false;
                    bool newShapeSet = true;
                    shapeSets++;
                    for (int rotFront = 0; rotFront < 4; rotFront++)
                    {
                        rdegsSide = 0;
                        for (int rotSide = 0; rotSide < 4; rotSide++)
                        {
                            rdegsTop = 0;
                            for (int rotTop = 0; rotTop < 4; rotTop++)
                            {
                                for (int mirFront = 0; mirFront < 3; mirFront++)
                                {
                                    for (int mirSide = 0; mirSide < 3; mirSide++)
                                    {
                                        for (int mirTop = 0; mirTop < 3; mirTop++)
                                        {
                                            filename = $"{fileFront.Name[0]}_r{rdegsFront}{(xmirFront ? "_xmir" : "")}{(ymirFront ? "_ymir" : "")}" +
                                                $"{fileSide.Name[0]}_r{rdegsSide}{(xmirSide ? "_xmir" : "")}{(ymirSide ? "_ymir" : "")}"+
                                                $"{fileTop.Name[0]}_r{rdegsTop}{(xmirTop ? "_xmir" : "")}{(ymirTop ? "_ymir" : "")}";

                                            results++;
                                            //TODO: run this for every graph id and only output that 3D subgraph
                                            if (!IsSilhouetteValid())
                                            {
                                                connectedResults += IsTripletConnected() ? 1 : 0;

                                            }
                                            else
                                            {
                                                validResults++;
                                                if (IsTripletConnected())
                                                {
                                                    connectedResults++;
                                                    validConnectedResults++;
                                                    if(newShapeSet)
                                                    {
                                                        newShapeSet = false;
                                                        validAndConnectedShapeSets++;
                                                    }
                                                    voxelGrid.SaveToFile(new string[] { $"{savePath}/{filename}.obj" });
                                                }   
                                            }
                                            


                                            if (!xmirTop && !ymirTop)
                                            {
                                                topCellGrid.MirrorLeftRight();
                                                xmirTop = true;
                                            }
                                            if (xmirTop)
                                            {
                                                topCellGrid.MirrorLeftRight();
                                                xmirTop = false;
                                                topCellGrid.MirrorTopBottom();
                                                ymirTop = true;
                                            }
                                            if (ymirTop)
                                            {
                                                topCellGrid.MirrorTopBottom();
                                                ymirTop = false;
                                            }
                                        }
                                        if (!xmirSide && !ymirSide)
                                        {
                                            sideCellGrid.MirrorLeftRight();
                                            xmirSide = true;
                                        }
                                        if (xmirSide)
                                        {
                                            sideCellGrid.MirrorLeftRight();
                                            xmirSide = false;
                                            sideCellGrid.MirrorTopBottom();
                                            ymirSide = true;
                                        }
                                        if (ymirSide)
                                        {
                                            sideCellGrid.MirrorTopBottom();
                                            ymirSide = false;
                                        }
                                    }
                                    if (!xmirFront && !ymirFront)
                                    {
                                        frontCellGrid.MirrorLeftRight();
                                        xmirFront = true;
                                    }
                                    if (xmirFront)
                                    {
                                        frontCellGrid.MirrorLeftRight();
                                        xmirFront = false;
                                        frontCellGrid.MirrorTopBottom();
                                        ymirFront = true;
                                    }
                                    if (ymirFront)
                                    {
                                        frontCellGrid.MirrorTopBottom();
                                        ymirFront = false;
                                    }
                                }
                                topCellGrid.RotateClockWise();
                                rdegsTop += 90;
                            }
                            sideCellGrid.RotateClockWise();
                            rdegsSide += 90;
                        }
                        frontCellGrid.RotateClockWise();
                        rdegsFront += 90;
                    }
                }
            }
        }

        frontCellGrid.LoadFillValueList(frontBackup);
        sideCellGrid.LoadFillValueList(sideBackup);
        topCellGrid.LoadFillValueList(topBackup);
        Debug.Log($"{results}:{validResults}:{connectedResults}:{validConnectedResults}");
        Debug.Log($"{shapeSets}:{validAndConnectedShapeSets}");
    }


    public IEnumerator GetPaths()
    {
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.DisableCamControl();
        IEnumerator dialog = FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, title: "Load dataset");
        yield return StartCoroutine(dialog);
        if (FileBrowser.Success)
        {
            datasetPath = FileBrowser.Result[0];

            dialog = FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Folders, title: "Set save folder for results");
            yield return StartCoroutine(dialog);
            if (FileBrowser.Success)
                saveResultPath = FileBrowser.Result[0];
        }
        controller.EnableCamControl();
    }

}
