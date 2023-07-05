using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BestOrientationClickReceiver : ClickReceiver
{
    public TripletBuilder TripletBuilder;

    public override void OnClick()
    {
        TripletBuilder.FindBestShapeOrientation();
    }
}
