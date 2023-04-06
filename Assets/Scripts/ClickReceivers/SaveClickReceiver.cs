using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveClickReceiver : ClickReceiver
{
    public CellGrid cellGrid;

    public override void OnClick()
    {
        cellGrid.SaveToFileDialog();
    }
}
