#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AlignObj : MonoBehaviour
{
         [SerializeField]Transform start;
    [SerializeField]Transform end;
    [SerializeField]GameObject[] objs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Align(){
        if(objs.Length<2)return;
        var diff = end.position - start.position;
        var space = diff / (objs.Length - 1);
        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].transform.position = start.position + space * i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
#endif