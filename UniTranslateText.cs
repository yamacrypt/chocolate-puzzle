
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UniTranslateText : UdonSharpBehaviour
{
    [SerializeField]UdonLab.TranslateManager translateManager;
    [SerializeField,TextArea(3,10)]string jp;
    [SerializeField,TextArea(3,10)]string en;
    public string GetText(){
        switch(translateManager.returnLanguageName()){
            case "jp":
                return jp;
            case "eng":
                return en;
        }
        return jp;
    }
   
}
