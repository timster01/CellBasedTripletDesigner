using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripetSaveClickReceiver : ClickReceiver
{
    public TripletBuilder tripletBuilder;

    public override void OnClick()
    {
        tripletBuilder.voxelGrid.SaveToFileDialog();
    }
}
