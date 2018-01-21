using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CreativeSpore.SmartColliders
{
    [RequireComponent(typeof(SmartPlatformCollider))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SmartPlatformController : MonoBehaviour
    {
        public enum eState
        {
            Idle,
            Walking,
            Jumping,
            Falling,
            Climbing,
            ClimbingIdle,
            Dying,
        }

        public delegate void OnStateChangedDelegate(SmartPlatformController source, eState prevState, eState newState);

        /// <summary>
        /// Called when State changed to a different state
        /// </summary>
        public OnStateChangedDelegate OnStateChanged;
        
        #region Public Properties

        /// <summary>
        /// Current state of the Smart Platform Controller. OnStateChanged event will be triggered when state changed to a different state.
        /// </summary>
        public eState State
        {
            get { return m_state; }            
        }

        /// <summary>
        /// This is used to flip the sprite properly when facing each direction.
        /// If true, it means, the sprite will be flipped when facing left
        /// </summary>
        [Tooltip("This is used to flip the sprite properly when facing each direction. If true, it means, the sprite will be flipped when facing left")]
        public bool IsSpriteFacingRight = true;

        /// <summary>
        /// Speed applied when jumping
        /// </summary>
        [Tooltip("Speed applied when jumping")]
        public float JumpSpeed = 5f;
        /// <summary>
        /// Speed applied when jumping while swimming
        /// </summary>
        [Tooltip("Speed applied when jumping while swimming")]
        public float SwimmingJumpSpeed = 4f;
        /// <summary>
        /// If jump button is released while jumping, jump speed will be clamped between it's current value and CutJumpSpeedLimit
        /// </summary>
        [Tooltip("If jump button is released while jumping, jump speed will be clamped between it's current value and CutJumpSpeedLimit")]
        public float CutJumpSpeedLimit = 2f;
        /// <summary>
        /// This factor will be multiplied to m_rigidBody2D.drag to calculate the jump drag
        /// </summary>
        [Tooltip("This factor will be multiplied to m_rigidBody2D.drag to calculate the jump drag")]
        public float JumpDragFactor = 0.2f;
        /// <summary>
        /// Maximum walking speed
        /// </summary>
        [Tooltip("Maximum walking speed")]
        public float WalkSpeed = 2f;
        /// <summary>
        /// Walking acceleration
        /// </summary>
        [Tooltip("Walking acceleration")]
        public float WalkAcc = .2f;
        /// <summary>
        /// Walking reactivity to joystick horizontal value
        /// </summary>
        [Tooltip("Walking reactivity to joystick horizontal value")]
        public float WalkReactivity = .1f;
        /// <summary>
        /// Maximum moving speed while climbing
        /// </summary>
        [Tooltip("Maximum moving speed while climbing")] 
        public float ClimbingSpeed = 1f;
        /// <summary>
        /// Time while collisions with pass trough platforms will be disabled when dropping down from platforms
        /// </summary>
        [Tooltip("Time while collisions with pass trough platforms will be disabled when dropping down from platforms")]
        public float PlatformDropTime = 0.15f;
        /// <summary>
        /// Drag applied to rigid body when player is swimming
        /// </summary>
        [Tooltip("Drag applied to rigid body when player is swimming")]
        public float SwimmingDrag = 25f;
        /// <summary>
        /// This allow jumping during a while after falling down
        /// </summary>
        [Tooltip("This allow jumping during a while after falling down")]
        public float FallingJumpTolerance = 0.1f;
        /// <summary>
        /// If player is killed, it will be moved to this check point instead loading the level again
        /// </summary>
        [Tooltip("If player is killed, it will be moved to this check point instead loading the level again")]
        public Transform CheckPoint = null;
        /// <summary>
        /// Returns true if any of the skin bottom rays find a collision
        /// </summary>
        public bool IsGrounded { get { return m_isGrounded; } }
        /// <summary>
        /// Returns true if climbing
        /// </summary>
        public bool IsClimbing { get { return m_isClimbing; } }
        /// <summary>
        /// Returns true if swimming
        /// </summary>
        public bool IsSwimming { get { return m_isSwimming; } }
        /// <summary>
        /// Tell if being stuck between two colliders kills
        /// </summary>
        [Tooltip("Tell if being stuck between two colliders kills")]
        public bool IsStuckKills = true;
        /// <summary>
        /// Layers cosidered water
        /// </summary>
        [Tooltip("Layers cosidered water")]
        public LayerMask WaterLayers;
        /// <summary>
        /// Layers that allow climbing
        /// </summary>
        [Tooltip("Layers that allow climbing")]
        public LayerMask ClimbingLayers;

        #endregion

        #region Private Attributes 
        private Rigidbody2D m_rigidBody2D;
        private SmartPlatformCollider m_smartRectCollider;
        
        [SerializeField]
        private SpriteRenderer m_spriteRenderer;

        private Animator m_animator;
        private Vector3 m_walkVeloc;
        private float m_jumpSpeed;
        private float m_walkingDrag;
        private float m_fallingJumpToleranceTimer;
        private bool m_jumpReleased = true;
        [SerializeField]
        private eState m_state = eState.Idle;
        private eState m_nextState = eState.Idle;
        private bool m_isGrounded = true;
        private bool m_isSwimming = false;
        private bool m_isClimbing = false;
        private Collider2D m_currentClimbingCollider; 
        private float m_savedGravScale;
        #endregion

        #region MonoBehaviour Methods

        [SerializeField]
        bool m_isController = true;//能够被控制

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

        int m_PlayerNumber = 0;

        public void Init(int p)
        {
            m_PlayerNumber = p;

            string pStr = string.Format("_p{0}", p);
            m_Horizontal = KeyHorizontal + pStr;
            m_Vertical = KeyVertical + pStr;
            m_Fire1 = KeyFire1 + pStr;
            m_Fire2 = KeyFire2 + pStr;
            m_Fire3 = KeyFire3 + pStr;
            m_Jump = KeyJump + pStr;
        }

        // Use this for initialization
        void Start()
        {
            m_rigidBody2D = GetComponent<Rigidbody2D>();

            if(m_spriteRenderer==null) m_spriteRenderer = GetComponent<SpriteRenderer>();

            m_animator = GetComponent<Animator>();
            m_smartRectCollider = GetComponent<SmartPlatformCollider>();
            m_walkingDrag = m_rigidBody2D.drag;

            // Add an offset to horizontal raycasts to avoid missing collisions with lateral moving platforms
            m_smartRectCollider.SkinRightOff01 = 0.1f;
            m_smartRectCollider.SkinLeftOff01 = 0.1f;

            OnStateChanged += _OnStateChanged;
            SetNextState(eState.Idle);
        }

        private void _OnStateChanged(SmartPlatformController source, eState prevState, eState newState)
        {
            if (m_animator != null)
            {
                m_animator.ResetTrigger(prevState.ToString()); //NOTE: be sure the last trigger is the one used in the animator
                m_animator.SetTrigger(newState.ToString());
            }
        }

        private void _OnSideCollision(SmartCollision2D collision, GameObject collidedObject)
        {
            // check for horizontal collision to reset walking velocity
            Vector3 vLocImpulse = transform.rotation != Quaternion.identity ? Quaternion.Inverse(transform.rotation) * collision.impulse : collision.impulse;
            if (vLocImpulse.x <= -Vector3.kEpsilon && m_walkVeloc.x >= Vector3.kEpsilon ||
                vLocImpulse.x >= Vector3.kEpsilon && m_walkVeloc.x <= -Vector3.kEpsilon)
            {
                float dot = Vector3.Dot(collision.contacts[0].normal, transform.up);
                //Debug.Log("Dot: " + dot + " Angle: " + Mathf.Acos(dot) * Mathf.Rad2Deg );
                if (dot <= SmartRectCollider2D.k_OneSideSlopeNormThreshold)
                {
                    m_walkVeloc = Vector3.zero;
                    if (State == eState.Walking) SetNextState(eState.Idle);
                }
            }
            
            // check for bottom side collision and if this object was going up, restore the transform.position back to previous position
            // This means, the bottom collisions are only taking into account if moving down, not moving up
            if( vLocImpulse.y > Vector3.kEpsilon && // if bottom collision 
                (m_jumpSpeed + Vector3.Project( m_rigidBody2D.velocity, transform.up ).y) > m_smartRectCollider.SkinBottomWidth / Time.deltaTime ) // if moving up fast enough
            {
                transform.position -= Vector3.Project(vLocImpulse, transform.up);
            }

            if( vLocImpulse.y < -Vector3.kEpsilon ) // if top collision
            {
                m_jumpSpeed = 0f; // reset jump velocity
            }
        }


        private LayerMask m_savedOneWayCollisionDown;
        private float m_platformDropTimer = 0f;
        void Update()
        {

            if( m_state != m_nextState)
            {
                eState prevState = m_state;
                m_state = m_nextState;
                if(OnStateChanged != null)
                    OnStateChanged(this, prevState, m_state);
            }

            if (State != eState.Dying)
            {
                if (m_platformDropTimer > 0f)
                {
                    m_platformDropTimer -= Time.deltaTime;
                    if (m_platformDropTimer <= 0f)
                    {   // Restore the One Way Down collision again after the time is over
                        m_smartRectCollider.OneWayCollisionDown = m_savedOneWayCollisionDown;
                    }
                }

                // This timer allow to jump during a while if smart collider is not grounded
                if (m_fallingJumpToleranceTimer > 0f) m_fallingJumpToleranceTimer -= Time.deltaTime;

                if (m_platformDropTimer <= 0f)
                {
                    if ((IsActionJump(m_Jump)) && m_jumpReleased && (m_isGrounded || m_isSwimming || m_isClimbing || m_fallingJumpToleranceTimer > 0))
                    {
                        StopClimbing();
                        m_jumpSpeed = (m_isSwimming? SwimmingJumpSpeed : JumpSpeed) * Mathf.Clamp01(1 - m_rigidBody2D.drag * JumpDragFactor * Time.deltaTime);
                        m_jumpReleased = false;
                    }
                }

                if (IsActionJumpUp(m_Jump))
                {
                    m_jumpReleased = true;
                    if (m_jumpSpeed > CutJumpSpeedLimit)
                    {
                        m_jumpSpeed = CutJumpSpeedLimit;
                    }
                }
            }
        }

        // Update is called once per frame
        bool m_skipFirstFixedUpdate = true;
        void FixedUpdate()
        {

            // Fix an issue when m_isGrounded is always false the first update if SmartRectCollider2D was not updated first
            if( m_skipFirstFixedUpdate )
            {
                m_skipFirstFixedUpdate = false;
                return;
            }

            Vector3 vLocVelocity = transform.rotation != Quaternion.identity ? Quaternion.Inverse(transform.rotation) * m_smartRectCollider.InstantVelocity : m_smartRectCollider.InstantVelocity;

            m_isGrounded = m_smartRectCollider.enabled &&
                m_smartRectCollider.IsGrounded() &&
                vLocVelocity.y <= Vector3.kEpsilon;

            if (m_isGrounded)
            {
                m_fallingJumpToleranceTimer = FallingJumpTolerance;
            }

            if(m_isController)
            {
                if (State != eState.Dying)
                {
                    m_spriteRenderer.color = Color.white;

                    UpdatePad(m_Horizontal,m_Vertical);

                    float fHorAxis = GetAxis(m_Horizontal);
                    float fVerAxis = GetAxis(m_Vertical);

                    // Fix issue when using keys, because the time to go from 0 to 1 or 1 to 0 is too high by default Unity parameters
                    // So if a moving key is pressed, the horizontal axis will be set to the right value directly
                    if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    {
                        fHorAxis = 1f;
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                    {
                        fHorAxis = -1f;
                    }
                    //---

                    if (m_isClimbing)
                    {
                        // isLadder is true when the collider width is small enough to avoid moving horizontal and center the player in the center of the collider.
                        // By default, it is true when collider width is less than two times the smart rect collider width
                        bool isLadder = (m_currentClimbingCollider is BoxCollider2D) && ((BoxCollider2D)m_currentClimbingCollider).size.x < 2 * m_smartRectCollider.Size.x;

                        Vector3 vDisp = new Vector3(isLadder ? 0f : fHorAxis, fVerAxis);

                        if (isLadder)
                        {
                            // Snap to ladder
                            BoxCollider2D box2DCollider = (BoxCollider2D)m_currentClimbingCollider;
                            Vector3 center = box2DCollider.transform.TransformPoint(box2DCollider.offset);
                            Vector3 snapPos = Vector3.Project(transform.position - center, m_currentClimbingCollider.transform.up); // this allow rotated ladders, like in pirate ship demo
                            snapPos += center;
                            snapPos.z = transform.position.z;
                            transform.position = Vector3.Lerp(transform.position, snapPos, 0.5f);
                        }

                        if (vDisp.magnitude > 0.2f)
                        {
                            transform.position += transform.rotation * vDisp * ClimbingSpeed * Time.deltaTime;
                            SetNextState(eState.Climbing);
                        }
                        else
                        {
                            SetNextState(eState.ClimbingIdle);
                        }
                    }
                    else // normal walking
                    {
                        bool isWalking = (Mathf.Abs(fHorAxis) > 0.7f);
                        fHorAxis = Mathf.Sign(fHorAxis) * Mathf.Max(Mathf.Abs(fHorAxis), 0.4f);
                        if (isWalking)
                        {
                            SetNextState(m_isGrounded ? eState.Walking : (m_smartRectCollider.InstantVelocity.y > 0f ? eState.Jumping : eState.Falling));
                            float walkAcc = fHorAxis > 0 ? WalkAcc : -WalkAcc;
                            float fHorAxisAbs = Mathf.Abs(fHorAxis);
                            if (Mathf.Sign(fHorAxis) != Mathf.Sign(m_walkVeloc.x) && m_walkVeloc.x != 0)
                            {
                                m_walkVeloc.x += walkAcc + walkAcc * WalkReactivity * fHorAxisAbs;
                            }
                            else
                            {
                                m_walkVeloc.x += walkAcc;
                            }
                            float walkSpeed = m_isSwimming ? WalkSpeed / 2 : WalkSpeed;
                            m_walkVeloc.x = Mathf.Clamp(m_walkVeloc.x, -walkSpeed * fHorAxisAbs, walkSpeed * fHorAxisAbs);
                            float movingSign = (IsSpriteFacingRight ? Mathf.Sign(m_walkVeloc.x) : -Mathf.Sign(m_walkVeloc.x));
                            if (movingSign != Mathf.Sign(transform.localScale.x))
                            {
                                Vector3 tempScale = transform.localScale;
                                tempScale.x = -tempScale.x; // flip player
                                transform.localScale = tempScale;
                            }
                        }
                        else
                        {
                            SetNextState(m_isGrounded ? eState.Idle : (m_smartRectCollider.InstantVelocity.y > 0f ? eState.Jumping : eState.Falling));
                            m_walkVeloc *= Mathf.Clamp01(1 - m_rigidBody2D.drag * Time.deltaTime);
                        }

                        // Apply walk velocity
                        Vector3 locWalkVeloc = transform.rotation != Quaternion.identity ? transform.rotation * m_walkVeloc : m_walkVeloc;
                        transform.position += locWalkVeloc * Time.deltaTime;

                        // Apply jump speed
                        if (m_jumpSpeed > 0f)
                        {
                            Vector3 gravity = Physics2D.gravity * (m_rigidBody2D.gravityScale == 0f ? 1f : m_rigidBody2D.gravityScale);
                            Vector3 locJumpVeloc = m_jumpSpeed * transform.up;
                            transform.position += locJumpVeloc * Time.deltaTime + 0.5f * gravity * Time.deltaTime * Time.deltaTime;
                            m_jumpSpeed = Mathf.Max(0f, m_jumpSpeed - (gravity.magnitude + m_rigidBody2D.drag * JumpDragFactor) * Time.deltaTime);
                            if (m_jumpSpeed < Vector3.kEpsilon) m_jumpSpeed = 0f;
                        }
                    }

                   

                    /*/ Used to teleport player to mouse position and test collisions with world
                    if( Input.GetMouseButton(0) )
                    {
                        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        pos.z = transform.position.z;
                        transform.position = pos;
                    }
                    //*/
                }
            }
            else
            {
               
            }


        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The state to be set on next Update
        /// </summary>
        /// <param name="state"></param>
        public void SetNextState( eState state )
        {
            m_nextState = state;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the Collider2D of a climbing collider below the smart rect collider
        /// </summary>
        /// <returns></returns>
        private Collider2D GetClimbingColliderBelow(float SkinBottomWidthFactor = 1.1f)
        {
            float dist = (m_smartRectCollider.SkinBottomWidth * SkinBottomWidthFactor + SmartRectCollider2D.k_SkinMinWidth) * transform.localScale.y;
            //for (int i = 0; i < m_smartRectCollider.BottomCheckPoints.Count; ++i)
            int i = (m_smartRectCollider.BottomCheckPoints.Count+1)/2;
            { //NOTE: the distance is (SkinBottomWidth + k_SkinMinWidth) because when resolving collisions, the smart rect is placed over the below collider, not touching it
                Vector3 vCheckPoint = transform.TransformPoint(m_smartRectCollider.BottomCheckPoints[i]);
                RaycastHit2D hit = Physics2D.Raycast(vCheckPoint, -transform.up, dist, ClimbingLayers);
                if (hit.collider != null)
                {
                    return hit.collider;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the Collider2D of a climbing collider above or inside the smart rect collider
        /// </summary>
        /// <returns></returns>
        private Collider2D GetClimbingColliderAbove()
        {
            float dist = SmartRectCollider2D.k_SkinMinWidth * transform.localScale.y;
            //for (int i = 0; i < m_smartRectCollider.TopCheckPoints.Count; ++i)
            int i = (m_smartRectCollider.TopCheckPoints.Count + 1) / 2;
            {
                Vector3 vCheckPoint = transform.TransformPoint(m_smartRectCollider.TopCheckPoints[i]);
                RaycastHit2D hit = Physics2D.Raycast(vCheckPoint, -transform.up, dist, ClimbingLayers);
                if (hit.collider != null)
                {
                    return hit.collider;
                }
            }
            return null;
        }

        private void StartClimbing()
        {
            if (!m_isClimbing)
            {
                m_isClimbing = true;
                m_jumpSpeed = 0f;
                m_savedGravScale = m_rigidBody2D.gravityScale;
                m_rigidBody2D.gravityScale = 0f;
                m_rigidBody2D.velocity = Vector2.zero;
            }
        }

        private void StopClimbing()
        {
            if (m_isClimbing)
            {
                m_isClimbing = false;
                m_currentClimbingCollider = null;
                m_rigidBody2D.gravityScale = m_savedGravScale;
                // fix an issue when reaching top of ladder it is alternating between grounded and not grounded
                transform.position -= new Vector3(0f, 10f*SmartRectCollider2D.k_SkinMinWidth);
            }
        }

        #endregion


        public bool IsActionUse(string fire2 = "Fire2")
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
            return Input.GetButtonDown(fire2);
        }

        public bool IsActionDrop(string down = "down", string vertical = "vertical")
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
            return false;
            return (Input.GetKeyDown(down) || Input.GetAxis(vertical) < -0.5f) && IsActionJumpDown();
        }

        public bool IsActionAttack(string fire1 = "Fire1")
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

        public bool IsActionJump(string jump = "Jump")
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
            return Input.GetKeyDown(Utils.GetKeyCodeByPlayer("Jump", m_PlayerNumber));
        }

        public bool IsActionJumpDown(string jump = "Jump")
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
            return Input.GetKeyDown(Utils.GetKeyCodeByPlayer("Jump", m_PlayerNumber));
        }

        public bool IsActionJumpUp(string jump = "Jump")
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
            return Input.GetKeyDown(Utils.GetKeyCodeByPlayer("Jump", m_PlayerNumber));
        }

        public float GetAxis(string axisName)
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

        public bool IsPadRight = false;
        public bool IsPadLeft = false;
        public bool IsPadUp = false;
        public bool IsPadDown = false;

#if UNITY_ANDROID
    // Used for control of player on device ( needs a gyroscope )
    private static float s_fGyroBaseOffset = 0f;
    private static float s_fGyroMovingOff = 0.1f;		// Gyro_x considered full speed
    private static float s_fBaseDistMaxOffset = 0.05f;	// Max distance the vertical base can be moved
    private static float s_fGyroFollowDist = 0.12f;	// Minimum dist for base to follow GyroX
    //private static float s_fSensibilityX	= 0.0225f;
#endif

        float _fHorXPrev = 0;
        float _fHorYPrev = 0;
        void UpdatePad(string Horizontal = "Horizontal", string Vertical = "Vertical")
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