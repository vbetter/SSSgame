using UnityEngine;
using System.Collections;

public class DemoPirateShip : MonoBehaviour 
{
    public GameObject PirateShip;

    public float TimeScale = 1f;
    public float NoiseScale = 1f;
    public float SlerpTime = 0.2f;

	void FixedUpdate () 
    {
        float noise = Mathf.PerlinNoise(TimeScale * Time.timeSinceLevelLoad, 0f) - 0.5f; // noise = [-0.5, 0.5]

        float deg = noise * NoiseScale;
        PirateShip.transform.rotation = Quaternion.Slerp(PirateShip.transform.rotation, Quaternion.Euler(0f, 0f, deg), SlerpTime);
	}
}
