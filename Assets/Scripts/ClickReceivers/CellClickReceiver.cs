using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellClickReceiver : ClickReceiver
{
    public Cell cell;

    public override void OnClick()
    {
        cell.CycleFillValue();
    }
}