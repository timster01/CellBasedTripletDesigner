using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{

    public GameObject VoxelPrefab;
    public int dimension = 5;
    public TripletBuilder parent;
    public GameObject combinedMesh;

    public bool test = false;

    List<List<List<Voxel>>> grid;

    // Start is called before the first frame update
    void Start()
    {
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




    public void UpdateVoxelColumnX(int z, int y)
    {
        for (int x = 0; x < dimension; x++)
        {
            grid[x][y][z].UpdateVoxel();
        }
    }

    public void UpdateVoxelColumnY(int x, int z)
    {
        for (int y = 0; y < dimension; y++)
        {
            grid[x][y][z].UpdateVoxel();
        }
    }

    public void UpdateVoxelColumnZ(int x, int y)
    {
        for (int z = 0; z < dimension; z++)
        {
            grid[x][y][z].UpdateVoxel();
        }
    }

    public bool IsConnected()
    {
        return false;
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
            if (meshFilters[b].mesh != null)
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
                combine[c].mesh = meshFilters[i].mesh;
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

    // Update is called once per frame
    void Update()
    {
        if (test)
        {
            test = false;
            MergeVoxels();
        }
    }
}
