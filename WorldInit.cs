
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WorldInit : UdonSharpBehaviour
{
    [SerializeField]ChocolatePuzzleManager puzzleManager;

    private void Start()
    {
        SendCustomEventDelayedFrames(nameof(DelayInit),10);
    }

    [SerializeField] private Vector3[] pieceInfos;
    public void DelayInit()
    {
        for (int i = 0; i < pieceInfos.Length; i++)
        {
            var pieceIndex=(byte)pieceInfos[i].x;
            var holeIndex=(byte)pieceInfos[i].y;
            int rot=(int)pieceInfos[i].z;
            puzzleManager.Attach(pieceIndex, holeIndex,(PieceRot)rot);
        }
    }
}
