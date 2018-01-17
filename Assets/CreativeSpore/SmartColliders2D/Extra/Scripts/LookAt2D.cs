using UnityEngine;
using System.Collections;

public class LookAt2D : MonoBehaviour 
{

    public Transform Target;

	// Update is called once per frame
	void Update () 
    {
        if( Target != null )
        {
            Vector3 target = Target.transform.position;
            var dir = target - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
	} 
}
