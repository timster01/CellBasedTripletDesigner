using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using Clipper2Lib;
using SimpleFileBrowser;

public class BatchRunner : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ("" != datasetPath && "" != saveResultPath)
        {
            RunBatch();
            datasetPath = "";
            saveResultPath = "";
        }
    }

    public void StartBatch()
    {
        StartCoroutine(GetPaths());
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
            if (shapeEntry.Extension == ".shape")
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

    public void RunBatch()
    {
        Debug.LogError("Not yet fully implemented");
        return;
        SimpleTripletBuilder tripletBuilder = new SimpleTripletBuilder(5);
        ResetResultValues();
        FileSystemEntry[] files = FileBrowserHelpers.GetEntriesInDirectory(datasetPath, false);
        List<FileSystemEntry> temp = new List<FileSystemEntry>();
        foreach (FileSystemEntry entry in files)
            if (entry.Extension == ".shape")
                temp.Add(entry);
        resetResultDictionaries(temp);
        int rdegsFront, rdegsSide, rdegsTop;
        bool xmirFront, xmirSide, xmirTop;
        bool ymirFront, ymirSide, ymirTop;
        int previousGraphCount;
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
        List<List<FileSystemEntry>> fileCombinationsWithRepetition = GenerateCombinations(new List<FileSystemEntry>(temp), 3);
        fileCombinationsWithRepetition.Sort(SortCombinationsHelper.SortCombinations());
        List<FileSystemEntry> combination;
        for (int c = 0; c < fileCombinationsWithRepetition.Count; c++)
        {
            combination = fileCombinationsWithRepetition[c];
            fileFront = combination[0];
            fileSide = combination[1];
            fileTop = combination[2];
            tripletBuilder.loadShapeFromFile(ref tripletBuilder.frontCellGrid,fileFront.Path);
            tripletBuilder.loadShapeFromFile(ref tripletBuilder.sideCellGrid, fileSide.Path);
            tripletBuilder.loadShapeFromFile(ref tripletBuilder.topCellGrid, fileTop.Path);
            shapesInSet = new List<string>();

            //Split so name does not contain extension, technically clears anything after the first dot in the filename
            shapesInSet.Add(fileFront.Name.Split(".")[0]);
            if (fileFront.Name != fileSide.Name)
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

                                        //TODO: dfs volume graphs

                                        if (tripletBuilder.graphCount == 1)
                                        {
                                            if (tripletBuilder.AreSilhouettesValid())
                                            {
                                                volumeConnectedValidGraphFound = true;
                                                break;
                                            }
                                        }
                                        if (volumeConnectedValidSubgraphFound)
                                            break;
                                        if (tripletBuilder.graphCount > 1)
                                        {
                                            for (int i = 0; i < tripletBuilder.graphCount; i++)
                                            {
                                                if (tripletBuilder.AreSilhouettesValid(i))
                                                {
                                                    volumeConnectedValidSubgraphFound = true;
                                                    break;
                                                }
                                            }
                                            if (volumeConnectedValidSubgraphFound)
                                                break;
                                        }
                                        previousGraphCount = tripletBuilder.graphCount;

                                        //Edge connected
                                        if (edgeConnectedValidGraphFound)
                                            break;

                                        //TODO: dfs edge graphs

                                        if (tripletBuilder.graphCount < previousGraphCount)
                                        {
                                            if (tripletBuilder.graphCount == 1)
                                            {
                                                if (tripletBuilder.AreSilhouettesValid())
                                                {
                                                    edgeConnectedValidGraphFound = true;
                                                    break;
                                                }
                                            }
                                            if (edgeConnectedValidSubgraphFound)
                                                break;
                                            if (tripletBuilder.graphCount > 1)
                                            {
                                                for (int i = 0; i < tripletBuilder.graphCount; i++)
                                                {
                                                    if (tripletBuilder.AreSilhouettesValid(i))
                                                    {
                                                        edgeConnectedValidSubgraphFound = true;
                                                        break;
                                                    }
                                                    if (edgeConnectedValidSubgraphFound)
                                                        break;
                                                }
                                            }
                                            previousGraphCount = tripletBuilder.graphCount;
                                        }

                                        //Vertex connected
                                        if (vertexConnectedValidGraphFound)
                                            break;

                                        //TODO: dfs vertex graphs

                                        if (tripletBuilder.graphCount < previousGraphCount)
                                        {
                                            if (tripletBuilder.graphCount == 1)
                                            {
                                                if (tripletBuilder.AreSilhouettesValid())
                                                {
                                                    vertexConnectedValidGraphFound = true;
                                                    break;
                                                }
                                            }
                                            if (vertexConnectedValidSubgraphFound)
                                                break;
                                            if (tripletBuilder.graphCount > 1)
                                            {
                                                for (int i = 0; i < tripletBuilder.graphCount; i++)
                                                {
                                                    if (tripletBuilder.AreSilhouettesValid(i))
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

                                        if (tripletBuilder.graphCount > 1)
                                            unconnectedValidGraphFound = tripletBuilder.AreSilhouettesValid();

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
                                    tripletBuilder.MirrorLeftRight(ref tripletBuilder.topCellGrid);
                                }
                                tripletBuilder.MirrorLeftRight(ref tripletBuilder.sideCellGrid);
                            }
                            tripletBuilder.MirrorLeftRight(ref tripletBuilder.frontCellGrid);
                        }
                        tripletBuilder.RotateClockWise(ref tripletBuilder.topCellGrid);
                        rdegsTop += 90;
                    }
                    tripletBuilder.RotateClockWise(ref tripletBuilder.sideCellGrid);
                    rdegsSide += 90;
                }
                tripletBuilder.RotateClockWise(ref tripletBuilder.frontCellGrid);
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
            foreach (string shapeName in shapesInSet)
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
            }
            volumeConnectedValidGraphFound = false;
            volumeConnectedValidSubgraphFound = false;
            edgeConnectedValidGraphFound = false;
            edgeConnectedValidSubgraphFound = false;
            vertexConnectedValidGraphFound = false;
            vertexConnectedValidSubgraphFound = false;
            unconnectedValidGraphFound = false;
        }


        //Slight overestimation of string length
        int expectedLength = Mathf.RoundToInt(Mathf.Pow(shapeSets, 4) * 8);

        StringBuilder result = new StringBuilder($"involved shapes;count;volume connected graph;volume connected subgraph;edge connected graph;edge connected subgraph;vertex connected graph;vertex connected subgraph;unconnected;invalid{System.Environment.NewLine}", expectedLength);
        result.Append($"all;{shapeSets};{volumeConnectedValidGraphs};{volumeConnectedValidSubgraphs};{edgeConnectedValidGraphs};{edgeConnectedValidSubgraphs};{vertexConnectedValidGraphs};{vertexConnectedValidSubgraphs};{unconnectedValidGraphs};{invalidGraphs}{System.Environment.NewLine}");
        string name;
        foreach (FileSystemEntry entry in files)
        {
            name = entry.Name.Split(".")[0];
            result.Append($"{name};{shapeSetsPerShape[name]};{volumeConnectedValidGraphsPerShape[name]};{volumeConnectedValidSubgraphsPerShape[name]};{edgeConnectedValidGraphsPerShape[name]};{edgeConnectedValidSubgraphsPerShape[name]};{vertexConnectedValidGraphsPerShape[name]};{vertexConnectedValidSubgraphsPerShape[name]};{unconnectedValidGraphsPerShape[name]};{invalidGraphsPerShape[name]}{System.Environment.NewLine}");
        }
        string resultString = result.ToString();
        Debug.Log(resultString);
        FileBrowserHelpers.WriteTextToFile(saveResultPath + "/results.csv", resultString);
        EditorUtility.ClearProgressBar();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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

public class SortCombinationsHelper : IComparer<List<FileSystemEntry>>
{
    int IComparer<List<FileSystemEntry>>.Compare(List<FileSystemEntry> x, List<FileSystemEntry> y)
    {
        int shortestListLength;
        if (x.Count > y.Count)
            shortestListLength = y.Count;
        else
            shortestListLength = x.Count;

        for (int i = 0; i < shortestListLength; i++)
        {
            if (x[i].Name != y[i].Name)
            {
                return x[i].Name.CompareTo(y[i].Name);
            }
        }

        if (x.Count > y.Count)
            return 1;
        if (y.Count > x.Count)
            return -1;
        return 0;
    }

    public static IComparer<List<FileSystemEntry>> SortCombinations()
    {
        return (IComparer<List<FileSystemEntry>>)new SortCombinationsHelper();
    }
}
