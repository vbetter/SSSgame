using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class SnailBehaviour : MonoBehaviour 
{
    public float WalkSpeed = 0.1f;
    public bool IsDying = false;
    public bool IsFriendly = false;

    private Rigidbody2D m_rigidBody2D;
    private Animator m_animator;
    private SmartRectCollider2D m_smartRectCollider;

	void Start () 
    {
        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_smartRectCollider = GetComponent<SmartRectCollider2D>();
	}

    private float m_turnTimer = 0f;
	void Update() 
    {
        if (!IsDying)
        {
            if (m_turnTimer > 0f)
            {
                m_turnTimer -= Time.deltaTime;
            }

            if (
                   m_turnTimer <= 0f &&
                   (!m_smartRectCollider.SkinBottomRayContacts[0] || // there is no a collision with floor
                   m_smartRectCollider.SkinLeftRayContacts.Contains(true))  // or collision with front side NOTE: front is left side because Snail sprite is looking left
               )
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                m_turnTimer = 1f;
            }

            if (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 >= 0.6)
            {
                m_rigidBody2D.AddForce(transform.localScale.x > 0 ? -transform.right * WalkSpeed * Time.deltaTime : transform.right * WalkSpeed * Time.deltaTime, ForceMode2D.Impulse);
            }
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
        if (isPlayer && !IsFriendly)
        {
            // if dot > 0, the collision is with top side
            if (dot > SmartRectCollider2D.k_OneSideSlopeNormThreshold)
            {
                // Kill the enemy, add player impulse                
                PlatformCharacterController platformCtrl = playerCtrl.GetComponent<PlatformCharacterController>();
                if (platformCtrl)
                {
                    platformCtrl.PlatformCharacterPhysics.Velocity = 1.5f * platformCtrl.JumpingSpeed * smartContactPoint.otherCollider.transform.up;
                }
                else
                {
                    smartContactPoint.otherCollider.RigidBody2D.velocity = new Vector2(smartContactPoint.otherCollider.RigidBody2D.velocity.x, 0f);
                    smartContactPoint.otherCollider.RigidBody2D.AddForce(5f * smartContactPoint.otherCollider.RigidBody2D.transform.up, ForceMode2D.Impulse);
                }
                Kill();
                m_smartRectCollider.enabled = false;
            }
            else
            {
                playerCtrl.SendMessage("Kill", SendMessageOptions.DontRequireReceiver);
            }
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
        transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        m_rigidBody2D.velocity = Vector2.zero;
        m_rigidBody2D.AddForce( transform.up, ForceMode2D.Impulse);
        Destroy(gameObject, 5f);
    }
}
