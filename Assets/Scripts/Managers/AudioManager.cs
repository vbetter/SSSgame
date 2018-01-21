using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SoundType
{
    BackgroundMusic = 1,//背景音乐
    GameEffectsMusic = 2,//游戏音效
    Max,
}

public enum Sound
{
    None = 0,
    BackgroundMusic = 1,        //游戏中的背景音乐
    ButtonClick,
    BG_Fight,
    Login,
    Max
}

public class AudioManager : Template.MonoSingleton<AudioManager>
{

    public Dictionary<Sound, AudioClip> dicSound = new Dictionary<Sound, AudioClip>();
    private  AudioSource BGMusicSource = null;
    private  List<AudioSource> gameEffectSourceList=new List<AudioSource> ();
    private List<AudioSource> gameEffectSourceList_3D = new List<AudioSource>();

    bool m_isInitDone = false;

    public override void Init()
    {
        base.Init();
        LoadData();

    }

    public virtual void LoadData()
    {
        PreloadSounds();


    }

    void PreloadSounds()     //加载声音
    {
        if (m_isInitDone)
            return;
        m_isInitDone = true;

        int count = (int)Sound.Max;
        for (int i = 1; i < count; ++i)
        {
            if (System.Enum.IsDefined(typeof(Sound),i))
            {
                Sound s = (Sound)i;
                string path = "Sound/" + s.ToString();
                AudioClip ac = Resources.Load(path) as AudioClip;
                if (ac == null)
                {
					Debug.LogError("===================路径中没有此声音文件  path=" + path);
                    continue;
                }
                dicSound.Add(s, ac);
            }
        }
    }

    Sound currentPlayingBGMusic = Sound.Max;
    public void PlayBackgroundMusic(Sound sound, bool isLoop = true, float volume = 0.4f, float pitch = 1.0f)    //播放背景音乐
    {
        if (sound == currentPlayingBGMusic) return;
        //=================================
        if (BGMusicSource == null) BGMusicSource = gameObject.AddComponent<AudioSource>();
        BGMusicSource.Stop();
        //=================================
        if (dicSound.ContainsKey(sound)) 
        {
            AudioClip audioClip = dicSound[sound];
            if (audioClip != null)
            {
                BGMusicSource.clip = audioClip;
                BGMusicSource.loop = isLoop;
                BGMusicSource.volume = volume;
                BGMusicSource.pitch = pitch;
                BGMusicSource.Play();
                currentPlayingBGMusic = sound;
            }
        }
        else 
        {
			Debug.LogError("===================路径中没有此声音文件  path=Sound/" + sound);
        }
    }

    public void PlayGameEffectsMusic(Sound sound, bool isLoop = false, float volume = 1.0f, float pitch = 1.0f, bool isRandom = false)//播放游戏音效
    {

        //=====================
        AudioSource gameEffectSource = null;
		for(int i = 0 ; i < gameEffectSourceList.Count ; i ++)
		{
			AudioSource item = gameEffectSourceList[i];
            if (!item.isPlaying)
            {
                gameEffectSource = item;
                break;
            }
        }
        if (gameEffectSource == null)
        {
            gameEffectSource = gameObject.AddComponent<AudioSource>();
            gameEffectSourceList.Add(gameEffectSource);
        }
        //=====================
        if (dicSound.ContainsKey(sound))
        {
            AudioClip audioClip = dicSound[sound];
            if (audioClip != null)
            {
                gameEffectSource.clip = audioClip;
                gameEffectSource.loop = isLoop;
                gameEffectSource.volume = volume;
                gameEffectSource.pitch = pitch;
                gameEffectSource.Play();
            }
        }
        else
        {
            Debug.LogError("===================路径中没有此声音文件  path=Sound/" + sound);
        }
    }

    public void PlayGameEffectsMusic(AudioSource _3dAudioSource ,Sound sound, bool isLoop = false, float volume = 1.0f, float pitch = 1.0f) 
    {
        if (dicSound.ContainsKey(sound))
        {
            AudioClip audioClip = dicSound[sound];
            if (audioClip != null)
            {
                _3dAudioSource.clip = audioClip;
                _3dAudioSource.loop = isLoop;
                _3dAudioSource.volume = volume;
                _3dAudioSource.pitch = pitch;
                _3dAudioSource.Play();
            }
            if (!gameEffectSourceList_3D.Contains(_3dAudioSource)) gameEffectSourceList_3D.Add(_3dAudioSource);
        }
    }

    /// <summary>
    /// 停止播放音乐
    /// </summary>
    /// <param name="type">停止播放背景音乐还是游戏音效</param>
    /// <param name="sound">停止的声音</param>
    public void StopMusic(Sound sound = Sound.Max, SoundType type = SoundType.GameEffectsMusic)
    {
        //停止背景音乐,只传第一个参数就够了
        if (type == SoundType.BackgroundMusic)
        {
            if (BGMusicSource == null) BGMusicSource = gameObject.AddComponent<AudioSource>();
            BGMusicSource.Stop();
            currentPlayingBGMusic = Sound.Max;
        }
        else if (type == SoundType.GameEffectsMusic)
        {
            //停止所有的游戏音效
            if (sound == Sound.Max)
            {
				for(int i = 0 ; i < gameEffectSourceList.Count ; i ++)
				{
					AudioSource item = gameEffectSourceList[i];
                    item.Stop();
                }
                return;
            }

            //停止指定的游戏音效
            if (dicSound.ContainsKey(sound))
            {
                AudioClip audioClip = dicSound[sound];
                if (audioClip != null)
                {
                    if (gameEffectSourceList != null && gameEffectSourceList.Count > 0)
                    {
						for(int i = 0 ; i < gameEffectSourceList.Count ; i ++)
						{
							AudioSource item = gameEffectSourceList[i];
                            if (item.isPlaying && item.clip == audioClip)
                            {
                                item.Stop();
                                return;
                            }
                        }
                    }

                    if (gameEffectSourceList_3D != null && gameEffectSourceList_3D.Count > 0)
                    {
                        for (int i = 0; i < gameEffectSourceList_3D.Count; ++i)
                        {
                            AudioSource item=gameEffectSourceList_3D[i];
                            if (item == null)
                            {
                                //gameEffectSourceList_3D.Remove(item);
                            }
                            else if (item.isPlaying && item.clip == audioClip)
                            {
                                item.Stop();
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetMusicState(SoundType type, bool isOpen)    // 设置声音状态
    {
       
    }

    public void Mute(bool isMute)    // 静音状态
    {
        if (BGMusicSource != null) BGMusicSource.mute = isMute;
        if (gameEffectSourceList != null && gameEffectSourceList.Count > 0) 
        {
			for(int i = 0 ; i < gameEffectSourceList.Count ; i ++)
			{
				AudioSource item = gameEffectSourceList[i];
                if (item != null) item.mute = isMute;
            }
        }
        if (gameEffectSourceList_3D != null && gameEffectSourceList_3D.Count > 0)
        {
            for (int i = 0; i < gameEffectSourceList_3D.Count; ++i)
            {
                AudioSource item = gameEffectSourceList_3D[i];
                if (item != null)item.mute = isMute;
            }
        }
	}

    public void Clear3DSound() 
    {
        if (gameEffectSourceList_3D != null && gameEffectSourceList_3D.Count > 0)  gameEffectSourceList_3D.Clear();
    }

}
