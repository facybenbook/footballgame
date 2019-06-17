using UnityEngine;
using System.Collections;
//using UnityEngine.SceneManagement;

public class DemoShoot : MonoBehaviour
{
    public static DemoShoot share;
    public float shootAfter = 1.5f;
    public bool forceGKAtInit = true;
    public bool forceShootByAI = true;

    //HENRY CLOSE
    private bool _isWallKick;
/*
    public bool IsWallKick
    {
        get { return _isWallKick; }
        set
        {
            _isWallKick = value;
            Wall.share.IsWall = _isWallKick;
            if (_isWallKick)
            {
                Wall.share.setWall(Shoot.share._ball.transform.position);
            }
        }
    }
    */

    [SerializeField] private int initialGKLevel = 0;

    private bool _autoShoot;
    public bool AutoShoot
    {
        get { return _autoShoot; }
        set { _autoShoot = value; }
    }

    void Awake()
    {
        share = this;
        //Henry add
        //AutoShoot = forceGKAtInit;
        //Shoot.EventDidPrepareNewTurn += OnNewTurn;

    }


    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        //HENRY CLOSE

        GoalKeeperLevel.share.setLevel(initialGKLevel);
        //GoalKeeperHorizontalFly.share.IsAIControl = true;
        //ShootAI.shareAI.willBeShootByUser = false;
        //SwitchCamera.share.OnToggle_BeGoalKeeper(false);


    }

 
    public void Reset(bool shouldRandomNewPos)
    {
        // ShootAI reset logic must be called first, to reset new ball poosition, reset() method of other components must come after this
        if(shouldRandomNewPos)
            ShootAI.shareAI.reset();                // used this method to reset new randomised ball's position
        else
            ShootAI.shareAI.reset(ShootAI.shareAI.BallPositionX, ShootAI.shareAI.BallPositionZ);        // call like this to reset new turn with current ball position

       //HENRY CLOSE
        SlowMotion.share.reset();                   // reset the slowmotion logic

        GoalKeeperHorizontalFly.share.reset();      // reset ve vi tri giua goal sau moi luot sut .goalkeeperhorizontalfly logic
        GoalKeeper.share.reset();                   // reset goalkeeper logic
        GoalDetermine.share.reset();                // reset goaldetermine logic so that it's ready to detect new goal
                                                    /*
                                                    if (Wall.share != null)                     // if there is wall in this scene
                                                    {
                                                        Wall.share.IsWall = IsWallKick;         // set is wall state
                                                        if (IsWallKick)                         // if we want wall kick
                                                            Wall.share.setWall(Shoot.share._ball.transform.position);       // set wall position with respect to ball position
                                                    }
                                            */


        //Test autoshoot
        //Put Ball to the corner
        //yield return new WaitForEndOfFrame();       // wait for the execution of Start method of DemoShoot

        //GKHorizontalFlyDebug.share.OnToggle_BeGoalKeeper(true);
        //SwitchCamera.share.OnToggle_BeGoalKeeper(true);
        // Use this for initialization
        //Remove de dam bao camera keep duoc ball
        //RunAfter.removeTasks(gameObject);
        //De enable shoot by AI: willBeShootByUser = false, IsAIControl = false, _isShootByAI = true, _isShooting = true
        ShootAI.shareAI.willBeShootByUser = false;
        //GoalKeeperHorizontalFly.share.IsAIControl = true;
        //ShootAI.shareAI.willBeShootByUser = false;
        //SwitchCamera.share.OnToggle_BeGoalKeeper(false);
        ShootCorner.shareCorner.cornerReset(); 
        RunAfter.runAfter(gameObject, () =>
        {
            ShootCorner.shareCorner.cornerShoot(); 
        }, shootAfter);
        // reset camera position
        CameraManager.share.reset();
    }
    /*
    public void OnClick_NewTurnRandomPosition()
    {
        Reset(true);
    }

    public void OnClick_NewTurnSamePosition()
    {
        Reset(false);
    }*/
}
