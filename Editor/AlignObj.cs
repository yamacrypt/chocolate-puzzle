#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

 [SerializeField]
 public class SaveTransform
 {
     //参照型を保存するとインスタンスIDのみが保存され、再生前の状態に戻るので値型を保存する
     [SerializeField] private Vector3 position;
     [SerializeField] private Quaternion rotation;
     [SerializeField] private Vector3 scale;
     public Transform GetValue(Transform t)
     {
          t.position = position;
          t.rotation = rotation;
          t.localScale = scale;
          return t;
      }

     public void SetValue(Transform t)
     {    
          position = t.position;
          rotation = t.rotation;
          scale = t.localScale;
     }
 }

[CustomEditor(typeof(AlignObj), true)]
 public class Align : Editor
 {
    AlignObj alignObj;
    private void OnEnable()
     {    
        alignObj = target as AlignObj;
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