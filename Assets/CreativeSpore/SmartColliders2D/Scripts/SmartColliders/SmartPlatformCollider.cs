using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CreativeSpore.SmartColliders
{

    public class SmartPlatformCollider : SmartRectCollider2D
    {
        private bool m_isOnMovingPlatform = false;

        public bool IsGrounded()
        {
            if (m_isOnMovingPlatform) return true;


            float dist = SkinBottomWidth * transform.localScale.y + 10f * SmartPlatformCollider.k_SkinMinWidth;
            for (int i = 0; i < BottomCheckPoints.Count; ++i)
            {
                Vector3 start = transform.TransformPoint(BottomCheckPoints[i]);
                Vector3 dir = -transform.up;
                SmartRaycastHit smartHit = SmartRaycast(start, dir, dist, LayerCollision | OneWayCollisionDown);
                if (smartHit != null)
                {
                    return true;
                }
            }
            return false;
        }

        protected override void UpdateCollisions()
        {
            base.UpdateCollisions();
            m_isOnMovingPlatform = m_movingPlatforms.Count > 0;
        }

        /// <summary>
        /// Solve the collisions based on skin of the smart collider for each side, so the smart collider will be separated the width of the skin
        /// </summary>
        /// <returns></returns>
        protected override bool DoSolveStaticCollisions()
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

            isColliding = isColliding || bottomImpulseDist != 0 || topImpulseDist != 0;
            if (isColliding)
            {
                transform.position += vTopImpulse + vBottomImpulse;
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
                transform.position = vPrevPos + vLeftImpulse + vRightImpulse + vTopImpulse + vBottomImpulse;
            }

            m_fixPositionBeforeRender |= isColliding;
            if (isColliding && OnCollision != null)
            {
                OnCollision(transform.position - vPrevPos, isHStuck, isVStuck);
            }

            // Now the transform has been updated, get the inverse position for all registered moving platforms
            for (int i = 0; i < m_movingPlatforms.Count; ++i)
            {
                m_movingPlatforms[i] = new KeyValuePair<Transform, Vector3>(m_movingPlatforms[i].Key, m_movingPlatforms[i].Key.InverseTransformPoint(transform.position));
            }

            return isColliding;
        }
    }
}