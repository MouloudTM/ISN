using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public static class SoundManager
{
    public enum Sound {
        SnakeMove,
        SnakeDie,
        SnakeEat,
        ButtonClick,
        ButtonOver,
    }
    public static void PlaySound(Sound sound) {
        GameObject soundGameObject = new GameObject("Sound");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(GetAudioClip(sound));
    }

    private static AudioClip GetAudioClip(Sound sound) {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.i.SoundAudioClipArray) {
            if (soundAudioClip.sound == sound) {
                return soundAudioClip.audioClip;
            }
        }
        Debug.LogError("Sound" + sound + "not found !");
        return null;
    }

    public static void AddButtonSounds(this Button_UI button_UI) {
        button_UI.MouseOverOnceFunc += () => SoundManager.PlaySound(Sound.ButtonOver);
        button_UI.ClickFunc += () => SoundManager.PlaySound(Sound.ButtonClick);
    }
}
