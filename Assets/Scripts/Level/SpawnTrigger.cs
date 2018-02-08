using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour {
    [SerializeField]
    GameObject m_Clone;
	// Use this for initialization
	void Start () {
		
	}
	
	void OnSpawnTriggerEvent()
    {
        GameObject go= Instantiate(m_Clone);

    }
}
