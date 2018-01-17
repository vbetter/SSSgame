using UnityEngine;
using System.Collections;

public class RotationBehaviour : MonoBehaviour 
{

    public float AngSpeed = 1f;

	// Update is called once per frame
	void Update () 
    {
        float fAng = transform.eulerAngles.z + AngSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3( transform.eulerAngles.x, transform.eulerAngles.y, fAng );
	}
}
