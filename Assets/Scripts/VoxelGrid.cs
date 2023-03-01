using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{

    public GameObject VoxelPrefab;
    public int dimension = 5;
    public TripletBuilder parent;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
