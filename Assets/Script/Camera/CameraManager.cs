using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Path;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;
using System;


public class CameraManager : MonoBehaviour
{

    public static CameraManager share;

    public static Action EventReset = delegate { };
    public static Action EventBeginIntroMovement = delegate { };
    public static Action EventEndIntroMovement = delegate { };

    public GameObject _cameraMain;
    public GameObject _cameraBack;
    //henry add
    public GameObject _cameraCorner;
    public Camera[] _camerasMain;

    //[SerializeField]
    int _FOVportrait = 50; //Edit view area of Camera, old: 40
    //[SerializeField]
    int _FOVlandscape = 40; //old: 30

    private bool isPortrait = true;
    public Camera _cameraMainComponent;
    public Collider _colliderFullScreen;

    [SerializeField]
    private float yBackCamera = 1f;//Henry edit old: 1f
    [SerializeField]
    private float cameraMainDistanceToBall = -4f; //Gia tri am ; Dat camera sau bong, +: Dat camera truoc bong
    [SerializeField]
    private float cameraMainY = 1.7f; //Henry edit old: 1.7f - Do cao cua Camera

    void Awake()
    {
        share = this;
        _cameraMainComponent = _cameraMain.GetComponent<Camera>();


    }

    // Use this for initialization
    void Start()
    {
        updateCameraFOV();
        Shoot.EventShoot += eventShoot;
    }

    void OnDestroy()
    {
        Shoot.EventShoot -= eventShoot;
    }

    public void turnOn()
    {
        _cameraMain.SetActive(false);
        _cameraBack.SetActive(false);
        //henry add
        _cameraCorner.SetActive(true);
    }

    public void turnOff()
    {
        _cameraMain.SetActive(false);
        _cameraBack.SetActive(false);
        _cameraCorner.SetActive(false);
    }

    private bool _isCameraMoving = false;
   //Henry add more
    private bool _isCornerCameraMoving = false;
    Tweener _tweenCamera;

    [SerializeField] private float testXZ = 0.8f;
    [SerializeField] private float testY = 5f; //Henry edit: old 5f

    private void eventShoot()
    {
        MoveCameraWhenShoot();
        //Henry add more
        MoveCornerCameraWhenShoot();
    }

    public void MoveCameraWhenShoot()
    {
        //Day moi la ham chay Camera theo vi tri cua Ball
        if (_isCameraMoving == true)
            return;

        _isCameraMoving = true;

        Transform ballTrans = Shoot.share._ball.transform;
        _cameraMain.GetComponent<SmoothLookAt>().target = ballTrans;

        _tweenCamera = HOTween.To(_cameraMain.transform, 3f, new TweenParms()
                  .Prop("position", new Vector3(ballTrans.position.x * testXZ, testY, ballTrans.position.z * testXZ))
                  .Loops(1, LoopType.Restart)
                  .Ease(EaseType.Linear)
                  .OnComplete(cameraFinishMoving)
                  );
        _tweenCamera.autoKillOnComplete = true;
        _currentFOV = _FOVportrait;
    }
    //henry add more
    public void MoveCornerCameraWhenShoot()
    {
        //Day moi la ham chay Camera theo vi tri cua Ball
        if (_isCornerCameraMoving == true)
            return;

        _isCornerCameraMoving = true;

        Transform ballTrans = Shoot.share._ball.transform;
        _cameraCorner.GetComponent<SmoothLookAt>().target = ballTrans;

        _tweenCamera = HOTween.To(_cameraCorner.transform, 3f, new TweenParms()
                  .Prop("position", new Vector3(ballTrans.position.x * testXZ, testY, ballTrans.position.z * testXZ))
                  .Loops(1, LoopType.Restart)
                  .Ease(EaseType.Linear)
                  .OnComplete(cameraCornerFinishMoving)
                  );
        _tweenCamera.autoKillOnComplete = true;
        _currentFOV = _FOVportrait;
    }


    public float _currentFOV;
    private void cameraFinishMoving()
    {
             _cameraMain.GetComponent<SmoothLookAt>().target = null;
    }
    //Henry add more
    private void cameraCornerFinishMoving()
    {
        _cameraCorner.GetComponent<SmoothLookAt>().target = null;
    }

    public void introMovement(Action callback, Transform target)
    {
        EventBeginIntroMovement();

        Vector3[] path = new Vector3[3];
        path[2] = target.position;
        path[0] = new Vector3(0, 1.5f, -60f);
        path[1] = (path[2] + path[0]) / 2;
        path[1].y = 6f;

        Quaternion rotation = target.rotation;
        _cameraMain.transform.localEulerAngles = Vector3.zero;
        _cameraMain.transform.position = path[0];

        HOTween.To(_cameraMain.transform, 6f, new TweenParms()
                   .Prop("position", new PlugVector3Path(path, PathType.Curved))
                   .Prop("rotation", rotation)
                   .Loops(1, LoopType.Restart)
                   .Ease(EaseType.EaseInOutQuad)
                   .OnComplete(() => {

                       if (callback != null)
                           callback();

                       EventEndIntroMovement();
                   })
                   .AutoKill(true)
                   );
    }

    public void reset()
    {
        if (_tweenCamera != null)
            _tweenCamera.Kill();
        _cameraMain.GetComponent<SmoothLookAt>().target = null;
        //Henry add more
        _cameraCorner.GetComponent<SmoothLookAt>().target = null;
        _isCornerCameraMoving = false;

        _isCameraMoving = false;

        updateCameraFOV();

        StartCoroutine(resetPosition());

        EventReset();
    }

  


    private IEnumerator resetPosition()
    {
        //Co hai tinh toan: - Vi tri maicamera theo bong
        //                  - Goc ong kinh Maincamera
        Transform ball = Shoot.share._ball.transform;

        Vector3 diff = -ball.position;
        diff.Normalize();
        float angleRadian = Mathf.Atan2(diff.x, diff.z);
        float angle = angleRadian * Mathf.Rad2Deg;          // goc lech so voi goc toa do
        angleRadian = angle * Mathf.Deg2Rad;

        Vector3 pos = ball.position;        // pos se duoc gan' la vi tri cua camera
        pos.y = cameraMainY;            // camera cach' mat dat 1.7m

        if (isPortrait)
        {       // neu la portrait thi camera nam dang sau trai banh 4m va huong ve goc toa do, noi cach khac' la cung huong' voi' truc z cua parent cua ball
            pos.x += cameraMainDistanceToBall * Mathf.Sin(angleRadian);
            pos.z += cameraMainDistanceToBall * Mathf.Cos(angleRadian);
        }
        else
        {       // neu la landscape thi camera nam dang sau trai banh 4m va huong ve goc toa do, noi cach khac' la cung huong' voi' truc z cua parent cua ball
            pos.x += cameraMainDistanceToBall * Mathf.Sin(angleRadian);
            pos.z += cameraMainDistanceToBall * Mathf.Cos(angleRadian);
        }
        //Gan lai vi tri cho MainCamera
        _cameraMain.transform.position = pos;

        //Tinh toan goc huong cua Main Camera
        Vector3 rotation = _cameraMain.transform.eulerAngles;
        rotation.y = angle;
        rotation.x = 6f;  // quay 6 do theo truc x
        rotation.z = 0f;
        _cameraMain.transform.eulerAngles = rotation;

        float distanceBackCameraToBall = 10f; // 18.31871f;

        float x = distanceBackCameraToBall * Mathf.Sin(angleRadian);
        float z = distanceBackCameraToBall * Mathf.Cos(angleRadian);

        if (_cameraBack)
        {
            _cameraBack.transform.position = new Vector3(x, yBackCamera, z);

            //Henry close
            //Vector3 posGK = GoalKeeper.share.transform.position;
            // posGK.y = yBackCamera;

            Quaternion rotationLook = Quaternion.LookRotation(Shoot.share._cachedTrans.position - _cameraBack.transform.position);
            _cameraBack.transform.rotation = rotationLook;
        }

        //Henry add: Reset cho CameraCorner
        if (_cameraCorner)
        {
            //Reset vi tri cua CornerCamera o goc trai shut phat
            _cameraCorner.transform.position = new Vector3(-50f, 2.5f, 0.5f);

            //Reset goc quay cua CornerCamera o goc shut phat trai
            Vector3 rotationC = _cameraCorner.transform.eulerAngles;
            rotationC.x = 0f;
            rotationC.y = 106f;  
            rotationC.z = 0f;
            _cameraCorner.transform.eulerAngles = rotationC;

        }

        yield break;
    }


    public void updateCameraFOV()
    {

        if (Screen.height > Screen.width)
        {   // portrait
            isPortrait = true;
            foreach (Camera camera in _camerasMain)
            {
                if (camera.orthographic == false)
                {
                    camera.fieldOfView = _FOVportrait;
                    _currentFOV = _FOVportrait;
                }
            }
        }
        else
        {           //landscape
            isPortrait = false;
            foreach (Camera camera in _camerasMain)
            {
                if (camera.orthographic == false)
                {
                    camera.fieldOfView = _FOVlandscape;
                    _currentFOV = _FOVlandscape;
                }
            }
        }

        //reset();
    }



}
