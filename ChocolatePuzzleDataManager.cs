
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ChocolatePuzzleDataManager : UdonSharpBehaviour
{
    private byte[][] pieceCenterTileHoleIndexArrList;
    private PieceRot[][] pieceRotArrList;
    [SerializeField]PuzzleCollectionManager[] puzzleCollectionManagers;
    private void Start()
    {
        pieceCenterTileHoleIndexArrList= new byte[2400][];
        pieceRotArrList= new PieceRot[2400][];
        completeCount = 0;
    }

    private int completeCount = 0;

    public void Add(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr)
    {
        pieceCenterTileHoleIndexArrList[completeCount] = pieceCenterTileHoleIndexArr;
        pieceRotArrList[completeCount] = rotArr;
        puzzleCollectionManagers[completeCount%puzzleCollectionManagers.Length]. Show(pieceCenterTileHoleIndexArr,rotArr);
        completeCount++;
    }

    public void Load()
    {
        completeCount = 0;
    }

    public String Save()
    {
        var rootToken=new DataDictionary();
        {
            var puzzleListToken=new DataList();
            for (int i = 0; i < pieceCenterTileHoleIndexArrList.Length; i++)
            {
                puzzleListToken.Add(PuzzleToToken(pieceCenterTileHoleIndexArrList[i],pieceRotArrList[i]));
            }
            rootToken.Add("puzzles",puzzleListToken);
        }
        VRCJson.TrySerializeToJson(rootToken, JsonExportType.Minify,out var jsonToken);
        return jsonToken.String;
    }
    
    public DataToken PuzzleToToken(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr)
    {
        var rootToken = new DataDictionary();
        {
            var holeIndexArrToken = new DataList();
            for (int i = 0; i < pieceCenterTileHoleIndexArr.Length; i++)
            {
                holeIndexArrToken.Add(new DataToken(pieceCenterTileHoleIndexArr[i]));
            }
            rootToken.Add("holeArr", holeIndexArrToken);
            var rotArrToken = new DataList();
            for (int i = 0; i < rotArr.Length; i++)
            {
                rotArrToken.Add(new DataToken((int)rotArr[i]));
            }
            rootToken.Add("rotArr", rotArrToken);
        }
        VRCJson.TrySerializeToJson(rootToken, JsonExportType.Minify,out var jsonToken);
        return jsonToken;
    }
    
    public int JsonToHash(string json)
    {
        return json.GetHashCode();
    }
    
}
