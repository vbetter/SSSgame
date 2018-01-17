using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCameraCtrl : MonoBehaviour 
{
	public Camera Camera{ get; private set; }

    public float Zoom = 1f;
    public float PixelToUnits = 100f;
    public bool PixelPerfectEnabled = true;

    public Rect BoundingBox;
    public bool KeepInsideBoundingBox = false;

	// Use this for initialization
	void Start () 
	{
		Camera = GetComponent<Camera>();
	}

    void LateUpdate()
    {
        if (Camera != null)
        {
            if (KeepInsideBoundingBox)
            {
                DoKeepInsideBounds();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            if (Camera == null) Camera = GetComponent<Camera>();
            OnPreCull();
        }
    }
	
	Vector3 m_vCamRealPos;
    void OnPreCull() 
	{
        if (Camera != null)
        {
            if (KeepInsideBoundingBox)
            {
                DoKeepInsideBounds();
            }

            //Note: ViewCamera.orthographicSize is not a real zoom based on pixels. This is the formula to calculate the real zoom.
            Camera.orthographicSize = (Camera.pixelRect.height) / (2f * Zoom * PixelToUnits);

            m_vCamRealPos = Camera.transform.position;

            if (PixelPerfectEnabled)
            {
                Vector3 vPos = Camera.transform.position;
                float mod = (1f / (Zoom * PixelToUnits));
                float modX = vPos.x > 0 ? vPos.x % mod : -vPos.x % mod;
                float modY = vPos.y > 0 ? vPos.y % mod : -vPos.y % mod;
                vPos.x -= modX;
                vPos.y -= modY;

                Camera.transform.position = vPos;
            }
        }
	}

    void OnPostRender()
	{
        if (Camera != null)
        {
            Camera.transform.position = m_vCamRealPos;
        }
	}

    void DoKeepInsideBounds()
    {
        Rect rCamera = new Rect();
        rCamera.width = Camera.pixelRect.width / (PixelToUnits * Zoom);
        rCamera.height = Camera.pixelRect.height / (PixelToUnits * Zoom);
        rCamera.center = Camera.transform.position;

        Vector3 vOffset = Vector3.zero;

        float right = (rCamera.x < BoundingBox.x)? BoundingBox.x - rCamera.x : 0f;
        float left = (rCamera.xMax > BoundingBox.xMax)? BoundingBox.xMax - rCamera.xMax : 0f;
        float down = (rCamera.y < BoundingBox.y)? BoundingBox.y - rCamera.y : 0f;
        float up = (rCamera.yMax > BoundingBox.yMax)? BoundingBox.yMax - rCamera.yMax : 0f;

        Vector3 vCamPos = Camera.transform.position;
        vOffset.x = right + left;
        vOffset.y = up + down;
        vCamPos += vOffset;
        if (rCamera.width >= Mathf.Abs(BoundingBox.width)) vCamPos.x = BoundingBox.center.x;
        if (rCamera.height >= Mathf.Abs(BoundingBox.height)) vCamPos.y = BoundingBox.center.y;
        Camera.transform.position = vCamPos;
    }
}