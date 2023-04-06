using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateClickReceiver : ClickReceiver
{
    public CellGrid cellGrid;

    public override void OnClick()
    {
        cellGrid.RotateClockWise();
    }
}
