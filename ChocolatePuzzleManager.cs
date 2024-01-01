
using System;
using HarmonyLib;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
///  puzzleの完成まで状態を管理する
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ChocolatePuzzleManager : UdonSharpBehaviour
{
    [SerializeField]Transform[] holes;
    private bool[] isFilled;

    private void Start()
    {
        isFilled=new bool[holes.Length];
        pieceToHolesDict.SetCapacity(100);
        pieceIDs=new int[allPieces.Length];
        for(int i=0;i<allPieces.Length;i++)
        {
            allPieces[i].index=i;
            pieceIDs[i]=allPieces[i].gameObject.GetInstanceID();
        }
    }

    [SerializeField] private ChocolatePiece[] allPieces;
    private int[] pieceIDs;

    private const int xMax = 10;
    private const int yMax = 6;
    // 0<x<10 0<x<6 
    int GetHoleIndex(int index,int x, int y)
    {
        var baseX = index % 10;
        var baseY = index / 10;
        var xOffset = x + baseX;
        var yOffset = y + baseY;
        if (xOffset < 0 || xOffset >= xMax || yOffset < 0 || yOffset >= yMax)
        {
            return -1;
        }
        return xOffset + yOffset * 10;
    }
    [SerializeField]IntKeyIntArrDictionary pieceToHolesDict;

    byte GetNearestHoleIndex(Transform t)
    {
        var distance=float.MaxValue;
        byte index = Byte.MaxValue;
        for (byte i = 0; i < holes.Length; i++)
        {
            var d = Vector3.Distance(t.position, holes[i].position);
            if (d < distance)
            {
                distance = d;
                index = i;
            }
        }

        return index;
    }
    
    bool IsCompleted()
    {
        for (int i = 0; i < isFilled.Length; i++)
        {
            if (!isFilled[i])
            {
                return false;
            }
        }

        return true;
    }
  
    // TODO: pazzle scaleを考慮する必要がある
    [SerializeField] private float threshold = 0.1f;
    private byte[] pieceCenterTileHoleIndexArr = new byte[12];
    private PieceRot[] pieceRotArr = new PieceRot[12];

    public bool Attach(byte pieceIndex, byte holeIndex,PieceRot rot)
    {
        return Attach(allPieces[pieceIndex], holeIndex,rot);
    }
    bool Attach(ChocolatePiece piece, byte holeIndex,PieceRot nearestRot)
    {
        // ケースとピースの対応関係を確認
        var id=piece.gameObject.GetInstanceID();
        bool isMatched=false;
        foreach(var pieceID in pieceIDs)
        {
            if (pieceID==id)
            {
                isMatched =true;
            }
        }

        if (!isMatched)
        {
            Debug.LogWarning("piece is not registerd");
            return false;
        }
        if (IsEmpty(holeIndex,nearestRot,piece))
        {
            piece.SetPieceRotToTransformRot(nearestRot);
            //piece.transform.rotation = piece.GetNearestPieceRotQuaternion();; 
            var centerDiff=holes[holeIndex].position-piece.CenterTile.position;
            piece.transform.position += centerDiff;
            var fillArr=new int[piece.PieceLength];
            for (int i = 0; i < piece.PieceLength; i++)
            {
                var xOffset = piece.GetRevisedXOffset(nearestRot,i);
                var zOffset = piece.GetRevisedZOffset(nearestRot,i);
                var index = GetHoleIndex(holeIndex,xOffset, zOffset);
                isFilled[index] = true;
                fillArr[i]=index;
                Debug.Log($"fill hole index is {index}");
                //piece.Tiles[i].position = holes[index].position;
                pieceCenterTileHoleIndexArr[piece.index] = holeIndex;
                pieceRotArr[piece.index] = nearestRot;
            }
            pieceToHolesDict.AddOrSetValue(piece.index,fillArr);
            if (IsCompleted())
            {
                OnComplete();
            }
            return true;
        }
        else
        {
            Debug.Log("not empty");
        }

        return false;
    }
    [SerializeField]ChocolatePuzzleDataManager dataManager;
    void OnComplete()
    {
        Debug.Log("Complete puzzle");
        dataManager.Add(pieceCenterTileHoleIndexArr,pieceRotArr);
    }
    public bool Attach(ChocolatePiece piece)
    {
        // from center
        var nearestHoleIndex=GetNearestHoleIndex(piece.CenterTile);
        var distance = Vector3.Distance(piece.CenterTile.position, holes[nearestHoleIndex].position);
        if (distance > threshold)
        {
            Debug.Log("distance is too far");
            return false;
        };
        var nearestRot=piece.GetNearestPieceRot();
        Debug.Log($"nearest rot is {nearestRot}");
        return Attach(piece, nearestHoleIndex,nearestRot);
    }
    
    public void Detach(ChocolatePiece piece)
    {
        var id=piece.gameObject.GetInstanceID();
        if (pieceToHolesDict.HasItem(id))
        {
            var holeIndexes=pieceToHolesDict.GetValue(piece.gameObject.GetInstanceID());
            foreach (var holeIndex in holeIndexes)
            {
                isFilled[holeIndex] = false;
            }
        }
        else
        {
            Debug.LogWarning("pieceToHolesDict doesn't have key");
        }
    }

    bool IsEmpty(int nearestHOleIndex,PieceRot nearestRot, ChocolatePiece piece)
    {
        for (int i = 0; i < piece.PieceLength; i++)
        {
            var xOffset = piece.GetRevisedXOffset(nearestRot,i);
            var zOffset = piece.GetRevisedZOffset(nearestRot,i);
            var index = GetHoleIndex(nearestHOleIndex,xOffset, zOffset);
            if (index==Byte.MaxValue || isFilled[index])
            {
                Debug.Log($"hole is already filled {index}");
                return false;
            }
        }

        return true;
    }
   
}
