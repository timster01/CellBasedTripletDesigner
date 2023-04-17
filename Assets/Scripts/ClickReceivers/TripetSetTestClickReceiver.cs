using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripetSetTestClickReceiver : ClickReceiver
{
    public TripletBuilder tripletBuilder;

    public override void OnClick()
    {
        tripletBuilder.TestSet();
    }
}
