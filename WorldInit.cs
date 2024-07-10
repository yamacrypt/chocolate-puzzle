
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WorldInit : UdonSharpBehaviour
{
    [SerializeField]IChocolatePuzzleManager puzzleManager;

    private void Start()
    {
        if (Networking.IsMaster)
        {
            SendCustomEventDelayedFrames(nameof(DelayInit), 100);
        }
    }

    [SerializeField] private Vector4[] pieceInfos;
    [SerializeField]ChocolatePuzzleDataManager dataManager;
    public void DelayInit()
    {
        for (int i = 0; i < pieceInfos.Length; i++)
        {
            var pieceIndex=(byte)pieceInfos[i].x;
            var holeIndex=(byte)pieceInfos[i].y;
            int rot=(int)pieceInfos[i].z;
            int xzRot=(int)pieceInfos[i].w;
            puzzleManager.Attach(pieceIndex, holeIndex,(PieceRot)rot,(PieceXZRot)xzRot);
        }

        /*
        var saveData=dataManager.Save();
        Debug.Log("saveData: "+saveData);
        dataManager.Load(saveData);
        */
    }
}
