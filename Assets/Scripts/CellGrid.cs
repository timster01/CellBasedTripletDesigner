using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{

    public GameObject CellPrefab;
    public int dimension = 5;

    List<List<Cell>> grid;

    // Start is called before the first frame update
    void Start()
    {
        grid = new List<List<Cell>>();
        GameObject cellObject;
        Cell cell;
        for (int x = 0; x < dimension; x++)
        {
            List<Cell> column = new List<Cell>();
            for (int y = 0; y < dimension; y++)
            {
                cellObject = GameObject.Instantiate(CellPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
                cell = cellObject.GetComponent<Cell>();
                cell.x = x;
                cell.y = y;
                column.Add(cell);
            }
            grid.Add(column);
        }
    }

    Cell GetCellAtCoords(int x, int y)
    {
        return grid[x][y];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
