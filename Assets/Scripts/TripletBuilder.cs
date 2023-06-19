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

    string datasetPath = "";
    string saveResultPath = "";
    int shapeSets = 0;
    int volumeConnectedValidGraphs = 0;
    int volumeConnectedValidSubgraphs = 0;
    int edgeConnectedValidGraphs = 0;
    int edgeConnectedValidSubgraphs = 0;
    int vertexConnectedValidGraphs = 0;
    int vertexConnectedValidSubgraphs = 0;
    int unconnectedValidGraphs = 0;
    int invalidGraphs = 0;

    Dictionary<string, int> shapeSetsPerShape;
    Dictionary<string, int> volumeConnectedValidGraphsPerShape;
    Dictionary<string, int> volumeConnectedValidSubgraphsPerShape;
    Dictionary<string, int> edgeConnectedValidGraphsPerShape;
    Dictionary<string, int> edgeConnectedValidSubgraphsPerShape;
    Dictionary<string, int> vertexConnectedValidGraphsPerShape;
    Dictionary<string, int> vertexConnectedValidSubgraphsPerShape;
    Dictionary<string, int> unconnectedValidGraphsPerShape;
    Dictionary<string, int> invalidGraphsPerShape;

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

    public void ResetResultValues()
    {
        shapeSets = 0;
        volumeConnectedValidGraphs = 0;
        volumeConnectedValidSubgraphs = 0;
        edgeConnectedValidGraphs = 0;
        edgeConnectedValidSubgraphs = 0;
        vertexConnectedValidGraphs = 0;
        vertexConnectedValidSubgraphs = 0;
        
    }

    public void resetResultDictionaries(IEnumerable<FileSystemEntry> shapeEntries)
    {
        List<KeyValuePair<string, int>> values = new List<KeyValuePair<string, int>>();
        foreach (FileSystemEntry shapeEntry in shapeEntries)
            if(shapeEntry.Extension == ".shape")
                values.Add(new KeyValuePair<string, int>(shapeEntry.Name.Split(".")[0], 0));
        shapeSetsPerShape = new Dictionary<string, int>(values);
        volumeConnectedValidGraphsPerShape = new Dictionary<string, int>(values);
        volumeConnectedValidSubgraphsPerShape = new Dictionary<string, int>(values);
        edgeConnectedValidGraphsPerShape = new Dictionary<string, int>(values);
        edgeConnectedValidSubgraphsPerShape = new Dictionary<string, int>(values);
        vertexConnectedValidGraphsPerShape = new Dictionary<string, int>(values);
        vertexConnectedValidSubgraphsPerShape = new Dictionary<string, int>(values);
        unconnectedValidGraphsPerShape = new Dictionary<string, int>(values);
        invalidGraphsPerShape = new Dictionary<string, int>(values);
    }

    public void RunTestSet(string dataPath, string savePath)
    {
        ResetResultValues();
        bool currentAutoDisplayMeshValue = autoDisplayCombinedMesh;
        autoDisplayCombinedMesh = false;
        bool currentAutoDisplaySilhouetteCellsValue = autoDisplaySilhouetteCells;
        autoDisplaySilhouetteCells = false;
        List<List<Voxel>> graphs;
        int graphCount;
        List<List<Cell.FillValue>> frontBackup = frontCellGrid.GridFillValues();
        List<List<Cell.FillValue>> sideBackup = sideCellGrid.GridFillValues();
        List<List<Cell.FillValue>> topBackup = topCellGrid.GridFillValues();
        FileSystemEntry[] files = FileBrowserHelpers.GetEntriesInDirectory(dataPath, false);
        resetResultDictionaries(files);
        int rdegsFront, rdegsSide, rdegsTop;
        bool xmirFront, xmirSide, xmirTop;
        bool ymirFront, ymirSide, ymirTop;
        //string filename;
        bool volumeConnectedValidGraphFound = false;
        bool volumeConnectedValidSubgraphFound = false;
        bool edgeConnectedValidGraphFound = false;
        bool edgeConnectedValidSubgraphFound = false;
        bool vertexConnectedValidGraphFound = false;
        bool vertexConnectedValidSubgraphFound = false;
        bool unconnectedValidGraphFound = false;

        List<string> shapesInSet;
        FileSystemEntry fileFront;
        FileSystemEntry fileSide;
        FileSystemEntry fileTop;
        List<List<FileSystemEntry>> fileCombinationsWithRepetition = GenerateCombinations(new List<FileSystemEntry>(files), 3);
        foreach (List<FileSystemEntry> combination in fileCombinationsWithRepetition)
        { 
            fileFront = combination[0];
            fileSide = combination[1];
            fileTop = combination[2];
            if (fileFront.Extension != ".shape" || fileSide.Extension != ".shape" || fileTop.Extension != ".shape")
                continue;
            frontCellGrid.LoadFromFile(new string[] { fileFront.Path });
            sideCellGrid.LoadFromFile(new string[] { fileSide.Path });
            topCellGrid.LoadFromFile(new string[] { fileTop.Path });
            shapesInSet = new List<string>();

            //Split so name does not contain extension, technically clears anything after the first dot in the filename
            shapesInSet.Add(fileFront.Name.Split(".")[0]);
            if(fileFront.Name != fileSide.Name)
                shapesInSet.Add(fileSide.Name.Split(".")[0]);
            if (fileFront.Name != fileTop.Name && fileSide.Name != fileTop.Name)
                shapesInSet.Add(fileSide.Name.Split(".")[0]);

            rdegsFront = 0;
            xmirFront = xmirSide = xmirTop = false;
            ymirFront = ymirSide = ymirTop = false;
               
            shapeSets++;

            EditorUtility.DisplayProgressBar("Simple Progress Bar", "Doing some work...", shapeSets / (float)fileCombinationsWithRepetition.Count);
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
                                    //filename = $"{fileFront.Name[0]}_r{rdegsFront}{(xmirFront ? "_xmir" : "")}{(ymirFront ? "_ymir" : "")}" +
                                    //    $"{fileSide.Name[0]}_r{rdegsSide}{(xmirSide ? "_xmir" : "")}{(ymirSide ? "_ymir" : "")}"+
                                    //    $"{fileTop.Name[0]}_r{rdegsTop}{(xmirTop ? "_xmir" : "")}{(ymirTop ? "_ymir" : "")}";

                                    


                                    //Handle this situation for the current shapeset
                                    while (true)
                                    {
                                        //Volume connected
                                        if (volumeConnectedValidGraphFound)
                                            break;
                                        graphs = voxelGrid.GetConnectedGraphs();
                                        voxelGrid.MarkGraphId(graphs);
                                        graphCount = graphs.Count;
                                        if (graphs.Count == 1)
                                        {
                                            if (IsSilhouetteValid())
                                            {
                                                volumeConnectedValidGraphFound = true;
                                                break;
                                            }
                                        }
                                        if (volumeConnectedValidSubgraphFound)
                                            break;
                                        if (graphs.Count > 1)
                                        {
                                            for (int i = 0; i < graphs.Count; i++)
                                            {
                                                if (IsSilhouetteValid(i))
                                                {
                                                    volumeConnectedValidSubgraphFound = true;
                                                    break;
                                                }
                                            }
                                            if(volumeConnectedValidSubgraphFound)
                                                break;
                                        }

                                        //Edge connected
                                        if (edgeConnectedValidGraphFound)
                                            break;
                                        graphs = voxelGrid.GetConnectedGraphs(false, true, false);
                                        voxelGrid.MarkGraphId(graphs);
                                        if(graphs.Count < graphCount)
                                        {
                                            graphCount = graphs.Count;
                                            if (graphs.Count == 1)
                                            {
                                                if (IsSilhouetteValid())
                                                {
                                                    edgeConnectedValidGraphFound = true;
                                                    break;
                                                }
                                            }
                                            if (edgeConnectedValidSubgraphFound)
                                                break;
                                            if (graphs.Count > 1)
                                            {
                                                for (int i = 0; i < graphs.Count; i++)
                                                {
                                                    if (IsSilhouetteValid(i))
                                                    {
                                                        edgeConnectedValidSubgraphFound = true;
                                                        break;
                                                    }
                                                    if (edgeConnectedValidSubgraphFound)
                                                        break;
                                                }
                                            }
                                        }

                                        //Vertex connected
                                        if (vertexConnectedValidGraphFound)
                                            break;
                                        graphs = voxelGrid.GetConnectedGraphs(false, false, true);
                                        voxelGrid.MarkGraphId(graphs);
                                        if (graphs.Count < graphCount)
                                        {
                                            graphCount = graphs.Count;
                                            if (graphs.Count == 1)
                                            {
                                                if (IsSilhouetteValid())
                                                {
                                                    vertexConnectedValidGraphFound = true;
                                                    break;
                                                }
                                            }
                                            if (vertexConnectedValidSubgraphFound)
                                                break;
                                            if (graphs.Count > 1)
                                            {
                                                for (int i = 0; i < graphs.Count; i++)
                                                {
                                                    if (IsSilhouetteValid(i))
                                                    {
                                                        vertexConnectedValidSubgraphFound = true;
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
                                        
                                        if(graphCount > 1)
                                            unconnectedValidGraphFound = IsSilhouetteValid();

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

            //Handle the best case connectedness and validity for this shapeset
            if (volumeConnectedValidGraphFound)
                volumeConnectedValidGraphs++;
            else if (volumeConnectedValidSubgraphFound)
                volumeConnectedValidSubgraphs++;
            else if (edgeConnectedValidGraphFound)
                edgeConnectedValidGraphs++;
            else if (edgeConnectedValidSubgraphFound)
                edgeConnectedValidSubgraphs++;
            else if (vertexConnectedValidGraphFound)
                vertexConnectedValidGraphs++;
            else if (vertexConnectedValidSubgraphFound)
                vertexConnectedValidSubgraphs++;
            else if (unconnectedValidGraphFound)
                unconnectedValidGraphs++;
            else
                invalidGraphs++;

            //Up the same values for connectedness and validity levels for each shape in a dict
            foreach(string shapeName in shapesInSet)
            {
                shapeSetsPerShape[shapeName]++;
                if (volumeConnectedValidGraphFound)
                    volumeConnectedValidGraphsPerShape[shapeName]++;
                else if (volumeConnectedValidSubgraphFound)
                    volumeConnectedValidSubgraphsPerShape[shapeName]++;
                else if (edgeConnectedValidGraphFound)
                    edgeConnectedValidGraphsPerShape[shapeName]++;
                else if (edgeConnectedValidSubgraphFound)
                    edgeConnectedValidSubgraphsPerShape[shapeName]++;
                else if (vertexConnectedValidGraphFound)
                    vertexConnectedValidGraphsPerShape[shapeName]++;
                else if (vertexConnectedValidSubgraphFound)
                    vertexConnectedValidSubgraphsPerShape[shapeName]++;
                else if (unconnectedValidGraphFound)
                    unconnectedValidGraphsPerShape[shapeName]++;
                else
                    invalidGraphsPerShape[shapeName]++;

                volumeConnectedValidGraphFound = false;
                volumeConnectedValidSubgraphFound = false;
                edgeConnectedValidGraphFound = false;
                edgeConnectedValidSubgraphFound = false;
                vertexConnectedValidGraphFound = false;
                vertexConnectedValidSubgraphFound = false;
                unconnectedValidGraphFound = false;
            }
        }

        autoDisplayCombinedMesh = currentAutoDisplayMeshValue;
        autoDisplaySilhouetteCells = currentAutoDisplaySilhouetteCellsValue;
        frontCellGrid.LoadFillValueList(frontBackup);
        sideCellGrid.LoadFillValueList(sideBackup);
        topCellGrid.LoadFillValueList(topBackup);
        
        //Slight overestimation of string length
        int expectedLength = Mathf.RoundToInt(Mathf.Pow(shapeSets, 4) * 8);

        StringBuilder result = new StringBuilder($"involved shapes;count;volume connected graph;volume connected subgraph;edge connected graph;edge connected subgraph;vertex connected graph;vertex connected subgraph;unconnected;invalid{System.Environment.NewLine}", expectedLength);
        result.Append($"all;{shapeSets};{volumeConnectedValidGraphs};{volumeConnectedValidSubgraphs};{edgeConnectedValidGraphs};{edgeConnectedValidSubgraphs};{vertexConnectedValidGraphs};{vertexConnectedValidSubgraphs};{unconnectedValidGraphs};{invalidGraphs}{System.Environment.NewLine}");
        foreach (FileSystemEntry entry in files)
        {
            result.Append($"{entry.Name};{shapeSetsPerShape[entry.Name]};{volumeConnectedValidGraphsPerShape[entry.Name]};{volumeConnectedValidSubgraphsPerShape[entry.Name]};{edgeConnectedValidGraphsPerShape[entry.Name]};{edgeConnectedValidSubgraphsPerShape[entry.Name]};{vertexConnectedValidGraphsPerShape[entry.Name]};{vertexConnectedValidSubgraphsPerShape[entry.Name]};{unconnectedValidGraphsPerShape[entry.Name]};{invalidGraphsPerShape[entry.Name]}{System.Environment.NewLine}");
        }
        string resultString = result.ToString();
        Debug.Log(resultString);
        FileBrowserHelpers.WriteTextToFile(saveResultPath + "/results.csv", resultString);
        EditorUtility.ClearProgressBar();
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

    //Source: https://rosettacode.org/wiki/Combinations_with_repetitions#C#
    private static List<List<T>> GenerateCombinations<T>(List<T> combinationList, int k)
    {
        var combinations = new List<List<T>>();

        if (k == 0)
        {
            var emptyCombination = new List<T>();
            combinations.Add(emptyCombination);

            return combinations;
        }

        if (combinationList.Count == 0)
        {
            return combinations;
        }

        T head = combinationList[0];
        var copiedCombinationList = new List<T>(combinationList);

        List<List<T>> subcombinations = GenerateCombinations(copiedCombinationList, k - 1);

        foreach (var subcombination in subcombinations)
        {
            subcombination.Insert(0, head);
            combinations.Add(subcombination);
        }

        combinationList.RemoveAt(0);
        combinations.AddRange(GenerateCombinations(combinationList, k));

        return combinations;
    }

}
