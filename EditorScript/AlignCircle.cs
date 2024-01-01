#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AlignCircle : MonoBehaviour
{
    [SerializeField]float radius = 10.0f; // 円の半径
    [SerializeField]Transform[] objects;
    [SerializeField]float height=5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Align()
    {
        int objectCount = objects.Length;
        float angleStep = 360.0f / objectCount;

        for (int i = 0; i < objectCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad; // 角度をラジアンに変換
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 position = new Vector3(x, height, z);
            objects[i].position = position + transform.position; // オブジェクトの新しい位置を設定
        }
    }

}
#endif