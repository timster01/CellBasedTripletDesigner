using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public int x, y;
    public CellGrid parent;
    public GameObject childShape;

    public enum FillValue { Empty, Full, BottomLeft, BottomRight, TopLeft, TopRight}



    public FillValue currentFillValue = FillValue.Empty;

    void Start()
    {
    }

    public void CycleFillValue()
    {
        currentFillValue = (FillValue)(((int)currentFillValue + 1) % 6);
        UpdateShape();
    }

    public void UpdateShape()
    {
        if (currentFillValue == Cell.FillValue.Empty)
            childShape.GetComponent<MeshFilter>().mesh.Clear();

        string objPath = $"BaseShapes/{currentFillValue.ToString()}";
        Mesh mesh = Resources.Load<Mesh>(objPath);
        childShape.GetComponent<MeshFilter>().mesh = mesh;
    }

    public bool IsSilhouetteValid()
    {
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
