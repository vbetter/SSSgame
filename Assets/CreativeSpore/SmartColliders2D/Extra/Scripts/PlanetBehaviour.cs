using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using CreativeSpore.SmartColliders;
using System.Collections.Generic;
using CreativeSpore;

public class PlanetBehaviour : MonoBehaviour 
{

    public float GravityField = 1f;
    public float Gravity = 1f;

    /// <summary>
    /// A list with all colliders found inside the GravityField in previous FixedUpdate call
    /// Colliders touching the ground are not included because those colliders are moved by moving platforms feature
    /// This is used to move the colliders not touching the ground but still moved by planet atmosphere
    /// </summary>
    private List<KeyValuePair<Transform, KeyValuePair<Vector3, Vector3>>> m_attractedCollidersNotGrounded = new List<KeyValuePair<Transform, KeyValuePair<Vector3, Vector3>>>(50);

    private static int s_fixedFrameCnt;
    private static int s_frameCnt;
    private static SmartRectCollider2D[] s_aSmartRectColliders;

    private PixelPerfectCameraCtrl m_camera2D;
    private SmartPlatformController m_playerCtrl;
    void Start()
    {
        m_camera2D = FindObjectOfType<PixelPerfectCameraCtrl>();
        m_playerCtrl = FindObjectOfType<SmartPlatformController>();
    }

    void Update()
    {

        // FindObjectsOfType<SmartRectCollider2D>() will be called once per frame, instead make every PlanetBehaviour call it
        if (Time.frameCount != s_frameCnt)
        {
            ++s_fixedFrameCnt;
            s_frameCnt = Time.frameCount;
            s_aSmartRectColliders = FindObjectsOfType<SmartRectCollider2D>();
        }


        // Apply planet rotation, translation and scaling, only if not grounded
        // Because for them grounded, SmartRectCollider2D is already doing that using the moving platform feature
        for (int i = 0; i < m_attractedCollidersNotGrounded.Count; ++i)
        {
		    KeyValuePair<Transform, KeyValuePair<Vector3, Vector3>> kvp = m_attractedCollidersNotGrounded[i];
		    // if transform was destroyed, continue
            if (kvp.Key != null)
            {
                Transform smartColliderTrans = kvp.Key;
                KeyValuePair<Vector3, Vector3> kvpInvPrev = kvp.Value;
                smartColliderTrans.position += transform.TransformPoint(kvpInvPrev.Key) - kvpInvPrev.Value;
            }            
        }
        m_attractedCollidersNotGrounded.Clear();

        for (int i = 0; i < s_aSmartRectColliders.Length; ++i)
        {
            SmartRectCollider2D smartRectCollider = s_aSmartRectColliders[i];
            Vector2 vDiff = transform.position - smartRectCollider.transform.position;
            if( vDiff.magnitude <= GravityField )
            {
                if( smartRectCollider.gameObject == m_playerCtrl.gameObject )
                {
                    m_camera2D.Zoom = Mathf.SmoothStep(m_camera2D.Zoom, 6f / GravityField, 0.1f); ;
                }

                // destroy objects when they are close to the center
                if (vDiff.magnitude <= 0.5)
                {
                    SmartPlatformController playerCtrl = smartRectCollider.gameObject.GetComponent<SmartPlatformController>();
                    if (playerCtrl != null)
                    {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
                        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); 
#else
                        Application.LoadLevel(Application.loadedLevel);
#endif
                    }
                    Destroy(smartRectCollider.gameObject);
                }
                else
                {                    
                    // register the smart collider is it's grounded
                    if (!smartRectCollider.SkinBottomRayContacts.Contains(true))
                    {
                        KeyValuePair<Vector3, Vector3> kvp = new KeyValuePair<Vector3, Vector3>(transform.InverseTransformPoint(smartRectCollider.transform.position), smartRectCollider.transform.position);
                        m_attractedCollidersNotGrounded.Add(new KeyValuePair<Transform, KeyValuePair<Vector3, Vector3>>(smartRectCollider.transform, kvp));                        
                    }                    

                    if (Gravity > 0f)
                    {
                        if (smartRectCollider.RigidBody2D != null)
                        {
                            smartRectCollider.RigidBody2D.velocity += vDiff.normalized * Gravity * Time.deltaTime / vDiff.sqrMagnitude;
                        }
                        else if (smartRectCollider.RigidBody != null)
                        {
                            smartRectCollider.RigidBody.velocity += (Vector3)vDiff.normalized * Gravity * Time.deltaTime / vDiff.sqrMagnitude;
                        }

                        if (smartRectCollider.SkinBottomRayContacts.Contains(true))
                        {
                            // avoid player ground flickering
                            smartRectCollider.transform.position += (Vector3)vDiff.normalized * 100 * SmartRectCollider2D.k_SkinMinWidth;
                        }
                    }
                    float rot_z = Mathf.Atan2(vDiff.y, vDiff.x) * Mathf.Rad2Deg;
                    Quaternion dstRot = Quaternion.Euler(0f, 0f, rot_z + 90);
                    float angDiff = Quaternion.Angle(smartRectCollider.transform.rotation, dstRot);
                    // NOTE: (n / angDiff), n will be the angle degrees per second to be rotated. Small values make the collider swipe over moving circular platforms.
                    // and high values make it move to fast when two planets are trying to rotate the same collider
                    smartRectCollider.transform.rotation = Quaternion.Lerp(smartRectCollider.transform.rotation, dstRot, (360f / angDiff) * Time.deltaTime);                    
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        EditorCompatibilityUtils.CircleCap(0, transform.position, transform.rotation, GravityField);
    }
#endif
}
