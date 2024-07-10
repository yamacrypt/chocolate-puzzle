
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DisappearUI : UdonSharpBehaviour
{
    void Start()
    {
        
    }

    public override void Interact()
    {
        this.gameObject.SetActive(false);
    }
}
