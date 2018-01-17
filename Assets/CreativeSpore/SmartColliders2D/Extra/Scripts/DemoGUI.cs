using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DemoGUI : MonoBehaviour 
{

    public float GlobalPixelCameraZoom = 4f;
    public string[] Scenes;

    private ComboBox m_comboBox;

#if UNITY_EDITOR
    private string[] ReadNames()
    {
        List<string> temp = new List<string>();
        foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
        {
            if (S.enabled)
            {
                string name = S.path.Substring(S.path.LastIndexOf('/') + 1);
                name = name.Substring(0, name.Length - 6);
                temp.Add(name);
            }
        }
        return temp.ToArray();
    }

    private void Reset()
    {
        Scenes = ReadNames();
    }

    [MenuItem("Smart2DColliders/Samples/Setup Project")]
    private static void ConfigureProjectForDemo()
    {
        if(
            EditorUtility.DisplayDialog("Setup Project?",
			"Are you sure you want to change project settings to fit samples? This will change Physics2D and Layers settings", "Yes", "No") 
        )
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty serializedLayers = tagManager.FindProperty("layers");
            serializedLayers.GetArrayElementAtIndex(8).stringValue = "Wall";
            serializedLayers.GetArrayElementAtIndex(9).stringValue = "PassThrough";
            serializedLayers.GetArrayElementAtIndex(10).stringValue = "OneWayUp";
            serializedLayers.GetArrayElementAtIndex(11).stringValue = "OneWayRight";
            serializedLayers.GetArrayElementAtIndex(12).stringValue = "OneWayLeft";
            serializedLayers.GetArrayElementAtIndex(13).stringValue = "OneWayDown";
            serializedLayers.GetArrayElementAtIndex(14).stringValue = "Climbing";        
            tagManager.ApplyModifiedProperties();

            SerializedObject physicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Physics2DSettings.asset")[0]);
            SerializedProperty serializedPhy2DSettings = physicsSettings.FindProperty("m_LayerCollisionMatrix");
            for (int i = 0; i < serializedPhy2DSettings.arraySize; ++i )
            {
                SerializedProperty layerCollProp = serializedPhy2DSettings.GetArrayElementAtIndex(i);
                if(i == 0 || i == 9)
                {
                    layerCollProp.longValue = unchecked((uint)-8193);
                }
                else if( i == 13 )
                {
                    layerCollProp.longValue = unchecked((uint)-514);
                }
                else
                {
                    layerCollProp.longValue = unchecked((uint)-1);
                }
                //Debug.Log(layerCollProp.name + "  " + layerCollProp.displayName + " " + layerCollProp.intValue);
            }
            physicsSettings.ApplyModifiedProperties();
        }
     }
#endif

    float m_timer = 1f;
    float m_savedFrames = 0f;
    float m_savedStartFrames = 0f;
    float m_frameCount = 0f;
    float m_fps = 0f;

    void Start()
    {
        m_savedStartFrames = Time.frameCount;
        FindObjectOfType<PixelPerfectCameraCtrl>().Zoom = GlobalPixelCameraZoom;
    }

    void Update()
    {
        m_frameCount = Time.frameCount - m_savedStartFrames;

        m_timer -= Time.deltaTime;
        if (m_timer <= 0f)
        {
            m_timer += 1;
            m_fps = Time.frameCount - m_savedFrames;
            m_savedFrames = Time.frameCount;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadLevel(GetLoadedLevelId());
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            FollowController cameraFollowCtrl = Camera.main.GetComponent<FollowController>();
            if(cameraFollowCtrl != null)
            {
                cameraFollowCtrl.ApplyTargetRotation = !cameraFollowCtrl.ApplyTargetRotation;
            }
        }

        
        if (Input.GetKeyDown(KeyCode.G) && GetLoadedLevelName().Contains("Galaxy") )
        {
            StartCoroutine(GalaxyWorldView());
        }
    }

    string GetLoadedLevelName()
    {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
#else
        return Application.loadedLevelName;
#endif
    }

    int GetLoadedLevelId()
    {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
#else
        return Application.loadedLevel;
#endif
    }

    void LoadLevel( int idx )
    {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.LoadScene(idx);        
#else
        Application.LoadLevel(idx);
#endif
    }

    void LoadLevel(string name)
    {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
#else
        Application.LoadLevel(name);
#endif
    }

    IEnumerator GalaxyWorldView()
    {
        PixelPerfectCameraCtrl cameraCtrl = FindObjectOfType<PixelPerfectCameraCtrl>();
        FollowController camFollowCtrl = cameraCtrl.GetComponent<FollowController>();

        camFollowCtrl.Target = null;//GameObject.Find("Planet (2)").transform;
        camFollowCtrl.DampTime = 3f;
        camFollowCtrl.ApplyTargetRotation = false;
        camFollowCtrl.RotationDampTime = 0.001f;

        float oriZoom = cameraCtrl.Zoom;
        float t = 0f;
        for (float zoom = cameraCtrl.Zoom; zoom >= oriZoom / 5; zoom -= 0.002f, t += 0.000005f)
        {
            Vector3 vTarget = Vector3.zero;
            PlanetBehaviour[] aPlanets = FindObjectsOfType<PlanetBehaviour>();
            for (int i = 0; i < aPlanets.Length; ++i)
            {
                vTarget += aPlanets[i].transform.position;
            }
            vTarget /= aPlanets.Length;
            vTarget.z = cameraCtrl.transform.position.z;

            cameraCtrl.transform.position = Vector3.Slerp(cameraCtrl.transform.position, vTarget, t);
            cameraCtrl.Zoom = zoom;
            yield return null;
        }

        yield return null;
    }

    private GUIStyle m_textStyle;
    void OnGUI()
    {

        if (m_comboBox == null)
        {
            int fontSize = 30;
            GUIStyle listStyle = new GUIStyle();
            listStyle.normal.textColor = Color.grey;
            listStyle.onHover.background =
            listStyle.hover.background = new Texture2D(2, 2);
            listStyle.padding.left =
            listStyle.padding.right =
            listStyle.padding.top =
            listStyle.padding.bottom = 4;
            listStyle.fontSize = fontSize;

            int selectedIdx = 0;
            for (int i = 0; i < Scenes.Length; ++i)
            {
                if (GetLoadedLevelName() == Scenes[i])
                {
                    selectedIdx = i;
                }
            }   

            m_comboBox = new ComboBox(new Rect(300f, 0f, 550f, 50f), selectedIdx, Scenes, listStyle);
            m_comboBox.buttonStyle.fontSize = fontSize;
            m_comboBox.boxStyle.fontSize = fontSize;

            m_textStyle = new GUIStyle("label");
            m_textStyle.fontSize = fontSize;
        }

        if( GUILayout.Button("Reset", m_comboBox.buttonStyle, GUILayout.Width(200)) )
        {
            LoadLevel( GetLoadedLevelId() );
        }

        GUILayout.Label("FPS: " + Mathf.RoundToInt(m_fps), m_textStyle, GUILayout.Width(400));
        GUILayout.Label("FPS (Avg): " + Mathf.RoundToInt(m_frameCount / Time.timeSinceLevelLoad), m_textStyle, GUILayout.Width(400));

        string sLevel = Scenes[m_comboBox.Show()];
        if( sLevel != GetLoadedLevelName() )
        {
            LoadLevel( sLevel );
        }
    }
}
