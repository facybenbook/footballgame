using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum SOUND_NAME
{
    Shoot,
    GoalKeeper_Catch,
    Whistle,
    Crowd_Goal,
    Crowd_Out,
    Button,
    Ball_Hit_Bar,
    Ball_Hit_Net,
    Ball_Hit_Player_Wall,
    Ball_Hit_Goal,
    Ball_Hit_Goal_Extra,
    BG_Crowd,
    BG_Menu,
    BG_Menu1,
    None
}

public class SoundManager : MonoBehaviour
{


    public static SoundManager share;
    public static Action<bool, bool> EventRecheckAllSound = delegate { };

    private Hashtable soundBG;
    private Hashtable soundSFX;

    private SOUND_NAME currentThemeSound;

    private bool isSFXOn = true;

    public bool IsSFXOn
    {
        get
        {
            return isSFXOn;
        }
        set
        {
            isSFXOn = value;
            recheckAllSound();
        }
    }

    private bool isBGMusicOn = true;

    public bool IsBGMusicOn
    {
        get
        {
            return isBGMusicOn;
        }
        set
        {
            isBGMusicOn = value;
            recheckAllSound();
        }
    }

    private bool _backUpStateSFX;
    private bool _backUpStateBG;

    public void turnOffSoundTemporary()
    {
        _backUpStateSFX = IsSFXOn;
        _backUpStateBG = IsBGMusicOn;

        IsSFXOn = false;
        IsBGMusicOn = false;
    }

    public void recoverSoundState()
    {
        IsSFXOn = _backUpStateSFX;
        IsBGMusicOn = _backUpStateBG;
    }

    public ArrayList sounds;


    void Awake()
    {
        share = this;
        sounds = new ArrayList();
        soundBG = new Hashtable();
        soundSFX = new Hashtable();
        currentThemeSound = SOUND_NAME.None;

        _backUpStateBG = true;
        _backUpStateSFX = true;
    }

    void Start()
    {
        _backUpStateBG = IsBGMusicOn;
        _backUpStateSFX = IsSFXOn;

        //Henry edit: Start -> Turn on, don't wait click enable button.
        SoundManager.share.playSoundSFX(SOUND_NAME.Button);
    }

    private void clearAll()
    {
        soundBG.Clear();
        soundSFX.Clear();
    }

    private void recheckAllSound()
    {
        EventRecheckAllSound(isBGMusicOn, isSFXOn);

        if (!isBGMusicOn)
        {
            muteAllBGMusic();
        }
        else
        {
            unMuteAllCurrentBGMusic();
        }

        if (!isSFXOn)
            muteAllSFXMusic();
        else
            unMuteAllCurrentSFX();
    }

    private void unMuteAllCurrentSFX()
    {

        foreach (GameObject music in soundSFX.Values)
        {
            if (music.GetComponent<AudioSource>())
                music.GetComponent<AudioSource>().mute = false;
        }
    }

    private void unMuteAllCurrentBGMusic()
    {
        foreach (GameObject music in soundBG.Values)
        {
            if (music.GetComponent<AudioSource>())
                music.GetComponent<AudioSource>().mute = false;
        }

    }

    private void muteAllBGMusic()
    {
        foreach (GameObject music in soundBG.Values)
        {
            if (music.GetComponent<AudioSource>() != null)
                music.GetComponent<AudioSource>().mute = true;
        }

    }

    private void muteAllSFXMusic()
    {
        foreach (GameObject music in soundSFX.Values)
        {
            if (music.GetComponent<AudioSource>())
                music.GetComponent<AudioSource>().mute = true;
        }
    }

    public GameObject playSoundBackGround(SOUND_NAME type)
    {
        GameObject retVal = null;

        if (currentThemeSound == type)
            return null;

        if (soundBG[type] != null)
        {
            GameObject sound = (GameObject)soundBG[type];
            if (sound != null)
            {
                sound.GetComponent<AudioSource>().Play();
                sound.GetComponent<AudioSource>().volume = 1f;
                retVal = sound;
            }
            else
            {
                soundBG.Remove(type);

            }
        }

        if (retVal == null)
        {
            AudioClip clipAudio = getSound(type);

            GameObject go = new GameObject("AudioSource", typeof(AudioSource));
            AudioSource audioSource = go.GetComponent<AudioSource>();
            go.transform.parent = transform;
            go.name = type.ToString();
            soundBG.Add(type, go);

            audioSource.clip = clipAudio;
            audioSource.loop = true;
            audioSource.spatialBlend = 0.0f;
            audioSource.volume = 1f;
            audioSource.Play();

            retVal = go;
        }

        if (currentThemeSound != SOUND_NAME.None)
        {
            if (soundBG[currentThemeSound] != null)
            {
                GameObject sound = (GameObject)soundBG[currentThemeSound];
                if (sound != null)
                {
                    sound.GetComponent<AudioSource>().Stop();
                }
                else
                {
                    soundBG.Remove(currentThemeSound);
                }
            }
        }

        currentThemeSound = type;
        recheckAllSound();

        return retVal;
    }

    void OnApplicationPause(bool pause)
    {
        Debug.Log("Pause : " + pause);
        if (pause)
        {
            turnOffSoundTemporary();
        }
        else
        {
            recoverSoundState();
        }
    }

    public GameObject playSoundSFX(SOUND_NAME type)
    {
        //      Debug.Log("playSound : " + type);
        GameObject retVal = null;
        bool flag = true;

        if (soundSFX[type] != null)
        {
            GameObject sound = (GameObject)soundSFX[type];
            if (sound != null)
            {
                sound.GetComponent<AudioSource>().Play();
                retVal = sound;
                //          Debug.Log("Play 1");
                flag = false;
            }
            else
            {
                soundSFX.Remove(type);

            }
        }

        if (flag)
        {
            AudioClip sound = getSound(type);

            GameObject audioSource = new GameObject("AudioSource", typeof(AudioSource));
            audioSource.transform.parent = transform;
            audioSource.GetComponent<AudioSource>().clip = sound;
            audioSource.name = type.ToString();
            soundSFX.Add(type, audioSource);
            audioSource.GetComponent<AudioSource>().spatialBlend = 0.0f;
            audioSource.GetComponent<AudioSource>().Play();

            retVal = audioSource;
        }

        recheckAllSound();
        return retVal;
    }


    public static AudioClip getSound(SOUND_NAME type)
    {
        string name = "";
        switch (type)
        {
            case SOUND_NAME.Shoot:
                name = "Sound/shoot";
                break;
            case SOUND_NAME.GoalKeeper_Catch:
                name = "Sound/goalkeeper_catch";
                break;
            case SOUND_NAME.Whistle:
                name = "Sound/whistle";
                break;
            case SOUND_NAME.Crowd_Goal:
                name = "Sound/crowd_goal";
                break;
            case SOUND_NAME.Crowd_Out:
                name = "Sound/crowd_out";
                break;
            case SOUND_NAME.Button:
                name = "Sound/button";
                break;
            case SOUND_NAME.Ball_Hit_Bar:
                name = "Sound/ball_hit_bar";
                break;
            case SOUND_NAME.Ball_Hit_Net:
                name = "Sound/ball_hit_net";
                break;
            case SOUND_NAME.Ball_Hit_Player_Wall:
                name = "Sound/ball_hit_player_wall";
                break;
            case SOUND_NAME.Ball_Hit_Goal:
                name = "Sound/ball_hit_goal_signal";
                break;
            case SOUND_NAME.Ball_Hit_Goal_Extra:
                name = "Sound/ball_hit_goal_extra_signal";
                break;
            case SOUND_NAME.BG_Crowd:
                name = "Sound/BG_crowd";
                break;
            case SOUND_NAME.BG_Menu:
                name = "Sound/BG_Football_Menu";
                break;
            case SOUND_NAME.BG_Menu1:
                name = "Sound/BG_Football_Fast";
                break;

            default:
                break;
        }

        return Resources.Load(name, typeof(AudioClip)) as AudioClip;
    }


   // public void onClick_Button()
   // {
    //    SoundManager.share.playSoundSFX(SOUND_NAME.Button);
   // }
}
