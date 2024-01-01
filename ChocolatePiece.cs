
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public enum PieceRot
{
    Rot0,
    Rot90,
    Rot180,
    Rot270
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ChocolatePiece : UdonSharpBehaviour
{
    // center tile must be at index 0
    [SerializeField] private Transform[] tiles;
    [SerializeField] private int[] xOffsets;
    [SerializeField] private int[] zOffsets;
    public Transform CenterTile => tiles[0];
    public int PieceLength => xOffsets.Length;
    public int index;
    PieceRot GetNearestPieceRot(Quaternion rot)
    {
        var rotY=rot.eulerAngles.y;
        if(rotY<45||rotY>=315)
        {
            return PieceRot.Rot0;
        }
        else if(rotY<135)
        {
            return PieceRot.Rot90;
        }
        else if(rotY<225)
        {
            return PieceRot.Rot180;
        }
        else
        {
            return PieceRot.Rot270;
        }
    }
    public PieceRot GetNearestPieceRot()
    {
        var selfRot = (int)GetNearestPieceRot(transform.localRotation);
        var childRot=(int)GetNearestPieceRot(child.localRotation);
        return (PieceRot)((selfRot+childRot)%4);
       
    }

    public void SetPieceRotToTransformRot(PieceRot rot)
    {
        child.localRotation = Quaternion.Euler(0,0,0);
        switch (rot)
        {
            case PieceRot.Rot0:
                transform.localRotation = Quaternion.Euler(0,0,0);
                break;
            case PieceRot.Rot90:
                transform.localRotation = Quaternion.Euler(0,90,0);
                break;
            case PieceRot.Rot180:
                transform.localRotation = Quaternion.Euler(0,180,0);
                break;
            case PieceRot.Rot270:
                transform.localRotation = Quaternion.Euler(0,270,0);
                break;
            default:
                Debug.LogError("Invalid PieceRot");
                break;
        }
        
    }
   
    public int GetRevisedXOffset(PieceRot pieceRot,int index)
    {
        switch (pieceRot)
        {
            case PieceRot.Rot0:
                return xOffsets[index];
            case PieceRot.Rot90:
                return zOffsets[index];
            case PieceRot.Rot180:
                return -xOffsets[index];
            case PieceRot.Rot270:
                return -zOffsets[index];
            default:
                Debug.LogError("Invalid PieceRot");
                return 0;
        }
    }
    public int GetRevisedZOffset(PieceRot pieceRot,int index)
    {
        switch (pieceRot)
        {
            case PieceRot.Rot0:
                return zOffsets[index];
            case PieceRot.Rot90:
                return -xOffsets[index];
            case PieceRot.Rot180:
                return -zOffsets[index];
            case PieceRot.Rot270:
                return xOffsets[index];
            default:
                Debug.LogError("Invalid PieceRot");
                return 0;
        }
    }
    [SerializeField]ChocolatePuzzleManager manager;

    public void Attach()
    {
        manager.Attach(this);
    }
    public void Detach()
    {
        manager.Detach(this);
    }
    public override void OnDrop()
    {        
        SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Attach));
    }
    public override void OnPickup()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Detach));
    }

    public void Rot90()
    {
        child.localRotation *= Quaternion.Euler(0,90,0);
    }

    [SerializeField] private Transform child;
    public override void OnPickupUseDown()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Rot90));
    }

    [SerializeField] private float tileSize = 2;
    public void Align()
    {
        //var center=CenterTile.localPosition;
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].localPosition =/*center+*/ new Vector3(xOffsets[i] * tileSize, 0, zOffsets[i] * tileSize);
        }
    }
                                                                        
}
