using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorTBClickReceiver : ClickReceiver
{
    public CellGrid cellGrid;

    public override void OnClick()
    {
        cellGrid.MirrorTopBottom();
    }
}
