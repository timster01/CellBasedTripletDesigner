using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorLRClickReceiver : ClickReceiver
{
    public CellGrid cellGrid;

    public override void OnClick()
    {
        cellGrid.MirrorLeftRight();
    }
}
