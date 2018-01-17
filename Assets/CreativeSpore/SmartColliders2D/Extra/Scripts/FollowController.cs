using UnityEngine;
using System.Collections;
using CreativeSpore.SmartColliders;

public class FollowController : MonoBehaviour 
{
    public enum eUpdateMode
    {
        /// <summary>
        /// Use this if you are updating Target position during Update call
        /// </summary>
        LateUpdate,
        /// <summary>
        /// Use this if you are updating Target position during FixedUpdate call
        /// </summary>
        FixedUpdate
    }

	public Transform Target;
	public float DampTime = 0.15f;
    public bool ApplyTargetRotation = false;
    public float RotationDampTime = 0.25f;
    public eUpdateMode UpdateMode = eUpdateMode.LateUpdate;


	private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (Target)
        {
            transform.position = new Vector3(Target.position.x, Target.position.y, transform.position.z);
        }
    }
	
    void LateUpdate()
    {
        if (UpdateMode == eUpdateMode.LateUpdate)
            UpdatePosition();
    }
    
	void FixedUpdate()
    {
        if (UpdateMode == eUpdateMode.FixedUpdate)
            UpdatePosition();
    }

    //NOTE: this has to be always different to the event where the player position is Updated. So if this is LateUpdate, player position should be always changed in Update
	void UpdatePosition() 
	{
		if (Target)
		{
            Vector3 destination = Target.position; destination.z = transform.position.z;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, DampTime);
            if( ApplyTargetRotation )
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Target.localRotation, RotationDampTime);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, RotationDampTime);
            }
		}		
	}
}
