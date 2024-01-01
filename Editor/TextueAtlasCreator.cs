#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureAtlasCreator), true)]
 public class TextureAtlasCreatorEditor : Editor
 {
    TextureAtlasCreator alignChunk;
    private void OnEnable()
     {    
        alignChunk = target as TextureAtlasCreator;
      }

      public override void OnInspectorGUI()
      {
        base.OnInspectorGUI();
        if (GUILayout.Button("atlas"))
        {
            alignChunk.CreateTextureAtlas();
        }
         if (GUILayout.Button("toPNG"))
        {
            alignChunk.AtlasToPng();
        }
      }
 }
 #endif