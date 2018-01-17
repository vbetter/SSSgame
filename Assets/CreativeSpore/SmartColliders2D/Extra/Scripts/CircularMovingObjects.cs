using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircularMovingObjects : MonoBehaviour
{

    public List<Transform> ObjectList = new List<Transform>();
    public float Radious = 1f;
    public float Frequency = 1f;

    private float m_curAng = 0f;

    [ContextMenu("Reset ObjectList")]
    void Reset()
    {
        ObjectList.Clear();
        for (int i = 0; i < transform.childCount; ++i)
        {
            ObjectList.Add(transform.GetChild(i));
        }
        Update();
    }

    void Update()
    {
        float ang = m_curAng;
        float angOff = 2 * Mathf.PI / ObjectList.Count;
        for (int i = 0; i < ObjectList.Count; ++i, ang += angOff)
        {
            Transform childTransform = ObjectList[i];
            childTransform.localPosition = new Vector3(Radious * Mathf.Cos(ang), Radious * Mathf.Sin(ang), 0f);
        }

        if (Frequency >= 0f && Frequency < Vector3.kEpsilon) Frequency = Vector3.kEpsilon;
        else if (Frequency <= 0f && Frequency > -Vector3.kEpsilon) Frequency = -Vector3.kEpsilon;
        m_curAng += 2 * Time.deltaTime * Mathf.PI / Frequency;
        m_curAng %= 2 * Mathf.PI;
    }
}
