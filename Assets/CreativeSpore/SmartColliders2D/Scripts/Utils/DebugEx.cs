using UnityEngine;
using System.Collections;

namespace CreativeSpore.SmartColliders
{
    public static class DebugEx
    {
        /// <summary>
        /// Draw a debug rectangle in the editor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public static void DebugDrawRect(Vector3 pos, Rect rect, Color color)
        {
            rect.position += new Vector2(pos.x, pos.y);
            Debug.DrawLine(new Vector3(rect.x, rect.y, pos.z), new Vector3(rect.x + rect.width, rect.y, pos.z), color);
            Debug.DrawLine(new Vector3(rect.x, rect.y, pos.z), new Vector3(rect.x, rect.y + rect.height, pos.z), color);
            Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y, pos.z), new Vector3(rect.x + rect.width, rect.y + rect.height, pos.z), color);
            Debug.DrawLine(new Vector3(rect.x, rect.y + rect.height, pos.z), new Vector3(rect.x + rect.width, rect.y + rect.height, pos.z), color);
        }

        /// <summary>
        /// Draw a debug rectangle in the editor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DebugDrawRect(Vector3 pos, Rect rect, Color color, float duration)
        {
            rect.position += new Vector2(pos.x, pos.y);
            Debug.DrawLine(new Vector3(rect.x, rect.y, pos.z), new Vector3(rect.x + rect.width, rect.y, pos.z), color, duration);
            Debug.DrawLine(new Vector3(rect.x, rect.y, pos.z), new Vector3(rect.x, rect.y + rect.height, pos.z), color, duration);
            Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y, pos.z), new Vector3(rect.x + rect.width, rect.y + rect.height, pos.z), color, duration);
            Debug.DrawLine(new Vector3(rect.x, rect.y + rect.height, pos.z), new Vector3(rect.x + rect.width, rect.y + rect.height, pos.z), color, duration);
        }

        /// <summary>
        /// Draw a debug dot in the editor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DebugDrawDot(Vector3 pos, float size, Color color)
        {
            DebugDrawRect(pos, new Rect(-size / 2, -size / 2, size, size), color);
        }

        /// <summary>
        /// Draw a debug dot in the editor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DebugDrawDot(Vector3 pos, float size, Color color, float duration)
        {
            DebugDrawRect(pos, new Rect(-size / 2, -size / 2, size, size), color, duration);
        }
    }
}