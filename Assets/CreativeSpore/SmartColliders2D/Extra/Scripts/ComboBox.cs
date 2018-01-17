/* ref: http://wiki.unity3d.com/wiki/index.php?title=PopupList
 * 
// Popup list created by Eric Haines
// ComboBox Extended by Hyungseok Seo.(Jerry) sdragoon@nate.com
// Refactored by zhujiangbo jumbozhu@gmail.com
// Slight edit for button to show the previously selected item AndyMartin458 www.clubconsortya.blogspot.com
// 
// -----------------------------------------------
// This code working like ComboBox Control.
// I just changed some part of code, 
// because I want to seperate ComboBox button and List.
// ( You can see the result of this code from Description's last picture )
// -----------------------------------------------
//
// === usage ======================================
using UnityEngine;
using System.Collections;
 
public class ComboBoxTest : MonoBehaviour
{
	GUIContent[] comboBoxList;
	private ComboBox comboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();
 
	private void Start()
	{
		comboBoxList = new GUIContent[5];
		comboBoxList[0] = new GUIContent("Thing 1");
		comboBoxList[1] = new GUIContent("Thing 2");
		comboBoxList[2] = new GUIContent("Thing 3");
		comboBoxList[3] = new GUIContent("Thing 4");
		comboBoxList[4] = new GUIContent("Thing 5");
 
		listStyle.normal.textColor = Color.white; 
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.left =
		listStyle.padding.right =
		listStyle.padding.top =
		listStyle.padding.bottom = 4;
 
		comboBoxControl = new ComboBox(new Rect(50, 100, 100, 20), comboBoxList[0], comboBoxList, "button", "box", listStyle);
	}
 
	private void OnGUI () 
	{
		comboBoxControl.Show();
	}
}
 
*/

using UnityEngine;

public class ComboBox
{
    public Rect Rect;
    public bool IsDropDownListVisible = false;

    private static bool forceToUnShow = false;
    private static int useControlID = -1;
    private int selectedItemIndex = 0;

    private GUIContent buttonContent;
    private GUIContent[] listContent;
    public GUIStyle buttonStyle;
    public GUIStyle boxStyle;
    public GUIStyle listStyle;

    public ComboBox(Rect rect, int selectedIdx, string[] options, GUIStyle listStyle)
    {
        GUIContent[] listContent = new GUIContent[options.Length];
        for (int i = 0; i < options.Length; ++i)
        {
            listContent[i] = new GUIContent(options[i]);
        }

        this.Rect = rect;
        selectedItemIndex = selectedIdx;
        this.buttonContent = listContent[selectedIdx];
        this.listContent = listContent;
        this.buttonStyle = new GUIStyle("button");
        this.boxStyle = new GUIStyle("box");
        this.listStyle = listStyle;
    }

    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
    {
        this.Rect = rect;
        this.buttonContent = buttonContent;
        this.listContent = listContent;
        this.buttonStyle = new GUIStyle("button");
        this.boxStyle = new GUIStyle("box");
        this.listStyle = listStyle;
    }

    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, string boxStyle, GUIStyle listStyle)
    {
        this.Rect = rect;
        this.buttonContent = buttonContent;
        this.listContent = listContent;
        this.buttonStyle = buttonStyle;
        this.boxStyle = boxStyle;
        this.listStyle = listStyle;
    }

    public int Show()
    {
        if (forceToUnShow)
        {
            forceToUnShow = false;
            IsDropDownListVisible = false;
        }

        bool done = false;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.mouseUp:
                {
                    if (IsDropDownListVisible)
                    {
                        done = true;
                    }
                }
                break;
        }

        if (GUI.Button(Rect, buttonContent, buttonStyle))
        {
            if (useControlID == -1)
            {
                useControlID = controlID;
                IsDropDownListVisible = false;
            }

            if (useControlID != controlID)
            {
                forceToUnShow = true;
                useControlID = controlID;
            }
            IsDropDownListVisible = true;
        }

        if (IsDropDownListVisible)
        {
            Rect listRect = new Rect(Rect.x, Rect.y + listStyle.CalcHeight(listContent[0], 1.0f),
                      Rect.width, listStyle.CalcHeight(listContent[0], 1.0f) * listContent.Length);

            GUI.Box(listRect, "", boxStyle);
            int newSelectedItemIndex = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, listStyle);
            if (newSelectedItemIndex != selectedItemIndex)
            {
                SelectedItemIndex = newSelectedItemIndex;
            }
        }

        if (done)
            IsDropDownListVisible = false;

        return selectedItemIndex;
    }

    public int SelectedItemIndex
    {
        get
        {
            return selectedItemIndex;
        }
        set
        {
            selectedItemIndex = value;
            buttonContent = listContent[selectedItemIndex];
        }
    }
}