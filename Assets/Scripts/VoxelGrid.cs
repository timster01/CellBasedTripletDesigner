using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;
using SimpleFileBrowser;
using System.Text;

public class VoxelGrid : MonoBehaviour
{

    public GameObject VoxelPrefab;
    public int dimension = 5;
    public TripletBuilder parent;
    public GameObject combinedMesh;

    List<List<List<Voxel>>> grid;
    List<Color> graphColors;

    public int nrOfGraphs = 0;
    
    

    // Start is called before the first frame update
    void Start()
    {
        graphColors = new List<Color>();
        grid = new List<List<List<Voxel>>>();
        GameObject voxelObject;
        Voxel voxel;
        for (int x = 0; x < dimension; x++)
        {
            List<List<Voxel>> plane = new List<List<Voxel>>();
            for (int y = 0; y < dimension; y++)
            {
                List<Voxel> depthColumn = new List<Voxel>();
                for (int z = 0; z < dimension; z++)
                {
                    voxelObject = GameObject.Instantiate(VoxelPrefab,  this.transform);
                    voxelObject.transform.localPosition = new Vector3(x, y, z);
                    voxelObject.transform.localRotation = Quaternion.identity;
                    voxel = voxelObject.GetComponent<Voxel>();
                    voxel.x = x;
                    voxel.y = y;
                    voxel.z = z;
                    voxel.parent = this;
                    depthColumn.Add(voxel);

                }
                plane.Add(depthColumn);
            }
            grid.Add(plane);
        }
        UpdateAllVoxels();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SaveToFileDialog()
    {
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.DisableCamControl();
        FileBrowser.SetFilters(false, new string[] { ".obj" });
        FileBrowser.SetDefaultFilter(".obj");
        FileBrowser.ShowSaveDialog(SaveToFile, CancelFileDialog, FileBrowser.PickMode.Files);

    }

    public void SaveToFile(string[] paths)
    {
        SaveToFile(paths, -1);
    }

    public void SaveToFile(string[] paths, int graphId)
    {
        string objString = GenerateObjString(graphId: graphId);
        FileBrowserHelpers.WriteTextToFile(paths[0], objString);
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.EnableCamControl();
    }

    public void CancelFileDialog()
    {
        CamController controller = GameObject.Find("CamController").GetComponent<CamController>();
        controller.EnableCamControl();
    }

    public string GenerateObjString(string tripletName = "triplet", int graphId = -1)
    {   
        Mesh mesh = GenerateCombinedMesh(graphId);
        int expectedLength = mesh.vertexCount * (4 + System.Environment.NewLine.Length + 20) + mesh.triangles.Length * (4 + mesh.triangles.Length.ToString().Length + System.Environment.NewLine.Length) + tripletName.Length;
        StringBuilder result = new StringBuilder($"o {tripletName}{System.Environment.NewLine}", expectedLength);
        foreach (Vector3 vertex in mesh.vertices)
        {
            result.Append($"v {vertex.x.ToString("F10")} {vertex.y.ToString("F10")} {vertex.z.ToString("F10")}{System.Environment.NewLine}");
        }

        for (int i = 0; i < mesh.triangles.Length; i+=3)
        {
            result.Append($"f {mesh.triangles[i] + 1} {mesh.triangles[i + 1] + 1} {mesh.triangles[i + 2] + 1}{System.Environment.NewLine}");
        }
        return result.ToString();
    }

    public Color GetGraphColor(int graphId)
    {
        for (int i = graphColors.Count; i <= graphId; i++)
        {
            Color newColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0f, 1f);
            graphColors.Add(newColor);
        }
        return graphColors[graphId];
    }

    public void UpdateVoxelColumnX(int z, int y)
    {
        for (int x = 0; x < dimension; x++)
        {
            grid[x][y][z].UpdateVoxel();
        }
        parent.sideCellGrid.UpdateSilhouetteCell(z, y);
        for (int i = 0; i < dimension; i++)
        {
            parent.frontCellGrid.UpdateSilhouetteCell(i, y);
            parent.topCellGrid.UpdateSilhouetteCell(i, z);
        }
        MarkGraphId();
        DisplayCombinedMesh();
    }

    public void UpdateVoxelColumnY(int x, int z)
    {
        for (int y = 0; y < dimension; y++)
        {
            grid[x][y][z].UpdateVoxel();
        }
        parent.topCellGrid.UpdateSilhouetteCell(x, z);
        for (int i = 0; i < dimension; i++)
        {
            parent.sideCellGrid.UpdateSilhouetteCell(i, z);
            parent.frontCellGrid.UpdateSilhouetteCell(x, i);
        }
        MarkGraphId();
        DisplayCombinedMesh();
    }

    public void UpdateVoxelColumnZ(int x, int y)
    {
        for (int z = 0; z < dimension; z++)
        {
            grid[x][y][z].UpdateVoxel();
        }
        parent.frontCellGrid.UpdateSilhouetteCell(x, y);
        for (int i = 0; i < dimension; i++)
        {
            parent.sideCellGrid.UpdateSilhouetteCell(i, y);
            parent.topCellGrid.UpdateSilhouetteCell(x, i);
        }
        MarkGraphId();
        DisplayCombinedMesh();
    }

    public void UpdateAllVoxels()
    {
        for (int x = 0; x < dimension; x++)
            for (int y = 0; y < dimension; y++)
                for (int z = 0; z < dimension; z++)
                {
                    grid[x][y][z].UpdateVoxel();
                }
        parent.frontCellGrid.UpdateAllSilhouetteCells();
        parent.sideCellGrid.UpdateAllSilhouetteCells();
        parent.topCellGrid.UpdateAllSilhouetteCells();
        MarkGraphId();
        DisplayCombinedMesh();
    }

    public Voxel GetVoxelAtCoords(int x, int y, int z)
    {
        return grid[x][y][z];
    }

    public List<List<Voxel>> GetConnectedGraphs()
    {
        List<List<Voxel>> graphs = new List<List<Voxel>>();
        bool shouldBreak = false;
        bool notInAGraph;
        List<Voxel> graph;
        bool foundCell;
        while (true)
        {
            graph = new List<Voxel>();
            foundCell = false;
            foreach (List<List<Voxel>> plane in grid)
            {
                foreach (List<Voxel> column in plane)
                {
                    foreach (Voxel voxel in column)
                    {
                        notInAGraph = true;
                        if (voxel.childShape.GetComponent<MeshFilter>().mesh.triangles.Length > 0)
                        {
                            foreach (List<Voxel> foundCells in graphs)
                            {
                                if (foundCells.Contains(voxel))
                                {
                                    notInAGraph = false;
                                    break;
                                }
                            }

                            if (notInAGraph)
                            {
                                graph.Add(voxel);
                                graph.AddRange(voxel.ConnectedVoxels());
                                shouldBreak = true;
                                foundCell = true;
                                break;
                            }
                        }
                    }
                    if (shouldBreak)
                        break;
                }
                if (shouldBreak)
                    break;
            }
            shouldBreak = false;
            if (!foundCell)
            {
                break;
            }
            int i = 1;
            while (i < graph.Count)
            {
                foreach (Voxel voxel in graph[i].ConnectedVoxels())
                {
                    if (!graph.Contains(voxel))
                        graph.Add(voxel);
                }
                i++;
            }
            graphs.Add(graph);
        }
        return graphs;
    }

    public void MarkGraphId()
    {
        int i = 0;
        foreach (List<Voxel> graph in GetConnectedGraphs())
        {
            foreach (Voxel voxel in graph)
            {
                voxel.graphId = i;
            }
            i++;
        }
        nrOfGraphs = i;
    }

    public bool IsTripletConnected()
    {
        List<Voxel> graph = new List<Voxel>();
        bool shouldBreak = false;
        foreach (List<List<Voxel>> plane in grid)
        {
            foreach (List<Voxel> column in plane)
            {
                foreach (Voxel voxel in column)
                {
                    if (voxel.childShape.GetComponent<MeshFilter>().mesh.triangles.Length > 0)
                    {
                        graph.Add(voxel);
                        graph.AddRange(voxel.ConnectedVoxels());
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak)
                    break;
            }
            if (shouldBreak)
                break;
        }
        if (graph.Count == 0)
            return true;
        int i = 1;
        while (i < graph.Count)
        {
            foreach (Voxel voxel in graph[i].ConnectedVoxels())
            {
                if (!graph.Contains(voxel))
                    graph.Add(voxel);
            }
            i++;
        }
        foreach (List<List<Voxel>> plane in grid)
        {
            foreach (List<Voxel> column in plane)
            {
                foreach (Voxel voxel in column)
                {
                    if (voxel.childShape.GetComponent<MeshFilter>().mesh.triangles.Length > 0 && !graph.Contains(voxel))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public PathsD FlattenVoxelColumnX(int z, int y, int graphId = -1)
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int x = 0; x < dimension; x++)
        {
            if (graphId != -1 && graphId != grid[x][y][z].graphId)
                continue;
            if (grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh != null)
                meshes.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh);
        }
        PathsD polygons = new PathsD();
        foreach (Mesh mesh in meshes)
        {
            foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
            {
                Triangle2D tri2D = tri.FlattenX();
                if (tri2D.IsInLine())
                    continue;
                if (tri2D.IsCounterClockWise())
                    tri2D = tri2D.FlipCoordOrder();
                polygons.Add(tri2D.ToPathD());
            }
        }
        polygons = Clipper.Union(polygons, FillRule.NonZero);
        polygons = Clipper.SimplifyPaths(polygons, 0.0);
        return polygons;
    }

    public PathsD FlattenVoxelColumnY(int x, int z, int graphId = -1)
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int y = 0; y < dimension; y++)
        {
            if (graphId != -1 && graphId != grid[x][y][z].graphId)
                continue;
            if (grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh != null)
                meshes.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh);
        }
        PathsD polygons = new PathsD();
        foreach (Mesh mesh in meshes)
        {
            foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
            {
                Triangle2D tri2D = tri.FlattenY();
                if (tri2D.IsInLine())
                    continue;
                if (tri2D.IsCounterClockWise())
                    tri2D = tri2D.FlipCoordOrder();
                polygons.Add(tri2D.ToPathD());
            }
        }
        polygons = Clipper.Union(polygons, FillRule.NonZero);
        polygons = Clipper.SimplifyPaths(polygons, 0.0);
        return polygons;
    }

    public PathsD FlattenVoxelColumnZ(int x, int y, int graphId = -1)
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int z = 0; z < dimension; z++)
        {
            if (graphId != -1 && graphId != grid[x][y][z].graphId)
                continue;
            if(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh != null)
                meshes.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh);
        }
        
        PathsD polygons = new PathsD();
        foreach(Mesh mesh in meshes)
        {
            foreach(Triangle tri in Triangle.MeshToTriangles(mesh))
            {
                Triangle2D tri2D = tri.FlattenZ();
                if (tri2D.IsInLine())
                    continue;
                if (tri2D.IsCounterClockWise())
                    tri2D = tri2D.FlipCoordOrder();
                polygons.Add(tri2D.ToPathD());
            }
        }
        polygons = Clipper.Union(polygons, FillRule.NonZero);
        polygons = Clipper.SimplifyPaths(polygons, 0.0);
        return polygons;
    }


    public Mesh GenerateCombinedMesh(int graphId = -1)
    {
        //TODO: remove duplicate vertices, not very important
        //Can probably be done by removing all faces fully in the voxel face with a connencted voxel through that face
        //https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html

        List<MeshFilter> meshFilters = new List<MeshFilter>();
        List<Voxel> voxels = new List<Voxel>();
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                for (int z = 0; z < dimension; z++)
                {
                    if(graphId == -1 || graphId == grid[x][y][z].graphId)
                    {
                        meshFilters.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>());
                        voxels.Add(grid[x][y][z]);
                    }
                        
                }
            }
        }
        int b = 0;
        int length = 0;
        while (b < meshFilters.Count)
        {
            if (meshFilters[b].sharedMesh != null)
            {
                length++;
            }
            b++;
        }
        CombineInstance[] combine = new CombineInstance[length];


        int i = 0;
        int c = 0;
        while (i < meshFilters.Count)
        {
            if (meshFilters[i].sharedMesh != null)
            {
                combine[c].mesh = CutConnectionFaces(meshFilters[i].sharedMesh, voxels[i]);
                combine[c].transform = meshFilters[i].transform.localToWorldMatrix;
                c++;
            }
            i++;
        }
        Mesh result = new Mesh();
        result.CombineMeshes(combine);
        result.Optimize();
        result.RecalculateNormals();
        result.RecalculateTangents();
        return result;
    }

    public Mesh CutConnectionFaces(Mesh input, Voxel voxel)
    {
        List<int> newTriangles = new List<int>();
        bool up = voxel.y != dimension - 1 && voxel.graphId == grid[voxel.x][voxel.y + 1][voxel.z].graphId;
        bool down = voxel.y != 0 && voxel.graphId == grid[voxel.x][voxel.y - 1][voxel.z].graphId;
        bool left = voxel.x != 0 && voxel.graphId == grid[voxel.x - 1][voxel.y][voxel.z].graphId;
        bool right = voxel.x != dimension - 1 && voxel.graphId == grid[voxel.x + 1][voxel.y][voxel.z].graphId;
        bool back = voxel.z != dimension - 1 && voxel.graphId == grid[voxel.x][voxel.y][voxel.z + 1].graphId;
        bool front = voxel.z != 0 && voxel.graphId == grid[voxel.x][voxel.y][voxel.z - 1].graphId;
        bool remove;
        for (int i = 0; i < input.triangles.Length; i+=3)
        {
            remove = false;
            remove = remove || (up && input.vertices[input.triangles[i]].y == 0.5f &&
                input.vertices[input.triangles[i + 1]].y == 0.5f &&
                input.vertices[input.triangles[i + 2]].y == 0.5f);
            remove = remove || (down && input.vertices[input.triangles[i]].y == -0.5f &&
                input.vertices[input.triangles[i + 1]].y == -0.5f &&
                input.vertices[input.triangles[i + 2]].y == -0.5f);
            remove = remove || (left && input.vertices[input.triangles[i]].x == -0.5f &&
                input.vertices[input.triangles[i + 1]].x == -0.5f &&
                input.vertices[input.triangles[i + 2]].x == -0.5f);
            remove = remove || (right && input.vertices[input.triangles[i]].x == 0.5f &&
                input.vertices[input.triangles[i + 1]].x == 0.5f &&
                input.vertices[input.triangles[i + 2]].x == 0.5f);
            remove = remove || (front && input.vertices[input.triangles[i]].z == -0.5f &&
                input.vertices[input.triangles[i + 1]].z == -0.5f &&
                input.vertices[input.triangles[i + 2]].z == -0.5f);
            remove = remove || (back && input.vertices[input.triangles[i]].z == 0.5f &&
                input.vertices[input.triangles[i + 1]].z == 0.5f &&
                input.vertices[input.triangles[i + 2]].z == 0.5f);
            Debug.Log("hello");
            if (remove)
                Debug.Log("removed something");
            if (!remove)
            {
                newTriangles.Add(input.triangles[i]);
                newTriangles.Add(input.triangles[i+1]);
                newTriangles.Add(input.triangles[i+2]);
            }
        }
        Mesh output = new Mesh();
        List<Vector3> newVertices = new List<Vector3>();
        newVertices.AddRange(input.vertices);
        for (int i = 0; i < newVertices.Count; i++)
        {
            if (!newTriangles.Contains(i))
            {
                newVertices.RemoveAt(i);
                for (int j = 0; j < newTriangles.Count; j++)
                {
                    if (newTriangles[j] > i)
                        newTriangles[j]--;
                }
                i--;
            }
                
        }
        output.SetVertices(newVertices.ToArray());
        output.SetTriangles(newTriangles.ToArray(), 0);
        return output;
    }

    public void DisplayCombinedMesh()
    {
        
        combinedMesh.transform.localPosition = new Vector3(-dimension - 10, 0, 0);
        combinedMesh.GetComponent<MeshFilter>().mesh.Clear();
        if (!parent.autoDisplayCombinedMesh)
            return;
        combinedMesh.GetComponent<MeshFilter>().mesh = GenerateCombinedMesh();
        combinedMesh.gameObject.SetActive(true);
    }
}