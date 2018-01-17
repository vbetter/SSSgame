using UnityEngine;
using System.Collections;

namespace CreativeSpore.SmartColliders
{

    [System.Serializable]
    public class PlatformCharacterPhysics
    {
        public const float k_MaxTimeToSolveTimeToReachSpeed = 5f;

        public Vector3 Position { get { return m_pos; } set { m_pos = value; } }
        public Vector3 Velocity { get { return m_vel; } set { m_vel = value; } }
        public Vector3 Acceleration { get { return m_acc; } set { m_acc = value; } }
        public Vector3 Gravity { get { return m_gravity; } set { m_gravity = value; } }
        public float GravityScale { get { return m_gravityScale; } set { m_gravityScale = Mathf.Max(0f, value); } }
        public float TerminalVelocity { get { return m_terminalVel; } set { m_terminalVel = value; } }
        public float MaxHSpeed { get { return m_maxHSpeed; } set { m_maxHSpeed = value; } }
        public Vector3 Drag { get { return m_drag; } set { m_drag = value; } }

        public Vector3 DeltaDisp { get { return m_pos - m_prevPos; } }
        public float HSpeed { get { return m_vel.x; } set { m_vel.x = value; } }
        public float VSpeed { get { return m_vel.y; } set { m_vel.y = value; } }
        public float HDrag { get { return m_drag.x; } set { m_drag.x = value; } }
        public void AddAcceleration(Vector3 acc) { m_acc += acc; }

        [SerializeField]
        private Vector3 m_pos;
        [SerializeField]
        private Vector3 m_vel;
        [SerializeField]
        private Vector3 m_acc;
        [SerializeField]
        private Vector3 m_gravity = 9.8f * Vector3.down;
        [SerializeField]
        private float m_gravityScale = 1f;
        [SerializeField]
        private float m_terminalVel = 0f;
        [SerializeField]
        private float m_maxHSpeed = 0f;
        [SerializeField]
        private Vector3 m_drag;

        private Vector3 m_prevPos;

        public PlatformCharacterPhysics() { }
        public PlatformCharacterPhysics(PlatformCharacterPhysics other)
        {
            m_pos = other.m_pos;
            m_vel = other.m_vel;
            m_acc = other.m_acc;
            m_gravity = other.m_gravity;
            m_gravityScale = other.m_gravityScale;
            m_terminalVel = other.m_terminalVel;
            m_maxHSpeed = other.m_maxHSpeed;
            m_drag = other.m_drag;
        }

        public void UpdatePhysics(float timeDt)
        {
            // Apply Gravity
            m_acc += m_gravity * m_gravityScale;
            // Apply Velocity Limits
            if (m_terminalVel > 0f && m_vel.y < -m_terminalVel) m_vel.y = -m_terminalVel;
            if (m_maxHSpeed >= 0f) m_vel.x = Mathf.Clamp(m_vel.x, -m_maxHSpeed, m_maxHSpeed);
            // Update Position
            m_prevPos = m_pos;
            Vector3 disp = m_vel * timeDt + .5f * m_acc * timeDt * timeDt;

			float l_maxHDisp = m_maxHSpeed * timeDt;

			disp.x = Mathf.Clamp(disp.x, -l_maxHDisp, l_maxHDisp);

            m_pos += disp;
            // Update Velocity
            m_vel += m_acc * timeDt;
            // Apply Velocity Limits
            if (m_terminalVel > 0f && m_vel.y < -m_terminalVel) m_vel.y = -m_terminalVel;
            if (m_maxHSpeed >= 0f) m_vel.x = Mathf.Clamp(m_vel.x, -m_maxHSpeed, m_maxHSpeed);
            //Apply Drag
            m_vel.x *= Mathf.Clamp01(1f - m_drag.x * timeDt);
            m_vel.y *= Mathf.Clamp01(1f - m_drag.y * timeDt);
            // Reset acceleration
            m_acc = Vector3.zero;
        }

        public float SolveTimeToPosY(float posY)
        {
            float scaledGravity = m_gravity.y * m_gravityScale;
            float sqrt = Mathf.Sqrt(m_vel.y * m_vel.y - 2 * scaledGravity * (m_pos.y - posY));
            float time = (-m_vel.y - sqrt) / scaledGravity;
            return time;
        }

        public float SolveMaxJumpHeight( float jumpSpeed, float jumpAcc = 0f, float jumpAccTime = 0f )
        {
            float scaledGravity = m_gravity.y * m_gravityScale;
            //NOTE: the jumpAcc is always applied the first frame if jumpAccTime > 0f
            if(jumpAccTime > 0f)
            {
                // Then it is applied each fixedUpdate, not being applied the last update if the remainder is less than fixedDeltaTime
                float remainder = jumpAccTime % Time.fixedDeltaTime;
                if (remainder > Vector3.kEpsilon)
                {
                    jumpAccTime += Time.fixedDeltaTime;
                    jumpAccTime -= remainder;
                }
            }
            float acc = jumpAcc + scaledGravity;
            float time = -jumpSpeed / acc;
            if(time <= jumpAccTime)
            {
                return jumpSpeed * time + .5f * acc * time * time;
            }
            else
            {
                float height = jumpSpeed * jumpAccTime + .5f * acc * jumpAccTime * jumpAccTime;
                float vspeed = jumpSpeed + acc * jumpAccTime;
                time = -vspeed / scaledGravity;
                height += vspeed * time + .5f * scaledGravity * time * time;
                return height;
            }
        }

        public float SolveJumpSpeedToReachHeight( float height )
        {
            float scaledGravity = m_gravity.y * m_gravityScale;
            return Mathf.Sqrt(-2f * scaledGravity * height);
        }

        public float SolveJumpAccToReachHeight( float height, float jumpSpeed, float jumpAccTime )
        {
            float scaledGravity = m_gravity.y * m_gravityScale;
            //NOTE: the jumpAcc is always applied the first frame if jumpAccTime > 0f
            if(jumpAccTime > 0f)
            {
                // Then it is applied each fixedUpdate, not being applied the last update if the remainder is less than fixedDeltaTime
                float remainder = jumpAccTime % Time.fixedDeltaTime;
                if (remainder > Vector3.kEpsilon)
                {
                    jumpAccTime += Time.fixedDeltaTime;
                    jumpAccTime -= remainder;
                }
            }
            float t0 = jumpAccTime;
            float v0 = jumpSpeed;            
            float a = -t0 * t0;
            float b = -2f * v0 * t0 + scaledGravity * t0 * t0;
            float c = 2 * scaledGravity * (v0 * t0 - height) - v0 * v0;
            float sqrt = Mathf.Sqrt(b * b - 4 * a * c);
            float A = (-b - sqrt) / (2f * a);
            //float jumpAcc = A - m_gravity.y;
            //Debug.Log("A+: " + ((-b + sqrt) / (2f * a)) + " A-: " + ((-b - sqrt) / (2f * a)) + " jumpAcc " + jumpAcc + "|" + (-(v0 * v0) / (2f * height) - m_gravity.y) + " v1 " + (v0 + A * t0) + " t1 " + (-(v0 + A * t0) / m_gravity.y));
            float t1 = -(v0 + A * t0) / scaledGravity;
            if (t1 <= 0f) // jumpAccTime >= timeToReachHeight
            {
                A = -(v0 * v0) / (2f * height);
            }
            return A - scaledGravity;            
        }

        /// <summary>
        /// Solve the maximum speed applying an acceleration with specified drag
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="drag"></param>
        /// <returns></returns>
        public float SolveMaxSpeedWithAccAndDrag(float acc, float drag)
        {
            return ((1 - drag * Time.fixedDeltaTime) * acc * Time.fixedDeltaTime) / (drag * Time.fixedDeltaTime);
        }

        public float SolveMapHorizontalSpeedWithAcc(float acc)
        {
            return SolveMaxSpeedWithAccAndDrag(acc, m_drag.x);
        }
        
        public float SolveTimeToReachSpeed(float speed, float acc, float drag, float precision = 0.01f)
        {
            float R = drag * Time.fixedDeltaTime;
            Debug.Assert(R <= 1f, "Drag * TimeDt should be <= 1f");
            if (R == 1f) return 0f;
            speed = Mathf.Abs(speed);
            acc = Mathf.Abs(acc);
            float maxSpeed = SolveMaxSpeedWithAccAndDrag(acc, drag);
            if(speed > maxSpeed)
            {
                return float.PositiveInfinity;
            }

            float time = 0f;
            float v = 0f;
            while (speed - v > precision)
            {
                time += Time.fixedDeltaTime;
                if (time >= k_MaxTimeToSolveTimeToReachSpeed) return k_MaxTimeToSolveTimeToReachSpeed;
                v += acc * Time.fixedDeltaTime;
                v *= (1 - drag * Time.fixedDeltaTime);
            }
            return time;
        }
    }
}
