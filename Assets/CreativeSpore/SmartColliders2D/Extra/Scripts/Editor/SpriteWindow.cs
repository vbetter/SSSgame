using UnityEngine;
using UnityEditor;
using System.Linq;

public class SpriteWindow : EditorWindow
{

    private GameObject m_selectedObj;
    private SpriteRenderer m_sprRenderer;
    private Sprite m_sprite;
    private TextureImporter m_sprTexImporter;    
    private SpriteRenderer m_newSpriteRenderer;

    private GUIStyle m_boxSkin;

    private Vector2 m_sprScrollView;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Smart2DColliders/Window/SpriteWindow")]
    static void Init()
    {
        // Gets existing open window or if none, make a new one:
        SpriteWindow window = (SpriteWindow)EditorWindow.GetWindow(typeof(SpriteWindow));
        window.Show();
        window.name = "Sprite Window";
    }

    void OnSelectionChange()
    {
        
    }

    void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        if(m_boxSkin == null)
        {
            m_boxSkin = new GUIStyle("Box");
            m_boxSkin.normal.background = new Texture2D(1, 1);
            m_boxSkin.normal.background.hideFlags = HideFlags.DontSave;
        }

        //EditorGUILayout.Foldout(false, "Selection Settings");

        // Check if sprite renderer has changed to update the sprite texture importer
        GameObject prevObj = m_selectedObj;
        m_selectedObj = Selection.activeGameObject;        
        if ( prevObj != m_selectedObj || m_sprRenderer == null )
        {            
            m_sprRenderer = m_selectedObj != null?  m_selectedObj.GetComponent<SpriteRenderer>() : null;
            m_sprite = null; 
            m_sprTexImporter = null;
        }

        // Check if sprite has changed to update m_sprTexImporter
        Sprite prevSprite = m_sprite;
        m_sprite = m_sprRenderer != null ? m_sprRenderer.sprite : null;
        if (prevSprite != m_sprite && m_sprite != null)
        {
            m_sprTexImporter = null;
            string spritePath = AssetDatabase.GetAssetPath(m_sprRenderer.sprite);
            if (!string.IsNullOrEmpty(spritePath))
            {
                m_sprTexImporter = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            }
        }

        if (m_sprRenderer == null)
        {
            EditorGUILayout.HelpBox("Select a sprite first", MessageType.Info);
        }

        // draw sprite sheet
        if (m_sprTexImporter != null && m_sprTexImporter.spritesheet != null)
        {
            EditorGUI.BeginChangeCheck();
            Color bgColor = EditorGUILayout.ColorField(m_boxSkin.normal.background.GetPixel(0, 0));
            if (EditorGUI.EndChangeCheck())
            {
                m_boxSkin.normal.background.SetPixel(0, 0, bgColor);
                m_boxSkin.normal.background.Apply();
            }

            m_sprScrollView = GUILayout.BeginScrollView(m_sprScrollView);
            {                
                GUILayout.Box(m_sprRenderer.sprite.texture, m_boxSkin);
                Rect rSprSheet = GUILayoutUtility.GetLastRect();
                rSprSheet.position += Vector2.one; // add the border pixel
                if (Event.current.button == 0 && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp))
                {
                    Vector2 mouseRelPos = Event.current.mousePosition - rSprSheet.position;
                    mouseRelPos.y = m_sprRenderer.sprite.texture.height - mouseRelPos.y;
                    for (int i = 0; i < m_sprTexImporter.spritesheet.Length; ++i)
                    {
                        SpriteMetaData sprMetaData = m_sprTexImporter.spritesheet[i];
                        if (sprMetaData.rect.Contains(mouseRelPos))
                        {
                            string spritePath = AssetDatabase.GetAssetPath(m_sprRenderer.sprite);
                            Sprite selSprite = (Sprite)AssetDatabase.LoadAllAssetsAtPath(spritePath).First(x => x is Sprite && x.name == sprMetaData.name);
                            if (Event.current.type == EventType.MouseUp)
                            {
                                if (selSprite != m_sprRenderer.sprite)
                                {
                                    Undo.RecordObject(m_sprRenderer, "Changed Sprite");
                                    EditorUtility.SetDirty(m_sprRenderer);
                                }
                                m_sprRenderer.sprite = selSprite;
                                // Apply to all selected sprites
                                for (int objIdx = 0; objIdx < Selection.gameObjects.Length; ++objIdx)
                                {
                                    SpriteRenderer sprRenderer = Selection.gameObjects[objIdx].GetComponent<SpriteRenderer>();
                                    if (sprRenderer != null && sprRenderer != m_sprRenderer)
                                    {
                                        if (sprRenderer.sprite != m_sprRenderer.sprite)
                                        {
                                            Undo.RecordObject(sprRenderer, "Changed Sprite");
                                            EditorUtility.SetDirty(sprRenderer);
                                        }
                                        sprRenderer.sprite = m_sprRenderer.sprite;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
        }
    }   
}