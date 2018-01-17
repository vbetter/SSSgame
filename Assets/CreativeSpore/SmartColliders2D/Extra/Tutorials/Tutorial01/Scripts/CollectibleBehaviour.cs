using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class CollectibleBehaviour : MonoBehaviour 
{

    private bool m_isCollected = false;
    private AudioSource m_audioSource = null;
    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

    void OnSmartTriggerStay2D(SmartContactPoint smartContactPoint)
    {
        if (!m_isCollected && smartContactPoint.otherCollider.CompareTag("Player"))
        {
            m_isCollected = true;
            if (m_audioSource != null)
            {
                m_audioSource.Play();
            }
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Renderer>().enabled = false;
            Destroy(gameObject, m_audioSource.clip.length);
        }
    }
}
