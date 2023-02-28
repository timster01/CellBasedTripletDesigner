using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public int x, y;
    public CellGrid parent;
    
    public enum FillValue { Empty, Full, BottomLeft, BottomRight, TopLeft, TopRight}



    public FillValue currentFillValue = FillValue.Empty;

    void Start()
    {
        currentFillValue = (FillValue)Random.Range(0, 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
