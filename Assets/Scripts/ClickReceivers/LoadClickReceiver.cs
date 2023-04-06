using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadClickReceiver : ClickReceiver
{
    public CellGrid cellGrid;

    public override void OnClick()
    {
        cellGrid.LoadFromFileDialog();
    }
}
