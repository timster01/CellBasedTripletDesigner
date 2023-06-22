using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

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

    public void RunBatch()
    {

    }

    
}
