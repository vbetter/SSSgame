using UnityEngine;
using System.Collections;

public class CannonBehaviour : MonoBehaviour 
{

    public float TimeBetweenRockets = 2f;
    public GameObject RocketPrefab;
    public Vector2 RocketOffset;
    public float RocketSpeed = 0.5f;
    
    private float m_timer;

    void Start()
    {
        m_timer = TimeBetweenRockets;
    }
	
	void Update () 
    {
        m_timer -= Time.deltaTime;
	    if( m_timer <= 0 )
        {
            CreateRocket();
            m_timer = TimeBetweenRockets;
        }
	}

    void CreateRocket()
    { 
        GameObject rocket = Instantiate(RocketPrefab);
        Destroy(rocket, 20f); // be sure it's destroyed after a while
        rocket.transform.parent = transform.parent;
        rocket.transform.position = transform.position + transform.TransformVector(RocketOffset);
        rocket.transform.rotation = transform.rotation;
        RotateTowards parentRotateTowards = GetComponent<RotateTowards>();
        if( parentRotateTowards != null )
        {
            RotateTowards rotateTowardsComp = rocket.AddComponent<RotateTowards>();
            rotateTowardsComp.Target = parentRotateTowards.Target;
            rotateTowardsComp.LockDistanceToTarget();
            rocket.transform.localScale = transform.localScale;
        }
    }
}
