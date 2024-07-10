
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

public enum PieceXZRot
{
    Rot0,
    RotX,
    RotZ,
    RotXZ,
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
    public byte index{get;private set;}
    PieceRot GetNearestPieceRot(float rotY)
    {
        //Debug.Log($"euler {rot.eulerAngles}");
        //var rotY=rot.eulerAngles.y;
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

    bool isReverse(float angle)
    {
        if (angle<90) return false;
        if (angle<270) return true; 
        return false;
    }
    public PieceXZRot GetNearestPieceXZRot(Vector3 closestEuler)
    {
        //var closestEuler= FindClosestEulerAngle(transform.localRotation*child.localRotation);
        var rotX = closestEuler.x;//transform.localRotation.eulerAngles.x;
        var rotZ = closestEuler.z;//transform.localRotation.eulerAngles.z;
        Debug.Log($"euler xz {transform.localRotation.eulerAngles}");
        int isRotX= isReverse(rotX)?1:0;
        int isRotZ= isReverse(rotZ)?1:0;
        switch(isRotX+isRotZ*2)
        {
            case 0:
                return PieceXZRot.Rot0;
            case 1:
                return PieceXZRot.RotX;
            case 2:
                return PieceXZRot.RotZ;
            case 3:
                return PieceXZRot.RotXZ;
            default:
                Debug.LogError("Invalid PieceXZRot");
                return PieceXZRot.Rot0;
        }
    }

    public PieceRot GetNearestPieceRot(Vector3 closestEuler)
    {
        //var closestEuler= FindClosestEulerAngle(transform.localRotation*child.localRotation);
        return GetNearestPieceRot(closestEuler.y);
        var selfRot = (int)GetNearestPieceRot(transform.localRotation.eulerAngles.y);
        var childRot=(int)GetNearestPieceRot(child.localRotation.eulerAngles.y);
        Debug.Log($"GetNearestPieceRot selfRot:{selfRot} childRot:{childRot}");
        return (PieceRot)((selfRot+childRot)%4);
       
    }

    public Vector3 FindClosestEulerAngle()
    {
        return FindClosestEulerAngle(transform.localRotation);
    }
    Vector3 FindClosestEulerAngle(Quaternion directionQ)
    {
        var direction = directionQ * Vector3.one;
        Vector3 closestEuler = Vector3.zero;
        float maxDot = float.MinValue;
        //Debug.Log($"direction {direction}");
        foreach (Vector3 euler in eulerAngles)
        {
            Quaternion rotation = Quaternion.Euler(euler);
            Vector3 dir = rotation * Vector3.one;
            float dot = Vector3.Dot(direction.normalized, dir.normalized);
            //Debug.Log($"calc dir {dir} dot {dot} euler {euler}");
            // 内積が最大のものを探す
            if (dot > maxDot)
            {
                maxDot = dot;
                closestEuler = euler;
            }
        }

        return closestEuler;
    }
    private Vector3[] eulerAngles = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 90, 0),
        new Vector3(0, 180, 0),
        new Vector3(0, 270, 0),
        new Vector3(180, 0, 0),
        new Vector3(180, 90, 0),
        new Vector3(180, 180, 0),
        new Vector3(180, 270, 0),
        new Vector3(0, 0, 180),
        new Vector3(0, 90, 180),
        new Vector3(0, 180, 180),
        new Vector3(0, 270, 180),
        new Vector3(180, 0, 180),
        new Vector3(180, 90, 180),
        new Vector3(180, 180, 180),
        new Vector3(180, 270, 180),
    };

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
    public void SetPieceRotToTransformRot(PieceRot rot,PieceXZRot xzRot)
    {
        //child.localRotation = Quaternion.Euler(0,0,0);
        //child.localPosition = Vector3.zero;
        transform.localRotation = GetQuaternionFromPieceRot(rot,xzRot);
       
    }

    bool isXRot(PieceXZRot rot)
    {
        return rot == PieceXZRot.RotX || rot == PieceXZRot.RotXZ;
    }
    bool isZRot(PieceXZRot rot)
    {
        return rot == PieceXZRot.RotZ || rot == PieceXZRot.RotXZ;
    }

    public Vector3 GetRevisedOffset(PieceRot pieceRot, PieceXZRot pieceXZRot, int index)
    {
        var q=GetQuaternionFromPieceRot(pieceRot,pieceXZRot);
        return q * new Vector3(xOffsets[index], 0, zOffsets[index]);
    }
    /*
     public int GetRevisedXOffset(PieceRot pieceRot, PieceXZRot pieceXZRot, int index)
    {
        var q=GetQuaternionFromPieceRot(pieceRot,pieceXZRot);
        switch (pieceRot)
        {
            case PieceRot.Rot0:
                return xOffsets[index] * (isXRot(pieceXZRot) ? -1 : 1);
            case PieceRot.Rot90:
                return zOffsets[index] * (isXRot(pieceXZRot) ? -1 : 1);
            case PieceRot.Rot180:
                return -xOffsets[index] * (isXRot(pieceXZRot) ? -1 : 1);
            case PieceRot.Rot270:
                return -zOffsets[index] * (isXRot(pieceXZRot) ? -1 : 1);
            default:
                Debug.LogError("Invalid PieceRot");
                return 0;
        }
    }
    public int GetRevisedZOffset(PieceRot pieceRot,PieceXZRot pieceXZRot,int index)
    {
        switch (pieceRot)
        {
            case PieceRot.Rot0:
                return zOffsets[index] * (isZRot(pieceXZRot) ? -1 : 1) ;
            case PieceRot.Rot90:
                return -xOffsets[index] * (isZRot(pieceXZRot) ? -1 : 1) ;
            case PieceRot.Rot180:
                return -zOffsets[index] * (isZRot(pieceXZRot) ? -1 : 1) ;
            case PieceRot.Rot270:
                return xOffsets[index] * (isZRot(pieceXZRot) ? -1 : 1) ;
            default:
                Debug.LogError("Invalid PieceRot");
                return 0;
        }
    }
    */
    public void Reset()
    {
        Detach();
        transform.localPosition=initLocalPos;
        transform.localRotation=Quaternion.identity;
        child.localRotation=Quaternion.identity;
        child.localPosition=Vector3.zero;
    }

    public void ResetChild()
    {
        child.localRotation=Quaternion.identity;
    }

    private Vector3 initLocalPos;
    public void Init(byte i)
    {
        index=i;
        initLocalPos = transform.localPosition;
    }

    
    [SerializeField]IChocolatePuzzleManager manager;
    public bool IsAttached => _isAttached;
    bool _isAttached;
    public void Attach()
    {
        manager.Attach(this);
        _isAttached=true;
    }
    public void Detach()
    {
        manager.Detach(this);
        _isAttached = false;
    }
    [SerializeField]private float autoReturnThreshold=45f;
    private float AutoReturnThreshold => autoReturnThreshold * manager.PuzzleRoot.transform.localScale.x;
    public override void OnDrop()
    {        
        this.transform.rotation*=child.localRotation;
        child.localRotation=Quaternion.identity;
        //SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Attach));
        Attach();
        if(Vector3.Distance(manager.PuzzlePickupable.transform.position,transform.position)>AutoReturnThreshold)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Reset));
        }
        //Debug.Log("Ondrop IsFront:"+IsFront());
    }
    public override void OnPickup()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Detach));
    }
    
    public void Rot90()
    {
        child.localRotation = Quaternion.Euler(0,90,0);
        localRot=PieceRot.Rot90;
    }
    public void Rot180()
    {
        child.localRotation = Quaternion.Euler(0,180,0);
        localRot=PieceRot.Rot180;
    }
    public void Rot270()
    {
        child.localRotation = Quaternion.Euler(0,270,0);
        localRot=PieceRot.Rot270;
    }
    public void Rot0()
    {
        child.localRotation = Quaternion.Euler(0,0,0);
        localRot=PieceRot.Rot0;
    }

    private PieceRot localRot=PieceRot.Rot0;
    [SerializeField] private Transform child;
    public override void OnPickupUseDown()
    {
        switch (localRot)
        {
            case PieceRot.Rot0:
                SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Rot90));
                break;
            case PieceRot.Rot90:
                SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Rot180));
                break;
            case PieceRot.Rot180:
                SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Rot270));
                break;
            case PieceRot.Rot270:
                SendCustomNetworkEvent(NetworkEventTarget.All,nameof(Rot0));
                break;
        }
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
