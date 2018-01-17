using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class GravitySwitcher : MonoBehaviour 
{
    public float GravitySwitchIncrement = 90f;
    public float SwitchTime = 3f;


    private float m_switchValue = 0f;
    private float m_switchProgress = 0f;
    private bool m_swithInProgress = false;

    private Vector3 m_startGravity;
    private Vector3 m_startGravity2D;
    private SmartPlatformController m_player;
    private SpriteRenderer m_spriteRenderer;

    // NOTE: Sometimes Start is no called before OnDestroy. 
    // To be sure original gravity is restored when changing level, keep m_startGravity set inside Awake
    void Awake()
    {
        m_startGravity = Physics.gravity;
        m_startGravity2D = Physics2D.gravity;
        m_spriteRenderer = GetComponent<SpriteRenderer>();
	}

    void OnDestroy()
    {
        Physics.gravity = m_startGravity;
        Physics2D.gravity = m_startGravity2D;
    }
	
	// Update is called once per frame
	void Update () 
    {
	    if( m_swithInProgress )
        {
            m_switchProgress += Time.deltaTime;
            float value = m_switchValue + Mathf.Clamp01(m_switchProgress / SwitchTime) * GravitySwitchIncrement;
            value %= 360;

            Physics.gravity = Quaternion.Euler(0f, 0f, value) * m_startGravity;
            Physics2D.gravity = Quaternion.Euler(0f, 0f, value) * m_startGravity2D;
            m_player.transform.rotation = Quaternion.Euler(0f, 0f, value);


            if( m_switchProgress >= SwitchTime )
            {
                m_switchProgress = 0f;
                m_swithInProgress = false;
                m_switchValue = value;
            }

            m_spriteRenderer.color = new Color(1f, 1f, Mathf.Cos(2 * Mathf.PI * Mathf.Clamp01(m_switchProgress / SwitchTime) * 10));
        }
        else
        {
            m_spriteRenderer.color = Color.white;
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        SmartPlatformController playerCtrl = other.gameObject.GetComponent<SmartPlatformController>();
        if (playerCtrl != null && !m_swithInProgress)
        {
            m_player = playerCtrl;
            m_swithInProgress = true;
            m_spriteRenderer.color = Color.red;
        }
    }
}
