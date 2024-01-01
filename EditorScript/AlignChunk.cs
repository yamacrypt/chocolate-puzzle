#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AlignChunk : MonoBehaviour
{
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
            if (ones[i+3] != null) {
                ones[i+3].transform.localPosition = new Vector3(i,0,-3);
            }
            if (twos[i+3] != null) {
                twos[i+3].transform.localPosition = new Vector3(i,0,-2);
            }
            if (threes[i+3] != null) {
                threes[i+3].transform.localPosition = new Vector3(i,0,-1);
            }
            if (fours[i+3] != null) {
                fours[i+3].transform.localPosition = new Vector3(i,0,0);
            }
            if (fives[i+3] != null) {
                fives[i+3].transform.localPosition = new Vector3(i,0,1);
            }
            if (sixes[i+3] != null) {
                sixes[i+3].transform.localPosition = new Vector3(i,0,2);
            }
            if (sevens[i+3] != null) {
                sevens[i+3].transform.localPosition = new Vector3(i,0,3);
            }

        }
    }
}
#endif