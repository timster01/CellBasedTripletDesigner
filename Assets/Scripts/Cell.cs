using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public int x, y;
    
    public enum FillValue { Empty, Full, BottomLeft, BottomRight, TopLeft, TopRight}

    public FillValue CurrentFillValue = FillValue.Empty;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
