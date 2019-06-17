using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Path;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;

public enum Area
{
    Top,
    Left,
    Right,
    Normal,
    CornerLeft,
    CornerRight,
    None
}
public delegate void GoalEvent(bool isGoal, Area area);

public class GoalDetermine : MonoBehaviour
{

    public static GoalDetermine share;

    public static GoalEvent EventFinishShoot;
    public GameObject _prefabGoalSuccess;

    private bool _isGoal;
    public bool _goalCheck;

    private Transform _ball;

    [SerializeField]
    private Transform _pointLeft;
    [SerializeField]
    private Transform _pointRight;
    [SerializeField]
    private Transform _pointUp;
    [SerializeField]
    private Transform _pointDown;
    [SerializeField]
    private Transform _pointBack;

    public Renderer _areaTop;
    public Renderer _areaCornerLeft;
    public Renderer _areaCornerRight;
    public Renderer _areaLeft;
    public Renderer _areaRight;

    public Renderer _effectTop;
    public Renderer _effectLeft;
    public Renderer _effectCornerLeft;
    public Renderer _effectCornerRight;
    public Renderer _effectRight;

    public Transform _poleLeft;
    public Transform _poleRight;

    public Material _matCenter;
    public Texture2D _centerNormal;
    public Texture2D _centerCritical;

    private Area _poleArea;

    void Awake()
    {
        share = this;
        _prefabGoalSuccess.SetActive(false);
    }

    // Use this for initialization
    void Start()
    {
        _ball = Shoot.share._ball.transform;
        Shoot.EventShoot += eventShoot;
    }


    void OnDestroy()
    {
        Shoot.EventShoot -= eventShoot;
    }

    private Vector3 _contactPointWithPole;

    public void hitPole(Area poleArea, Vector3 contactPoint)
    {
        if (_goalCheck)
        {
            _contactPointWithPole = contactPoint;
            _poleArea = poleArea;
        }
    }

    private void eventShoot()
    {


        _count1 = _count2 = _count3 = 0;
        _goalCheck = true;
        _posPrev = _posCur = _ball.position;

        _poleArea = Area.None;
    }

    public void reset()
    {
        _goalCheck = false;
    }

    public void flashingHighScoreAreas()
    {
        _effectTop.enabled = true;
        _effectLeft.enabled = true;
        _effectRight.enabled = true;
        _effectCornerLeft.enabled = true;
        _effectCornerRight.enabled = true;

        Material mat = _effectTop.sharedMaterial;
        Color col = mat.GetColor("_TintColor");
        col.a = 50f / 255f;
        mat.SetColor("_TintColor", col);

        mat = _effectCornerLeft.sharedMaterial;
        col = mat.GetColor("_TintColor");
        col.a = 50f / 255f;
        mat.SetColor("_TintColor", col);

        iTween.ValueTo(gameObject, iTween.Hash("time", 1.5f
                                               , "delay", 0.5f
                                               , "from", 50f / 255f
                                               , "to", 0f
                                               , "easetype", iTween.EaseType.linear
                                               , "onupdate", "onUpdateFlashing"
                                               , "oncomplete", "completeFlashing"
                                               ));
    }

    private void onUpdateFlashing(float val)
    {
        Material mat = _effectTop.sharedMaterial;
        Color col = mat.GetColor("_TintColor");
        col.a = val;
        mat.SetColor("_TintColor", col);

        mat = _effectCornerLeft.sharedMaterial;
        col = mat.GetColor("_TintColor");
        col.a = 50f / 255f;
        mat.SetColor("_TintColor", col);
    }

    private void completeFlashing()
    {
        _effectTop.enabled = false;
        _effectLeft.enabled = false;
        _effectRight.enabled = false;
        _effectCornerLeft.enabled = false;
        _effectCornerRight.enabled = false;
    }

    private Vector3 _posPrev;
    private Vector3 _posCur;

    private float _count1;
    private float _count2;
    private float _count3;

    float _distance;

    // Update is called once per frame
    void Update()
    {

        if (_goalCheck)
        {

            _posPrev = _posCur;
            _posCur = _ball.position;

            if (_posCur.z >= _pointDown.position.z)
            {       // ko check goal nua
                if (_posCur.z <= _pointBack.position.z && _posCur.x < _pointRight.position.x && _posCur.x > _pointLeft.position.x && _posCur.y < _pointUp.position.y && _posCur.y > 0)
                {
                    _count1 = 0;

                    _goalCheck = false;

                    Area area = Area.Normal;

                    if (_poleArea != Area.None)
                    {       // neu truoc do trung xa ngang hay cot doc xong roi banh vo luoi 

                        area = Area.CornerLeft;
                        setGoalSuccesIcon(area, _contactPointWithPole);

                    }
                    else
                    {       // khong trung xa ngang cot doc gi het
                        if (_areaTop.bounds.Contains(_posCur))
                            area = Area.Top;
                        else if (_areaLeft.bounds.Contains(_posCur))
                        {
                            area = Area.Left;
                        }
                        else if (_areaRight.bounds.Contains(_posCur))
                        {
                            area = Area.Right;
                        }
                        else if (_areaCornerLeft.bounds.Contains(_posCur))
                        {
                            area = Area.CornerLeft;
                        }
                        else if (_areaCornerRight.bounds.Contains(_posCur))
                        {
                            area = Area.CornerRight;
                        }

                        setGoalSuccesIcon(area, _ball.transform.position);
                    }


                    if (EventFinishShoot != null)
                        EventFinishShoot(true, area);
                }
                else
                {
                    _count1 += Time.deltaTime;
                    if (_count1 > 1f)
                    {
                        _goalCheck = false;
                        if (EventFinishShoot != null)
                            EventFinishShoot(false, Area.None);
                    }
                }
            }
            else
            {      // tiep tuc check goal
                if (_posCur.z > _posPrev.z)
                {
                    _count2 = 0;

                    _distance = 10f;
                    if (Shoot.share._ball.velocity.sqrMagnitude < 0.3f)
                        _distance = 0f;
                    else if (Shoot.share._ball.velocity.sqrMagnitude < 2f)
                        _distance = 4f;

                    if (Mathf.Abs(_posCur.x) > _distance && (Mathf.Abs(_posCur.x) > Mathf.Abs(_posPrev.x) || Shoot.share._ball.velocity.sqrMagnitude < 0.3f))
                    {
                        _count3 += Time.deltaTime;
                        if (_count3 > 1f)
                        {
                            _goalCheck = false;
                            if (EventFinishShoot != null)
                                EventFinishShoot(false, Area.None);
                        }
                    }
                    else
                    {
                        _count3 = 0;
                    }
                }
                else
                {
                    _count2 += Time.deltaTime;
                    if (_count2 > 1f)
                    {
                        _goalCheck = false;
                        if (EventFinishShoot != null)
                            EventFinishShoot(false, Area.None);
                    }
                }
            }
        }
    }

    private void setGoalSuccesIcon(Area area, Vector3 position)
    {
        _prefabGoalSuccess.SetActive(true);
        _prefabGoalSuccess.transform.position = position;
        /*
        iTween.ScaleAdd(_prefabGoalSuccess, iTween.Hash("time", 0.25f
                                                        ,"looptype", iTween.LoopType.pingPong
                                                        ,"amount", new Vector3(0.3f, 0.3f, 0)
                                                        ,"easetype", iTween.EaseType.linear
                                                        ));
*/

        if (area == Area.None || area == Area.Normal)
        {
            _matCenter.mainTexture = _centerNormal;

        }
        else
        {
            _matCenter.mainTexture = _centerCritical;

        }

        _prefabGoalSuccess.transform.localScale = new Vector3(0.6f, 0.6f, 1);
        HOTween.To(_prefabGoalSuccess.transform, 1f, new TweenParms()
                   .Prop("localScale", new Vector3(0.9f, 0.9f, 1), false)
                   .Loops(1, LoopType.Restart)
                   .Ease(EaseType.Linear)
                   .OnComplete(animationFinished)
                   );

        Material mat = _prefabGoalSuccess.GetComponent<Renderer>().sharedMaterial;
        Color col = mat.GetColor("_TintColor");
        col.a = 0.5f;
        mat.SetColor("_TintColor", col);

        iTween.ValueTo(gameObject, iTween.Hash("time", 0.5f
                                               , "from", 0.5f
                                               , "to", 1f
                                               , "easetype", iTween.EaseType.easeInCubic
                                               , "onupdate", "onUpdateColor"
                                               , "oncomplete", "complete1"
                                               ));

        flashingHighScoreAreas();
    }

    private void onUpdateColor(float val)
    {
        Material mat = _prefabGoalSuccess.GetComponent<Renderer>().sharedMaterial;
        Color col = mat.GetColor("_TintColor");
        col.a = val;
        mat.SetColor("_TintColor", col);
    }

    private void complete1()
    {
        iTween.ValueTo(gameObject, iTween.Hash("time", 0.5f
                                               , "from", 1f
                                               , "to", 0f
                                               , "easetype", iTween.EaseType.easeOutCubic
                                               , "onupdate", "onUpdateColor"
                                               ));
    }

    private void animationFinished()
    {
        _prefabGoalSuccess.SetActive(false);
    }
}
