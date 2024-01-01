#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;

public class GenerateChunk : MonoBehaviour
{
    [SerializeField]GameObject plain;
    [SerializeField]GameObject[] ones;
    [SerializeField]GameObject[] twos;
    [SerializeField]GameObject[] threes;
    [SerializeField]GameObject[] fours;
    [SerializeField]GameObject[] fives;
    [SerializeField]GameObject[] sixes;
    [SerializeField]GameObject[] sevens;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Align(){






            for(int i=-3;i<4;i++){
            if (sevens[i+3] != null) {
                 var obj = PrefabUtility.InstantiatePrefab(sevens[i+3]) as GameObject;
                obj.transform.SetParent(plain.transform);
                obj.transform.localPosition = new Vector3(i,0,-3);
                 obj.transform.localScale = new Vector3(1,1,1);
                
            }
            }
            for(int i=-3;i<4;i++){
            if (sixes[i+3] != null) {
                 var obj = PrefabUtility.InstantiatePrefab(sixes[i+3]) as GameObject;
                obj.transform.SetParent(plain.transform);
                obj.transform.localPosition = new Vector3(i,0,-2);
                 obj.transform.localScale = new Vector3(1,1,1);
                
            }
            }
            for(int i=-3;i<4;i++){
            if (fives[i+3] != null) {
                 var obj = PrefabUtility.InstantiatePrefab(fives[i+3]) as GameObject;
                obj.transform.SetParent(plain.transform);
                obj.transform.localPosition = new Vector3(i,0,-1);
                 obj.transform.localScale = new Vector3(1,1,1);
                
            }
            }
            for(int i=-3;i<4;i++){
            if (fours[i+3] != null) {
                 var obj = PrefabUtility.InstantiatePrefab(fours[i+3]) as GameObject;
                obj.transform.SetParent(plain.transform);
                obj.transform.localPosition = new Vector3(i,0,0);
                 obj.transform.localScale = new Vector3(1,1,1);
                
            }
            }
            for(int i=-3;i<4;i++){
            if (threes[i+3] != null) {
                 var obj = PrefabUtility.InstantiatePrefab(threes[i+3]) as GameObject;
                obj.transform.SetParent(plain.transform);
                obj.transform.localPosition = new Vector3(i,0,1);
                 obj.transform.localScale = new Vector3(1,1,1);
                
            }
            }
            for(int i=-3;i<4;i++){
            if (twos[i+3] != null) {
                var obj = PrefabUtility.InstantiatePrefab(twos[i+3]) as GameObject;
                obj.transform.SetParent(plain.transform);
                obj.transform.localPosition = new Vector3(i,0,2);
                 obj.transform.localScale = new Vector3(1,1,1);
                
            }
            }
            for(int i=-3;i<4;i++){
                if (ones[i+3] != null) {
                    var obj = PrefabUtility.InstantiatePrefab(ones[i+3]) as GameObject;
                    obj.transform.SetParent(plain.transform);
                    obj.transform.localPosition = new Vector3(i,0,3);
                    
                    obj.transform.localScale = new Vector3(1,1,1);
                }
            }
    }

}
#endif