using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GOEngine
{
    public enum AudioType
    {
        AUDIO_TYPE_EFFECT = 10,
        AUDIO_TYPE_BGM = 20,
    }
}
namespace GOEngine.Implement
{
#if UNITY_EDITOR
	public
#else
    internal
#endif
 class GOEAudioMgr : IGOEAudioMgr
    {
        private List<AudioInfo> autoDestoryAudios = new List<AudioInfo>();
        private List<AudioInfo> needFadeAudios = new List<AudioInfo>();
        public AudioSource AddSound(string name, GameObject obj, bool loop = false, bool removeAtEnd = true,
            AudioType type = AudioType.AUDIO_TYPE_EFFECT, bool notFade = false)
        {
            AudioClipLoader loader = obj.AddComponent<AudioClipLoader>();
            loader.ResName = name;
            if (loader.AudioSource == null)
            {
                GameObject.DestroyImmediate(loader);
                return null;
            }
            loader.AudioSource.playOnAwake = !IsMute;
            loader.AudioSource.loop = loop;
            loader.AudioSource.volume = GetVolumeByType(type);
			loader.AudioSource.mute = loader.AudioSource.volume <= 0; 
            loader.LoadSound();
            if (!loop && removeAtEnd)
            {
                autoDestoryAudios.Add(new AudioInfo(loader.AudioSource));
            }
            else if (loop && !IsMute && !notFade)
            {
                AudioInfo info = new AudioInfo(loader.AudioSource);
                info.SetFadeIn();
                info.type = type;
                loader.AudioSource.volume = 0;
                addToFadePool(info);
            }
            return loader.AudioSource;
        }

        public static float GetVolumeByType(AudioType type)
        {
            switch (type)
            {
                case AudioType.AUDIO_TYPE_EFFECT:
                default:
                    return EngineDelegate.MaxSoundVolume;
                case AudioType.AUDIO_TYPE_BGM:
                    return EngineDelegate.MaxBGMVolume;
            }
        }
        public void Update()
        {
            for (int i = 0; i < autoDestoryAudios.Count; i++)
            {
                AudioInfo info = autoDestoryAudios[i];
                if (!info.audioSource)
                {
                    autoDestoryAudios.RemoveAt(i);
                    i--;
                    continue;
                }
                if (info.audioSource.clip == null)
                    continue;
                if (info.timeLeft == -1)
                    info.timeLeft = info.audioSource.clip.length;
                else
                {
                    info.timeLeft -= GOERoot.RealTime.DeltaTime;
                    if (info.timeLeft < 0)
                    {
                        RemoveSound(info.audioSource);
                        i--;
                    }
                }
            }
            for (int i = 0; i < needFadeAudios.Count; i++)
            {
                AudioInfo info = needFadeAudios[i];
                if (!info.audioSource)
                {
                    needFadeAudios.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!info.isFadeIn && info.audioSource.clip == null)
                {
                    RemoveSound(info.audioSource, false);
                    i--;
                    continue;
                }
                if (info.audioSource.clip == null || !info.audioSource.isPlaying)
                    continue;
                info.timeLeft -= GOERoot.RealTime.DeltaTime;
                if (info.timeLeft > 0)
                    info.SetVolume();
                else
                {
                    if (info.removeAtEnd)
                        RemoveSound(info.audioSource, false);
                    else
                    {
                        if (!info.isFadeIn)
                            info.audioSource.Stop();
                        needFadeAudios.RemoveAt(i);
                    }
                    i--;
                }
            }
        }

        private void addToFadePool(AudioInfo info)
        {
            foreach (AudioInfo ai in needFadeAudios)
            {
                if (ai.audioSource == info.audioSource)
                {
                    needFadeAudios.Remove(ai);
                    break;
                }
            }
            needFadeAudios.Add(info);
        }

        public void RemoveSound(AudioSource aus, bool needFade = true)
        {
            if (needFade && aus.loop && aus.isPlaying)
            {
                AudioInfo info = new AudioInfo(aus);
                info.type = AudioType.AUDIO_TYPE_BGM;
                info.SetFadeOut(true);
                addToFadePool(info);
                return;
            }
            AudioClipLoader loader = GetReleatedLoader(aus);
            if (loader != null)
                GameObject.Destroy(loader);
            GameObject.Destroy(aus);
            for (int i = 0; i < autoDestoryAudios.Count; i++)
            {
                AudioInfo info = autoDestoryAudios[i];
                if (info.audioSource == aus)
                {
                    autoDestoryAudios.RemoveAt(i);
                    return;
                }
            }
            for (int i = 0; i < needFadeAudios.Count; i++)
            {
                AudioInfo info = needFadeAudios[i];
                if (info.audioSource == aus)
                {
                    needFadeAudios.RemoveAt(i);
                    return;
                }
            }
        }


        public AudioClipLoader GetReleatedLoader(AudioSource audioScource)
        {
            if (audioScource.gameObject == null)
                return null;
            AudioClipLoader[] loaders = audioScource.gameObject.GetComponents<AudioClipLoader>();
            foreach (AudioClipLoader loader in loaders)
            {
                if (loader.AudioSource == audioScource)
                    return loader;
            }
            return null;
        }

        public bool IsMute = false;
        public void StopAudio(bool fade = true)
        {
            IsMute = true;
            AudioSource[] audios = GameObject.FindObjectsOfType<AudioSource>();
            foreach (AudioSource audioSource in audios)
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    if (audioSource.isPlaying)
                    {
                        if (audioSource.loop && fade)
                        {
                            AudioInfo info = new AudioInfo(audioSource);
                            info.SetFadeOut(false);
                            addToFadePool(info);
                        }
                        else
                            audioSource.Stop();
                    }
                }
                else
                    audioSource.playOnAwake = false;
            }
        }

        public void PlayAudio()
        {
            IsMute = false;
            AudioSource[] audios = GameObject.FindObjectsOfType<AudioSource>();
            foreach (AudioSource audioSource in audios)
            {
                if (audioSource.enabled)
                {
                    if (audioSource != null && audioSource.loop && audioSource.clip != null)
                    {
                        if (!audioSource.isPlaying)
                        {
                            AudioInfo info = new AudioInfo(audioSource);
                            info.SetFadeIn();
                            addToFadePool(info);
                            audioSource.Play();
                        }
                    }
                    else
                        audioSource.playOnAwake = true;
                }
            }
        }
    }

    class AudioInfo
    {
        public float timeLeft = -1;
        public bool isFadeIn = true;
        public bool removeAtEnd = false;
        public AudioType type;
        public AudioSource audioSource;
        public AudioInfo(AudioSource source)
        {
            audioSource = source;
        }

        public void SetFadeIn()
        {
            timeLeft = EngineDelegate.VolumeDuration;
            isFadeIn = true;
            removeAtEnd = false;
            audioSource.volume = 0;
        }

        public void SetFadeOut(bool remove = false)
        {
            timeLeft = EngineDelegate.VolumeDuration;
            isFadeIn = false;
            audioSource.volume = GOEAudioMgr.GetVolumeByType(type);
            removeAtEnd = remove;
        }

        public void SetVolume()
        {
            if (isFadeIn)
                audioSource.volume =((EngineDelegate.VolumeDuration - timeLeft) * GOEAudioMgr.GetVolumeByType(type) / EngineDelegate.VolumeDuration);
            else
                audioSource.volume = timeLeft * GOEAudioMgr.GetVolumeByType(type) / EngineDelegate.VolumeDuration;
        }
    }
}
