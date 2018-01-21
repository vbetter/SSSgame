using UnityEngine;
using System.Collections;

namespace CreativeSpore.SmartColliders
{
    [RequireComponent(typeof(PlatformCharacterController))]
    public class PlatformCharacterInput : MonoBehaviour
    {
        public enum eInputMode
        {
            Keyboard,
            Gamepad
        }
        public eInputMode InputMode = eInputMode.Gamepad;      
        
        /// <summary>
        /// If true, the moving speed will be proportional to the axis value
        /// </summary>
        public bool UseAxisAsSpeedFactor = true;
        /// <summary>
        /// Minimum axis value to start moving
        /// </summary>
        public float AxisMovingThreshold = 0.2f;

        private PlatformCharacterController m_platformCtrl;

        const string KeyHorizontal = "Horizontal";
        const string KeyVertical = "Vertical";
        const string KeyFire1 = "Fire1";
        const string KeyFire2 = "Fire2";
        const string KeyFire3 = "Fire3";
        const string KeyJump = "Jump";

        string m_Horizontal = "Horizontal";
        string m_Vertical = "Vertical";
        string m_Fire1 = "Fire1";
        string m_Fire2 = "Fire2";
        string m_Fire3 = "Fire3";
        string m_Jump = "Jump";



        void Start()
        {
            m_platformCtrl = GetComponent<PlatformCharacterController>();

            InputMode = eInputMode.Gamepad;
        }

        public void Init(int p)
        {
            string pStr = string.Format("_p{0}",p);
            m_Horizontal = KeyHorizontal + pStr;
            m_Vertical = KeyVertical + pStr;
            m_Fire1 = KeyFire1 + pStr;
            m_Fire2 = KeyFire2 + pStr;
            m_Fire3 = KeyFire3 + pStr;
            m_Jump = KeyJump + pStr;
        }

        void Update()
        {
            //+++Autodetecting input device. Comment or remove this to manually specify the input management
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                InputMode = eInputMode.Keyboard;
            }
            else if (Input.GetKey(KeyCode.Joystick1Button0))
            {
                InputMode = eInputMode.Gamepad;
            }
            //---
            if (InputMode == eInputMode.Gamepad)
            {
                float fHorAxis = Input.GetAxis(m_Horizontal);  fHorAxis *= Mathf.Abs(fHorAxis);
                float fVerAxis = Input.GetAxis(m_Vertical); fVerAxis *= Mathf.Abs(fVerAxis);
                float fAbsHorAxis = Mathf.Abs(fHorAxis);
                float fAbsVerAxis = Mathf.Abs(fVerAxis);

                if (fAbsHorAxis >= AxisMovingThreshold)
                    m_platformCtrl.HorizontalSpeedScale = UseAxisAsSpeedFactor ? fAbsHorAxis : 1f;
                if (fAbsVerAxis >= AxisMovingThreshold)
                    m_platformCtrl.VerticalSpeedScale = UseAxisAsSpeedFactor ? fAbsVerAxis : 1f;

                m_platformCtrl.SetActionState(eControllerActions.Left, fHorAxis <= -AxisMovingThreshold);
                m_platformCtrl.SetActionState(eControllerActions.Right, fHorAxis >= AxisMovingThreshold); 
                m_platformCtrl.SetActionState(eControllerActions.Down, fVerAxis <= -AxisMovingThreshold);
                m_platformCtrl.SetActionState(eControllerActions.Up, fVerAxis >= AxisMovingThreshold);

                m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, (Input.GetButton(m_Fire1) || Input.GetButton(m_Jump)) && (fVerAxis <= -AxisMovingThreshold));
                m_platformCtrl.SetActionState(eControllerActions.Jump, (Input.GetButton(m_Fire1) || Input.GetButton(m_Jump))/* && !(fVerAxis <= -AxisMovingThreshold)*/); // commented to fix bug when holding jump while climbing a ladder and moving down the player jumps
            }
            else //if( InputMode == eInputMode.Keyboard )
            {
                m_platformCtrl.HorizontalSpeedScale = m_platformCtrl.VerticalSpeedScale = 1f;
                m_platformCtrl.SetActionState(eControllerActions.Left, Input.GetKey(KeyCode.LeftArrow));
                m_platformCtrl.SetActionState(eControllerActions.Right, Input.GetKey(KeyCode.RightArrow));
                m_platformCtrl.SetActionState(eControllerActions.Up, Input.GetKey(KeyCode.UpArrow));
                m_platformCtrl.SetActionState(eControllerActions.Down, Input.GetKey(KeyCode.DownArrow));
                m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, Input.GetButton(m_Jump) && Input.GetKey(KeyCode.DownArrow));
                m_platformCtrl.SetActionState(eControllerActions.Jump, Input.GetButton(m_Jump) && !Input.GetKey(KeyCode.DownArrow));                
            }
        }        
    }
}