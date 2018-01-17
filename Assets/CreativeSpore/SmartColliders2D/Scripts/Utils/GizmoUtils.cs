using UnityEngine;
using System.Collections;

namespace CreativeSpore.SmartColliders
{
    public static class GizmoUtils
    {
        public static float GetGizmoSize(Vector3 position)
        {
            Camera current = Camera.current;
            position = Gizmos.matrix.MultiplyPoint(position);

            if (current)
            {
                Transform transform = current.transform;
                Vector3 position2 = transform.position;
                float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
                Vector3 a = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
                Vector3 b = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
                float magnitude = (a - b).magnitude;
                return 80f / Mathf.Max(magnitude, 0.0001f);
            }

            return 20f;
        }
    }
}