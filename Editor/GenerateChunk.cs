#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateChunk), true)]
 public class GenerateChunkEditor : Editor
 {
    GenerateChunk alignChunk;
    private void OnEnable()
     {    
        alignChunk = target as GenerateChunk;
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