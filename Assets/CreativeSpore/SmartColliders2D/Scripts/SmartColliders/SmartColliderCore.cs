using UnityEngine;
using System.Collections;

namespace CreativeSpore.SmartColliders
{
    public static class SmartColliderCore
    {
        static bool s_isInitialized = false;
        static LayerMask[] s_layerCollision3DMask;
        static LayerMask[] s_layerCollision2DMask;

        /// <summary>
        /// Gets the layers mask for a layer ( see PhysicManager\Layer Collision Matrix  )
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static LayerMask GetCollisionLayerMask3D(int layer)
        {
            return s_layerCollision3DMask[layer];
        }

        /// <summary>
        /// Gets the layers mask for a layer ( see Physics2DSettings\Layer Collision Matrix  )
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static LayerMask GetCollisionLayerMask2D(int layer)
        {
            return s_layerCollision2DMask[layer];
        }

        public static void TryInitialize()
        {
            if (!s_isInitialized)
            {
                s_isInitialized = true;
                Initialize();
            }
        }

        public static void Initialize()
        {
            s_layerCollision3DMask = new LayerMask[32];
            for (int i = 0; i < 32; ++i)
            {
                for (int j = 0; j < 32; ++j)
                {
                    bool isIgnored = Physics.GetIgnoreLayerCollision(i, j);
                    if (!isIgnored)
                    {
                        s_layerCollision3DMask[i].value = s_layerCollision3DMask[i].value | (1 << j);
                    }
                }
            }

            s_layerCollision2DMask = new LayerMask[32];
            for (int i = 0; i < 32; ++i)
            {
                for (int j = 0; j < 32; ++j)
                {
                    bool isIgnored = Physics2D.GetIgnoreLayerCollision(i, j);
                    if (!isIgnored)
                    {
                        s_layerCollision2DMask[i].value = s_layerCollision2DMask[i].value | (1 << j);
                    }
                }
            }
        }
    }
}