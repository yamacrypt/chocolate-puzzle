//SmartPickupSharp ver1.0

//当プログラムは、NEET ENIGNEER様のUdon Graphコード"Smart Pickup"を
//InPlanariaがUdon Sharp化し、機能追加を行ったものです。
//利用規約
//有償無償問わず再配布可　改変可　VRChatのprivateまたはpublicワールドへの組み込み可　クレジット表記不要　ただし製作者の詐称を禁止する

//This program is based on the Udon Graph code "Smart Pickup" by NEET ENIGNEER, which was transcribed by InPlanaria into Udon Sharp with additional functionality.
//Terms of Use
//Redistribution is allowed with or without compensation. Modifications are allowed.　Can be incorporated into private or public VRChat worlds. No credit needed, but do not misrepresent the its creator.

//SmartPickup by NEET ENGINEER https://neet-shop.booth.pm/items/2981343
//SmartPickupSharp by InPlanaria https://inplanaria.booth.pm/items/3640206

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ChocolatePieceSmartPickupSharp : UdonSharpBehaviour
{
    private VRCPlayerApi LocalPlayer;
    [UdonSynced]private Vector3 RelativePos;
    [UdonSynced]private Quaternion RelativeRot;
    [UdonSynced]private Vector3 Pos;
    [UdonSynced]private Quaternion Rot;
    [UdonSynced]private bool IsHeld;
    /*public*/ VRC_Pickup Pickup;
    [UdonSynced]private bool RightHand;

    [Header("   ")]
    [Header("現在の手とオブジェクト間の相対位置が、掴んだ時の相対位置からMax**Err以上ずれた場合")]
    [Header("位置を再同期します。これによる同期の間隔はSyncInterval[秒]より短くなりません")]
    [Header("If the relative position between the current hand and the object deviates ")]
    [Header("more than Max**Err from the relative position at the time of grabbing,")]
    [Header("the position will be resynchronized. The synchronization interval will not be shorter than SyncInterval [seconds].")]


    public float MaxDistanceErr = 0.05f;
    public float MaxRotationErr = 5.0f;
    private float SinceLastRequest;
    private Vector3 RelativePosBuff;
    private Quaternion RelativeRotBuff;
    public float SyncInterval = 0.5f;
    private BoxCollider ThisCol;//BoxCollider以外を使いたい場合はThisColの型とStart()内の一行を変える
    private float Tmr;
    private Transform TransformCache;

    [Header("   ")]
    [Header("トリガー押下時にCustomEventを送るUdonBehaviorとEvent名")]
    [Header("UdonBehavior and Event name to send CustomEvent when the trigger is pressed.")]
    public UdonBehaviour SendEvent_UseDown;
    public string SendEventName_UseDown;
 
    [Header("   ")]
    [Header("トリガー離した時にCustomEventを送るUdonBehaviorとEvent名")]
    [Header("UdonBehavior and Event name to send CustomEvent when the trigger is released.")]
    public UdonBehaviour SendEvent_UseUp;
    public string SendEventName_UseUp;

    [Header("   ")]
    [Header("つかんだ時にCustomEventを送るUdonBehaviorとEvent名")]
    [Header("UdonBehavior and Event name to send CustomEvent when picked up.")]
    public UdonBehaviour SendEvent_Pickup;
    public string SendEventName_Pickup;
    [Header("   ")]
    [Header("Drop時にCustomEventを送るUdonBehaviorとEvent名")]
    [Header("UdonBehavior and Event name to send CustomEvent when dropped.")]
    public UdonBehaviour SendEvent_Drop;
    public string SendEventName_Drop;
    
    private Vector3 First_Pos;//位置リセットメソッド用
    private Quaternion First_Rot;//位置リセットメソッド用
   
    void Start()
    {
        ThisCol = this.GetComponent<BoxCollider>();//BoxCollider以外を使いたい場合はこの行とThisColの型を変える
        TransformCache = this.GetComponent<Transform>();
        Pickup = this.GetComponent<VRC_Pickup>();
        if(Networking.LocalPlayer != null){
            LocalPlayer = Networking.LocalPlayer;

            OnDrop_Dlayed();

            First_Pos = TransformCache.position;//位置リセットメソッド用
            First_Rot = TransformCache.rotation;//位置リセットメソッド用
        }
    }

    public void ResetPosition(){//位置リセットメソッド
        TransformCache.SetPositionAndRotation(First_Pos,First_Rot);
        if(Networking.IsOwner(this.gameObject)){
            if(Pickup.IsHeld){
                Pickup.Drop();
            }else{
                SendCustomEventDelayedSeconds(nameof(OnDrop_Dlayed),0.1f);
            }
        }

    }

    public override void OnPickupUseDown()//Planaria追加、UseDownでイベント送信
    {
        if(SendEventName_UseDown != null && SendEvent_UseDown != null){
            SendEvent_UseDown.SendCustomEvent(SendEventName_UseDown);
        }
    }

    public override void OnPickupUseUp()//Planaria追加
    {
        if(SendEventName_UseUp != null && SendEvent_UseUp != null){
            SendEvent_UseUp.SendCustomEvent(SendEventName_UseUp);
        }
    }

    public override void OnDrop(){
        SendCustomEventDelayedSeconds(nameof(OnDrop_Dlayed),0.1f);
        OnDrop_Dlayed();


        if(SendEventName_Drop != null && SendEvent_Drop != null){//Planaria追加
            SendEvent_Drop.SendCustomEvent(SendEventName_Drop);
        }
    }

    public void OnDrop_Dlayed(){
        Pos = TransformCache.localPosition;
        Rot = TransformCache.localRotation;
        IsHeld = false;
        RequestSerialization();
    }

    public override void OnPickup(){
        LocalPlayer = Networking.LocalPlayer;
        Networking.SetOwner(LocalPlayer,this.gameObject);
        IsHeld = LocalPlayer.IsUserInVR();

        
        SendCustomEventDelayedSeconds(nameof(OnPickup_Delayed),0.5f);
        OnPickup_Dlayed2();
        if(SendEventName_Pickup != null && SendEvent_Pickup != null){//Planaria追加
            SendEvent_Pickup.SendCustomEvent(SendEventName_Pickup);
        }
    }

    public void OnPickup_Delayed(){

        IsHeld = true;
        OnPickup_Dlayed2(); 
    } 

    public void OnPickup_Dlayed2(){
        LocalPlayer = Networking.LocalPlayer;
        Vector3 HandPosR = LocalPlayer.GetBonePosition(HumanBodyBones.RightHand);
        Vector3 HandPosL = LocalPlayer.GetBonePosition(HumanBodyBones.LeftHand);

        //RightHand = Vector3.Distance(HandPosR,TransformCache.position) 
        //           <= Vector3.Distance(TransformCache.position,HandPosL);
        RightHand = Pickup.currentHand == VRC_Pickup.PickupHand.Right;//Planaria変更


        if(RightHand){
            Quaternion HandRotR = LocalPlayer.GetBoneRotation(HumanBodyBones.RightHand);
            RelativePos = Quaternion.Inverse(HandRotR) * (TransformCache.position - HandPosR);
            RelativeRot = Quaternion.Inverse(HandRotR) * TransformCache.rotation;
            RequestSerialization();
        }
        else{
            Quaternion HandRotL = LocalPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
            RelativePos = Quaternion.Inverse(HandRotL) * (TransformCache.position - HandPosL);
            RelativeRot = Quaternion.Inverse(HandRotL) * TransformCache.rotation;
            RequestSerialization();
        }
    }


    void Update()
    {
        SinceLastRequest = SinceLastRequest + Time.deltaTime;
        if(LocalPlayer != null){
            if(Networking.IsOwner(this.gameObject)){
                ThisCol.enabled = true;
                if(IsHeld){
                    if(Pickup.IsHeld){
                        Vector3 HandPos;
                        Quaternion HandRot;
                        if(RightHand){
                            HandPos = LocalPlayer.GetBonePosition(HumanBodyBones.RightHand);
                            HandRot = LocalPlayer.GetBoneRotation(HumanBodyBones.RightHand);
                        }else{
                            HandPos = LocalPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                            HandRot = LocalPlayer.GetBoneRotation(HumanBodyBones.LeftHand);
                        }

                        RelativePosBuff = Quaternion.Inverse(HandRot) * (TransformCache.position - HandPos);
                        RelativeRotBuff = Quaternion.Inverse(HandRot) * TransformCache.rotation;
                        if( (   ( Vector3.Distance(RelativePosBuff,RelativePos) >= MaxDistanceErr) 
                                || (Quaternion.Angle(RelativeRotBuff,RelativeRot) >= MaxRotationErr)) 
                            && (SinceLastRequest >= SyncInterval))
                        {
                            SinceLastRequest = 0.0f;
                            RelativePos = RelativePosBuff;
                            RelativeRot = RelativeRotBuff;
                            RequestSerialization();
                        }

                    }else{
                        OnDrop_Dlayed();
                    }
                }
            }else{ //not isowner(this gameobject)
                if(IsHeld){
                    ThisCol.enabled = false;

                    Vector3 HandPos;
                    Quaternion HandRot;

                    if(RightHand){
                        HandPos = Networking.GetOwner(this.gameObject).GetBonePosition(HumanBodyBones.RightHand);
                        HandRot = Networking.GetOwner(this.gameObject).GetBoneRotation(HumanBodyBones.RightHand);
                    }else{
                        HandPos = Networking.GetOwner(this.gameObject).GetBonePosition(HumanBodyBones.LeftHand);
                        HandRot = Networking.GetOwner(this.gameObject).GetBoneRotation(HumanBodyBones.LeftHand);
                    }
                    TransformCache.SetPositionAndRotation(HandPos + (HandRot*RelativePos) , HandRot * RelativeRot);
                    Tmr = 0.0f;
                }else {
                    Tmr += Time.deltaTime;
                    if(Tmr >= 0.2f){
                        Tmr = 0.0f;
                        ThisCol.enabled = true;
                        if (this.transform.position !=Pos)
                        {
                            Debug.Log("chocolate piece:位置がずれているので同期します");
                            TransformCache.SetLocalPositionAndRotation(Pos, Rot);
                            Piece.ResetChild();
                            Piece.Detach();
                            Piece.Attach();
                        }
                    }
                }
            }
        }
    }

    private ChocolatePiece Piece
    {
        get
        {
            if(piece==null)piece=GetComponent<ChocolatePiece>();
            return piece;
        }
    }
    private ChocolatePiece piece;

}
 