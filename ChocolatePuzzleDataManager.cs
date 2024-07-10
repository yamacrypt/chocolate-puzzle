
using System;
using RC4Cryptography;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ChocolatePuzzleDataManager : UdonSharpBehaviour
{
    private byte[][] pieceCenterTileHoleIndexArrList;
    private byte[][] pieceRotArrList;
    private byte[][] pieceRotXZArrList;
    const int maxPuzzleCount=2400;
    [SerializeField]PuzzleCollectionManager[] puzzleCollectionManagers;
    private void Start()
    {
        pieceCenterTileHoleIndexArrList= new byte[maxPuzzleCount][];
        pieceRotArrList= new byte[maxPuzzleCount][];
        pieceRotXZArrList= new byte[maxPuzzleCount][];
        hashes=new int[maxPuzzleCount];
        completeCount = 0;
    }
    [SerializeField]PuzzleCompleteRewardManager puzzleCompleteRewardManager;
    private int completeCount = 0;
    int[] hashes;
    private const int width = 10;
    private const int height = 6;
    [SerializeField]ChocolatePiece[] allPieces;
    byte GetHoleIndex(int index,int x, int y)
    {
        var baseX = index % 10;
        var baseY = index / 10;
        var xOffset = x + baseX;
        var yOffset = y + baseY;
        if (xOffset < 0 || xOffset >= width || yOffset < 0 || yOffset >= height)
        {
            Debug.LogWarning($"hole index {index} is out of range : {xOffset},{yOffset}");
            return byte.MaxValue;
        }
        return (byte)(xOffset + yOffset * 10);
    }
    int ComputeHash(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr,PieceXZRot[] rotXZArr)
    {
        var firstIndex = pieceCenterTileHoleIndexArr[0];
        //Debug.Log($"firstIndex: {firstIndex}");
        var firstIndexX = firstIndex % 10;
        var firstIndexZ = firstIndex / 10;
        var swapX=firstIndexX>(width/2);
        var swapZ=firstIndexZ>(height/2);
        //var swapArr = new byte[pieceCenterTileHoleIndexArr.Length];
        var holeArr=new byte[width*height];
        for (byte i = 0; i < pieceCenterTileHoleIndexArr.Length; i++)
        {
            var indexX = pieceCenterTileHoleIndexArr[i] % 10;
            var indexZ = pieceCenterTileHoleIndexArr[i] / 10;
            //var rotX = (int)rotXZArr[i] % 2;
            //var rotZ = (int)rotXZArr[i] / 2;
            var swapIndexX = swapX ? width - indexX - 1 : indexX;
            var swapIndexZ = swapZ ? height - indexZ - 1 : indexZ;
            //var swapRotX=(rotX + (swapX ? 1 : 0)) % 2;
            //var swapRotZ=(rotZ + (swapZ ? 1 : 0)) % 2;
            //var swapRotXZ=(PieceXZRot)(swapRotZ*2+swapRotX);
            //var swapQuaternion=Quaternion.Euler(swapZ?180:0,0,swapX?180:0);
            //Debug.Log($"swapArr[{i}] : {swapIndexZ} {swapIndexX}");
            var swapHoleIndex= (byte)(swapIndexX + swapIndexZ * 10);
            var piece=allPieces[i];
            for (int j = 0; j < piece.PieceLength; j++)
            {
                var offset=piece.GetRevisedOffset(rotArr[i],rotXZArr[i],j);
                var offsetX=Mathf.RoundToInt(offset.x) * (swapX?-1:1);
                var offsetZ=Mathf.RoundToInt(offset.z) * (swapZ?-1:1);
                var index = GetHoleIndex(swapHoleIndex, offsetX,offsetZ);
                //Debug.Log($"holeArr[{index}] : {i}");
                holeArr[index] = i;
            }
        }
        var hash=_hashLibrary.MD5_Bytes(holeArr);
        //Debug.Log($"hash string: {hash}");
        return hash.GetHashCode();
    }
    public bool IsDuplicated(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr,PieceXZRot[] rotXZArr)
    {
        var hash=ComputeHash(pieceCenterTileHoleIndexArr,rotArr,rotXZArr);
        return IsDuplicated(hash);
    }
    bool IsDuplicated(int hash)
    {
        for (int i = 0; i < completeCount; i++)
        {
            //Debug.Log($"hash: {hash} , hashes[{i}]: {hashes[i]}");
            if(hash==hashes[i])
            {
                return true;
            }
        }
        return false;
    }
    // must check not isDuplicated before call this
    byte[] CopyByteArr(byte[] from)
    {
        var dest=new byte[from.Length];
        for (int i = 0; i < from.Length; i++)
        {
            dest[i] = from[i];
        }

        return dest;
    }
    public bool Add(byte[] pieceCenterTileHoleIndexArr,PieceRot[] rotArr,PieceXZRot[] rotXZArr,bool calcReward=true)
    {
        var hash=ComputeHash(pieceCenterTileHoleIndexArr,rotArr,rotXZArr);
        if(IsDuplicated(pieceCenterTileHoleIndexArr,rotArr,rotXZArr))
        {
            Debug.LogWarning("IsDuplicated(pieceCenterTileHoleIndexArr)");
            return false;
        }
        pieceCenterTileHoleIndexArrList[completeCount]= CopyByteArr(pieceCenterTileHoleIndexArr);
        pieceRotArrList[completeCount] = PieceRotArrToBytes(rotArr);
        pieceRotXZArrList[completeCount] = PieceRotXZArrToBytes(rotXZArr);
        puzzleCollectionManagers[completeCount % puzzleCollectionManagers.Length]
            .Show(pieceCenterTileHoleIndexArr, rotArr,rotXZArr);
        hashes[completeCount] = hash;
        //Debug.Log($"hash[{completeCount}]: {hashes[completeCount]}");
        completeCount++;
        if (calcReward)
        {
            puzzleCompleteRewardManager.Show(completeCount);
        }
        return true;
    }
    public static bool IsBase64String(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0)
        {
            return false;
        }

        int paddingCount = 0;
        foreach (char c in value)
        {
            if (c == '=')
            {
                paddingCount++;
            }
            else if (!IsBase64Char(c))
            {
                return false;
            }
        }

        return paddingCount == 0 || paddingCount == 1 || paddingCount == 2;
    }

    private static bool IsBase64Char(char c)
    {
        return char.IsLetterOrDigit(c) || c == '+' || c == '/';
    }
    public bool Load(String saveData)
    {
        if (isOperating)
        {
            Debug.LogWarning("isOperating now");
            return false;
        }
        isOperating = true;
        pieceCenterTileHoleIndexArrList= new byte[maxPuzzleCount][];
        pieceRotArrList= new byte[maxPuzzleCount][];
        pieceRotXZArrList= new byte[maxPuzzleCount][];
        hashes=new int[maxPuzzleCount];
        completeCount = 0;
        foreach (var collectionManager in puzzleCollectionManagers)
        {
            collectionManager.Hide();
        }
        if (saveData.Length < 64)
        {
            Debug.LogWarning("saveData.Length < 64");
        }
        else
        {
            string hash = saveData.Substring(0, 64);
            var base64Str = saveData.Substring(64);
            if (!IsBase64String(base64Str))
            {
                Debug.LogWarning("not IsBase64String(base64Str)");
            }
            else
            {
                byte[] encrypted = Convert.FromBase64String(base64Str);
                byte[] payload = _rc4Library.Apply(encrypted, ToUTF8(GetPassword().ToCharArray()));
                string inputHash = _hashLibrary.SHA256_Bytes(payload);
                if (hash != inputHash)
                {
                    Debug.LogWarning($"hash check failed: {hash} {inputHash}");
                }
                else
                {
                    isOperating = false;
                    return BytesToCompletePuzzles(payload);
                }
            }
        }
        isOperating = false;
        return false;
    }
    bool isOperating=false;
    
    [SerializeField] private string secretToken = "default password";
    [SerializeField] private UdonHashLib _hashLibrary;
    [SerializeField] private UdonRC4 _rc4Library;
    private string GetPassword()
    {
        return secretToken;
    }
    private char[] FromUTF8(byte[] bytes)
    {
        char[] buffer = new char[bytes.Length];
        int writeIndex = 0;

        for (int i = 0; i < bytes.Length; i++)
        {
            uint character = bytes[i];

            if (character < 0x80) {
                buffer[writeIndex++] = (char)character;
            } else if (character < 0xE0) {
                buffer[writeIndex++] = (char)(
                    (character & 0b11111) << 6 |
                    (bytes[++i] & (uint)0b111111)
                );
            } else if (character < 0xF0) {
                buffer[writeIndex++] = (char)(
                    (character & 0b1111) << 12 |
                    (bytes[++i] & (uint)0b111111) << 6 |
                    (bytes[++i] & (uint)0b111111)
                );
            } else {
                buffer[writeIndex++] = (char)(
                    (character & 0b111) << 18 |
                    (bytes[++i] & (uint)0b111111) << 12 |
                    (bytes[++i] & (uint)0b111111) << 6 |
                    (bytes[++i] & (uint)0b111111)
                );
            }
        }

        // We do this to truncate off the end of the array
        // This would be a lot easier with Array.Resize, but Udon once again does not allow access to it.
        char[] output = new char[writeIndex];

        for (int i = 0; i < writeIndex; i++)
            output[i] = buffer[i];

        return output;
    }
    // Copy from UdonHashLib
    private byte[] ToUTF8(char[] characters)
    {
        byte[] buffer = new byte[characters.Length * 4];

        int writeIndex = 0;
        for (int i = 0; i < characters.Length; i++)
        {
            uint character = characters[i];

            if (character < 0x80)
            {
                buffer[writeIndex++] = (byte)character;
            } else if (character < 0x800)
            {
                buffer[writeIndex++] = (byte)(0b11000000 | ((character >> 6) & 0b11111));
                buffer[writeIndex++] = (byte)(0b10000000 | (character & 0b111111));
            } else if (character < 0x10000)
            {
                buffer[writeIndex++] = (byte)(0b11100000 | ((character >> 12) & 0b1111));
                buffer[writeIndex++] = (byte)(0b10000000 | ((character >> 6) & 0b111111));
                buffer[writeIndex++] = (byte)(0b10000000 | (character & 0b111111));
            } else
            {
                buffer[writeIndex++] = (byte)(0b11110000 | ((character >> 18) & 0b111));
                buffer[writeIndex++] = (byte)(0b10000000 | ((character >> 12) & 0b111111));
                buffer[writeIndex++] = (byte)(0b10000000 | ((character >> 6) & 0b111111));
                buffer[writeIndex++] = (byte)(0b10000000 | (character & 0b111111));
            }
        }

        // We do this to truncate off the end of the array
        // This would be a lot easier with Array.Resize, but Udon once again does not allow access to it.
        byte[] output = new byte[writeIndex];

        for (int i = 0; i < writeIndex; i++)
            output[i] = buffer[i];

        return output;
    }

    byte[] PieceRotArrToBytes(PieceRot[] rots)
    {
        var res = new byte[rots.Length];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = PieceRotToByte(rots[i]);
        }

        return res;
    }

    byte[] PieceRotXZArrToBytes(PieceXZRot[] rots)
    {
        var res = new byte[rots.Length];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = PieceRotXZToByte(rots[i]);
        }

        return res;
    }
    // BytesToPieceXZRotArr
    PieceXZRot[] BytesToPieceRotXZArr(byte[] bytes)
    {
        var res = new PieceXZRot[bytes.Length];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = ByteToPieceXZRot(bytes[i]);
        }

        return res;
    }
    PieceRot[] BytesToPieceRotArr(byte[] bytes)
    {
        var res = new PieceRot[bytes.Length];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = ByteToPieceRot(bytes[i]);
        }

        return res;
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
    private byte[] threeBitArrCache = new byte[3];
    private byte[] fourPieceRotByteArrCache = new byte[4];
    byte[] ThreeBitNumToByteArr(int i)
    {
        threeBitArrCache[0] = (byte)(i >> 16);
        threeBitArrCache[1] = (byte)((i >> 8)%(1<<8));
        threeBitArrCache[2] = (byte)(i%(1<<8));
        return threeBitArrCache;
    }
    byte[] ByteToPieceRotByteArr(byte b)
    {
        fourPieceRotByteArrCache[0] = (byte)((b >> 6)%(1<<2));
        fourPieceRotByteArrCache[1] = (byte)((b >> 4)%(1<<2));
        fourPieceRotByteArrCache[2] = (byte)((b >> 2)%(1<<2));
        fourPieceRotByteArrCache[3] = (byte)(b%(1<<2));
        return fourPieceRotByteArrCache;
    }
 
    byte[] CompletePuzzlesToBytes()
    {
        var rotByteSize = 2 * 12 / 8; //2bit * 12piece / 8bit = 3byte
        var rotXZByteSize = 2 * 12 / 8; //2bit * 12piece / 8bit = 3byte
        var holeIndexByteSize = 6 * 12 / 8; //6bit * 12piece / 8bit = 9byte
        var puzzleByteSize=rotByteSize+rotXZByteSize+holeIndexByteSize;
        var outputs=new byte[puzzleByteSize*completeCount];
        Debug.Log($"completeCount:{completeCount}");
        for (int i = 0; i < completeCount; i++)
        {
             var rotArr = pieceRotArrList[i];
             var holeIndexArr = pieceCenterTileHoleIndexArrList[i];
             var rotXZArr = pieceRotXZArrList[i];
             if(rotArr==null || holeIndexArr==null || rotXZArr==null)
             {
                 
                 Debug.LogWarning("rotArr==null || holeIndexArr==null || rotXZArr==null");
                 break;
             }
             // y rot
             {
                 var rotByteIndex1 = 0 + puzzleByteSize * i;
                 var rotByteIndex2 = 1 + puzzleByteSize * i;
                 var rotByteIndex3 = 2 + puzzleByteSize * i;
                 outputs[rotByteIndex1] = (byte)(rotArr[0] << 6 | rotArr[1] << 4 | rotArr[2] << 2 | rotArr[3]);
                 outputs[rotByteIndex2] = (byte)(rotArr[4] << 6 | rotArr[5] << 4 | rotArr[6] << 2 | rotArr[7]);
                 outputs[rotByteIndex3] = (byte)(rotArr[8] << 6 | rotArr[9] << 4 | rotArr[10] << 2 | rotArr[11]);
             }

             //xz rot
             {
                 var rotXZByteIndex1 = rotByteSize + 0 + puzzleByteSize * i;
                 var rotXZByteIndex2 = rotByteSize + 1 + puzzleByteSize * i;
                 var rotXZByteIndex3 = rotByteSize + 2 + puzzleByteSize * i;
                 outputs[rotXZByteIndex1] =
                     (byte)(rotXZArr[0] << 6 | rotXZArr[1] << 4 | rotXZArr[2] << 2 | rotXZArr[3]);
                 outputs[rotXZByteIndex2] =
                     (byte)(rotXZArr[4] << 6 | rotXZArr[5] << 4 | rotXZArr[6] << 2 | rotXZArr[7]);
                 outputs[rotXZByteIndex3] =
                     (byte)(rotXZArr[8] << 6 | rotXZArr[9] << 4 | rotXZArr[10] << 2 | rotXZArr[11]);
             }

             // hole index
             {
                 int holeIndexInt1 = holeIndexArr[0] << 18 | holeIndexArr[1] << 12 | holeIndexArr[2] << 6 |
                                     holeIndexArr[3];
                 int holeIndexInt2 = holeIndexArr[4] << 18 | holeIndexArr[5] << 12 | holeIndexArr[6] << 6 |
                                     holeIndexArr[7];
                 int holeIndexInt3 = holeIndexArr[8] << 18 | holeIndexArr[9] << 12 | holeIndexArr[10] << 6 |
                                     holeIndexArr[11];
                 Debug.Log("holeIndexInt1:" + holeIndexInt1 + " holeIndexInt2:" + holeIndexInt2 + " holeIndexInt3:" +
                           holeIndexInt3 + "");
                 var holeIndexByteArr1 = ThreeBitNumToByteArr(holeIndexInt1);
                 for (int j = 0; j < 3; j++)
                 {
                     outputs[rotByteSize + rotXZByteSize + j + puzzleByteSize * i] = holeIndexByteArr1[j];
                 }

                 var holeIndexByteArr2 = ThreeBitNumToByteArr(holeIndexInt2);
                 for (int j = 0; j < 3; j++)
                 {
                     outputs[rotByteSize + rotXZByteSize + 3 + j + puzzleByteSize * i] = holeIndexByteArr2[j];
                 }

                 var holeIndexByteArr3 = ThreeBitNumToByteArr(holeIndexInt3);
                 for (int j = 0; j < 3; j++)
                 {
                     outputs[rotByteSize + rotXZByteSize + 6 + j + puzzleByteSize * i] = holeIndexByteArr3[j];
                 }
             }
        }
        return outputs;
    }

    bool BytesToCompletePuzzles(byte[] bytes)
    {
        Debug.Log($"BytesToCompletePuzzles bytes.length:{bytes.Length}");
        // reverse operation
        var rotByteSize = 2 * 12 / 8; //2bit * 12piece / 8bit = 3byte
        var rotXZByteSize = 2 * 12 / 8; //2bit * 12piece / 8bit = 3byte
        var holeIndexByteSize = 6 * 12 / 8; //6bit * 12piece / 8bit = 9byte
        var puzzleByteSize=rotByteSize+rotXZByteSize+holeIndexByteSize;
        var puzzleCount=bytes.Length/puzzleByteSize;
        if (bytes.Length % puzzleByteSize != 0)
        {
            Debug.LogWarning("bytes.Length %puzzleByteSize!=0");
            return false;
        }
        for (int i = 0; i < puzzleCount; i++)
        {
            // yrot
            var pieceRotArr = new byte[12];
            {
                var rotByteIndex1 = 0 + puzzleByteSize * i;
                var rotByteIndex2 = 1 + puzzleByteSize * i;
                var rotByteIndex3 = 2 + puzzleByteSize * i;
                var rotByte1 = bytes[rotByteIndex1];
                var rotByte2 = bytes[rotByteIndex2];
                var rotByte3 = bytes[rotByteIndex3];
                var rotPieceArr1 = ByteToPieceRotByteArr(rotByte1);
                for (int j = 0; j < 4; j++)
                {
                    pieceRotArr[j] = rotPieceArr1[j];
                }

                var rotPieceArr2 = ByteToPieceRotByteArr(rotByte2);
                for (int j = 0; j < 4; j++)
                {
                    pieceRotArr[j + 4] = rotPieceArr2[j];
                }

                var rotPieceArr3 = ByteToPieceRotByteArr(rotByte3);
                for (int j = 0; j < 4; j++)
                {
                    pieceRotArr[j + 8] = rotPieceArr3[j];
                }
            }
            // zrot
            var pieceXZRotArr = new byte[12];
            {
                var rotByteIndex1 = rotByteSize+0 + puzzleByteSize * i;
                var rotByteIndex2 = rotByteSize+1 + puzzleByteSize * i;
                var rotByteIndex3 = rotByteSize+2 + puzzleByteSize * i;
                var rotByte1 = bytes[rotByteIndex1];
                var rotByte2 = bytes[rotByteIndex2];
                var rotByte3 = bytes[rotByteIndex3];
                var rotPieceArr1 = ByteToPieceRotByteArr(rotByte1);
                for (int j = 0; j < 4; j++)
                {
                    pieceXZRotArr[j] = rotPieceArr1[j];
                }

                var rotPieceArr2 = ByteToPieceRotByteArr(rotByte2);
                for (int j = 0; j < 4; j++)
                {
                    pieceXZRotArr[j + 4] = rotPieceArr2[j];
                }

                var rotPieceArr3 = ByteToPieceRotByteArr(rotByte3);
                for (int j = 0; j < 4; j++)
                {
                    pieceXZRotArr[j + 8] = rotPieceArr3[j];
                }
            }

            // holeindex
            var pieceCenterTileHoleIndexArr = new byte[12];
            {
                {
                    int holeIndexInt1 = 0;
                    for (int j = 0; j < 3; j++)
                    {
                        holeIndexInt1 += bytes[rotByteSize + rotXZByteSize + j + puzzleByteSize * i] << (8 * (2 - j));
                    }

                    pieceCenterTileHoleIndexArr[0] = (byte)((holeIndexInt1 >> 18) % (1 << 6));
                    pieceCenterTileHoleIndexArr[1] = (byte)((holeIndexInt1 >> 12) % (1 << 6));
                    pieceCenterTileHoleIndexArr[2] = (byte)((holeIndexInt1 >> 6) % (1 << 6));
                    pieceCenterTileHoleIndexArr[3] = (byte)(holeIndexInt1 % (1 << 6));
                }
                {
                    var holeIndexInt2 = 0;
                    for (int j = 0; j < 3; j++)
                    {
                        holeIndexInt2 += bytes[rotByteSize + rotXZByteSize + 3 + j + puzzleByteSize * i] << (8 * (2 - j));
                    }

                    pieceCenterTileHoleIndexArr[4] = (byte)((holeIndexInt2 >> 18) % (1 << 6));
                    pieceCenterTileHoleIndexArr[5] = (byte)((holeIndexInt2 >> 12) % (1 << 6));
                    pieceCenterTileHoleIndexArr[6] = (byte)((holeIndexInt2 >> 6) % (1 << 6));
                    pieceCenterTileHoleIndexArr[7] = (byte)(holeIndexInt2 % (1 << 6));
                }
                {
                    var holeIndexInt3 = 0;
                    for (int j = 0; j < 3; j++)
                    {
                        holeIndexInt3 += bytes[rotByteSize + rotXZByteSize + 6 + j + puzzleByteSize * i] << (8 * (2 - j));
                    }

                    pieceCenterTileHoleIndexArr[8] = (byte)((holeIndexInt3 >> 18) % (1 << 6));
                    pieceCenterTileHoleIndexArr[9] = (byte)((holeIndexInt3 >> 12) % (1 << 6));
                    pieceCenterTileHoleIndexArr[10] = (byte)((holeIndexInt3 >> 6) % (1 << 6));
                    pieceCenterTileHoleIndexArr[11] = (byte)(holeIndexInt3 % (1 << 6));
                }
            }
            var success=Add(pieceCenterTileHoleIndexArr,BytesToPieceRotArr(pieceRotArr),BytesToPieceRotXZArr(pieceXZRotArr),false);
            /*if (success)
            {
                var index = completeCount - 1;
                foreach (var rot in pieceRotArrList[index])
                {
                    Debug.Log("rot:" + rot);
                }

                foreach (var rot in pieceRotXZArrList[index])
                {
                    Debug.Log("xz rot:" + rot);
                }

                foreach (var pieceHoleIndex in pieceCenterTileHoleIndexArrList[index])
                {
                    Debug.Log("pieceHoleIndex:" + pieceHoleIndex);
                }
            }*/
        }
        puzzleCompleteRewardManager.Show(completeCount);
        Debug.Log($"completeCount:{completeCount}");
        return true;
    }
    private string Encrypt()
    {
        byte[] payload = CompletePuzzlesToBytes();
        byte[] encrypted = _rc4Library.Apply(
            payload,
            ToUTF8(GetPassword().ToCharArray())
        );
        return _hashLibrary.SHA256_Bytes(payload) + Convert.ToBase64String(encrypted);
    }

    /*private string Decrypt(string saveData)
    {
        if (saveData.Length < 64) return "";
        string hash = saveData.Substring(0, 64);
        byte[] encrypted = Convert.FromBase64String(saveData.Substring(64));
        byte[] payload = _rc4Library.Apply(encrypted, ToUTF8(GetPassword().ToCharArray()));
        string inputHash = _hashLibrary.SHA256_Bytes(payload);
        if (hash != inputHash) {
            Debug.LogError($"hash check failed: {hash} {inputHash}");
            return "";
        }
        return new string(FromUTF8(payload));
    }*/

    public String Save()
    {
        if (isOperating)
        {
            Debug.LogWarning("isOperating now");
            return "isOperating now";
        }
        if(completeCount==0)
        {
            Debug.LogWarning("completeCount==0");
            return "no data";
        }
        isOperating = true;
        /*var rootToken=new DataDictionary();
        {
            var puzzleListToken=new DataList();
            for (int i = 0; i < completeCount; i++)
            {
                puzzleListToken.Add(PuzzleToToken(pieceCenterTileHoleIndexArrList[i],pieceRotArrList[i]));
            }
            rootToken.Add("puzzles",puzzleListToken);
        }
        VRCJson.TrySerializeToJson(rootToken, JsonExportType.Minify,out var jsonToken);*/
        var res=Encrypt();
        isOperating = false;
        return res;
    }
    
    /*public DataToken PuzzleToToken(byte[] pieceCenterTileHoleIndexArr,byte[] rotArr)
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
                rotArrToken.Add(new DataToken(rotArr[i]));
            }
            rootToken.Add("rotArr", rotArrToken);
        }
        VRCJson.TrySerializeToJson(rootToken, JsonExportType.Minify,out var jsonToken);
        return jsonToken;
    }
    
    public int JsonToHash(string json)
    {
        return json.GetHashCode();
    }*/
    
}
