using UnityEngine;
using System.Collections;
using UnityEditor;
using CreativeSpore;
using CreativeSpore.SmartColliders;

[CustomEditor(typeof(PatrolBehaviour))]
public class PatrolBehaviourEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    bool m_allowNodeCreation = false;
    bool m_moveBackToSavedPos = false;
    Vector3 m_savedPosition;
    public void OnSceneGUI()
    {
        PatrolBehaviour owner = (PatrolBehaviour)target;
        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector2 vWorldMouse = ray.origin;
        if( e.type == EventType.MouseUp )
        {
            m_allowNodeCreation = true;
            if (m_moveBackToSavedPos)
            {
                owner.transform.position = m_savedPosition;
            }
        }
        else if( e.type == EventType.MouseDown )
        {
            m_moveBackToSavedPos = false;
            m_savedPosition = owner.transform.position;
        }

        if( !Application.isPlaying )
        {
            owner.BasePos = owner.transform.position;
        }

        if (owner.Path != null)
        {
            for (int i = 0; i < owner.Path.Count; ++i)
            {                
                Vector3 vPathPos = owner.Path[i];
                Vector3 vDiff = m_moveBackToSavedPos ? owner.BasePos - m_savedPosition : Vector3.zero;
                Vector3 vWorldPathPos = owner.BasePos + vPathPos - vDiff;
                if (owner.WrapMode == PatrolBehaviour.eMode.Loop || (i != (owner.Path.Count - 1)))
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(vWorldPathPos, owner.BasePos + (Vector3)owner.Path[(i + 1) % owner.Path.Count] - vDiff);
                }

                EditorGUI.BeginChangeCheck();
                Handles.color = Color.white;
                if (
                    !Application.isPlaying &&
                    owner.Path.Count > 2 &&
                    e.control &&
                    Vector2.Distance(vWorldPathPos, vWorldMouse) <= 0.15f * HandleUtility.GetHandleSize(vPathPos)
                )
                {
                    Handles.color = Color.red;
                    // remove node only if mouse button is released and node was not changed
                    // to avoid removing the node when the intention was snapping the node position
                    if (e.type == EventType.MouseUp && !m_moveBackToSavedPos) 
                    {
                        owner.Path.RemoveAt(i);
                        --i;
                        continue;
                    }
                }
                vPathPos += Handles.FreeMoveHandle(vWorldPathPos, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(vPathPos), Vector3.zero, EditorCompatibilityUtils.SphereCap) - vWorldPathPos;
                if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {
                    if (e.control)
                    {
                        vPathPos.x = Mathf.Round(vPathPos.x / EditorPrefs.GetFloat("MoveSnapX")) * EditorPrefs.GetFloat("MoveSnapX");
                        vPathPos.y = Mathf.Round(vPathPos.y / EditorPrefs.GetFloat("MoveSnapY")) * EditorPrefs.GetFloat("MoveSnapY");
                    }

                    if (e.shift && m_allowNodeCreation)
                    {
                        owner.Path.Insert(i, vPathPos);
                        ++i;
                    }
                    else
                    {
                        owner.Path[i] = vPathPos;
                    }
                    m_allowNodeCreation = false;
                    m_moveBackToSavedPos = true;
                    owner.transform.position = owner.BasePos + vPathPos - vDiff;
                }
            }
        }

        SceneView.RepaintAll();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
