#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlignChunk), true)]
 public class AlignChunkEditor : Editor
 {
    AlignChunk alignChunk;
    private void OnEnable()
     {    
        alignChunk = target as AlignChunk;
      }

      public override void OnInspectorGUI()
      {
        base.OnInspectorGUI();
        if (GUILayout.Button("整列"))
        {
            alignChunk.Align();
        }
      }
 }
 #endif