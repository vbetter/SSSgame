using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class CheckPoint : MonoBehaviour 
{

    public Sprite Activated;
    public Sprite Deactivated;
    public SpriteRenderer FlagSprRenderer;

    void Start()
    {
        FlagSprRenderer.sprite = Deactivated;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        SmartPlatformController playerCtrl = other.gameObject.GetComponent<SmartPlatformController>();
        if (playerCtrl != null)
        {
            if (playerCtrl.CheckPoint != null)
            {
                CheckPoint checkPoint = playerCtrl.CheckPoint.GetComponent<CheckPoint>();
                if( checkPoint != null )
                {
                    checkPoint.FlagSprRenderer.sprite = Deactivated;
                }
            }
            playerCtrl.CheckPoint = this.transform;
            FlagSprRenderer.sprite = Activated;
        }
    }
}
