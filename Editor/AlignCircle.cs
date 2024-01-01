#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlignCircle), true)]
 public class AlignCircleEditor : Editor
 {
    AlignCircle alignCircle;
    private void OnEnable()
     {    
        alignCircle = target as AlignCircle;
      }

      public override void OnInspectorGUI()
      {
        base.OnInspectorGUI();
        if (GUILayout.Button("整列"))
        {
            alignCircle.Align();
        }
      }
 }
 #endif