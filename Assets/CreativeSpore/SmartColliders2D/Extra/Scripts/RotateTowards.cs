using UnityEngine;
using System.Collections;

public class RotateTowards : MonoBehaviour 
{

    public Transform Target;
    public float DistToTarget;
    
    private bool m_lockedTargetDistance = false;
    [SerializeField]
    private float m_lockedDistToTarget = 0f;

    public void LockDistanceToTarget(float distance)
    {
        m_lockedDistToTarget = distance;
        m_lockedTargetDistance = true;
    }

    public void LockDistanceToTarget()
    {
        m_lockedTargetDistance = Target != null;
        if(m_lockedTargetDistance)
            m_lockedDistToTarget = Vector2.Distance(transform.position, Target.position);
    }

    public void UnlockDistanceToTarget()
    {
        m_lockedTargetDistance = false;
    }

    void Reset()
    {
        Target = transform.parent;        
    }

	void Update () 
    {
        if (Target != null)
        {
            DistToTarget = Vector2.Distance(Target.position, transform.position);
            Vector2 vDiff = Target.position - transform.position;
            float rot_z = Mathf.Atan2(vDiff.y, vDiff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z + 90); ;
            if(m_lockedTargetDistance && Target != null)
            {
                Vector2 vOffset = transform.position - Target.position;
                vOffset = vOffset.normalized * m_lockedDistToTarget;
                transform.position = new Vector3(Target.position.x + vOffset.x, Target.position.y + vOffset.y, transform.position.z);
            }
        }
	}

    void OnDrawGizmos()
    {
        Update();
    }
}
