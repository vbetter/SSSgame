using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {

    static public readonly string Layer_Platform = "Platform";  //平台,跳跃可穿过，可站立之上
    static public readonly string Layer_Actor = "Actor";        //角色
    static public readonly string Layer_Wall = "Wall";        //墙，不可穿越


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static public KeyCode GetKeyCodeByPlayer(string key,int p)
    {
        KeyCode KeyCode = KeyCode.None;

        switch (key)
        {
            case "Jump":
                if(p ==1)
                {
                    KeyCode = KeyCode.Joystick1Button3;
                }
                else if(p ==2)
                {
                    KeyCode = KeyCode.Joystick2Button3;
                }
                else if (p == 3)
                {
                    KeyCode = KeyCode.Joystick3Button3;
                }
                break;
            case "Fire1":
                if (p == 1)
                {
                    KeyCode = KeyCode.Joystick1Button2;
                }
                else if (p == 2)
                {
                    KeyCode = KeyCode.Joystick2Button2;
                }
                else if (p == 3)
                {
                    KeyCode = KeyCode.Joystick3Button2;
                }
                break;
            case "Fire2":
                if (p == 1)
                {
                    KeyCode = KeyCode.Joystick1Button1;
                }
                else if (p == 2)
                {
                    KeyCode = KeyCode.Joystick2Button1;
                }
                else if (p == 3)
                {
                    KeyCode = KeyCode.Joystick3Button1;
                }
                break;
            case "Fire3":
                if (p == 1)
                {
                    KeyCode = KeyCode.Joystick1Button0;
                }
                else if (p == 2)
                {
                    KeyCode = KeyCode.Joystick2Button0;
                }
                else if (p == 3)
                {
                    KeyCode = KeyCode.Joystick3Button0;
                }
                break;
            default:
                break;
        }

        return KeyCode;
    }
}
