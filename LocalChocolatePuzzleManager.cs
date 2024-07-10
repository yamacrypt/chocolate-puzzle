
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
///  puzzleの完成まで状態を管理する
/// </summary>
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LocalChocolatePuzzleManager : IChocolatePuzzleManager
{
    [SerializeField]Transform[] holes;
    private bool[] isFilled;
    
    [SerializeField] private VRC_Pickup boardPickup;
   
    [SerializeField]Canvas canvas;
    public void Reset()
    {
        foreach(var piece in allPieces)piece.Reset();
    }

    bool[] changedBeforeSync=new bool[12];
    private void Start()
    {
        Init();
    }

    public void SetOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer,this.gameObject);
    }
    private bool isInit = false;
    void Init()
    {
        if (isInit) return;
        isInit=true;
        canvas.enabled = false;
        isFilled=new bool[holes.Length];
        pieceToHolesDict.SetCapacity(100);
        pieceIDs=new int[allPieces.Length];
        for(byte i=0;i<allPieces.Length;i++)
        {
            allPieces[i].Init(i);
            pieceIDs[i]=allPieces[i].gameObject.GetInstanceID();
        }

    }

    [SerializeField] private ChocolatePiece[] allPieces;
    private int[] pieceIDs;

    private const int xMax = 10;
    private const int yMax = 6;
    // 0<x<10 0<x<6 
    byte GetHoleIndex(int index,int x, int y)
    {
        var baseX = index % 10;
        var baseY = index / 10;
        var xOffset = x + baseX;
        var yOffset = y + baseY;
        if (xOffset < 0 || xOffset >= xMax || yOffset < 0 || yOffset >= yMax)
        {
            Debug.Log($"hole index {index} is out of range : {xOffset},{yOffset}");
            return byte.MaxValue;
        }
        return (byte)(xOffset + yOffset * 10);
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
    byte PieceRotToByte(PieceRot rot)
    {
        switch (rot)
        {
            case PieceRot.Rot0:
                return 0;
            case PieceRot.Rot90:
                return 1;
            case PieceRot.Rot180:
                return 2;
            case PieceRot.Rot270:
                return 3;
        }
        Debug.LogError("Invalid PieceRot");
        return 0;
    }
    byte PieceRotXZToByte(PieceXZRot rot)
    {
        switch (rot)
        {
            case PieceXZRot.Rot0:
                return 0;
            case PieceXZRot.RotX:
                return 1;
            case PieceXZRot.RotZ:
                return 2;
            case PieceXZRot.RotXZ:
                return 3;
        }
        Debug.LogError("Invalid PieceXZRot");
        return 0;
    }

    PieceXZRot ByteToPieceXZRot(byte b)
    {
        switch (b)
        {
            case 0:
                return PieceXZRot.Rot0;
            case 1:
                return PieceXZRot.RotX;
            case 2:
                return PieceXZRot.RotZ;
            case 3:
                return PieceXZRot.RotXZ;
        }
        Debug.LogError("Invalid PieceXZRot");
        return PieceXZRot.Rot0;
    }
    PieceRot ByteToPieceRot(byte b)
    {
        switch (b)
        {
            case 0:
                return PieceRot.Rot0;
            case 1:
                return PieceRot.Rot90;
            case 2:
                return PieceRot.Rot180;
            case 3:
                return PieceRot.Rot270;
        }
        Debug.LogError("Invalid PieceRot");
        return PieceRot.Rot0;
    }
    // TODO: pazzle scaleを考慮する必要がある
    [SerializeField] private float threshold = 0.1f;
    [SerializeField] private Transform puzzleRoot;
    [SerializeField] private Transform puzzlePickupable;
    public override Transform PuzzlePickupable => puzzlePickupable;
    public override Transform PuzzleRoot => puzzleRoot;
    private float Threshold => threshold * puzzleRoot.transform.localScale.x;
    private byte[] pieceCenterTileHoleIndexArr = new byte[12];
    private PieceRot[] pieceRotArr = new PieceRot[12];
    private PieceXZRot[] pieceRotXZArr = new PieceXZRot[12];

    public override bool Attach(byte pieceIndex, byte holeIndex,PieceRot rot,PieceXZRot xzRot)
    {
        return Attach(allPieces[pieceIndex], holeIndex,rot,xzRot);
    }
    byte[][] pieceToHolesArr=new byte[12][];
    bool Attach(ChocolatePiece piece, byte holeIndex,PieceRot nearestRot,PieceXZRot xzRot)
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
            Debug.LogWarning("piece is not registered");
            return false;
        }
        if (IsEmpty(holeIndex,nearestRot,xzRot,piece))
        {
            //Debug.Log($"nearestRot: {nearestRot}, xzRot: {xzRot}");
            piece.SetPieceRotToTransformRot(nearestRot,xzRot);
            //piece.transform.rotation = piece.GetNearestPieceRotQuaternion();; 
            var centerDiff=holes[holeIndex].position-piece.CenterTile.position;
            piece.transform.position += centerDiff;
            var fillArr=new int[piece.PieceLength];
            for (int i = 0; i < piece.PieceLength; i++)
            {
                //var xOffset = piece.GetRevisedXOffset(nearestRot,xzRot,i);
                //var zOffset = piece.GetRevisedZOffset(nearestRot,xzRot,i);
                var offset=piece.GetRevisedOffset(nearestRot,xzRot,i);
                //Debug.Log($"index {i} offset is {offset}");
                var index = GetHoleIndex(holeIndex, Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.z));
                isFilled[index] = true;
                fillArr[i]=index;
                //Debug.Log($"fill hole index is {index}");
                //piece.Tiles[i].position = holes[index].position;
            }
            pieceCenterTileHoleIndexArr[piece.index] = holeIndex;
            pieceRotArr[piece.index] = nearestRot;
            pieceRotXZArr[piece.index] = xzRot;
            pieceToHolesDict.AddOrSetValue(piece.index,fillArr);
            if (IsCompleted() )
            {
                OnComplete();
            }
            changedBeforeSync[piece.index] = true;
            if(boardPickup!=null)boardPickup.pickupable = false;
            return true;
        }
        else
        {
            Debug.Log("not empty");
        }

        return false;
    }
    [SerializeField]ChocolatePuzzleDataManager dataManager;
    [SerializeField]ParticleSystem[] completeParticleSystems;
    void OnComplete()
    {
        Debug.Log("Complete puzzle");
        canvas.enabled = true;
        var success=dataManager.Add(pieceCenterTileHoleIndexArr, pieceRotArr, pieceRotXZArr);
        if (success)
        {
            foreach(var ps in completeParticleSystems)
            {
                ps.Play();
            }
        }
        
    }

    public override bool Attach(ChocolatePiece piece)
    {
        // from center
        var nearestHoleIndex=GetNearestHoleIndex(piece.CenterTile);
        var distance = Vector3.Distance(piece.CenterTile.position, holes[nearestHoleIndex].position);
        if (distance > Threshold)
        {
            Debug.Log("distance is too far");
            return false;
        };
        var closestEuler = piece.FindClosestEulerAngle();
        var nearestRot=piece.GetNearestPieceRot(closestEuler);
        var nearestXZRot=piece.GetNearestPieceXZRot(closestEuler);
        Debug.Log($"nearest rot is {nearestRot}");
        Debug.Log($"nearest xzRot is {nearestXZRot}");
        
        return Attach(piece, nearestHoleIndex,nearestRot,nearestXZRot);
    }
    void Detach(byte pieceIndex)
    {
        Detach(allPieces[pieceIndex]);
    }
    public override void Detach(ChocolatePiece piece)
    {
        var pieceIndex = piece.index;
        if (pieceToHolesDict.HasItem(pieceIndex))
        {
            var holeIndexes=pieceToHolesDict.GetValue(pieceIndex);
            foreach (var holeIndex in holeIndexes)
            {
                isFilled[holeIndex] = false;
            }
            pieceToHolesDict.Remove(pieceIndex);
            changedBeforeSync[pieceIndex] = true;
            pieceCenterTileHoleIndexArr[pieceIndex] = byte.MaxValue;
            if(pieceToHolesDict.Count==0)
            {
                if(boardPickup!=null)boardPickup.pickupable = true;
            }
            canvas.enabled = false;
        }
        else
        {
            Debug.LogWarning("pieceToHolesDict doesn't have key");
        }
    }

    bool IsEmpty(int nearestHoleIndex,PieceRot nearestRot,PieceXZRot nearestXZRot, ChocolatePiece piece)
    {
        for (int i = 0; i < piece.PieceLength; i++)
        {
            var offset=piece.GetRevisedOffset(nearestRot,nearestXZRot,i);
            var index = GetHoleIndex(nearestHoleIndex, Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.z));
            if (index==Byte.MaxValue || isFilled[index])
            {
                Debug.Log($"hole is already filled {index}");
                return false;
            }
        }

        return true;
    }
}
