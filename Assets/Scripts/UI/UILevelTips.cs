using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eTipsType
{
    Level,
    Boss,
}

public struct LevelTipsStruct
{
    public eTipsType TipsType;
    public int LevelCount;
}

public class UILevelTips : MonoBehaviour {

    [SerializeField]
    UILabel m_labelTips;

	// Use this for initialization
	void Start () {
		
	}
	
	public void Show(LevelTipsStruct item)
    {
        gameObject.SetActive(true);

        switch (item.TipsType)
        {
            case eTipsType.Level:
                m_labelTips.text = string.Format("第{0}关", item.LevelCount);
                break;
            case eTipsType.Boss:
                break;
            default:
                break;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
