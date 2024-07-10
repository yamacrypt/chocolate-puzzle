
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PuzzleCollectionManager : UdonSharpBehaviour
{
   [SerializeField]Transform[] holes;

    [SerializeField] private Transform[] allPieces;

    public void Show(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr,PieceXZRot[] rotXZArr)
    {
        for (byte i = 0; i < pieceCenterTileHoleIndexArr.Length; i++)
        {
            var holeIndex = pieceCenterTileHoleIndexArr[i];
            Attach(i, holeIndex, rotArr[i],rotXZArr[i]);
        }
    }
    public void Hide()
    {
        for (int i = 0; i < allPieces.Length; i++)
        {
            allPieces[i].gameObject.SetActive(false);
        }
    }

    private const int xMax = 10;
    private const int yMax = 6;
  
    public bool Attach(byte pieceIndex, byte holeIndex,PieceRot rot,PieceXZRot xzRot)
    {
        return Attach(allPieces[pieceIndex], holeIndex,rot,xzRot);
    }
    Quaternion GetQuaternionFromPieceRot(PieceRot rot, PieceXZRot xzRot)
    {
        var xEuler = 0;
        var zEuler = 0;
        switch (xzRot)
        {
            case PieceXZRot.Rot0:
                xEuler = 0;
                zEuler = 0;
                break;
            case PieceXZRot.RotX:
                xEuler = 180;
                zEuler = 0;
                break;
            case PieceXZRot.RotZ:
                xEuler = 0;
                zEuler = 180;
                break;
            case PieceXZRot.RotXZ:
                xEuler = 180;
                zEuler = 180;
                break;
        }
        switch (rot)
        {
            case PieceRot.Rot0:
                return Quaternion.Euler(xEuler,0,zEuler);
            case PieceRot.Rot90:
                return Quaternion.Euler(xEuler,90,zEuler);
            case PieceRot.Rot180:
                return Quaternion.Euler(xEuler,180,zEuler);
            case PieceRot.Rot270:
                return Quaternion.Euler(xEuler,270,zEuler);
            default:
                return Quaternion.identity;
        }
    }
    bool Attach(Transform piece, byte holeIndex,PieceRot nearestRot,PieceXZRot nearestXZRot)
    {
        piece.transform.localRotation = GetQuaternionFromPieceRot(nearestRot, nearestXZRot);
        //var centerDiff=holes[holeIndex].position-piece.position;
        piece.transform.position = holes[holeIndex].position;
        piece.gameObject.SetActive(true);
        return true;
    }
   
}
