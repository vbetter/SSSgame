using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace CreativeSpore.SmartColliders
{

    [CustomEditor(typeof(SmartRectCollider2D))]
    [CanEditMultipleObjects]
    public class SmartRectCollider2DEditor : Editor
    {
        class Styles
        {
            public static Styles Instance { get { return s_instance != null ? s_instance : s_instance = new Styles(); } }
            static Styles s_instance;

            public GUIStyle BoldFoldout = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0f, 0f, .4f, 1f) },
                onNormal = { textColor = new Color(0f, 0f, .4f, 1f) }
            };
        }

        private static bool s_editCollider = false;
        private static bool s_showSkinConfig = false;
        private static bool s_showInnerSkinConfig = false;
        private static bool s_showOneWayCollision = false;
        private static bool s_showMovingPlatformCollision = false;
        private static bool s_showPixelSnap = false;

        public override void OnInspectorGUI()
        {
            // NOTE: use PropertyField to be sure the change is managed by Undo

            serializedObject.Update();
            SmartRectCollider2D smartRect = (SmartRectCollider2D)target;

            s_editCollider = EditorUtils.DoToggleButton("Edit Collider", s_editCollider, EditorGUIUtility.IconContent("EditCollider"));
            if (s_editCollider)
            {
                EditorGUILayout.HelpBox("Hold Shift to drag the skin and body handles", MessageType.Info);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableCollision3D"), new GUIContent("Enable Collision 3D", "Enable the collisions with 3D colliders."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableCollision2D"), new GUIContent("Enable Collision 2D", "Enable the collisions with 2D colliders."));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_center"), new GUIContent("Center", "The local offset of the geometry collider."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_size"), new GUIContent("Size", "The size of the collider."));
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("R", GUILayout.Width(25)))
            {
                Undo.RecordObject(target, "Reset Collider");
                smartRect.Reset();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_updateMode"));

            s_showPixelSnap = EditorGUILayout.Foldout(s_showPixelSnap, "Pixel Snap Configuration", Styles.Instance.BoldFoldout);
            if(s_showPixelSnap)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pixelSnapEnabled"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_pixelToUnits"));
            }

            s_showSkinConfig = EditorGUILayout.Foldout(s_showSkinConfig, "Skin Configuration", Styles.Instance.BoldFoldout);
            if (s_showSkinConfig)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hSkinSubdivisions"), new GUIContent("Horizontal Subdivisions", "The horizontal sides subdivision. Higher values give more bottom and top collisions checking precision."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_vSkinSubdivisions"), new GUIContent("Vertical Subdivisions", "The vertical sides subdivision. Higher values give more right and left collisions checking precision."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinTopWidth"), new GUIContent("Top Skin Width", "The width between the top body side and the skin."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinBottomWidth"), new GUIContent("Bottom Skin Width", "The width between the bottom body side and the skin."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinLeftWidth"), new GUIContent("Left Skin Width", "The width between the left body side and the skin."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinRightWidth"), new GUIContent("Right Skin Width", "The width between the right body side and the skin."));
            }

            s_showInnerSkinConfig = EditorGUILayout.Foldout(s_showInnerSkinConfig, "Inner Skin Configuration", Styles.Instance.BoldFoldout);
            if (s_showInnerSkinConfig)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinTopOff01"), new GUIContent("Top Inner Skin", "With a value of 1, the skin raycast will start from inside the body. Use this for situations were the smart colliders is inside another collider, to push it back."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinBottomOff01"), new GUIContent("Bottom Inner Skin", "With a value of 1, the skin raycast will start from inside the body. Use this for situations were the smart colliders is inside another collider, to push it back."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinLeftOff01"), new GUIContent("Left Inner Skin", "With a value of 1, the skin raycast will start from inside the body. Use this for situations were the smart colliders is inside another collider, to push it back."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_skinRightOff01"), new GUIContent("Right Inner Skin", "With a value of 1, the skin raycast will start from inside the body. Use this for situations were the smart colliders is inside another collider, to push it back."));
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LayerCollision"), new GUIContent("Layer Collision", "The layers this collider collides with."));
                if (GUILayout.Button("R", GUILayout.Width(25)))
                {
                    Undo.RecordObject(target, "Reset Layer Collisions");
                    smartRect.ResetLayerCollisions();
                    EditorUtility.SetDirty(target);
                }                
            }
            EditorGUILayout.EndHorizontal();


            s_showOneWayCollision = EditorGUILayout.Foldout(s_showOneWayCollision, "One Way Collisions", Styles.Instance.BoldFoldout);
            if (s_showOneWayCollision)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OneWayCollisionUp"), new GUIContent("Up", "The layers this collider collides with when moving up."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OneWayCollisionDown"), new GUIContent("Down", "The layers this collider collides with when moving down."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OneWayCollisionRight"), new GUIContent("Right", "The layers this collider collides with when moving right."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OneWayCollisionLeft"), new GUIContent("Left", "The layers this collider collides with when moving left."));
            }

            s_showMovingPlatformCollision = EditorGUILayout.Foldout(s_showMovingPlatformCollision, "Moving Platform Collisions", Styles.Instance.BoldFoldout);
            if (s_showMovingPlatformCollision)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MovingPlatformCollisionUp"), new GUIContent("Up", "The layers this collider is attached when there is a top collision and will be moved if collided collider is moving."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MovingPlatformCollisionDown"), new GUIContent("Down", "The layers this collider is attached when there is a bottom collision and will be moved if collided collider is moving."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MovingPlatformCollisionRight"), new GUIContent("Right", "The layers this collider is attached when there is a right collision and will be moved if collided collider is moving."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MovingPlatformCollisionLeft"), new GUIContent("Left", "The layers this collider is attached when there is a left collision and will be moved if collided collider is moving."));
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                smartRect.UpdateCheckPoints();
                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
            }
        }

        public void OnSceneGUI()
        {
            SmartRectCollider2D smartRect = (SmartRectCollider2D)target;

            if(!s_editCollider)
            {
                return;
            }

            // Fix handles displacement while in play by the late physics update
            Vector3 savedPosition = smartRect.transform.position;
            if (Application.isPlaying)
            {
                smartRect.transform.position = smartRect.LastSolvedPosition;
            }

            Rect rBody = new Rect(smartRect.Size.x / 2, smartRect.Size.y / 2, smartRect.Size.x, smartRect.Size.y);
            rBody.center = smartRect.Center;
            HandlesEx.DrawRectWithOutline(smartRect.transform, rBody, new Color(0, 0, 0, 0), Color.cyan);

            // Draw Skin
            Rect rSkin = rBody;
            rSkin.position -= new Vector2(smartRect.SkinLeftWidth, smartRect.SkinBottomWidth);
            rSkin.width += (smartRect.SkinLeftWidth + smartRect.SkinRightWidth);
            rSkin.height += (smartRect.SkinTopWidth + smartRect.SkinBottomWidth);
            HandlesEx.DrawDottedLine(smartRect.transform, rSkin, 10f);

            if (Event.current.shift)
            {
                // Draw Body Moving Handlers
                _DoBodyFreeMoveHandle(new Vector3(smartRect.Size.x / 2, 0f));
                _DoBodyFreeMoveHandle(new Vector3(-smartRect.Size.x / 2, 0f));
                _DoBodyFreeMoveHandle(new Vector3(0f, smartRect.Size.y / 2, 0f));
                _DoBodyFreeMoveHandle(new Vector3(0f, -smartRect.Size.y / 2, 0f));

                // Draw Skin Moving Handlers
                _DoSkinFreeMoveHandle(new Vector3(smartRect.SkinRightWidth, 0f), new Vector3(smartRect.Size.x / 2, 0f));
                _DoSkinFreeMoveHandle(new Vector3(-smartRect.SkinLeftWidth, 0f), new Vector3(-smartRect.Size.x / 2, 0f));
                _DoSkinFreeMoveHandle(new Vector3(0f, smartRect.SkinTopWidth, 0f), new Vector3(0f, smartRect.Size.y / 2, 0f));
                _DoSkinFreeMoveHandle(new Vector3(0f, -smartRect.SkinBottomWidth, 0f), new Vector3(0f, -smartRect.Size.y / 2, 0f));

            }
            else
            {
                // Draw Center Moving Handler
                EditorGUI.BeginChangeCheck();
                Handles.color = Color.yellow;
                Vector3 vCenter = Handles.FreeMoveHandle(smartRect.transform.TransformPoint(smartRect.Center), Quaternion.identity, 0.05f * HandleUtility.GetHandleSize(smartRect.transform.position), Vector3.zero, EditorCompatibilityUtils.DotCap);
                vCenter = smartRect.transform.InverseTransformPoint(vCenter);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move Center");
                    smartRect.Center = vCenter;
                    EditorUtility.SetDirty(target);
                }

                //Draw Skin Check Points
                float dotSize = 0.08f * Mathf.Min(HandleUtility.GetHandleSize(smartRect.transform.position), 0.1f);
                List<Vector3> checkPossList = new List<Vector3>();
                checkPossList.AddRange(smartRect.TopCheckPoints);
                checkPossList.AddRange(smartRect.BottomCheckPoints);
                checkPossList.AddRange(smartRect.LeftCheckPoints);
                checkPossList.AddRange(smartRect.RightCheckPoints);
                foreach (Vector3 vPos in checkPossList)
                {
                    Rect rDot = new Rect(-dotSize / 2, -dotSize / 2, dotSize, dotSize);
                    rDot.position += (Vector2)vPos;
                    HandlesEx.DrawDotOutline(smartRect.transform, vPos, dotSize, new Color(0, 0, 0, 0), Color.cyan);
                }
            }

            // Restore position
            smartRect.transform.position = savedPosition;

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            Handles.color = Color.white;
        }

        private void _DoBodyFreeMoveHandle(Vector3 vBody)
        {
            SmartRectCollider2D smartRect = (SmartRectCollider2D)target;
            Vector3 vTransform = smartRect.transform.TransformPoint(smartRect.Center + vBody);

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.green;
            //NOTE: vBodyHandler will be the body size change difference
            Vector3 vBodyHandler = Handles.FreeMoveHandle(vTransform, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(smartRect.transform.position), Vector3.zero, EditorCompatibilityUtils.SphereCap) - vTransform;
            vBodyHandler = smartRect.transform.InverseTransformVector(vBodyHandler);

            if (EditorGUI.EndChangeCheck())
            {
                if (vBody.x > 0)
                {
                    Undo.RecordObject(target, "Modified Body Right");
                    smartRect.Size += new Vector2(vBodyHandler.x, 0f);
                    smartRect.Center += new Vector3(vBodyHandler.x / 2, 0f);
                }
                else if (vBody.x < 0)
                {
                    Undo.RecordObject(target, "Modified Body Left");
                    smartRect.Size += new Vector2(-vBodyHandler.x, 0f);
                    smartRect.Center += new Vector3(vBodyHandler.x / 2, 0f);
                }
                else if (vBody.y > 0)
                {
                    Undo.RecordObject(target, "Modified Body Up");
                    smartRect.Size += new Vector2(0f, vBodyHandler.y);
                    smartRect.Center += new Vector3(0f, vBodyHandler.y / 2);
                }
                else if (vBody.y < 0)
                {
                    Undo.RecordObject(target, "Modified Body Down");
                    smartRect.Size += new Vector2(0f, -vBodyHandler.y);
                    smartRect.Center += new Vector3(0f, vBodyHandler.y / 2);
                }
                EditorUtility.SetDirty(target);
            }
        }

        private void _DoSkinFreeMoveHandle(Vector3 vSkin, Vector3 vBody)
        {
            SmartRectCollider2D smartRect = (SmartRectCollider2D)target;
            Vector3 vTransform = smartRect.transform.TransformPoint(smartRect.Center + vBody);

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.grey;
            Vector3 vSkinHandler = Handles.FreeMoveHandle(smartRect.transform.TransformPoint(smartRect.Center + vBody + vSkin), Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(smartRect.transform.position), Vector3.zero, EditorCompatibilityUtils.SphereCap) - vTransform;
            vSkinHandler = smartRect.transform.InverseTransformVector(vSkinHandler);
            if (EditorGUI.EndChangeCheck())
            {
                if (vSkin.x > 0)
                {
                    Undo.RecordObject(target, "Modified SkinRightWidth");
                    smartRect.SkinRightWidth = vSkinHandler.x;
                }
                else if (vSkin.x < 0)
                {
                    Undo.RecordObject(target, "Modified SkinLeftWidth");
                    smartRect.SkinLeftWidth = -vSkinHandler.x;
                }
                else if (vSkin.y > 0)
                {
                    Undo.RecordObject(target, "Modified SkinTopWidth");
                    smartRect.SkinTopWidth = vSkinHandler.y;
                }
                else if (vSkin.y < 0)
                {
                    Undo.RecordObject(target, "Modified SkinBottomWidth");
                    smartRect.SkinBottomWidth = -vSkinHandler.y;
                }
                EditorUtility.SetDirty(target);
            }
        }
    }
}