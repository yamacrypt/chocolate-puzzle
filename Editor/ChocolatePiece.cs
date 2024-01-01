#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChocolatePiece), true)]
public class ChocolatePieceEditor : Editor
{
    ChocolatePiece alignObj;
    private void OnEnable()
    {    
        alignObj = target as ChocolatePiece;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("整列"))
        {
            alignObj.Align();
        }
    }
}
#endif