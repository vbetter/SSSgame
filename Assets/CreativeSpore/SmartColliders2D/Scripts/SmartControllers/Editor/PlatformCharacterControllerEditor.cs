using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CreativeSpore.SmartColliders
{
    [CustomEditor(typeof(PlatformCharacterController))]
    public class PlatformCharacterControllerEditor : Editor
    {
        class Styles
        {
            public static Styles Instance { get { return s_instance != null ? s_instance : s_instance = new Styles(); } }
            static Styles s_instance;

            public GUIStyle BoldFoldout = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0f, .4f, 0f, 1f)},
                onNormal = { textColor = new Color(0f, .4f, 0f, 1f)},
                onFocused = { textColor = new Color(0f, .6f, 0f, 1f) },
            };
        }

        [SerializeField]
        private static bool s_showPhysicParams = true;
        private static bool s_showMovingParams = true;
        private static bool s_showJumpingParams = true;
        private static bool s_showClimbingParams = true;

        public override void OnInspectorGUI()
        {
            PlatformCharacterController targetObj = target as PlatformCharacterController;
            serializedObject.Update();

            if(Application.isPlaying)
            {
                string sInGameInfo =
                    string.Format("HSpeed: {0:0.####}\t\t InstantHSpeed: {1:0.####}", targetObj.PlatformCharacterPhysics.HSpeed, targetObj.InstantVelocity.x) + "\n" +
                    string.Format("VSpeed: {0:0.####}\t\t InstantVSpeed: {1:0.####}", targetObj.PlatformCharacterPhysics.VSpeed, targetObj.InstantVelocity.y) + "\n" +
                    string.Format("IsGrounded: {0}", targetObj.IsGrounded) + "\n" +
                    string.Format("IsClimbing: {0}", targetObj.IsClimbing) + "\n" +
                    string.Format("Slope Angle: {0}", targetObj.SlopeAngle) + "\n" +
                    string.Format("Ground Dist: {0}", targetObj.GroundDist) + "\n" +
                    "";
                EditorGUILayout.HelpBox(sInGameInfo, MessageType.None);
            }

            serializedObject.FindProperty("m_showGuides").boolValue = EditorUtils.DoToggleButton("Show Guides", serializedObject.FindProperty("m_showGuides").boolValue, EditorGUIUtility.IconContent("EditCollider"));

            s_showPhysicParams = EditorGUILayout.Foldout(s_showPhysicParams, "Physic Parameters", Styles.Instance.BoldFoldout);
            if (s_showPhysicParams)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpingGuideMode"), new GUIContent("Jumping Guide Mode", "The different modes of showing the jumping guides."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpingGuideOffset"), new GUIContent("Jumping Guide Offset", "The offset position to show the jumping guides."));
                SerializedProperty physicsProperty = serializedObject.FindProperty("m_platformPhysics");
                Vector3 vGravity = physicsProperty.FindPropertyRelative("m_gravity").vector3Value;
                vGravity.y = -EditorGUILayout.FloatField("Gravity", -physicsProperty.FindPropertyRelative("m_gravity").vector3Value.y);
                vGravity.y = Mathf.Min( vGravity.y, 0f);
                physicsProperty.FindPropertyRelative("m_gravity").vector3Value = vGravity;
                EditorGUILayout.PropertyField(physicsProperty.FindPropertyRelative("m_gravityScale"));
                EditorGUILayout.PropertyField(physicsProperty.FindPropertyRelative("m_terminalVel"), new GUIContent("Terminal Velocity", "The maximum falling velocity. Set to 0 for no maximum velocity."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_platformDropTime"), new GUIContent("Platform Drop Time", "When dropping down from a platform, this is the time, the bottom colliders are disabled to allow the player going down."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maxSlope"), new GUIContent("Max Slope", "The maximum angle in degrees of the slope to help the player climbing up and down."));

                SerializedObject smartColliderObj = new SerializedObject(targetObj.GetComponent<SmartPlatformCollider>());
                EditorGUILayout.PropertyField(smartColliderObj.FindProperty("OneWayCollisionDown"), new GUIContent("Pass Through Layers", "The layers of colliders that will allow the player to pass through and only will block the down movement."));
                EditorGUILayout.PropertyField(smartColliderObj.FindProperty("MovingPlatformCollisionDown"), new GUIContent("Moving Platform Layers", "The layers of colliders that will move the player when it is over them."));
                smartColliderObj.ApplyModifiedProperties();

                // Keep safe values
                serializedObject.FindProperty("m_platformDropTime").floatValue = Mathf.Max(serializedObject.FindProperty("m_platformDropTime").floatValue, 0f);
                physicsProperty.FindPropertyRelative("m_terminalVel").floatValue = Mathf.Max(physicsProperty.FindPropertyRelative("m_terminalVel").floatValue, 0f);
                physicsProperty.FindPropertyRelative("m_gravityScale").floatValue = Mathf.Max(physicsProperty.FindPropertyRelative("m_gravityScale").floatValue, 0f);
            }
            s_showMovingParams = EditorGUILayout.Foldout(s_showMovingParams, "Moving Parameters", Styles.Instance.BoldFoldout);
            if(s_showMovingParams)
            {
                float maxWalkSpeed = targetObj.PlatformCharacterPhysics.SolveMaxSpeedWithAccAndDrag(targetObj.WalkingAcc, targetObj.WalkingDrag);
                float timeToMaxWalkSpeed = targetObj.PlatformCharacterPhysics.SolveTimeToReachSpeed( Mathf.Min(maxWalkSpeed, targetObj.MaxWalkingSpeed), targetObj.WalkingAcc, targetObj.WalkingDrag);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_walkingAcc"), new GUIContent("Walking Acc.", "The lateral acceleration applied to the player when walking."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_walkingDrag"), new GUIContent("Walking Drag", "The drag applied to the horizontal movement. This value affects to the maximum walking speed and the time the player needs to completely stop or ground slipperiness."));
                EditorGUILayout.Slider(serializedObject.FindProperty("m_maxWalkingSpeed"), 0f, maxWalkSpeed, new GUIContent("Max. Walking Speed", "The maximum speed allowed for the player. It goes from 0 to the maximum possible speed based on walking acceleration and drag."));

                // Keep safe values
                serializedObject.FindProperty("m_walkingAcc").floatValue = Mathf.Max(serializedObject.FindProperty("m_walkingAcc").floatValue, 0f);
                serializedObject.FindProperty("m_walkingDrag").floatValue = Mathf.Clamp(serializedObject.FindProperty("m_walkingDrag").floatValue, 0f, 1f / Time.fixedDeltaTime);
                serializedObject.FindProperty("m_maxWalkingSpeed").floatValue = Mathf.Clamp(serializedObject.FindProperty("m_maxWalkingSpeed").floatValue, 0f, Mathf.Max(maxWalkSpeed, serializedObject.FindProperty("m_maxWalkingSpeed").floatValue));

                // Display Info Box
                string sParamFormat = "{0,-30}";
                string sWalkingInfo =
                    string.Format(sParamFormat + "{1:0.####}", "Max reachable Speed:", maxWalkSpeed) + "\n" +
                    string.Format(sParamFormat + "{1:0.####} seconds", "Time to reach max speed:", timeToMaxWalkSpeed) + //"\n\n" +
                    "";
                EditorGUILayout.HelpBox(sWalkingInfo, MessageType.None);
            }

            s_showJumpingParams = EditorGUILayout.Foldout(s_showJumpingParams, "Jumping Parameters", Styles.Instance.BoldFoldout);
            if (s_showJumpingParams)
            {
                float maxAirborneSpeed = targetObj.PlatformCharacterPhysics.SolveMaxSpeedWithAccAndDrag(targetObj.AirborneAcc, targetObj.WalkingDrag);
                float timeToMaxAirborneSpeed = targetObj.PlatformCharacterPhysics.SolveTimeToReachSpeed(Mathf.Min(maxAirborneSpeed, targetObj.MaxWalkingSpeed), targetObj.AirborneAcc, targetObj.WalkingDrag);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_airborneAcc"), new GUIContent("Airborne Acc.", "The horizontal acceleration applied while in air if there is a lateral moving action."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpingSpeed"), new GUIContent("Jumping Speed", "The initial vertical speed when jumping."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpingAcc"), new GUIContent("Jumping Acc.", "Jumping acceleration applied while jumping is hold and until jumping acc. time is over. The maximum value is clamped by gravity."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpingTime"), new GUIContent("Jumping Acc. Time", "How much time the jumping acceleration is applied while jumping is hold."));

                // Keep safe values
                serializedObject.FindProperty("m_jumpingSpeed").floatValue = Mathf.Max(serializedObject.FindProperty("m_jumpingSpeed").floatValue, 0f);
                serializedObject.FindProperty("m_jumpingTime").floatValue = Mathf.Max(serializedObject.FindProperty("m_jumpingTime").floatValue, 0f);
                serializedObject.FindProperty("m_jumpingAcc").floatValue = Mathf.Clamp(serializedObject.FindProperty("m_jumpingAcc").floatValue, 0f, -targetObj.PlatformCharacterPhysics.Gravity.y - Vector3.kEpsilon);
                serializedObject.FindProperty("m_airborneAcc").floatValue = Mathf.Max(serializedObject.FindProperty("m_airborneAcc").floatValue, 0f);

                // Display Info Box
                string sParamFormat = "{0,-30}";
                string sWalkingInfo =
                    string.Format(sParamFormat + "{1:0.####}", "Max reachable Speed:", maxAirborneSpeed) + "\n" +
                    string.Format(sParamFormat + "{1:0.####} seconds", "Time to reach max speed:", timeToMaxAirborneSpeed) + //"\n\n" +
                    "";
                EditorGUILayout.HelpBox(sWalkingInfo, MessageType.None);
            }

            s_showClimbingParams = EditorGUILayout.Foldout(s_showClimbingParams, "Climbing Parameters", Styles.Instance.BoldFoldout);
            if (s_showClimbingParams)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_climbingSpeed"), new GUIContent("Climbing Speed", "The speed while moving in a climbing area."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ladderWidthFactor"), new GUIContent("Ladder Width Factor", "To consider a climbing collider as a ladder and snap the player to the center, the width of the collider needs to be less than player collider width multiplied by this value."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_climbingLayers"), new GUIContent("Climbing Layers", "The layers considered as climbing areas."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ladderJumpTimeThreshold"), new GUIContent("Jump Ladder Threshold", "Time needed to be able to jump again from the ladders."));

                // Keep safe values
                serializedObject.FindProperty("m_climbingSpeed").floatValue = Mathf.Max(serializedObject.FindProperty("m_climbingSpeed").floatValue, 0f);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_jumpingAdditionalParameters"), true);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        public void OnSceneGUI()
        {
            if (!serializedObject.FindProperty("m_showGuides").boolValue)
            {
                return;
            }

            PlatformCharacterController targetObj = target as PlatformCharacterController;
            serializedObject.Update();

            Vector3 simulationBaseWorldPos = serializedObject.FindProperty("m_jumpingGuideOffset").vector3Value + targetObj.transform.position;
            // Min Jump Height Handle
            float minJumpHeight = targetObj.PlatformCharacterPhysics.SolveMaxJumpHeight(targetObj.JumpingSpeed);
            minJumpHeight = Vector3.Project(Handles.FreeMoveHandle(simulationBaseWorldPos + minJumpHeight * targetObj.transform.up, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(targetObj.transform.position), Vector3.zero, EditorCompatibilityUtils.SphereCap) - simulationBaseWorldPos, targetObj.transform.up).magnitude;
            serializedObject.FindProperty("m_jumpingSpeed").floatValue = targetObj.PlatformCharacterPhysics.SolveJumpSpeedToReachHeight(minJumpHeight);
            // Max Jump Height Handle
            float maxJumpHeight = targetObj.PlatformCharacterPhysics.SolveMaxJumpHeight(targetObj.JumpingSpeed, targetObj.JumpingAcc, targetObj.JumpingAccTime);
            maxJumpHeight = Vector3.Project(Handles.FreeMoveHandle(simulationBaseWorldPos + maxJumpHeight * targetObj.transform.up, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(targetObj.transform.position), Vector3.zero, EditorCompatibilityUtils.SphereCap) - simulationBaseWorldPos, targetObj.transform.up).magnitude;
            serializedObject.FindProperty("m_jumpingAcc").floatValue = targetObj.PlatformCharacterPhysics.SolveJumpAccToReachHeight(maxJumpHeight, targetObj.JumpingSpeed, targetObj.JumpingAccTime);

            // Jumping Guide Offset
            serializedObject.FindProperty("m_jumpingGuideOffset").vector3Value = Handles.FreeMoveHandle(simulationBaseWorldPos, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(targetObj.transform.position), Vector3.zero, EditorCompatibilityUtils.SphereCap) - targetObj.transform.position;

            // Keep safe values
            serializedObject.FindProperty("m_jumpingSpeed").floatValue = Mathf.Max(serializedObject.FindProperty("m_jumpingSpeed").floatValue, 0f);
            serializedObject.FindProperty("m_jumpingAcc").floatValue = Mathf.Clamp(serializedObject.FindProperty("m_jumpingAcc").floatValue, 0f, -targetObj.PlatformCharacterPhysics.Gravity.y - Vector3.kEpsilon);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}