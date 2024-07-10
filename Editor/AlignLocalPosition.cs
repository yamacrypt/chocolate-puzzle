using UnityEditor;
using UnityEngine;

    [CustomEditor(typeof(AlignLocalPosition), true)]
    public class AlignLocalPositionEditor :Editor

    {
    AlignLocalPosition alignObj;

    private void OnEnable()
    {
        alignObj = target as AlignLocalPosition;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("整列"))
        {
            alignObj.AlignChildY();
        }
    }
    }
