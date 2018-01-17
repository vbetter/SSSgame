using UnityEngine;
using System.Collections;

namespace CreativeSpore.SmartColliders
{

    [System.Flags]
    public enum eControllerActions
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        Jump = 1 << 4,
        PlatformDropDown = 1 << 5,
    }

    [RequireComponent(typeof(SmartPlatformCollider))]
    public class PlatformCharacterController : MonoBehaviour
    {

        public enum eJumpingGuideMode
        {
            Right,
            Left,
            MovingDirection
        }

        public PlatformCharacterPhysics PlatformCharacterPhysics { get { return m_platformPhysics; } }
        /// <summary>
        /// The maximum angle in degrees of the slope to help the player climbing up and down.
        /// </summary>
        public float MaxSlope { get { return m_maxSlope; } set { m_maxSlope = Mathf.Abs(value % 90); } }
        /// <summary>
        /// When dropping down from a platform, this is the time, the bottom colliders are disabled to allow the player going down.
        /// </summary>
        public float PlatformDropTime { get { return m_platformDropTime; } set { m_platformDropTime = value; } }
        /// <summary>
        /// The lateral acceleration applied to the player when walking.
        /// </summary>
        public float WalkingAcc { get { return m_walkingAcc; } set { m_walkingAcc = value; } }
        /// <summary>
        /// The drag applied to the horizontal movement. This value affects to the maximum walking speed and the time the player needs to completely stop or ground slipperiness.
        /// </summary>
        public float WalkingDrag { get { return m_walkingDrag; } set { m_walkingDrag = value; } }
        /// <summary>
        /// The maximum speed allowed for the player. It goes from 0 to the maximum possible speed based on walking acceleration and drag.
        /// </summary>
        public float MaxWalkingSpeed { get { return m_maxWalkingSpeed; } set { m_maxWalkingSpeed = value; } }
        /// <summary>
        /// The horizontal acceleration applied while in air if there is a lateral moving action.
        /// </summary>
        public float AirborneAcc { get { return m_airborneAcc; } set { m_airborneAcc = value; } }
        /// <summary>
        /// The initial vertical speed when jumping.
        /// </summary>
        public float JumpingSpeed { get { return m_jumpingSpeed; } set { m_jumpingSpeed = value; } }
        /// <summary>
        /// Jumping acceleration applied while jumping is hold and until jumping acc. time is over. The maximum value is clamped by gravity.
        /// </summary>
        public float JumpingAcc { get { return m_jumpingAcc; } set { m_jumpingAcc = value; } }
        /// <summary>
        /// How much time the jumping acceleration is applied while jumping is hold.
        /// </summary>
        public float JumpingAccTime { get { return m_jumpingTime; } set { m_jumpingTime = value; } }
        /// <summary>
        /// Returns true is player is grounded
        /// </summary>
        public bool IsGrounded { get { return m_isGrounded; } set { m_isGrounded = value; } }
        /// <summary>
        /// The horizontal moving acceleration depending on the gounded state
        /// </summary>
        public float HorizontalMovingAcc { get { return IsGrounded ? m_walkingAcc : m_airborneAcc; } }
        /// <summary>
        /// The scale applied to the horizontal moving speed
        /// </summary>
        public float HorizontalSpeedScale { get { return m_horSpeedScale; } set { m_horSpeedScale = Mathf.Max(value, 0f); } }
        /// <summary>
        /// The scale applied to the vertival moving speed
        /// </summary>
        public float VerticalSpeedScale { get { return m_verSpeedScale; } set { m_verSpeedScale = Mathf.Max(value, 0f); } }
        /// <summary>
        /// The current angle of the ground in degrees
        /// </summary>
        public float SlopeAngle { get { return m_slopeAngle; } }        
        /// <summary>
        /// The instant velocity of this game object or how much it is moving from previous frame to the next one
        /// </summary>
        public Vector3 InstantVelocity { get { return m_instantVelocity; } }
        /// <summary>
        /// Distance to the ground
        /// </summary>
        public float GroundDist { get { return m_groundDist; } }
        /// <summary>
        /// Returns if an action is set
        /// </summary>
        public bool GetActionState(eControllerActions action) { return (m_prevActionFlags & action) != 0; }
        /// <summary>
        /// Returns if an action was set or reset during the previous frame
        /// </summary>
        public bool GetIfActionHasChanged(eControllerActions action) { return (m_actionChanged & action) != 0; }
        /// <summary>
        /// Sets an action
        /// </summary>
        public void SetActionState(eControllerActions action, bool value) { m_actionFlags = (value ? (m_actionFlags | action) : (m_actionFlags & ~action)); }     
   
        [System.Serializable]
        public class JumpingAdditionalParameters
        {
            [Tooltip("How many jumps you can do while on air without touching the ground. Use value -1 to infinite jumps.")]
            public int airJumps = 0;
            [Tooltip("How many jumps you can do over a wall without touching the floor. Use value -1 to infinite jumps.")]
            public int wallJumps = 0;
            [Tooltip("Speed applied in horizontal when jumping agains a wall.")]
            public float wallPushSpeed = 0f;
        }
        public JumpingAdditionalParameters JumpAdditionalParameters { get { return m_jumpingAdditionalParameters; } }

        ///<summary>
        /// Time needed to be able to jump again from the ladders
        ///</summary>
        public float LadderJumpTimeThreshold { get { return m_ladderJumpTimeThreshold; } }

        public float CurrentSlopeAngle{get { return this.m_slopeAngle; }}
        
        public void ResetLadderJumpTimeThreshold()
        {
            if(m_deltaLadderJumpTimeThreshold < 0f)
            {
                m_deltaLadderJumpTimeThreshold = m_ladderJumpTimeThreshold;
            }
        }
        
        [SerializeField, Range(0f, 90f)]
        private float m_maxSlope = 45f;
        [SerializeField]
        private float m_platformDropTime = 0.1f;
        [SerializeField]
        private float m_maxWalkingSpeed = 0f;
        [SerializeField]
        private float m_walkingAcc = 12f;
        [SerializeField]
        private float m_walkingDrag = 8f;
        [SerializeField]
        private float m_jumpingSpeed = 2f;
        [SerializeField]
        private float m_jumpingTime = .5f;
        [SerializeField]
        private float m_jumpingAcc = 5f;
        [SerializeField]
        private float m_airborneAcc = 8f;
        [SerializeField]
        private float m_ladderJumpTimeThreshold = 1f;
        [SerializeField]
        private JumpingAdditionalParameters m_jumpingAdditionalParameters = new JumpingAdditionalParameters();

        private bool m_isGrounded;
        private float m_slopeAngle;
        private Vector3 m_prevPos;
        private Vector3 m_instantVelocity;
        private float m_groundDist;
        private float m_horSpeedScale = 1f;
        private float m_verSpeedScale = 1f;
        private float m_deltaLadderJumpTimeThreshold = 0f;

        private eControllerActions m_actionFlags = eControllerActions.None;
        private eControllerActions m_prevActionFlags = eControllerActions.None;
        private eControllerActions m_actionChanged = eControllerActions.None;
        private float m_jumpingTimer = -1f;
        private LayerMask m_savedOneWayCollisionDown;
        private float m_platformDropTimer = 0f;
        private int m_airJumpsCounter = 0; // jumps in air, reset when grounded
        private int m_wallJumpsCounter = 0; // jumps over a wall, reset when grounded
        [SerializeField]
        private PlatformCharacterPhysics m_platformPhysics = new PlatformCharacterPhysics();
        
        private SmartPlatformCollider m_smartCollider;
        
        
        protected virtual void Start()
        {
            m_smartCollider = GetComponent<SmartPlatformCollider>();
            m_smartCollider.OnSideCollision += OnSideCollision;
            m_savedOneWayCollisionDown = m_smartCollider.OneWayCollisionDown;
            m_prevPos = transform.position;
            m_deltaLadderJumpTimeThreshold = 0f;

            // Automatically remove OneWayCollisionDown from layer collision in case user forgot to do that.
            // This allow player to move through OneWayCollisionDown ( or pass through) colliders in any direction but down.
            m_smartCollider.LayerCollision = m_smartCollider.LayerCollision & ~m_smartCollider.OneWayCollisionDown;
            // Same with climbing layers
            m_smartCollider.LayerCollision = m_smartCollider.LayerCollision & ~ClimbingLayers;
        }

        protected virtual void Reset()
        {
            m_maxWalkingSpeed = m_platformPhysics.SolveMaxSpeedWithAccAndDrag(m_walkingAcc, m_walkingDrag);
        }

        protected virtual void Update()
        {
            if (m_deltaLadderJumpTimeThreshold > 0f)
                m_deltaLadderJumpTimeThreshold -= Time.deltaTime;
            DoClimbing();

            if (!m_isClimbing)
            {
                // Jumping Action
                if (GetActionState(eControllerActions.Jump))
                {
                    bool jumpActionDown = GetIfActionHasChanged(eControllerActions.Jump);
                    int wallPushFactor = 0;
                    if (jumpActionDown && !IsGrounded)
                    {
                        wallPushFactor = m_smartCollider.SkinRightRayContacts.Contains(true) ? -1 : m_smartCollider.SkinLeftRayContacts.Contains(true) ? 1 : 0;
                        if (transform.localScale.x < 0)
                            wallPushFactor = -wallPushFactor;
                    }
                    //wall jump
                    if (jumpActionDown && !IsGrounded && (m_jumpingAdditionalParameters.wallJumps < 0 || m_jumpingAdditionalParameters.wallJumps > m_wallJumpsCounter)
                        && m_smartCollider.SkinRightRayContacts.Contains(true) || m_smartCollider.SkinLeftRayContacts.Contains(true)) // isMovingAgainstAWall
                    {
                        ++m_wallJumpsCounter;
                        ++m_airJumpsCounter;
                        m_jumpingTimer = JumpingAccTime;
                        m_platformPhysics.VSpeed = JumpingSpeed;
                        m_platformPhysics.HSpeed = wallPushFactor * m_jumpingAdditionalParameters.wallPushSpeed;
                    }
                    //air jump
                    else if(jumpActionDown && !IsGrounded && (m_jumpingAdditionalParameters.airJumps < 0 || m_jumpingAdditionalParameters.airJumps > m_airJumpsCounter))
                    {
                        ++m_airJumpsCounter;
                        m_jumpingTimer = JumpingAccTime;
                        m_platformPhysics.VSpeed = JumpingSpeed;
                    }
                    // ground jump
                    else if (
                        IsGrounded &&
                        //GetIfActionHasChanged(eControllerActions.Jumping) && //NOTE: if not commented, player needs to release and press jump to jump
                        m_jumpingTimer == -1 //NOTE: if not commented, player jumps only once if holding jumping while in air                    
                    )
                    {
                        m_jumpingTimer = JumpingAccTime;
                        m_platformPhysics.VSpeed = JumpingSpeed;
                        m_isGrounded = false;
                    }
                }
                else if (!GetActionState(eControllerActions.Jump))
                {
                    m_jumpingTimer = -1f;
                }

                // Moving Action
                if (GetActionState(eControllerActions.Right))
                {
                    m_platformPhysics.AddAcceleration(Vector2.right * HorizontalMovingAcc * m_horSpeedScale);
                }
                if (GetActionState(eControllerActions.Left))
                {
                    m_platformPhysics.AddAcceleration(-Vector2.right * HorizontalMovingAcc * m_horSpeedScale);
                }

                // Platform Drop Down Action
                if (m_isGrounded && m_platformDropTimer <= 0f && GetActionState(eControllerActions.PlatformDropDown))
                {
                    //NOTE: for this to work, OneWayCollisionDown should be removed from LayerCollisions
                    m_smartCollider.LayerCollision = m_smartCollider.LayerCollision & ~m_smartCollider.OneWayCollisionDown;
                    m_smartCollider.OneWayCollisionDown = 0;
                    m_platformDropTimer = PlatformDropTime;
                }

                // Jumping
                if (m_jumpingTimer > 0f)
                {
                    m_jumpingTimer -= Time.deltaTime;
                    m_platformPhysics.Acceleration += transform.up * m_jumpingAcc;
                }

                // Platform Drop Down
                if (m_platformDropTimer > 0f)
                {
                    m_platformDropTimer -= Time.deltaTime;
                    if (m_platformDropTimer <= 0f)
                    {   // Restore the One Way Down collision again after the time is over
                        m_smartCollider.OneWayCollisionDown = m_savedOneWayCollisionDown;
                    }
                }
                m_instantVelocity = (transform.position - m_prevPos) / Time.deltaTime;
                m_prevPos = transform.position;
                m_platformPhysics.Drag = new Vector2(m_walkingDrag, 0f);
                m_platformPhysics.MaxHSpeed = MaxWalkingSpeed * m_horSpeedScale;
                m_platformPhysics.Position = transform.position;
                m_platformPhysics.UpdatePhysics(Time.deltaTime);                
                m_groundDist = _CalculateGroundDist();
                bool wasGrounded = m_isGrounded;
                m_isGrounded = m_smartCollider.enabled && m_smartCollider.IsGrounded() && m_platformPhysics.DeltaDisp.y <= 0f;

                // NOTE: Unity 5.4 has a bug in TransformDirection being affected by scale (x is negated to flip the sprite). 
                // So instead of calling TransformDirection I will rotate it directly.
                transform.position = transform.position + transform.rotation * (m_platformPhysics.Position - transform.position);

                //+++ Do slopes
                // If over a slope, move the character in the slope direction
                if (m_slopeAngle != 0f)
                {
                    if (Mathf.Abs(m_slopeAngle) <= Mathf.Round(m_maxSlope))
                    {
                        transform.position += transform.up * m_platformPhysics.DeltaDisp.x * Mathf.Tan(m_slopeAngle * Mathf.Deg2Rad);
                        m_isGrounded = true;
                    }
                    else if (Mathf.Sign(m_slopeAngle) == Mathf.Sign(m_platformPhysics.DeltaDisp.x))
                    {
                        // Avoid climbing up the slope
                        transform.position = m_platformPhysics.Position = new Vector3(m_prevPos.x, transform.position.y, transform.position.z);
                        m_platformPhysics.HSpeed = 0f;
                        m_isGrounded = true;
                    }
                }
                // if not over a slope, check if there is a slope under the character to place it to the ground
                else if (!m_isGrounded && wasGrounded && m_platformPhysics.DeltaDisp.y <= 0f)
                {
                    m_groundDist = _CalculateGroundDist();
                    float magneticDist = Mathf.Abs(m_platformPhysics.DeltaDisp.x * Mathf.Tan((m_maxSlope) * Mathf.Deg2Rad));
                    if (m_groundDist <= magneticDist)
                    {
                        m_isGrounded = true;
                        m_groundDist += SmartRectCollider2D.k_SkinMinWidth;
                        transform.position += -transform.up * m_groundDist;
                    }
                }
                if(m_isGrounded)
                {
                    m_wallJumpsCounter = m_airJumpsCounter = 0;
                }
                m_slopeAngle = 0f;
                //---
            }

            //Update Actions
            m_actionChanged = m_prevActionFlags ^ m_actionFlags;
            m_prevActionFlags = m_actionFlags;
        }

        protected float _CalculateGroundDist()
        {
            float groundDist = float.MaxValue;
            for (int i = 0; i < m_smartCollider.BottomCheckPoints.Count; ++i)
            {
                SmartRaycastHit smartHit = m_smartCollider.SmartRaycast(
                        transform.TransformPoint(m_smartCollider.BottomCheckPoints[i]),
                        -transform.up,
                        float.MaxValue,
                        m_smartCollider.LayerCollision | m_smartCollider.OneWayCollisionDown);
                if (smartHit != null)
                {
                    groundDist = Mathf.Min(groundDist, smartHit.distance - m_smartCollider.SkinBottomWidth);
                }
            }
            return groundDist;
        }

        [SerializeField]
        private Vector3 m_jumpingGuideBasePos;
        [SerializeField]
        private Vector3 m_jumpingGuideOffset = Vector3.zero;
        [SerializeField]
        private eJumpingGuideMode m_jumpingGuideMode = eJumpingGuideMode.Right;
        [SerializeField]
        private bool m_showGuides = true;
        private float m_simulatedStartingHSpeed;
        private float m_simulatedMovingDir;
        void OnDrawGizmosSelected()
        {
            if (!m_showGuides)
            {
                return;
            }

            float maxWalkSpeed = m_platformPhysics.SolveMaxSpeedWithAccAndDrag(WalkingAcc, WalkingDrag);
            maxWalkSpeed = Mathf.Min(maxWalkSpeed, MaxWalkingSpeed);

            if (m_jumpingGuideMode == eJumpingGuideMode.MovingDirection)
            {
                if (m_platformPhysics.HSpeed > 0.1f * maxWalkSpeed)
                {
                    m_simulatedMovingDir = 1f;
                }
                else if (m_platformPhysics.HSpeed < -0.1f * maxWalkSpeed)
                {
                    m_simulatedMovingDir = -1f;
                }
            }
            else if(m_jumpingGuideMode == eJumpingGuideMode.Right)
            {
                m_simulatedMovingDir = 1f;
            }
            else if (m_jumpingGuideMode == eJumpingGuideMode.Left)
            {
                m_simulatedMovingDir = -1f;
            }

            if(!Application.isPlaying || IsGrounded || IsClimbing)
            {
                m_jumpingGuideBasePos = transform.position;
            }
            
            if(Application.isPlaying)
            {
                if(IsGrounded || IsClimbing)
                {
                    m_simulatedStartingHSpeed = m_platformPhysics.HSpeed;
                }
            }
            else
            {
                m_simulatedStartingHSpeed = maxWalkSpeed;
            }

            Vector3 simulationPos = m_jumpingGuideBasePos + m_jumpingGuideOffset;
            float maxHDist = 0f;
            float minHoldJumpTime = Time.fixedDeltaTime;
            int iters = Mathf.CeilToInt(JumpingAccTime / minHoldJumpTime) + 1;
            float jumpingAccTime = 0;
            float timeDt = Time.fixedDeltaTime;

            PlatformCharacterPhysics physics = new PlatformCharacterPhysics(m_platformPhysics);
            for (int iter = 0; iter < iters; ++iter, jumpingAccTime += minHoldJumpTime)
            {
                physics.Position = Vector3.zero;
                physics.HDrag = WalkingDrag;
                physics.MaxHSpeed = MaxWalkingSpeed;
                physics.Velocity = new Vector3(m_simulatedStartingHSpeed, JumpingSpeed);
                float time = 0f;
                float maxWhileIters = 1000;
                do
                {
                    Vector3 prevPos = physics.Position;
                    physics.AddAcceleration(m_simulatedMovingDir * Vector2.right * AirborneAcc);
                    if (time < jumpingAccTime)
                    {
                        physics.Acceleration += transform.up * m_jumpingAcc;
                    }
                    time += timeDt;
                    float timeToGround = physics.SolveTimeToPosY(0f);
                    if (timeToGround >= timeDt)
                    {
                        physics.UpdatePhysics(timeDt);
                    }
                    else
                    {
                        physics.UpdatePhysics(timeToGround);
                        physics.Position = new Vector3(physics.Position.x, 0f); // for float imprecisions, pos.y will me almost but not equal to 0f. This will end the loop.
                    }

                    Gizmos.color = Color.Lerp( new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 0.5f), 1f - (iter + 1f) / iters);
                    Gizmos.DrawLine(prevPos + simulationPos, physics.Position + simulationPos);
                    // Draw max and min high line depending on 
                    if (iter == 0 || iter == iters - 1)
                    {
                        Gizmos.color = new Color(0f, 0f, 0f, 0.2f);
                        Gizmos.DrawSphere(prevPos + simulationPos, 0.03f * GizmoUtils.GetGizmoSize(prevPos + simulationPos));
                    }                    
                }
                while (physics.Position.y > 0 && --maxWhileIters > 0);
                //Debug.Assert(maxIters > 0, "Infinite loop detected!");
                Gizmos.DrawSphere(physics.Position + simulationPos, 0.03f * GizmoUtils.GetGizmoSize(physics.Position + simulationPos));
                maxHDist = Mathf.Max(maxHDist, Mathf.Abs(physics.Position.x));
            }
            maxHDist *= m_simulatedMovingDir;
            float minJumpHeight = physics.SolveMaxJumpHeight(m_jumpingSpeed);
            float maxJumpHeight = physics.SolveMaxJumpHeight(m_jumpingSpeed, m_jumpingAcc, m_jumpingTime);
            Gizmos.color = new Color(1f, 1f, 0f, 1f);
            Gizmos.DrawLine(simulationPos + new Vector3(-maxHDist * .25f, minJumpHeight), simulationPos + new Vector3(maxHDist * .75f, minJumpHeight));
            Gizmos.color = new Color(1f, 1f, 0f, 1f);
            Gizmos.DrawLine(simulationPos + new Vector3(-maxHDist * .25f, maxJumpHeight), simulationPos + new Vector3(maxHDist * .75f, maxJumpHeight));
            Gizmos.color = Color.white;
        }

        private void OnSideCollision(SmartCollision2D collision, GameObject collidedObject)
        {
            if (collision.impulse.y > 0 && m_smartCollider.IsGrounded())
            {
                m_slopeAngle = Vector2.Angle(collision.contacts[0].normal, Vector2.right) - 90f;
            }
            else
            {
                m_slopeAngle = 0;
            }

            Vector3 velocity = m_platformPhysics.Velocity;
            // Reset vertical speed on opposite vertical collision
            if (collision.impulse.y > 0f && velocity.y < 0f ||
                collision.impulse.y < 0f && velocity.y > 0f)
            {
                velocity.y = 0f;
            }

            // Reset horizontal speed on opposite horizontal collision
            if (collision.impulse.x > 0f && velocity.x < 0f ||
                collision.impulse.x < 0f && velocity.x > 0f)
            {
                velocity.x = 0f;
            }
            m_platformPhysics.Velocity = velocity;            
        }

        public float ClimbingSpeed { get{ return m_climbingSpeed; } set{ m_climbingSpeed = value; } }
        public LayerMask ClimbingLayers { get { return m_climbingLayers; } set { m_climbingLayers = value; } }
        public float ClimbingLadderWidthFactor { get { return m_ladderWidthFactor; } set { m_ladderWidthFactor = value; } }
        public bool IsClimbing { get { return m_isClimbing; } }

        [SerializeField]
        private float m_climbingSpeed = 1f;
        [SerializeField]
        private LayerMask m_climbingLayers;
        [SerializeField]
        private float m_ladderWidthFactor = 2f;
        private bool m_isClimbing = false;
        private Collider2D m_currentClimbingCollider;

        protected void DoClimbing()
        {
            float fHorAxis = 0f;
            float fVerAxis = 0f;
           
            if (GetActionState(eControllerActions.Right)) fHorAxis += m_horSpeedScale;
            if (GetActionState(eControllerActions.Left)) fHorAxis -= m_horSpeedScale;
            if (GetActionState(eControllerActions.Up)) fVerAxis += m_verSpeedScale;
            if (GetActionState(eControllerActions.Down)) fVerAxis -= m_verSpeedScale;
            if (m_isClimbing)
            {
                if (
                    GetIfActionHasChanged(eControllerActions.Jump) && GetActionState(eControllerActions.Jump) ||
                    GetIfActionHasChanged(eControllerActions.PlatformDropDown) && GetActionState(eControllerActions.PlatformDropDown))
                {
                    m_isClimbing = false;
                    m_currentClimbingCollider = null;
                    m_isGrounded = true;
                    return;
                }

                // isLadder is true when the collider width is small enough to avoid moving horizontal and center the player in the center of the collider.
                // By default, it is true when collider width is less than two times the smart rect collider width
                bool isLadder = m_currentClimbingCollider.bounds.size.x < m_ladderWidthFactor * m_smartCollider.Size.x;

                Vector3 vDisp = new Vector3(isLadder ? 0f : fHorAxis, fVerAxis);

                //debug climbing area
                DebugEx.DebugDrawRect(Vector2.zero, new Rect(m_currentClimbingCollider.bounds.min, m_currentClimbingCollider.bounds.size), Color.blue);


                if (isLadder)
                {
                    // Snap to ladder
                    Vector3 center = m_currentClimbingCollider.bounds.center;
                    Vector3 snapPos = Vector3.Project(transform.position - center, m_currentClimbingCollider.transform.up); // this allow rotated ladders, like in pirate ship demo
                    snapPos += center;
                    snapPos.z = transform.position.z;
                    transform.position = Vector3.Lerp(transform.position, snapPos, 0.5f);
                }

                if (vDisp.magnitude > 0.2f)
                {
                    transform.position += transform.rotation * vDisp * ClimbingSpeed * Time.deltaTime;                    
                }

                m_isGrounded = m_smartCollider.enabled && m_smartCollider.IsGrounded();
            }

            // Check if going down and there is a climbing collider below
            float SkinBottomWidthFactor = 1.1f; //NOTE: set a value > 1f to allow climbing down when climb collision and platform collision are close
            Collider2D climbingColliderBelow = GetClimbingColliderBelow(SkinBottomWidthFactor);
            Collider2D climbingColliderAbove = GetClimbingColliderAbove();
            if (fVerAxis < -0.5f && m_currentClimbingCollider == null)
            {
                if (climbingColliderBelow != null)
                {
                    if (m_currentClimbingCollider == null && climbingColliderAbove == null)
                    {
                        //Teleport the player. TeleportTo will skip any collider in between the current position and the new position to skip any platform in between
                        m_smartCollider.TeleportTo(transform.position - transform.up * m_smartCollider.SkinBottomWidth * SkinBottomWidthFactor);
                    }
                    m_currentClimbingCollider = climbingColliderBelow;
                    StartClimbing();
                }
                else
                {
                    StopClimbing();
                }
            }
            // Check if going up and it is inside a climbing collider
            else if (fVerAxis > 0.5f)
            {
                if (climbingColliderAbove != null && !GetIfActionHasChanged(eControllerActions.Jump))
                {
                    m_currentClimbingCollider = climbingColliderAbove;
                    StartClimbing();
                }
                else if (m_smartCollider.SkinBottomRayContacts.Contains(true) || climbingColliderBelow == null)
                {
                    StopClimbing();
                }
            }
            // Stop climbing once the top is reached
            else if (m_isGrounded || (climbingColliderBelow == null && climbingColliderAbove == null))
            {
                StopClimbing();
            }
        }


        protected void StartClimbing()
        {
            if (!m_isClimbing)
            {
                if (m_deltaLadderJumpTimeThreshold <= 0)
                {
                    m_isClimbing = true;
                    m_platformPhysics.Velocity = Vector2.zero;
                    m_jumpingTimer = -1f;
                }
            }
        }

        protected void StopClimbing()
        {
            if (m_isClimbing)
            {
                m_isClimbing = false;
                m_currentClimbingCollider = null;
                // fix an issue when reaching top of leadder it is alternating between grounded and not grounded
                transform.position -= new Vector3(0f, 10 * SmartRectCollider2D.k_SkinMinWidth);
            }
        }

        /// <summary>
        /// Returns the Collider2D of a climbing collider below the smart rect collider
        /// </summary>
        /// <returns></returns>
        protected Collider2D GetClimbingColliderBelow(float SkinBottomWidthFactor = 1.1f)
        {
            //for (int i = 0; i < m_smartRectCollider.BottomCheckPoints.Count; ++i)
            int i = (m_smartCollider.BottomCheckPoints.Count + 1) / 2;
            { //NOTE: the distance is (SkinBottomWidth + k_SkinMinWidth) because when resolving collisions, the smart rect is placed over the below collider, not touching it
                Vector3 vCheckPoint = transform.TransformPoint(m_smartCollider.BottomCheckPoints[i]);
                float dist = (m_smartCollider.SkinBottomWidth * SkinBottomWidthFactor + SmartRectCollider2D.k_SkinMinWidth) * transform.localScale.y;
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
        protected Collider2D GetClimbingColliderAbove()
        {
            //for (int i = 0; i < m_smartRectCollider.TopCheckPoints.Count; ++i)
            int i = (m_smartCollider.TopCheckPoints.Count + 1) / 2;
            {
                Vector3 vCheckPoint = transform.TransformPoint(m_smartCollider.TopCheckPoints[i]);
                float dist = SmartRectCollider2D.k_SkinMinWidth * transform.localScale.y;
                RaycastHit2D hit = Physics2D.Raycast(vCheckPoint, -transform.up, dist, ClimbingLayers);
                if (hit.collider != null)
                {
                    return hit.collider;
                }
            }
            return null;
        }
    }
}
