using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CreativeSpore.SmartColliders
{

    public class SmartRectCollider2D : SmartCollider2D
    {

        /// <summary>
        /// Min value used for the skin, to make collisions work properly (use smaller values to avoid jittering)
        /// </summary>
        public const float k_SkinMinWidth = 1e-005f;

        /// <summary>
        /// Distance of separation when there is a collision with another collider. If this value is negative, the collision will be triggered every UpdateCollision.
        /// A negative value is needed to make sure the moving platforms will move the collider, mostly if the UpdateMode is set to OnUpdate.
        /// The absolute value should be less than the k_SkinMinWidth value
        /// </summary>
        public const float k_colliderSeparation = -k_SkinMinWidth / 2f;

        /// <summary>
        /// When checking one way collisions, this angle (in radians) is the max angle allowed to consider the collision in one specific direction.
        /// Ex: for side down and value of Mathf.Cos(60), the angle between collision hit norm vector and Vector3.Up ( the opposite of side direction ) should be
        /// <= 60 degrees to consider the collision.
        /// </summary>
        public static readonly float k_OneSideSlopeNormThreshold = Mathf.Cos(60f * Mathf.Deg2Rad);

        /// <summary>
        /// Collisions event triggered when there is a collision with the world colliders
        /// </summary>
        /// <param name="vSolveDisp">The displacement done to solve the collision</param>
        /// <param name="isHorizontalStuck">If the collision could not be solved because the smart collisions was stuck horizontally</param>
        /// <param name="isVerticalStuck">If the collision could not be solved because the smart collisions was stuck vertically</param>
        public delegate void OnCollisionDelegate(Vector3 vSolveDisp, bool isHorizontalStuck, bool isVerticalStuck);
        public OnCollisionDelegate OnCollision;

        /// <summary>
        /// Collision event triggered per each side of the smart collider
        /// </summary>
        /// <param name="collision">This smart collider</param>
        /// <param name="collidedObject">The collided object</param>
        public delegate void OnSideCollisionDelegate(SmartCollision2D collision, GameObject collidedObject);
        public OnSideCollisionDelegate OnSideCollision;

        /// <summary>
        /// The position of the smart collider in the object’s local space
        /// </summary>
        public Vector3 Center { get { return m_center; } set { bool isChanged = m_center != value; m_center = value; if (isChanged) UpdateCheckPoints(); } }
        /// <summary>
        /// The size of the smart collider in the X, Y, Z directions
        /// </summary>
        public Vector2 Size { get { return m_size; } set { bool isChanged = m_size != value; m_size = value; if (isChanged) UpdateCheckPoints(); } }
        /// <summary>
        /// Check points of the top side of the smart collider body
        /// </summary>
        public IList<Vector3> TopCheckPoints { get { return m_vTopCheckPoints; } }
        /// <summary>
        /// Check points of the bottom side of the smart collider body
        /// </summary>
        public IList<Vector3> BottomCheckPoints { get { return m_vBottomCheckPoints; } }
        /// <summary>
        /// Check points of the left side of the smart collider body
        /// </summary>
        public IList<Vector3> LeftCheckPoints { get { return m_vLeftCheckPoints; } }
        /// <summary>
        /// Check points of the right side of the smart collider body
        /// </summary>
        public IList<Vector3> RightCheckPoints { get { return m_vRightCheckPoints; } }

        /// <summary>
        /// Width of the skin on the top side of the smart collider
        /// </summary>
        public float SkinTopWidth { get { return m_skinTopWidth; } set { m_skinTopWidth = value; m_skinTopWidth = Mathf.Max(m_skinTopWidth, k_SkinMinWidth); } }
        /// <summary>
        /// Width of the skin on the bottom side of the smart collider
        /// </summary>
        public float SkinBottomWidth { get { return m_skinBottomWidth; } set { m_skinBottomWidth = value; m_skinBottomWidth = Mathf.Max(m_skinBottomWidth, k_SkinMinWidth); } }
        /// <summary>
        /// Width of the skin on the left side of the smart collider
        /// </summary>
        public float SkinLeftWidth { get { return m_skinLeftWidth; } set { m_skinLeftWidth = value; m_skinLeftWidth = Mathf.Max(m_skinLeftWidth, k_SkinMinWidth); } }
        /// <summary>
        /// Width of the skin on the right side of the smart collider
        /// </summary>
        public float SkinRightWidth { get { return m_skinRightWidth; } set { m_skinRightWidth = value; m_skinRightWidth = Mathf.Max(m_skinRightWidth, k_SkinMinWidth); } }

        /// <summary>
        /// The skin raycast will start from body side line for 0f and from the opposite body side line for 1f
        /// </summary>
        public float SkinBottomOff01 { get { return m_skinBottomOff01; } set { m_skinBottomOff01 = Mathf.Clamp01(value); } }
        /// <summary>
        /// The skin raycast will start from body side line for 0f and from the opposite body side line for 1f
        /// </summary>
        public float SkinTopOff01 { get { return m_skinTopOff01; } set { m_skinTopOff01 = Mathf.Clamp01(value); } }
        /// <summary>
        /// The skin raycast will start from body side line for 0f and from the opposite body side line for 1f
        /// </summary>
        public float SkinRightOff01 { get { return m_skinRightOff01; } set { m_skinRightOff01 = Mathf.Clamp01(value); } }
        /// <summary>
        /// The skin raycast will start from body side line for 0f and from the opposite body side line for 1f
        /// </summary>
        public float SkinLeftOff01 { get { return m_skinLeftOff01; } set { m_skinLeftOff01 = Mathf.Clamp01(value); } }

        /// <summary>
        /// The number of check points between the two points for each extreme of each horizontal side ( top and bottom ). 
        /// The final number of TopCheckPoints and BottomCheckPoints will be HSkinSubdivisions + 2
        /// </summary>
        public int HSkinSubdivisions { get { return m_hSkinSubdivisions; } set { m_hSkinSubdivisions = Mathf.Max(0, value); } }
        /// <summary>
        /// The number of check points between the two points for each extreme of each vertical side ( left and right ). 
        /// The final number of LeftCheckPoints and RightCheckPoints will be VSkinSubdivisions + 2
        /// </summary>
        public int VSkinSubdivisions { get { return m_vSkinSubdivisions; } set { m_vSkinSubdivisions = Mathf.Max(0, value); } }

        /// <summary>
        /// This list will contain a boolean to true for each check point inside TopCheckPoints that trigger a collision during the last FixedUpdate
        /// </summary>
        public IList<bool> SkinTopRayContacts { get { return m_skinTopRayContacts; } }
        /// <summary>
        /// This list will contain a boolean to true for each check point inside BottomCheckPoints that trigger a collision during the last FixedUpdate
        /// </summary>
        public IList<bool> SkinBottomRayContacts { get { return m_skinBottomRayContacts; } }
        /// <summary>
        /// This list will contain a boolean to true for each check point inside LeftCheckPoints that trigger a collision during the last FixedUpdate
        /// </summary>
        public IList<bool> SkinLeftRayContacts { get { return m_skinLeftRayContacts; } }
        /// <summary>
        /// This list will contain a boolean to true for each check point inside RightCheckPoints that trigger a collision during the last FixedUpdate
        /// </summary>
        public IList<bool> SkinRightRayContacts { get { return m_skinRightRayContacts; } }

        /// <summary>
        /// The velocity based on the moving distance between FixedUpdate calls
        /// </summary>
        public Vector3 InstantVelocity { get { return m_instantVelocity; } }

        /// <summary>
        /// Enables the pixel snap performed before rendering the object using the value of PixelToUnits
        /// </summary>
        public bool PixelSnapEnabled { get { return m_pixelSnapEnabled; } set { m_pixelSnapEnabled = value; } }

        /// <summary>
        /// How many pixels would equal 1 unit in the world space
        /// </summary>
        public float PixelToUnits { get { return m_pixelToUnits; } set { m_pixelToUnits = value; } }

        [SerializeField]
        protected Vector3 m_center;
        [SerializeField]
        protected Vector2 m_size;

        [SerializeField]
        protected int m_hSkinSubdivisions = 3;
        [SerializeField]
        protected int m_vSkinSubdivisions = 5;

        [SerializeField]
        protected List<Vector3> m_vTopCheckPoints = new List<Vector3>();
        [SerializeField]
        protected List<Vector3> m_vBottomCheckPoints = new List<Vector3>();
        [SerializeField]
        protected List<Vector3> m_vLeftCheckPoints = new List<Vector3>();
        [SerializeField]
        protected List<Vector3> m_vRightCheckPoints = new List<Vector3>();

        [SerializeField]
        protected List<bool> m_skinTopRayContacts;
        [SerializeField]
        protected List<bool> m_skinBottomRayContacts;
        [SerializeField]
        protected List<bool> m_skinLeftRayContacts;
        [SerializeField]
        protected List<bool> m_skinRightRayContacts;

        [SerializeField]
        protected float m_skinTopWidth = 0.1f;
        [SerializeField]
        protected float m_skinBottomWidth = 0.1f;
        [SerializeField]
        protected float m_skinLeftWidth = 0.1f;
        [SerializeField]
        protected float m_skinRightWidth = 0.1f;
        
        [SerializeField, Range(0, 1)]
        protected float m_skinBottomOff01 = 0f;
        [SerializeField, Range(0, 1)]
        protected float m_skinTopOff01 = 0f;
        [SerializeField, Range(0, 1)]
        protected float m_skinRightOff01 = 0f;
        [SerializeField, Range(0, 1)]
        protected float m_skinLeftOff01 = 0f;

        [SerializeField, Tooltip("Enables the pixel snap performed before rendering the object using the value of PixelToUnits")]
        protected bool m_pixelSnapEnabled = false;
        [SerializeField, Tooltip("How many pixels would equal 1 unit in the world space")]
        protected float m_pixelToUnits = 100f;        

        void Start()
        {
            SmartColliderCore.TryInitialize();
            m_rigidBody = GetComponent<Rigidbody>();
            m_rigidBody2D = GetComponent<Rigidbody2D>();
            m_prevPos = transform.position;
        }

        void OnEnable()
        {
            m_prevPos = transform.position;
            Camera.onPreCull += _OnPreCull;
            Camera.onPostRender += _OnPostRender;
        }

        void OnDisable()
        {
            Camera.onPreCull -= _OnPreCull;
            Camera.onPostRender -= _OnPostRender;
        }

        public void Reset()
        {
            ResetLayerCollisions();

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                float fDefaultSkingPercen = 0.1f;
                Size = spriteRenderer.sprite.bounds.size;
                SkinTopWidth = Size.y * fDefaultSkingPercen;
                SkinBottomWidth = Size.y * fDefaultSkingPercen;
                SkinLeftWidth = Size.x * fDefaultSkingPercen * .5f;
                SkinRightWidth = Size.x * fDefaultSkingPercen * .5f;
                Size -= Size * 2 * fDefaultSkingPercen;
                Center = spriteRenderer.sprite.bounds.center;
            }
        }

        bool m_preRenderPosSet = false;
        Vector3 m_preRenderPos;
        protected bool m_fixPositionBeforeRender = false;
        //NOTE: _OnPreCull could be called more than one time, without calling _OnPostRender. For example by selecting a Canvas Text object clicking in the Scene
        void _OnPreCull(Camera cam)
        {
            // NOTE: Physics are updated after FixedUpdate is called, like gravity. 
            // This is removing the modifications before rendering and restoring it after that.
            // NOTE2: changing position in onPreRender is not working, but it's working with onPreCull
            if (!m_preRenderPosSet)
            {
                m_preRenderPosSet = true;
                m_preRenderPos = transform.position;
            }
            bool isPaused = false;
#if UNITY_EDITOR
            // this check allow seeing the object moving when pause button is pressed
            isPaused = UnityEditor.EditorApplication.isPaused;
#endif
            if (m_fixPositionBeforeRender && !isPaused)
            {
                transform.position = m_prevPos;
                //Pixel Snap
                if (PixelSnapEnabled)
                {
                    transform.position = new Vector3(
                        Mathf.Round(transform.position.x * m_pixelToUnits) / m_pixelToUnits,
                        Mathf.Round(transform.position.y * m_pixelToUnits) / m_pixelToUnits,
                        Mathf.Round(transform.position.z * m_pixelToUnits) / m_pixelToUnits
                    );
                }
            }
        }

        void _OnPostRender(Camera cam)
        {
            transform.position = m_preRenderPos;
            m_preRenderPosSet = false;
        }

        private int m_lastFixedUpdateFrameCount;
        protected virtual void FixedUpdate()
        {
            //if (m_updateMode == eUpdateMode.OnFixedUpdate)
            {
                if (m_lastFixedUpdateFrameCount != Time.frameCount)
                {
                    //NOTE: maybe this should always be true, even if only FixedUpdate is called. The idea of this attribute was, allow moving the collider in the update
                    // call, but checking the collisions in the FixedUpdate, so FixedUpdate will set the calculated position before render only if a collision was made.
                    m_fixPositionBeforeRender = false;
                }
                m_lastFixedUpdateFrameCount = Time.frameCount;
                UpdateCollisions();
            }
        }

        protected virtual void Update()
        {
            if (m_updateMode == eUpdateMode.OnUpdate)
            {                
                //if (Time.frameCount != m_lastUpdateCollisionFrameCount)
                {
                    UpdateCollisions();
                }
                m_fixPositionBeforeRender = true;
            }
        }

        //NOTE: this avoid updating twice when FixedUpdate already update the collider, but calling it twice helps to avoid jittering when using a rigid body and gravity
        //private int m_lastUpdateCollisionFrameCount; 
        protected virtual void UpdateCollisions()
        {
            //m_lastUpdateCollisionFrameCount = Time.frameCount;
            ResolveCollisions();            
        }

        private Vector2 m_prevSize;
        private Vector3 m_prevCenter;
        void ResolveCollisions()
        {
            //fix: calling UpdateCheckPoints when Size or Center is being change through animation
            if (m_prevSize != m_size || m_prevCenter != m_center)
            {
                m_prevSize = m_size;
                m_prevCenter = m_center;
                UpdateCheckPoints();
            }

			m_instantVelocity = (transform.position - m_prevPos) / Time.deltaTime;

            if (m_movingPlatforms.Count > 0)
            {
                Vector3 vMovingPlarformDisp = Vector3.zero;
                for (int i = 0; i < m_movingPlatforms.Count; ++i )
                {
                    //NOTE: 
                    // - kvp.Key is the moving platform transform
                    // - kvp.Value is the InverseTransformPoint of the Smart Collider saved during previous FixedUpdate call
                    // This is adding the moving platform displacement, translation and scaling to the smart collider on it
                    KeyValuePair<Transform, Vector3> kvp = m_movingPlatforms[i];
                    if (kvp.Key != null)
                    {
                        vMovingPlarformDisp += kvp.Key.TransformPoint(kvp.Value) - m_prevPos;
                    }
                }
                vMovingPlarformDisp /= m_movingPlatforms.Count;
                m_movingPlatforms.Clear();
                transform.position += vMovingPlarformDisp;
            }

            /* NOTE: this code is buggy and affected by framerate, making the player to move down when it's over a platform, for example.
            Vector3 movingVect = (transform.position - m_prevPos);
            float movementDist = movingVect.magnitude;
            bool checkToContinue;
            int maxCycles = 5;
            do
            {
                DynamicCollisionPoint closestCollPoint = DoSolveDynamicCollisions();
                movementDist -= (transform.position - m_prevPos).magnitude;
                checkToContinue = (movementDist > 0.001f && closestCollPoint.distance != float.MaxValue && --maxCycles > 0);
                if (checkToContinue)
                {
                    Vector3 surfaceDir = Vector3.Cross(closestCollPoint.normal, transform.forward);
                    movingVect = Vector3.Project(movingVect, surfaceDir).normalized * movementDist;
                    m_prevPos = transform.position;
                    transform.position += movingVect;
                }
            }
            while (checkToContinue);*/

            DoSolveDynamicCollisions();
            DoSolveStaticCollisions();

            m_prevPos = transform.position;
            if( m_rigidBody != null )
            {
                m_prevVelocity = m_rigidBody.velocity;
            }
            else if (m_rigidBody2D != null)
            {
                m_prevVelocity = m_rigidBody2D.velocity;
            }
        }

        /// <summary>
        /// LayerCollision will be set with default values of Layer Collision Mask. 
        /// It will take the 3D or 2D settings depending on what rigid body the owner of this component have.
        /// </summary>
        public void ResetLayerCollisions()
        {
            SmartColliderCore.Initialize();
            if (GetComponent<Rigidbody>() != null)
            {
                LayerCollision = SmartColliderCore.GetCollisionLayerMask3D(gameObject.layer);
            }
            else
            {
                LayerCollision = SmartColliderCore.GetCollisionLayerMask2D(gameObject.layer);
            }
        }

        /// <summary>
        /// Use this method to move the object skipping collisions
        /// </summary>
        /// <param name="vPos"></param>
        public void TeleportTo(Vector3 vPos)
        {
            transform.position = vPos;
            m_prevPos = vPos;
            m_movingPlatforms.Clear();
        }

        [ContextMenu("UpdateCheckPoints")]
        /// <summary>
        /// Update the collision check points based on Center, Size and Skin data
        /// </summary>    
        public void UpdateCheckPoints()
        {
            m_vBottomCheckPoints.Clear();
            m_vTopCheckPoints.Clear();
            m_vLeftCheckPoints.Clear();
            m_vRightCheckPoints.Clear();

            m_skinTopRayContacts = Enumerable.Repeat(false, HSkinSubdivisions + 2).ToList();
            m_skinBottomRayContacts = Enumerable.Repeat(false, HSkinSubdivisions + 2).ToList();
            m_skinLeftRayContacts = Enumerable.Repeat(false, VSkinSubdivisions + 2).ToList();
            m_skinRightRayContacts = Enumerable.Repeat(false, VSkinSubdivisions + 2).ToList();

            // lateral movement
            {
                Vector3 vCheckPosRight = Center + (Vector3)Size / 2f; vCheckPosRight.y -= Size.y;
                Vector3 vCheckPosLeft = vCheckPosRight; vCheckPosLeft.x -= Size.x;

                m_vRightCheckPoints.Add(vCheckPosRight);
                m_vLeftCheckPoints.Add(vCheckPosLeft);
                float div = Size.y / (VSkinSubdivisions + 1);
                for (int i = 0; i <= VSkinSubdivisions; ++i)
                {
                    vCheckPosRight.y += div;
                    vCheckPosLeft.y += div;
                    m_vRightCheckPoints.Add(vCheckPosRight);
                    m_vLeftCheckPoints.Add(vCheckPosLeft);
                }
            }

            // vertical movement
            {
                Vector3 vCheckPosBottom = Center + (Vector3)Size / 2f; vCheckPosBottom.x -= Size.x;
                Vector3 vCheckPosTop = vCheckPosBottom; vCheckPosBottom.y -= Size.y;

                m_vTopCheckPoints.Add(vCheckPosTop);
                m_vBottomCheckPoints.Add(vCheckPosBottom);
                float div = Size.x / (HSkinSubdivisions + 1);
                for (int i = 0; i <= HSkinSubdivisions; ++i)
                {
                    vCheckPosTop.x += div;
                    vCheckPosBottom.x += div;
                    m_vTopCheckPoints.Add(vCheckPosTop);
                    m_vBottomCheckPoints.Add(vCheckPosBottom);
                }
            }
        }        

        /// <summary>
        /// Solve collisions of moving objects. 
        /// This is automatically called by FixedUpdate, but if you change the translation.position from code, 
        /// you have to call this method to be sure collisions are solved before rendering the object.
        /// </summary>
        protected DynamicCollisionPoint DoSolveDynamicCollisions()
        {
            Vector3 vDisp = (transform.position - m_prevPos);
            Vector3 vDir = vDisp.normalized;

            Vector3 vLocalDisp = transform.rotation != Quaternion.identity ? Quaternion.Inverse(transform.rotation) * vDisp : vDisp;
            //NOTE: One Way collisions will be checked separately
            LayerMask layerCollision = LayerCollision & ~(OneWayCollisionRight | OneWayCollisionLeft | OneWayCollisionUp | OneWayCollisionDown);
            bool flipX = transform.localScale.x < 0;
            bool flipY = transform.localScale.y < 0;
            List<Vector3> vRightCheckPoints = flipX ? m_vLeftCheckPoints : m_vRightCheckPoints;
            List<Vector3> vLeftCheckPoints = flipX ? m_vRightCheckPoints : m_vLeftCheckPoints;
            List<Vector3> vTopCheckPoints = flipY ? m_vBottomCheckPoints : m_vTopCheckPoints;
            List<Vector3> vBottomCheckPoints = flipY ? m_vTopCheckPoints : m_vBottomCheckPoints;
            DynamicCollisionPoint closestCollisionPoint = new DynamicCollisionPoint(float.MaxValue, Vector3.zero);

            // check for right movement
            //if (vLocalDisp.x >= 0.5f * SkinRightWidth * Mathf.Abs(transform.localScale.x))
            if (vLocalDisp.x >= Vector3.kEpsilon ) //NOTE: safer, but slower
            {
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vRightCheckPoints, vDisp, vDir, Vector3.zero, layerCollision));
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vRightCheckPoints, vDisp, vDir, transform.right, OneWayCollisionRight));
            }
            // check for left movement
            //else if (vLocalDisp.x <= -SkinLeftWidth * Mathf.Abs(transform.localScale.x))
            else if (vLocalDisp.x <= -Vector3.kEpsilon)
            {
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vLeftCheckPoints, vDisp, vDir, Vector3.zero, layerCollision));
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vLeftCheckPoints, vDisp, vDir, -transform.right, OneWayCollisionLeft));
            }

            // check for up movement
            //if (vLocalDisp.y >= SkinTopWidth * Mathf.Abs(transform.localScale.y))
            if (vLocalDisp.y >= Vector3.kEpsilon)
            {
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vTopCheckPoints, vDisp, vDir, Vector3.zero, layerCollision));
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vTopCheckPoints, vDisp, vDir, transform.up, OneWayCollisionUp));
            }
            // check for down movement
            //else if (vLocalDisp.y <= -SkinBottomWidth * Mathf.Abs(transform.localScale.y))
            else if (vLocalDisp.y <= -Vector3.kEpsilon)
            {
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vBottomCheckPoints, vDisp, vDir, Vector3.zero, layerCollision));
                closestCollisionPoint = DynamicCollisionPoint.Min(closestCollisionPoint, _DoSolveDynamicCollisionSide(vBottomCheckPoints, vDisp, vDir, -transform.up, OneWayCollisionDown));
            }

            // check if there was a collision            
            if (closestCollisionPoint.distance < float.MaxValue)
            {
                // new body
                Vector3 scaledSize = Vector3.Scale(Size, transform.localScale);
                DebugEx.DebugDrawRect(transform.transform.TransformPoint( Center - (Vector3)Size / 2f), new Rect(0f, 0f, scaledSize.x, scaledSize.y), Color.red, 0.5f);

                transform.position = m_prevPos + (closestCollisionPoint.distance - k_SkinMinWidth) * vDir; //NOTE: subtracting k_SkinMinWidth avoid precision errors
            }
            return closestCollisionPoint;
        }

        protected struct DynamicCollisionPoint
        {
            public DynamicCollisionPoint(float distance, Vector3 normal)
            {
                this.distance = distance;
                this.normal = normal;
            }
            public static DynamicCollisionPoint Min(DynamicCollisionPoint a, DynamicCollisionPoint b)
            {
                if (a.distance < b.distance) return a;
                else return b;
            }
            public float distance;
            public Vector3 normal;
        }
        protected DynamicCollisionPoint _DoSolveDynamicCollisionSide(IList<Vector3> vCheckPoints, Vector3 vDisp, Vector3 vDir, Vector3 vOneWayDir, LayerMask layerMask)
        {
            float collClosestDist = float.MaxValue;
            float dist = vDisp.magnitude;
            Vector3 vClosestHitNorm = Vector3.zero;
            Vector3 vClosestHit = Vector3.zero;
            for (int i = 0; i < vCheckPoints.Count; ++i)
            {
                Vector3 vCheckPos = vCheckPoints[i];

                Ray ray = new Ray(transform.TransformPoint(vCheckPos) - vDisp, vDir);
                //Debug.DrawRay(ray.origin, vDir * dist, Color.blue, 0.5f);
                // 3D
                if (EnableCollision3D)
                {
                    RaycastHit hitInfo = _GetClosestHitInfo(ray.origin, ray.direction, dist, layerMask);
                    if (hitInfo.collider != null && hitInfo.collider.gameObject != gameObject)
                    {
                        if (hitInfo.collider.isTrigger) //kept just in case, but not needed because _GetClosestHitInfo2D now skip trigger colliders
                        {
                            hitInfo.collider.gameObject.SendMessageUpwards(k_messageOnSmartTriggerStay2D, new SmartContactPoint(hitInfo.normal, this, hitInfo.point), SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            if (hitInfo.distance < collClosestDist)
                            {
                                collClosestDist = hitInfo.distance;
                                vClosestHit = hitInfo.point;
                                vClosestHitNorm = hitInfo.normal;
                            }
                        }
                    }
                }

                // 2D
                if (EnableCollision2D)
                {
                    RaycastHit2D hitInfo2D = _GetClosestHitInfo2D(ray.origin, ray.direction, dist, layerMask, true);
                    if (hitInfo2D.collider != null && hitInfo2D.collider.gameObject != gameObject)
                    {
                        if (hitInfo2D.collider.isTrigger) //kept just in case, but not needed because _GetClosestHitInfo2D now skip trigger colliders
                        {
                            hitInfo2D.collider.gameObject.SendMessageUpwards(k_messageOnSmartTriggerStay2D, new SmartContactPoint(hitInfo2D.normal, this, hitInfo2D.point), SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            if (hitInfo2D.distance < collClosestDist)
                            {
                                collClosestDist = hitInfo2D.distance;
                                vClosestHit = hitInfo2D.point;
                                vClosestHitNorm = hitInfo2D.normal;
                            }
                        }
                    }
                }
            }

            Debug.DrawRay(vClosestHit, vClosestHitNorm * 0.1f, Color.green, 0.5f);
            if (collClosestDist < float.MaxValue && vOneWayDir != Vector3.zero)
            {
                //Debug.Log(" Dot: " + Vector3.Dot(vClosestHitNorm, -vOneWayDir) + " Ang: " + Mathf.Acos( Vector3.Dot(vClosestHitNorm, -vOneWayDir) ) * Mathf.Rad2Deg);
                if (Vector3.Dot(vClosestHitNorm, -vOneWayDir) < k_OneSideSlopeNormThreshold)
                {
                    return new DynamicCollisionPoint(float.MaxValue, vClosestHitNorm);
                }
            }
            return new DynamicCollisionPoint(collClosestDist, vClosestHitNorm); ;
        }

        /// <summary>
        /// Solve the collisions based on skin of the smart collider for each side, so the smart collider will be separated the width of the skin
        /// </summary>
        /// <returns></returns>
        protected virtual bool DoSolveStaticCollisions()
        {
            Vector3 vPrevPos = transform.position;
            bool flipX = transform.localScale.x < 0;
            bool flipY = transform.localScale.y < 0;
            LayerMask layerCollision = LayerCollision & ~(OneWayCollisionRight | OneWayCollisionLeft | OneWayCollisionUp | OneWayCollisionDown);
            LayerMask layerMaskUp = layerCollision | (flipY ? OneWayCollisionDown : OneWayCollisionUp);
            LayerMask layerMaskDown = layerCollision | (flipY ? OneWayCollisionUp : OneWayCollisionDown);
            LayerMask layerMaskRight = layerCollision | (flipX ? OneWayCollisionLeft : OneWayCollisionRight);
            LayerMask layerMaskLeft = layerCollision | (flipX ? OneWayCollisionRight : OneWayCollisionLeft);

            Vector3 m_vSkinBottomOff = m_skinBottomOff01 == 0f ? Vector3.zero : new Vector3(0f, Size.y * m_skinBottomOff01);
            Vector3 m_vSkinTopOff = m_skinTopOff01 == 0f ? Vector3.zero : new Vector3(0f, -Size.y * m_skinTopOff01);
            Vector3 m_vSkinRightOff = m_skinRightOff01 == 0f ? Vector3.zero : new Vector3(-Size.x * m_skinRightOff01, 0f);
            Vector3 m_vSkinLeftOff = m_skinLeftOff01 == 0f ? Vector3.zero : new Vector3(Size.x * m_skinLeftOff01, 0f);

            bool isColliding = false;
            //NOTE: when scaling the object negatively, transform.right and transform.up remain in the same direction. This is used to fix that, so transform.right is opposite if fliX == true.
            Vector3 vScaledRight = transform.right * transform.localScale.x;
            Vector3 vScaledUp = transform.up * transform.localScale.y;


            Vector3 vBottomImpulse = _DoSolveStaticCollisionSide(m_vBottomCheckPoints, m_skinBottomRayContacts, SkinBottomWidth * -vScaledUp, m_vSkinBottomOff, layerMaskDown, MovingPlatformCollisionDown);
            float bottomImpulseDist = vBottomImpulse.magnitude;
            //Note: for top impulse, vBottomImpulse.y is added to SkinTopWidth, because we have to move this amount up as well
            Vector3 vTopImpulse = _DoSolveStaticCollisionSide(m_vTopCheckPoints, m_skinTopRayContacts, (SkinTopWidth + bottomImpulseDist) * vScaledUp, m_vSkinTopOff, layerMaskUp, MovingPlatformCollisionUp);
            float topImpulseDist = vTopImpulse.magnitude;
            // After moving down, bottom could be colliding if it was not colliding before
            if (topImpulseDist != 0 && bottomImpulseDist == 0)
            {
                vBottomImpulse = _DoSolveStaticCollisionSide(m_vBottomCheckPoints, m_skinBottomRayContacts, (SkinBottomWidth + topImpulseDist) * -vScaledUp, m_vSkinBottomOff, layerMaskDown, MovingPlatformCollisionDown);
                bottomImpulseDist = vBottomImpulse.magnitude;
            }
            //check if stuck ( NOTE: this happens if space between floor and ceil is high enough to contain body but not the skin )
            if (topImpulseDist != 0 && bottomImpulseDist != 0)
            {
                isColliding = true;
                // Try to scape moving back to previous position
                transform.position = m_prevPos;
                // this time there is no scape for being stuck
                vBottomImpulse = _DoSolveStaticCollisionSide(m_vBottomCheckPoints, m_skinBottomRayContacts, SkinBottomWidth * -vScaledUp, m_vSkinBottomOff, layerMaskDown, MovingPlatformCollisionDown);
                bottomImpulseDist = vBottomImpulse.magnitude;
                vTopImpulse = _DoSolveStaticCollisionSide(m_vTopCheckPoints, m_skinTopRayContacts, (SkinTopWidth + bottomImpulseDist) * vScaledUp, m_vSkinTopOff, layerMaskUp, MovingPlatformCollisionUp);
                topImpulseDist = vTopImpulse.magnitude;
            }

            Vector3 vLeftImpulse = _DoSolveStaticCollisionSide(m_vLeftCheckPoints, m_skinLeftRayContacts, SkinLeftWidth * -vScaledRight, m_vSkinLeftOff, layerMaskLeft, MovingPlatformCollisionLeft);
            float leftImpulseDist = vLeftImpulse.magnitude;
            Vector3 vRightImpulse = _DoSolveStaticCollisionSide(m_vRightCheckPoints, m_skinRightRayContacts, (SkinRightWidth + leftImpulseDist) * vScaledRight, m_vSkinRightOff, layerMaskRight, MovingPlatformCollisionRight);
            float rightImpulseDist = vRightImpulse.magnitude;
            // After moving left, right side could be colliding if it was not colliding before
            if (leftImpulseDist != 0 && rightImpulseDist == 0)
            {
                vLeftImpulse = _DoSolveStaticCollisionSide(m_vLeftCheckPoints, m_skinLeftRayContacts, (SkinLeftWidth + rightImpulseDist) * -vScaledRight, m_vSkinLeftOff, layerMaskLeft, MovingPlatformCollisionLeft);
                leftImpulseDist = vLeftImpulse.magnitude;
            }
            //check if stuck
            if (leftImpulseDist != 0 && rightImpulseDist != 0)
            {
                isColliding = true;
                // Try to scape moving back to previous position
                transform.position = m_prevPos;
                // this time there is no scape for being stuck
                vLeftImpulse = _DoSolveStaticCollisionSide(m_vLeftCheckPoints, m_skinLeftRayContacts, SkinLeftWidth * -vScaledRight, m_vSkinLeftOff, layerMaskLeft, MovingPlatformCollisionLeft);
                leftImpulseDist = vLeftImpulse.magnitude;
                vRightImpulse = _DoSolveStaticCollisionSide(m_vRightCheckPoints, m_skinRightRayContacts, (SkinRightWidth + leftImpulseDist) * vScaledRight, m_vSkinRightOff, layerMaskRight, MovingPlatformCollisionRight);
                rightImpulseDist = vRightImpulse.magnitude;
            }

            // 3D
            if (m_rigidBody != null)
            {
                Vector3 vFinalVeloc = m_rigidBody.velocity;
                //NOTE: vLocalVeloc is using m_relativeVelocity, not m_rigidBody.velocity, because m_relativeVelocity is the real displacement through time
                // ((Vector3)m_rigidBody2D.velocity - m_prevVelocity) is added because rigid body velocity could change between fixed update calls ( like calling AddForce)
                Vector3 vInstantVeloc = m_instantVelocity + (vFinalVeloc - m_prevVelocity);
                Vector3 vLocalVeloc = transform.rotation != Quaternion.identity ? Quaternion.Inverse(transform.rotation) * vInstantVeloc : vInstantVeloc;
                if (rightImpulseDist != 0 && vLocalVeloc.x > 0f || leftImpulseDist != 0 && vLocalVeloc.x < 0f)
                {
                    vFinalVeloc = Vector3.Project(vFinalVeloc, transform.up); // reset horizontal velocity
                }
                if (bottomImpulseDist != 0 && vLocalVeloc.y < 0f || topImpulseDist != 0 && vLocalVeloc.y > 0f)
                {
                    vFinalVeloc = Vector3.Project(vFinalVeloc, transform.right); // reset vertical velocity
                }
                m_rigidBody.velocity = vFinalVeloc;
            }

            // 2D
            if (m_rigidBody2D != null)
            {
                Vector3 vFinalVeloc = m_rigidBody2D.velocity;
                // NOTE: vLocalVeloc is using m_relativeVelocity, not m_rigidBody2D.velocity, because m_relativeVelocity is the real displacement through time
                // ((Vector3)m_rigidBody2D.velocity - m_prevVelocity) is added because rigid body velocity could change between fixed update calls ( like calling AddForce)
                Vector3 vInstantVeloc = m_instantVelocity + (vFinalVeloc - m_prevVelocity);
                Vector3 vLocalVeloc = transform.rotation != Quaternion.identity ? Quaternion.Inverse(transform.rotation) * vInstantVeloc : vInstantVeloc;
                if (rightImpulseDist != 0 && vLocalVeloc.x > 0f || leftImpulseDist != 0 && vLocalVeloc.x < 0f)
                {
                    vFinalVeloc = Vector3.Project(vFinalVeloc, transform.up); // reset horizontal velocity
                }
                if (bottomImpulseDist != 0 && vLocalVeloc.y < 0f || topImpulseDist != 0 && vLocalVeloc.y > 0f)
                {
                    vFinalVeloc = Vector3.Project(vFinalVeloc, transform.right); // reset vertical velocity
                }
                m_rigidBody2D.velocity = vFinalVeloc;
            }

            isColliding = isColliding || bottomImpulseDist != 0 || topImpulseDist != 0 || rightImpulseDist != 0 || leftImpulseDist != 0;
            bool isHStuck = (rightImpulseDist != 0 && leftImpulseDist != 0);
            bool isVStuck = (bottomImpulseDist != 0 && topImpulseDist != 0);

            if (isColliding && !isHStuck && !isVStuck)
            {
                transform.position += vLeftImpulse + vRightImpulse + vTopImpulse + vBottomImpulse;
            }

            m_fixPositionBeforeRender |= isColliding;
            if (isColliding && OnCollision != null)
            {
                OnCollision(transform.position - vPrevPos, isHStuck, isVStuck);
            }

            // Now the transform has been updated, get the inverse position for all registered moving platforms
            for (int i = 0; i < m_movingPlatforms.Count; ++i )
            {
                m_movingPlatforms[i] = new KeyValuePair<Transform,Vector3>( m_movingPlatforms[i].Key, m_movingPlatforms[i].Key.InverseTransformPoint(transform.position));
            }

            return isColliding;
        }
        
        protected Vector3 _DoSolveStaticCollisionSide(IList<Vector3> vCheckPoints, IList<bool> vRayContacts, Vector3 vSkin, Vector3 vOffset, LayerMask layerMask, LayerMask movingPlatformLayerMask)
        {
            float collClosestDist = float.MaxValue;
            float skinMagnitude = vSkin.magnitude;
            Vector3 vClosestHitNorm = Vector3.zero;
            Vector3 vClosestHit = Vector3.zero;
            GameObject collidedObject = null;
            Vector3 vAppliedForce = Vector3.Project(m_instantVelocity, vSkin) / vCheckPoints.Count;
            float fOffsetDist = Vector3.Scale(vOffset, transform.localScale).magnitude;
            LayerMask oneWayLayerMask = (OneWayCollisionDown | OneWayCollisionUp | OneWayCollisionLeft | OneWayCollisionRight);
            for (int i = 0; i < vCheckPoints.Count; ++i)
            {
                vRayContacts[i] = false;
                Vector3 vCheckPos = vCheckPoints[i];
                Ray ray = new Ray(transform.TransformPoint(vCheckPos + vOffset), vSkin);
                Debug.DrawRay(ray.origin, vSkin.normalized * (skinMagnitude + fOffsetDist), Color.magenta);

                // 3D
                if (EnableCollision3D)
                {
                    RaycastHit hitInfo = _GetClosestHitInfo(ray.origin, ray.direction, skinMagnitude + fOffsetDist, layerMask);
                    if (hitInfo.collider != null && hitInfo.collider.gameObject != gameObject)
                    {
                        if (hitInfo.collider.isTrigger)
                        {
                            hitInfo.collider.gameObject.SendMessageUpwards(k_messageOnSmartTriggerStay2D, new SmartContactPoint(hitInfo.normal, this, hitInfo.point), SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            vRayContacts[i] = true;

                            DebugEx.DebugDrawDot(hitInfo.point + new Vector3(0, 0, -0.1f), 0.01f, Color.red);

                            if ((movingPlatformLayerMask & (1 << hitInfo.collider.gameObject.layer)) != 0)
                            {
                                m_movingPlatforms.Add(new KeyValuePair<Transform, Vector3>(hitInfo.transform, Vector3.zero)); //NOTE: the kvp.Value will be set at the end of DoSolveStaticCollisions
                            }

                            // This is pushing the collided object if it has a rigid body attached
                            if (hitInfo.rigidbody != null)
                            {
                                hitInfo.rigidbody.AddForceAtPosition(vAppliedForce, hitInfo.point, ForceMode.Force);
                            }

                            if ( hitInfo.distance < collClosestDist &&
                                // avoid collision when slope is higher than threshold and collider object is one way
                                ((0 == ((1 << hitInfo.collider.gameObject.layer) & oneWayLayerMask)) || Vector3.Dot(hitInfo.normal, -vSkin.normalized) >= k_OneSideSlopeNormThreshold)
                            )
                            {
                                collClosestDist = hitInfo.distance;
                                vClosestHit = hitInfo.point;
                                vClosestHitNorm = hitInfo.normal;
                                collidedObject = hitInfo.collider.gameObject;
                            }
                        }
                    }
                }

                // 2D
                if (EnableCollision2D)
                {
                    RaycastHit2D hitInfo2D = _GetClosestHitInfo2D(ray.origin, ray.direction, skinMagnitude + fOffsetDist, layerMask, true);
                    if (hitInfo2D.collider != null && hitInfo2D.collider.gameObject != gameObject)
                    {
                        if (hitInfo2D.collider.isTrigger)
                        {
                            hitInfo2D.collider.gameObject.SendMessageUpwards(k_messageOnSmartTriggerStay2D, new SmartContactPoint(hitInfo2D.normal, this, hitInfo2D.point), SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            vRayContacts[i] = true;

                            DebugEx.DebugDrawDot((Vector3)hitInfo2D.point + new Vector3(0, 0, transform.position.z - 0.1f), 0.01f, Color.red);

                            if ((movingPlatformLayerMask & (1 << hitInfo2D.collider.gameObject.layer)) != 0)
                            {
                                m_movingPlatforms.Add(new KeyValuePair<Transform, Vector3>(hitInfo2D.transform, Vector3.zero)); //NOTE: the kvp.Value will be set at the end of DoSolveStaticCollisions
                            }

                            // This is pushing the collided object if it has a rigid body attached
                            if (hitInfo2D.rigidbody != null)
                            {
                                hitInfo2D.rigidbody.AddForceAtPosition(vAppliedForce, hitInfo2D.point, ForceMode2D.Force);
                            }

                            if ( hitInfo2D.distance < collClosestDist &&
                                // avoid collision when slope is higher than threshold and collider object is one way
                                ((0 == ((1 << hitInfo2D.collider.gameObject.layer) & oneWayLayerMask)) || Vector3.Dot(hitInfo2D.normal, -vSkin.normalized) >= k_OneSideSlopeNormThreshold)
                            )
                            {
                                collClosestDist = hitInfo2D.distance;
                                vClosestHit = hitInfo2D.point;
                                vClosestHitNorm = hitInfo2D.normal;
                                collidedObject = hitInfo2D.collider.gameObject;
                            }
                        }
                    }
                }                
            }

            // check if there was a collision
            if (collClosestDist < float.MaxValue)
            {
                //Debug.DrawRay(vClosestHit, vClosestHitNorm * 0.1f, Color.yellow, 0.5f);                
                float fImpulseDist = (skinMagnitude + fOffsetDist) - collClosestDist + k_colliderSeparation;
                Vector3 vImpulse = -vSkin.normalized * fImpulseDist; //TODO: Add smooth factor by multiplying vImpulse by this factor ( make climb steps smoother )

                // TODO: if this is slow because of the gravity, a collision is going to be made each fixedUpdate, improve by keeping a list of the hit objects
                // so this message is only sent once until there is no collision for one update and then call the OnSmartCollisionExit2D            
                SmartCollision2D smartCollision2D = new SmartCollision2D(
                    this,
                    new SmartContactPoint[] { new SmartContactPoint(vClosestHitNorm, this, vClosestHit) },
                    gameObject,
                    vImpulse,
                    m_instantVelocity,
                    transform,
                    m_rigidBody,
                    m_rigidBody2D);
                collidedObject.SendMessageUpwards(k_messageOnSmartCollisionStay2D, smartCollision2D, SendMessageOptions.DontRequireReceiver);

                if (OnSideCollision != null)
                {
                    OnSideCollision(smartCollision2D, collidedObject);
                }

                return vImpulse;
            }

            return Vector3.zero;
        }
    }
}