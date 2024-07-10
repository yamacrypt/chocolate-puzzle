
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PuzzleCompleteRewardManager : UdonSharpBehaviour
{
    [SerializeField]GameObject[] rewardObjects;
    [SerializeField] private TextMeshProUGUI completeCountText;

    private void Start()
    {
        foreach(var obj in rewardObjects)
        {
            obj.SetActive(false);
        }
    }

    public void Show(int c)
    {
        var count = c;
        completeCountText.text = count.ToString();
        for(int i=0;i<rewardObjects.Length;i++)
        {
            if (count == 0) break;
            var isActive = (count % 2)==1;
            rewardObjects[i].SetActive(isActive);
            count /= 2;
        }
    }
}
