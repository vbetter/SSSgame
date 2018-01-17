using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class RedMushroomBehaviour : MonoBehaviour 
{
    public bool WasEaten = false;

    public Vector3 TargetScale = new Vector3(3f, 3f, 1f);
    public float TransformationTime = 1.5f;
    public float EffectDuration = 5f;
    public float GrowingUpTime = 8f;

    [SerializeField]
    private float m_time = 0f;

	// Update is called once per frame
	void Update () 
    {
	    if( WasEaten )
        {
            // NOTE: transform.localScale.x could change when flipping the object
            Vector3 vOne = new Vector3( Mathf.Sign(transform.localScale.x), Mathf.Sign(transform.localScale.y), Mathf.Sign(transform.localScale.z) );
            Vector3 vTargetScale = new Vector3(Mathf.Sign(transform.localScale.x) * TargetScale.x, Mathf.Sign(transform.localScale.y) * TargetScale.y, Mathf.Sign(transform.localScale.z) * TargetScale.z);
            m_time += Time.deltaTime;            
            if( m_time <= TransformationTime )
            {
                transform.localScale = Vector3.Slerp(vOne, vTargetScale, Mathf.Clamp01(m_time / TransformationTime));
            }
            else if (m_time >= EffectDuration - TransformationTime)
            {
                transform.localScale = Vector3.Slerp(vTargetScale, vOne, Mathf.Clamp01(1 - (EffectDuration - m_time) / TransformationTime));
            }
        }
	}

    void OnDestroy()
    {
        Vector3 vOne = new Vector3(Mathf.Sign(transform.localScale.x), Mathf.Sign(transform.localScale.y), Mathf.Sign(transform.localScale.z));
        transform.localScale = vOne;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        SmartPlatformController playerCtrl = other.gameObject.GetComponent<SmartPlatformController>();
        if (playerCtrl != null)
        {
            RedMushroomBehaviour comp = other.gameObject.AddComponent<RedMushroomBehaviour>();
            comp.WasEaten = true;
            comp.TargetScale = TargetScale;
            comp.TransformationTime = TransformationTime;
            comp.EffectDuration = EffectDuration;
            comp.GrowingUpTime = GrowingUpTime;
            Destroy(comp, EffectDuration);
            //StartCoroutine(_SleepingCR());
            gameObject.SetActive(false);
            InvokeRepeating("WakeUp", GrowingUpTime, 0f);
        }
    }

    void WakeUp()
    {
        gameObject.SetActive(true);
    }
}
