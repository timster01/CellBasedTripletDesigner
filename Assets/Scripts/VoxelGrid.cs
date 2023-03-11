using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{

    public GameObject VoxelPrefab;
    public int dimension = 5;
    public TripletBuilder parent;
    public GameObject combinedMesh;

    //TODO: replace by button
    public bool test = false;
    public bool test2 = false;

    List<List<List<Voxel>>> grid;
    List<Color> graphColors;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (test)
        {
            test = false;
            MergeVoxels();
        }

        if (!test2)
            return;

        if (IsTripletConnected())
            Debug.Log("Triplet is connected");
        else
            Debug.Log("Triplet is not connected");

        test2 = false;
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
        MarkGraphId();
    }

    public void UpdateVoxelColumnY(int x, int z)
    {
        for (int y = 0; y < dimension; y++)
        {
            grid[x][y][z].UpdateVoxel();
        }
        MarkGraphId();
    }

    public void UpdateVoxelColumnZ(int x, int y)
    {
        for (int z = 0; z < dimension; z++)
        {
            grid[x][y][z].UpdateVoxel();
        }
        MarkGraphId();
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

    public List<polygon2D> FlattenVoxelColumnX(int z, int y, int graphId = -1)
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int x = 0; x < dimension; x++)
        {
            if (graphId != -1 && graphId != grid[x][y][z].graphId)
                continue;
            if (grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh != null)
                meshes.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh);
        }
        List<polygon2D> polygons = new List<polygon2D>();
        foreach (Mesh mesh in meshes)
        {
            foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
            {
                Triangle2D tri2D = tri.FlattenX();
                if (tri2D.IsInLine())
                    continue;
                if (tri2D.IsClockwise())
                    tri2D = tri2D.FlipCoordOrder();
                polygons.Add(tri2D.ToPolygon2D());
            }
        }
        polygons = polygon2D.MergePolygon2Ds(polygons);
        return polygons;
    }

    public List<polygon2D> FlattenVoxelColumnY(int x, int z, int graphId = -1)
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int y = 0; y < dimension; y++)
        {
            if (graphId != -1 && graphId != grid[x][y][z].graphId)
                continue;
            if (grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh != null)
                meshes.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh);
        }
        List<polygon2D> polygons = new List<polygon2D>();
        foreach (Mesh mesh in meshes)
        {
            foreach (Triangle tri in Triangle.MeshToTriangles(mesh))
            {
                Triangle2D tri2D = tri.FlattenY();
                if (tri2D.IsInLine())
                    continue;
                if (tri2D.IsClockwise())
                    tri2D = tri2D.FlipCoordOrder();
                polygons.Add(tri2D.ToPolygon2D());
            }
        }
        polygons = polygon2D.MergePolygon2Ds(polygons);
        return polygons;
    }

    public List<polygon2D> FlattenVoxelColumnZ(int x, int y, int graphId = -1)
    {
        List<Mesh> meshes = new List<Mesh>();
        for (int z = 0; z < dimension; z++)
        {
            if (graphId != -1 && graphId != grid[x][y][z].graphId)
                continue;
            if(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh != null)
                meshes.Add(grid[x][y][z].childShape.GetComponent<MeshFilter>().mesh);
        }
        List<polygon2D> polygons = new List<polygon2D>();
        foreach(Mesh mesh in meshes)
        {
            foreach(Triangle tri in Triangle.MeshToTriangles(mesh))
            {
                Triangle2D tri2D = tri.FlattenZ();
                if (tri2D.IsInLine())
                    continue;
                if (tri2D.IsClockwise())
                    tri2D = tri2D.FlipCoordOrder();
                polygons.Add(tri2D.ToPolygon2D());
            }
        }
        polygons = polygon2D.MergePolygon2Ds(polygons);
        return polygons;
    }


    public void MergeVoxels()
    {
        //https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html
        
        combinedMesh.transform.localPosition = new Vector3(-dimension - 3,0,0);

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        int b = 0;
        int length = 0;
        while (b < meshFilters.Length)
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
        while (i < meshFilters.Length)
        {
            if(meshFilters[i].sharedMesh != null)
            {
                combine[c].mesh = meshFilters[i].sharedMesh;
                combine[c].transform = meshFilters[i].transform.localToWorldMatrix;
                c++;
            }
            i++;
        }
        combinedMesh.GetComponent<MeshFilter>().mesh.Clear();
        combinedMesh.GetComponent<MeshFilter>().mesh = new Mesh();
        combinedMesh.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        combinedMesh.gameObject.SetActive(true);
    }
}

public struct polygon2D
{
    public List<Vector2> vertices;

    public int vertexCount { get { return vertices.Count; } }

    //TODO
    public static List<polygon2D> MergePolygon2Ds(List<polygon2D> polygons)
    {


        return polygons;
    }
}
