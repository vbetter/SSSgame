using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class KillsOnTouch : MonoBehaviour 
{

    void OnSmartTriggerStay2D(SmartContactPoint smartContactPoint)
    {
        GameObject playerCtrl = smartContactPoint.otherCollider.gameObject;
        bool isPlayer = playerCtrl.tag == "Player";
        if (isPlayer)
        {
            playerCtrl.SendMessage("Kill", SendMessageOptions.DontRequireReceiver);
        }
    }

}
