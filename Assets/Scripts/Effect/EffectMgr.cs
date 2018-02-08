using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

public enum eEffectType
{
	//注意::只能在后面加枚举,不能在前面插入枚举值
    /*
	[Description("Effects/None")]
	None,
    */
	[Description("Effects/Birth")]
	Birth,//出生特效
    [Description("Effects/Boom")]
    Boom,//爆炸特效
    Max
}

public class EffectMgr : Template.MonoSingleton<EffectMgr>
{
	private static string g_EffectPoolName = "Effect";
	private Dictionary<eEffectType, Transform> mDelayRecycleEffcet = new Dictionary<eEffectType, Transform> ();
	private Dictionary<eEffectType, float> mEffectDuration = new Dictionary<eEffectType, float>();
	public Dictionary<eEffectType, float> mEffectLastPlayTime = new Dictionary<eEffectType, float>();

	public virtual IEnumerator LoadData()
	{
		LoadEffectToPool();
		yield return null;
	}

	public void Reset ()
	{
		foreach (eEffectType key in mDelayRecycleEffcet.Keys)
		{
			DespawnEffect (key, mDelayRecycleEffcet [key]);
		}

		mDelayRecycleEffcet.Clear ();
		ResetPool ();
	}

	public float GetEffectLastPlayTime(eEffectType type)
	{
		if (mEffectLastPlayTime.ContainsKey(type))
		{
			return mEffectLastPlayTime[type];
		}
		return 0f;
	}

	private void ResetEffect (Transform trans)
	{
		ParticleSystem[] particles = trans.GetComponentsInChildren<ParticleSystem> ();
		int count = particles.Length;
		for (int i = 0; i < count; ++i)
		{
			particles [i].Stop(true);
			particles [i].Clear(true);
		}
	}

	public Transform GetEfffectTrans(eEffectType type) 
	{
		Transform existEffect = null;
		if (!mDelayRecycleEffcet.TryGetValue(type, out existEffect))
		{
			Debug.LogError("不存在特效 effect = " + type);
		}

		return existEffect;
	}

	public Transform CreateEffect(eEffectType type, Transform parent = null, float recycleTime = 0f, Vector3 localOffset = default(Vector3), Quaternion localRotation = default(Quaternion))
	{
		Transform existEffect = null;
		if (recycleTime <= 0f && mDelayRecycleEffcet.TryGetValue(type, out existEffect))
		{
			Debug.LogError("已经存在特效 effect = " + type);
			existEffect.localPosition = localOffset == default(Vector3) ? Vector3.zero : localOffset;
			existEffect.localRotation = localRotation == default(Quaternion) ? Quaternion.identity : localRotation;

			return existEffect;
		}

		return IgnoreExitCreateEffect(type, parent, recycleTime, localOffset, localRotation);
	}

	public Transform IgnoreExitCreateEffect (eEffectType type, Transform parent = null, float recycleTime = 0f, Vector3 localOffset = default(Vector3), Quaternion localRotation = default(Quaternion))
	{

		SpawnPool pool = EffectMgr.GetPool ();

		localOffset = localOffset == default(Vector3) ? Vector3.zero : localOffset;
		Vector3 creatPos = parent == null ? localOffset : parent.TransformPoint(localOffset);

		if(pool._perPrefabPoolOptions.Count<=(int)type)
		{
			Debug.LogError("生成特效失败，type:" + type);
			return null;
		}

		Transform trans = pool.Spawn(pool._perPrefabPoolOptions[(int)type].prefab, creatPos, Quaternion.identity);
		trans.parent = parent;
		if (parent != null)
		{

			SetLayer (trans.gameObject, parent.gameObject.layer);
		}
		trans.localRotation = localRotation == default(Quaternion) ? Quaternion.identity : localRotation;
		trans.localScale = Vector3.one;

		// ResetEffect(trans);
		if (recycleTime > 0f)
		{
			if (mEffectLastPlayTime != null)
			{
				if (mEffectLastPlayTime.ContainsKey(type))
				{
					mEffectLastPlayTime[type] = Time.time;
				}
				else
				{
					mEffectLastPlayTime.Add(type, Time.time);
				}
			}
			StartCoroutine (DespawnEffect (type, trans, recycleTime));
		}
		else
		{
			if (!mDelayRecycleEffcet.ContainsKey(type))
			{
				mDelayRecycleEffcet.Add(type, trans);
			}
		}

		return trans;
	}

	private IEnumerator DespawnEffect (eEffectType type, Transform trans, float waitTime)
	{
		yield return new WaitForSeconds (waitTime);

		DespawnEffect (type, trans);
	}

	public void DespawnEffect (eEffectType type, Transform trans)
	{
		if(trans == null)
		{
			return;
		}
		SpawnPool pool = EffectMgr.GetPool ();
		ResetEffect (trans);
		pool.Despawn (trans, pool._perPrefabPoolOptions [(int)type]);
		trans.parent = pool.transform;

		if (mEffectLastPlayTime.ContainsKey(type))
		{
			mEffectLastPlayTime.Remove(type);
		}
	}

	public void RecycleEffect (eEffectType type)
	{
		SpawnPool pool = EffectMgr.GetPool ();

		Transform existEffect = null;
		if (mDelayRecycleEffcet.TryGetValue (type, out existEffect))
		{
			DespawnEffect (type, existEffect);

			mDelayRecycleEffcet.Remove (type);
		}
		else
		{
			Debug.LogError("不存在特效 effect = " + type);
		}
	}

	#region 加载特效池
	private static SpawnPool GetPool ()
	{
		SpawnPool Pool = null;
		if (!PoolManager.Pools.TryGetValue (EffectMgr.g_EffectPoolName, out Pool))
		{
			Pool = PoolManager.Pools.Create (EffectMgr.g_EffectPoolName);
		}

		return Pool;
	}

	private void InsertPool (SpawnPool pool, GameObject obj)
	{
		PrefabPool prefabPool = new PrefabPool (obj.transform);

		prefabPool.preloadAmount = 1;
		prefabPool.limitAmount = 1;

		pool._perPrefabPoolOptions.Add (prefabPool);
		pool.CreatePrefabPool (prefabPool);
	}

	private void LoadEffectToPool ()
	{
		SpawnPool pool = EffectMgr.GetPool ();
		pool.transform.parent = transform;
		if (mEffectDuration == null)
		{
			mEffectDuration = new Dictionary<eEffectType, float>();
		}

		string strPath = "";
		for (int i = 0; i < (int)eEffectType.Max; ++i)
		{
			strPath = GetEnumDes ((eEffectType)i);
			UnityEngine.Object obj = Resources.Load(strPath);
			if (obj != null) 
			{
				GameObject go = obj as GameObject;
				InsertPool(pool, go);
				float dLastTime = 0f;
				ParticleSystem[] psArray = go.GetComponentsInChildren<ParticleSystem>();
				for (int j = 0; j < psArray.Length; j++)
				{
					dLastTime = Mathf.Max(dLastTime, psArray[j].startLifetime);
				}
				mEffectDuration.Add((eEffectType)i, dLastTime);
			}
			else
			{
				Debug.LogError(" 路径没有特效文件 path:" + strPath);
			}
		}
	}

	public static string GetEnumDes(Enum en)
	{
		Type type = en.GetType();
		MemberInfo[] memInfo = type.GetMember(en.ToString());

		if (memInfo != null && memInfo.Length > 0)
		{
			object[] attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

			if (attrs != null && attrs.Length > 0)
				return ((DescriptionAttribute)attrs[0]).Description;
		}

		return en.ToString();
	}

	public float GetEffectLastTime(eEffectType type)
	{
		if (mEffectDuration != null && mEffectDuration.ContainsKey(type))
		{
			return mEffectDuration[type];
		}

		return 1f;
	}

	public void ResetPool ()
	{
		SpawnPool pool = GetPool ();

		int count = pool._perPrefabPoolOptions.Count;
		for (int i = 0; i < count; ++i)
		{
			pool._perPrefabPoolOptions [i].ClearExcessPrefab ();
		}

		System.GC.Collect ();
	}

	static public void SetLayer(GameObject go, int layer)
	{
		go.layer = layer;

		Transform t = go.transform;

		for (int i = 0, imax = t.childCount; i < imax; ++i)
		{
			Transform child = t.GetChild(i);
			SetLayer(child.gameObject, layer);
		}
	}
	#endregion

}
