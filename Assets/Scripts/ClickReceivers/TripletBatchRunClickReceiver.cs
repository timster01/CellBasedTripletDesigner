using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripletBatchRunClickReceiver : ClickReceiver
{
    public BatchRunner batchrunner;

    public override void OnClick()
    {
        batchrunner.StartBatch();
    }
}
