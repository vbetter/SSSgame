using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class RocketBehaviour : MonoBehaviour 
{

    public float Speed = 1f;
    public bool IsDying = false;
    

    private Rigidbody2D m_rigidBody2D;

    void Start()
    {
        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_rigidBody2D.isKinematic = true;
    }

    void Update()
    {
        if (IsDying)
        {
            transform.position += 1f * -transform.up * Time.deltaTime;
        }
        else
        {
            transform.position += Speed * -transform.right * transform.localScale.x * Time.deltaTime;
        }
    }

    void OnSmartTriggerStay2D(SmartContactPoint smartContactPoint)
    {
        //NOTE: dot product will be 1 if collision in perpendicular and opposite facing direction and 0 if horizontal and < 0 if perpendicular but in the same direction as facing direction
        float dot = Vector3.Dot(transform.up, smartContactPoint.normal);
        GameObject playerCtrl = smartContactPoint.otherCollider.gameObject;
        bool isPlayer = playerCtrl.tag == "Player";
        //Debug.Log("dot: " + dot);
        //Debug.DrawRay(smartContactPoint.point, smartContactPoint.normal, Color.white, 3f);
        if (isPlayer)
        {
            // if dot > 0, the collision is with top side
            if (dot > SmartRectCollider2D.k_OneSideSlopeNormThreshold)
            {
                // Kill the enemy, add player impulse
                smartContactPoint.otherCollider.RigidBody2D.velocity = new Vector2(smartContactPoint.otherCollider.RigidBody2D.velocity.x, 0f);
                smartContactPoint.otherCollider.RigidBody2D.AddForce(5f * smartContactPoint.otherCollider.RigidBody2D.transform.up, ForceMode2D.Impulse);
                PlatformCharacterController platformCtrl = playerCtrl.GetComponent<PlatformCharacterController>();
                if (platformCtrl)
                {
                    platformCtrl.PlatformCharacterPhysics.Velocity = 2 * platformCtrl.JumpingSpeed * smartContactPoint.otherCollider.RigidBody2D.transform.up;
                }
                Kill();
            }
            else
            {
                playerCtrl.SendMessage("Kill", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsDying && !other.isTrigger)
        {
            Kill();
        }
    }

    void Kill()
    {
        IsDying = true;
        Collider2D[] aColliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < aColliders.Length; ++i)
        {
            aColliders[i].enabled = false;
        }
        RotateTowards rotateTowards = GetComponent<RotateTowards>();
        if (rotateTowards) rotateTowards.UnlockDistanceToTarget();
        transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        m_rigidBody2D.velocity = Vector2.zero;
        m_rigidBody2D.AddForce(transform.up, ForceMode2D.Impulse);
        Destroy(gameObject, 1f);
    }
}
