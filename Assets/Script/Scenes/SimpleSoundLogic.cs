using UnityEngine;
using System.Collections;

public class SimpleSoundLogic : MonoBehaviour {

    void Start()
    {
        Shoot.EventShoot += OnShoot;
        GoalKeeper.EventBallHitGK += OnBallHitGK;
        GoalDetermine.EventFinishShoot += OnGoal;
    }

    void OnDestroy()
    {
        Shoot.EventShoot -= OnShoot;
        GoalKeeper.EventBallHitGK -= OnBallHitGK;
        GoalDetermine.EventFinishShoot -= OnGoal;

    }

    void OnShoot()
    {
        SoundManager.share.playSoundSFX(SOUND_NAME.Shoot);
    }

    void OnBallHitGK()
    {
        SoundManager.share.playSoundSFX(SOUND_NAME.GoalKeeper_Catch);
    }

    void OnGoal(bool isGoal, Area area)
    {
        if (isGoal)
        {
            if (area == Area.None || area == Area.Normal)
            {
                SoundManager.share.playSoundSFX(SOUND_NAME.Ball_Hit_Goal);
            }
            else
            {
                SoundManager.share.playSoundSFX(SOUND_NAME.Ball_Hit_Goal_Extra);
            }
            SoundManager.share.playSoundSFX(SOUND_NAME.Crowd_Goal);
        }
        else
        {
            SoundManager.share.playSoundSFX(SOUND_NAME.Crowd_Out);
        }
    }
}
