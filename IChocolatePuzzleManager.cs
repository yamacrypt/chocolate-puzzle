
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class IChocolatePuzzleManager : UdonSharpBehaviour
{
    public virtual Transform PuzzleRoot { get; }
    public virtual Transform PuzzlePickupable { get; }
    public virtual bool Attach(ChocolatePiece piece)
    {
        return false;
    }

    public virtual void Detach(ChocolatePiece piece)
    {
    }

    public virtual bool Attach(byte pieceIndex, byte holeIndex, PieceRot rot, PieceXZRot xzRot)
    {
        return false;
    }
}
