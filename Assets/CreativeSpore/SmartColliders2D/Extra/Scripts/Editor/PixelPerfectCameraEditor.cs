using UnityEngine;
using System.Collections;
using UnityEditor;

using CreativeSpore.SmartColliders;
using CreativeSpore;

[CustomEditor(typeof(PixelPerfectCameraCtrl))]
public class PixelPerfectCameraEditor : Editor
{
    public void OnSceneGUI()
    {
        PixelPerfectCameraCtrl owner = (PixelPerfectCameraCtrl)target;

        if (owner.KeepInsideBoundingBox)
        {
            HandlesEx.DrawRectWithOutline(owner.BoundingBox, new Color(0, 0, 0, 0), Color.cyan);

            // Draw Center Moving Handler
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.yellow;
            Vector3 vCenter = Handles.FreeMoveHandle(owner.BoundingBox.center, Quaternion.identity, 0.05f * HandleUtility.GetHandleSize(owner.transform.position), Vector3.zero, EditorCompatibilityUtils.DotCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Center");
                owner.BoundingBox.center = vCenter;
                EditorUtility.SetDirty(target);
            }

            // Draw Body Moving Handlers
            _DoBodyFreeMoveHandle(new Vector3(owner.BoundingBox.size.x / 2, 0f));
            _DoBodyFreeMoveHandle(new Vector3(-owner.BoundingBox.size.x / 2, 0f));
            _DoBodyFreeMoveHandle(new Vector3(0f, owner.BoundingBox.size.y / 2, 0f));
            _DoBodyFreeMoveHandle(new Vector3(0f, -owner.BoundingBox.size.y / 2, 0f));

            if (owner.BoundingBox.width < 0f)
            {
                owner.BoundingBox.position = new Vector2(owner.BoundingBox.position.x + owner.BoundingBox.width, owner.BoundingBox.y);
                owner.BoundingBox.width = -owner.BoundingBox.width;
            }

            if (owner.BoundingBox.height < 0f)
            {
                owner.BoundingBox.position = new Vector2(owner.BoundingBox.position.x, owner.BoundingBox.y + owner.BoundingBox.height);
                owner.BoundingBox.height = -owner.BoundingBox.height;
            }
        }
    }

    private void _DoBodyFreeMoveHandle(Vector3 vBody)
    {
        PixelPerfectCameraCtrl owner = (PixelPerfectCameraCtrl)target;
        Vector3 vTransform = (Vector3)owner.BoundingBox.center + vBody;

        EditorGUI.BeginChangeCheck();
        Handles.color = Color.green;
        //NOTE: vBodyHandler will be the body size change difference
        Vector3 vBodyHandler = Handles.FreeMoveHandle(vTransform, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(owner.transform.position), Vector3.zero, EditorCompatibilityUtils.SphereCap) - vTransform;
        vBodyHandler = owner.transform.InverseTransformVector(vBodyHandler);

        if (EditorGUI.EndChangeCheck())
        {
            if (vBody.x > 0)
            {
                Undo.RecordObject(target, "Modified Body Right");
                owner.BoundingBox.size += new Vector2(vBodyHandler.x / 2, 0f);
            }
            else if (vBody.x < 0)
            {
                Undo.RecordObject(target, "Modified Body Left");
                owner.BoundingBox.size += new Vector2(-vBodyHandler.x / 2, 0f);
                owner.BoundingBox.center += new Vector2(vBodyHandler.x / 2, 0f);
            }
            else if (vBody.y > 0)
            {
                Undo.RecordObject(target, "Modified Body Up");
                owner.BoundingBox.size += new Vector2(0f, vBodyHandler.y / 2);
            }
            else if (vBody.y < 0)
            {
                Undo.RecordObject(target, "Modified Body Down");
                owner.BoundingBox.size += new Vector2(0f, -vBodyHandler.y / 2);
                owner.BoundingBox.center += new Vector2(0f, vBodyHandler.y / 2);
            }
            EditorUtility.SetDirty(target);
        }
    }
}
