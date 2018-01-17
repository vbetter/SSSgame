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
        void Start()
        {
            m_platformCtrl = GetComponent<PlatformCharacterController>();
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
                float fHorAxis = Input.GetAxis("Horizontal");  fHorAxis *= Mathf.Abs(fHorAxis);
                float fVerAxis = Input.GetAxis("Vertical"); fVerAxis *= Mathf.Abs(fVerAxis);
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

                m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, (Input.GetButton("Fire1") || Input.GetButton("Jump")) && (fVerAxis <= -AxisMovingThreshold));
                m_platformCtrl.SetActionState(eControllerActions.Jump, (Input.GetButton("Fire1") || Input.GetButton("Jump"))/* && !(fVerAxis <= -AxisMovingThreshold)*/); // commented to fix bug when holding jump while climbing a ladder and moving down the player jumps
            }
            else //if( InputMode == eInputMode.Keyboard )
            {
                m_platformCtrl.HorizontalSpeedScale = m_platformCtrl.VerticalSpeedScale = 1f;
                m_platformCtrl.SetActionState(eControllerActions.Left, Input.GetKey(KeyCode.LeftArrow));
                m_platformCtrl.SetActionState(eControllerActions.Right, Input.GetKey(KeyCode.RightArrow));
                m_platformCtrl.SetActionState(eControllerActions.Up, Input.GetKey(KeyCode.UpArrow));
                m_platformCtrl.SetActionState(eControllerActions.Down, Input.GetKey(KeyCode.DownArrow));
                m_platformCtrl.SetActionState(eControllerActions.PlatformDropDown, Input.GetButton("Jump") && Input.GetKey(KeyCode.DownArrow));
                m_platformCtrl.SetActionState(eControllerActions.Jump, Input.GetButton("Jump") && !Input.GetKey(KeyCode.DownArrow));                
            }
        }        
    }
}