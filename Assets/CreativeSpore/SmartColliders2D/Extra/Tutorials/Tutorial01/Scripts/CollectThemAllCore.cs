using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CollectThemAllCore : MonoBehaviour 
{
    public Text CollectibleCounterText;
    private List<CollectibleBehaviour> m_allCollectibles;
    private int m_totalCollectibles;

	void Start () 
    {
        m_allCollectibles = new List<CollectibleBehaviour>( FindObjectsOfType<CollectibleBehaviour>() );
        m_totalCollectibles = m_allCollectibles.Count;
	}
	
	void Update () 
    {
        m_allCollectibles.RemoveAll( x => x == null );
        CollectibleCounterText.text = (m_totalCollectibles - m_allCollectibles.Count) + "/" + m_totalCollectibles;
	}
}
