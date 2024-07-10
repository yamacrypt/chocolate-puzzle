using UnityEngine;

    public class AlignLocalPosition : MonoBehaviour
    {
        [SerializeField]GameObject[] objs;
        [SerializeField] private float yPos;
        public void AlignChildY(){
            foreach (var obj in objs)
            {
                var child = obj.transform.GetChild(0);
                child.transform.localPosition=new Vector3(child.transform.localPosition.x,yPos,child.transform.localPosition.z);
            }
        }
    }
