using UnityEngine;
using System.Collections;
namespace CreativeSpore.SmartColliders
{
    /// <summary>
    /// Virtual Pad is used to centralize all input controls in one place.
    /// </summary>
    public class VPad
    {

        public static bool IsActionUse(string fire2 = "Fire2")
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < Input.touches.Length; ++i)
                {
                    Touch touch = Input.touches[i];
                    float fDistPerSec = touch.deltaTime != 0f ? touch.deltaPosition.y / touch.deltaTime : 0f;
                    if ((fDistPerSec > 2000) && touch.position.x >= Screen.width / 2)
                    {
                        return true;
                    }
                }
            }
            return Input.GetButtonDown("Fire2");
        }

        public static bool IsActionDrop(string down= "down", string vertical = "vertical")
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < Input.touches.Length; ++i)
                {
                    Touch touch = Input.touches[i];
                    float fDistPerSec = touch.deltaTime != 0f ? touch.deltaPosition.y / touch.deltaTime : 0f;
                    if ((fDistPerSec < -2000) && touch.position.x >= Screen.width / 2)
                    {
                        return true;
                    }
                }
            }
            return (Input.GetKey(down) || Input.GetAxis(vertical) < -0.5f) && IsActionJumpDown();
        }

        public static bool IsActionAttack(string fire1 = "Fire1")
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < Input.touches.Length; ++i)
                {
                    Touch touch = Input.touches[i];
                    if (touch.phase == TouchPhase.Began && touch.position.x >= Screen.width / 2)
                    {
                        return true;
                    }
                }
            }
            return Input.GetButtonDown(fire1);
        }

        public static bool IsActionJump(string jump ="Jump")
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < Input.touches.Length; ++i)
                {
                    Touch touch = Input.touches[i];
                    if (touch.position.x < Screen.width / 2)
                    {
                        return true;
                    }
                }
            }
            return Input.GetButton(jump);
        }

        public static bool IsActionJumpDown(string jump ="Jump")
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < Input.touches.Length; ++i)
                {
                    Touch touch = Input.touches[i];
                    if (touch.position.x < Screen.width / 2)
                    {
                        return (touch.phase == TouchPhase.Began);
                    }
                }
            }
            return Input.GetButtonDown(jump);
        }

        public static bool IsActionJumpUp(string jump ="Jump")
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                for (int i = 0; i < Input.touches.Length; ++i)
                {
                    Touch touch = Input.touches[i];
                    if (touch.position.x < Screen.width / 2)
                    {
                        return (touch.phase == TouchPhase.Ended);
                    }
                }
            }
            return Input.GetButtonUp(jump) ;
        }

        public static float GetAxis(string axisName)
        {
#if UNITY_ANDROID
        if( axisName == "Horizontal" )
        {
            // Used for control of player on device ( needs a gyroscope )
            if (Application.platform == RuntimePlatform.Android)
            {
                float fDirX = 0f;

                Vector3 vGyro = (Input.gyro.gravity + Input.gyro.userAcceleration);
                float fBaseDist = vGyro.x - s_fGyroBaseOffset;
                if (Mathf.Abs(fBaseDist) > s_fGyroFollowDist)
                {
                    s_fGyroBaseOffset = vGyro.x + (vGyro.x > 0 ? -s_fGyroFollowDist : s_fGyroFollowDist);
                }
                s_fGyroBaseOffset = Mathf.Clamp(s_fGyroBaseOffset, -s_fBaseDistMaxOffset, s_fBaseDistMaxOffset);

                fDirX = (fBaseDist / s_fGyroMovingOff);
                fDirX *= fDirX > 0 ? fDirX : -fDirX;

                return fDirX;
            }
        }
#endif
            return Input.GetAxis(axisName);
        }

        public static bool IsPadRight = false;
        public static bool IsPadLeft = false;
        public static bool IsPadUp = false;
        public static bool IsPadDown = false;

#if UNITY_ANDROID
    // Used for control of player on device ( needs a gyroscope )
    private static float s_fGyroBaseOffset = 0f;
    private static float s_fGyroMovingOff = 0.1f;		// Gyro_x considered full speed
    private static float s_fBaseDistMaxOffset = 0.05f;	// Max distance the vertical base can be moved
    private static float s_fGyroFollowDist = 0.12f;	// Minimum dist for base to follow GyroX
    //private static float s_fSensibilityX	= 0.0225f;
#endif

        public static void Start()
        {
#if UNITY_ANDROID
        Input.multiTouchEnabled = true;
        Input.gyro.enabled = true;
#endif
        }

        static float _fHorXPrev = 0;
        static float _fHorYPrev = 0;
        public static void Update(string Horizontal = "Horizontal", string Vertical = "Vertical")
        {
            float fHorX = Input.GetAxis(Horizontal);
            float fHorY = Input.GetAxis(Vertical);
            float fAxisThreshold = 0.8f;
            IsPadRight = (_fHorXPrev < fAxisThreshold) && fHorX >= fAxisThreshold;
            IsPadLeft = (_fHorXPrev > -fAxisThreshold) && fHorX <= -fAxisThreshold;
            IsPadUp = (_fHorYPrev < fAxisThreshold) && fHorY >= fAxisThreshold;
            IsPadDown = (_fHorYPrev > -fAxisThreshold) && fHorY <= -fAxisThreshold;
            _fHorXPrev = fHorX;
            _fHorYPrev = fHorY;
        }
    }
}