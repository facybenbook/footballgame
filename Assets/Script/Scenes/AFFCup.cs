using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AFFCup : MonoBehaviour
{
    [SerializeField] 
    private Text textCountGoalMe;
    [SerializeField] 
    private Text textCountGoalAI;
    [SerializeField] 
    private Text textLevelShootAI;
    [SerializeField] 
    private Text textCountTurn;
    [SerializeField] 
    private Text textResult;
    [SerializeField] 
    private GameObject panelResult;

    public int maxTurn = 5;
    public float shootAfter = 2f;

    private int _count;
    private float _curveLevel;
    private float _difficultyShootLevel;

    private float[] _curveLevels = new float[] { 0f, 0.4f, 0.8f, 1f };
    private float[] _difficultyShootLevels = new float[]{0f, 0.4f, 0.8f, 1f};

    private int _countGoalMe;
    

    public int CountGoalMe
    {
        get { return _countGoalMe; }
        set
        {
            _countGoalMe = value;
            textCountGoalMe.text = "" + _countGoalMe;
        }
    }

    public int CountGoalAi
    {
        get { return _countGoalAI; }
        set
        {
            _countGoalAI = value;
            textCountGoalAI.text = "" + _countGoalAI;
        }
    }

    private int _countGoalAI;

    private void Start()
    {
        _curveLevel = _curveLevels[0];
        _difficultyShootLevel = _difficultyShootLevels[0];
        GoalDetermine.EventFinishShoot += EventShootFinish;

        GoalKeeperLevel.share.setLevel(0);
        OnChange_ShootAILevel(0f);

        OnClick_RestartGame();
    }

    private void OnDestroy()
    {
        GoalDetermine.EventFinishShoot -= EventShootFinish;
    }

    void NextTurn()
    {
        if (_count / 2 >= maxTurn)
        {
            Finalize();    
            
        }
        else
        {
            textCountTurn.text = (_count / 2 + 1).ToString();
           
            if (_count % 2 == 0)       // even turn is my turn to shoot
            {
                //Voi shut phat goc, do vi tri Camera reset khong phu thuoc vao vi tri Ball -> Dat switch Camera len dau
                SwitchCamera.share.setActiveCamera("CornerCamera"); //Maicamera se move theo vi tri cua Ball -> Chuyen ve phat goc
                //Do khi da phat goc, bong reset ve vi tri goc san nen phai goi ham Reset lien quan Goal Keeper truoc, ko se bi an theo bong
                GoalKeeperHorizontalFly.share.reset(); //Reset Goalkeeper ve vi tri o giua khung thanh
                GoalKeeper.share.reset();
                textCountTurn.text = "Trong ham count%2=0 shut goc";
                GoalKeeperHorizontalFly.share.IsAIControl = true;
                ShootAI.shareAI.willBeShootByUser = false;
                //GoalKeeperHorizontalFly.share.IsAIControl = true;
                //ShootAI.shareAI.willBeShootByUser = false;
                //SwitchCamera.share.OnToggle_BeGoalKeeper(false);
                ShootCorner.shareCorner.cornerReset(); //Goi ham reset vi tri cua Ball ve goc trai khung thanh

                GoalDetermine.share.reset(); //Reset lai tinh toan Goal

                CameraManager.share.reset(); //Reset vi tri va goc tuong ung cho Camera Corner va Main, Back Camera
                SlowMotion.share.reset();
                //Goi ham sut phat goc
                RunAfter.runAfter(gameObject, () =>
                {
                    //Khi goi ham sut phat goc, se goi ca ham dieu chinh camera doi theo goc sut bong
                    ShootCorner.shareCorner.cornerShoot();
                }, shootAfter);
                // reset camera position
                // CameraManager.share.reset();
                   //Set Main Camera Active
                //SwitchCamera.share.IsFront = true;
            }
            else        //  odd turn is AI turn to shoot
            {
                textCountTurn.text = "Shut AI";
                GoalKeeperHorizontalFly.share.IsAIControl = false;
                //Can reset bong ve vi tri truoc cau mon
                ShootAI.shareAI.reset(); //Ham nay chi reset vi tri cua Ball truoc cau mon cua Shoot AI
                GoalKeeperHorizontalFly.share.reset(); //Reset Goalkeeper ve vi tri o giua khung thanh
                GoalKeeper.share.reset();
                GoalDetermine.share.reset(); //Reset lai tinh toan Goal
                SwitchCamera.share.setActiveCamera("CameraMain");    //Set Main Camera Active
                CameraManager.share.reset(); //Reset vi tri va goc tuong ung cho Camera Corner va Main, Back Camera

                //Can ham reset camera theo vi tri moi cua Ball

                // SwitchCamera.share.IsFront = false;
                RunAfter.runAfter(gameObject, DoShootAI, 3f);
            }
        }
    }

    void Finalize()
    {
        textResult.text = "You Won";
        if (_countGoalMe < _countGoalAI)
        {
            textResult.text = "You Lose";    
        }
        else if (_countGoalMe == _countGoalAI)
        {
            textResult.text = "Duel";
        }

        panelResult.SetActive(true);
        textResult.enabled = true;
    }

    void DoShootAI()
    {
        ShootAI.shareAI.shoot(Direction.Both, _curveLevel, _difficultyShootLevel);
    }


    /// <summary>
    /// How to reset:
    /// 1/ Reset ball position
    /// 2/ Reset Wall position, this can be optional if we don't want to have wall kick in this turn
    /// 3/ Reset GoalKeeperHorizontalFly 
    /// 4/ Reset GoalKeeper
    /// 5/ Reset GoalDetermine
    /// 6/ Reset CameraManager
    /// 7/ Reset SlowMotion (optional)
    /// </summary>
    void Reset()
    {
        /*if (_count % 2 == 1)        // even means same turn
        {
            Shoot.share.reset(Shoot.share.BallPositionX, Shoot.share.BallPositionZ);        // reset with current ball position
        }
        else    // even means new turn, so will random new ball position
        {
            Shoot.share.reset();        // reset with new random ball position
        }
*/
        /*Wall.share.IsWall = ((_count / 2) % 2 != 0) ? true : false;     // each odd turn will be a wall kick
        if (Wall.share.IsWall)  // if wall kick
        {
            Wall.share.setWall(Shoot.share._ball.transform.position);   
        }*/

        /// must have reset call
        GoalKeeperHorizontalFly.share.reset();
        GoalKeeper.share.reset();
        GoalDetermine.share.reset();
        CameraManager.share.reset();

        //SwitchCamera.share.setActiveCamera("CameraMain");    //Set Main Camera Active
        SlowMotion.share.reset();
    }

    private void EventShootFinish(bool isGoal, Area area)
    {
        if (isGoal)
        {
            if (_count%2 == 0)
            {
                ++CountGoalMe;
            }
            else
            {
                ++CountGoalAi;
            }
        }

        ++_count;
        RunAfter.runAfter(gameObject, NextTurn, 2f);
    }

    public void OnChange_ShootAILevel(float val)
    {
        int level = (int)Mathf.Lerp(0, 3, val);
        textLevelShootAI.text = "" + level;

        _curveLevel = _curveLevels[level];
        _difficultyShootLevel = _difficultyShootLevels[level];
    }

    public void OnClick_RestartGame()
    {
        _count = 0;
        CountGoalMe = 0;
        CountGoalAi = 0;
        panelResult.SetActive(false);
        RunAfter.removeTasks(gameObject);
        NextTurn();
    }
}
