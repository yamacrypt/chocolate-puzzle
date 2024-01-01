
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

    public void Show(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr)
    {
        for (byte i = 0; i < pieceCenterTileHoleIndexArr.Length; i++)
        {
            var holeIndex = pieceCenterTileHoleIndexArr[i];
            var rot = rotArr[i];
            Attach(i, holeIndex, rot);
        }
    }

    private const int xMax = 10;
    private const int yMax = 6;
  
    public bool Attach(byte pieceIndex, byte holeIndex,PieceRot rot)
    {
        return Attach(allPieces[pieceIndex], holeIndex,rot);
    }
    bool Attach(Transform piece, byte holeIndex,PieceRot nearestRot)
    {
        switch (nearestRot)
        {
            case PieceRot.Rot0:
                piece.transform.localRotation = Quaternion.Euler(0,0,0);
                break;
            case PieceRot.Rot90:
                piece.transform.localRotation = Quaternion.Euler(0,90,0);
                break;
            case PieceRot.Rot180:
                piece.transform.localRotation = Quaternion.Euler(0,180,0);
                break;
            case PieceRot.Rot270:
                piece.transform.localRotation = Quaternion.Euler(0,270,0);
                break;
            default:
                Debug.LogError("Invalid PieceRot");
                break;
        }
        //var centerDiff=holes[holeIndex].position-piece.position;
        piece.transform.position = holes[holeIndex].position;
        piece.gameObject.SetActive(true);
        return true;
    }
   
}
