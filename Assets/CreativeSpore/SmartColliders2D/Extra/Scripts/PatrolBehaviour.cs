using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using CreativeSpore;
using CreativeSpore.SmartColliders;

public class PatrolBehaviour : MonoBehaviour 
{

    public enum eMode
    {
        None,       
        Loop,
        PingPong
    }

    /// <summary>
    /// List of all path nodes
    /// </summary>
    public List<Vector2> Path = new List<Vector2>();

    /// <summary>
    /// Wrap mode for platform movement
    /// </summary>
    public eMode WrapMode; 

    /// <summary>
    /// Time to way in a node before moving to the next one
    /// </summary>
    public float WaitingTime = 1f;

    /// <summary>
    /// Time to go from a path node to the next one
    /// </summary>
    public float MovingTime = 1f;
    
    /// <summary>
    /// Base position of all path nodes
    /// </summary>
    public Vector3 BasePos;

    private float m_curMovTime;
    private float m_waitingTime;
    private int m_pathIdx = 0;
    private bool m_pingPongForward = true;
    private Vector3 m_prevPos;

    void Reset()
    {
        Path.Add( Vector2.one );
        Path.Add( Vector2.zero );
    }

    void Start()
    {
        BasePos = transform.position;
        m_curMovTime = MovingTime;
        m_pathIdx = Path.Count - 1;
    }

    //NOTE: comment this if you don't want to see the patrol path in the editor
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Path != null && Selection.activeGameObject != gameObject)
        {
            for (int i = 0; i < Path.Count; ++i)
            {
                Vector3 vPathPos = Path[i];
                Vector3 vWorldPathPos = BasePos + vPathPos;
                if (WrapMode == PatrolBehaviour.eMode.Loop || (i != (Path.Count - 1)))
                {
                    Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    Handles.DrawLine(vWorldPathPos, BasePos + (Vector3)Path[(i + 1) % Path.Count]);
                    EditorCompatibilityUtils.SphereCap(0, vWorldPathPos, Quaternion.identity, 0.10f * HandleUtility.GetHandleSize(vPathPos));
                }
            }
        }
    }
#endif

    void Update()
    {
        if( Path != null && Path.Count > 0 )
        {
            if (m_waitingTime > 0f)
            {
                m_waitingTime -= Time.deltaTime;
            }
            else
            {
                m_pathIdx = (Path.Count + m_pathIdx) % Path.Count;
                if (m_curMovTime == 0f)
                {
                    m_prevPos = transform.position;
                }
                m_curMovTime += Time.deltaTime;
                Vector3 vTarget = (Vector3)Path[m_pathIdx] + BasePos;
                vTarget.z = transform.position.z;
                if (m_curMovTime < MovingTime)
                {
                    transform.position = Vector3.Lerp(m_prevPos, vTarget, m_curMovTime / MovingTime);
                }
                else
                {
                    // Position reached
                    transform.position = vTarget;

                    if (WrapMode == eMode.Loop)
                    {
                        m_curMovTime = 0f;
                        m_waitingTime = WaitingTime;
                        --m_pathIdx;
                    }
                    else if (WrapMode == eMode.PingPong)
                    {
                        m_curMovTime = 0f;
                        m_waitingTime = WaitingTime;
                        m_pingPongForward = m_pingPongForward? m_pathIdx != 0 : m_pathIdx == (Path.Count - 1);
                        if (m_pingPongForward)
                        {
                            --m_pathIdx;
                        }
                        else
                        {
                            ++m_pathIdx;
                        }
                    }
                }
            }
        }
    }
}
