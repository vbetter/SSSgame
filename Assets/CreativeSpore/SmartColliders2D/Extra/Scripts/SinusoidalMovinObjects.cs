using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SinusoidalMovinObjects : MonoBehaviour 
{

    public List<Transform> ObjectList = new List<Transform>();
    public float Amplitude = 1f;
    public float Frequency = 1f;
    public float Speed = 2f;
    public float Separation = 0.42f;
    public float MovingDistance = 1f;

    public float CurrentDist = 0f;
    public float AngOffset = 0f;

    void Start()
    {
        Reset();
    }

    [ContextMenu("Reset ObjectList")]
    void Reset()
    {
        ObjectList.Clear();
        for (int i = 0; i < transform.childCount; ++i)
        {
            ObjectList.Add(transform.GetChild( transform.childCount - i - 1));
        }
        Update();
    }

    void Update()
    {
        if (Frequency >= 0f && Frequency < Vector3.kEpsilon) Frequency = Vector3.kEpsilon;
        else if (Frequency <= 0f && Frequency > -Vector3.kEpsilon) Frequency = -Vector3.kEpsilon;

        bool isMoving = (MovingDistance != 0);

        float objOffset = CurrentDist;
        for (int i = 0; i < ObjectList.Count; ++i, objOffset += Speed >= 0? Separation : -Separation)
        {
            Transform childTransform = ObjectList[i];
            Vector3 childPos = Vector3.zero;
            childPos.x = isMoving ? objOffset % MovingDistance : (objOffset - CurrentDist);
            childPos.y = Amplitude * Mathf.Sin((objOffset + AngOffset) / Frequency);
            childTransform.localPosition = childPos;
        }

        CurrentDist += Speed * Time.deltaTime;
        CurrentDist %= isMoving ? MovingDistance : 2 * Mathf.PI;
    }
}
