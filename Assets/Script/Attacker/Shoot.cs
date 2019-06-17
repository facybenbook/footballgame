using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;


public class Shoot : MonoBehaviour {

	public static Shoot share;

	public static Action EventShoot = delegate {};
    public static Action<float> EventChangeSpeedZ = delegate {};
    public static Action<float> EventChangeBallZ = delegate { };
    public static Action<float> EventChangeBallX = delegate { };
    public static Action<float> EventChangeBallLimit = delegate { };
    public static Action<Collision> EventOnCollisionEnter = delegate { };
    public static Action EventDidPrepareNewTurn = delegate { };


    public float _ballControlLimit;

	public Transform _goalKeeper;
	public Transform _ballTarget;
	protected Vector3 beginPos;
	protected bool _isShoot = false;

	public float minDistance = 100;		// 40f


	public Rigidbody _ball;
	public float factorUp = 0.012f;				// 10f
	public float factorDown = 0.003f;			// 1f
	public float factorLeftRight = 0.025f;		// 2f
    public float factorLeftRightMultiply = 0.8f;		// 2f
	public float _zVelocity = 24f;

	public AnimationCurve _curve;
	protected Camera _mainCam;

	protected float factorUpConstant =  0.017f * 960f; 	// 0.015f * 960f;
	protected float factorDownConstant = 0.006f * 960f; // 0.005f * 960f;
	protected float factorLeftRightConstant =  0.0235f * 640f; // 0.03f * 640f; // 0.03f * 640f;

	public Transform _ballShadow;


	public float _speedMin = 18f;	// 20f;
	public float _speedMax = 30f;	// 36f;
    //Henry: Define the random range of position for Ball in each turn
    public float _distanceMinZ = -16f; //old: 16.5f
    public float _distanceMaxZ = 30f; //old: 35f

	public float _distanceMinX = -15f;
	public float _distanceMaxX = 15f;

	public bool _isShooting = false;   
	public bool _canControlBall = false;

    public Transform _cachedTrans;
    public Transform _cachedTransforCorner;

    public bool _enableTouch = false;
    public float screenWidth;
    public float screenHeight;

    Vector3 _prePos, _curPos;
    public float angle;
    protected ScreenOrientation orientation;

    protected Transform _ballParent;

    protected RaycastHit _hit;
    public bool _isInTutorial = false;
    public Vector3 ballVelocity;

    private float _ballPostitionZ = -10f;//Put position of Ball
    private float _ballPostitionX = 0f;

    public float BallPositionZ
    {
        get { return _ballPostitionZ; }
        set { _ballPostitionZ = value; }
    }

    public float BallPositionX
    {
        get { return _ballPostitionX; }
        set { _ballPostitionX = value; }
    }
    public TrailRenderer _effect;




	protected virtual void Awake() {
		share = this;
		_cachedTrans = transform;
		_isShooting = true;
        //Gan doi tuong transform Ball vao Ball trong Rigid
		_ballParent = _ball.transform.parent;

	   // _distanceMinX = -15f;
	   // _distanceMaxX = 15f;
       // _distanceMaxZ = 15f; //henry edit, old : 30f

	}

	// Use this for initialization
	protected virtual void Start () {
//		Application.targetFrameRate = 30;
		_mainCam = CameraManager.share._cameraMainComponent;

#if UNITY_WP8 || UNITY_ANDROID
		Time.maximumDeltaTime = 0.2f;
		Time.fixedDeltaTime = 0.008f;
#else
		Time.maximumDeltaTime = 0.1f;
		Time.fixedDeltaTime = 0.005f;
#endif

		orientation = Screen.orientation;
		calculateFactors();

        //_ballControlLimit = 6f;
	    EventChangeBallLimit(_ballControlLimit);

		reset();
		CameraManager.share.reset();
	    //GoalKeeper.share.reset();

		GoalDetermine.EventFinishShoot += goalEvent;
	}

	void OnDestroy() {
		GoalDetermine.EventFinishShoot -= goalEvent;
	}

	public virtual void goalEvent(bool isGoal, Area area) {
		_canControlBall = false;
		_isShooting = false;
	}

	public void calculateFactors() {
		screenHeight = Screen.height;
		screenWidth = Screen.width;
		
		minDistance = (100 * screenHeight) / 960f;
		factorUp = factorUpConstant / screenHeight;
		factorDown = factorDownConstant / screenHeight;
		factorLeftRight = factorLeftRightConstant / screenWidth;

		Debug.Log("Orientation : " + orientation + "\t Screen height = " + screenHeight 
            + "\t Screen width = " + screenWidth + "\t factorUp = " + factorUp + "\t factorDown = " + factorDown 
            + "\t factorLeftRight = " + factorLeftRight + "\t minDistance = " + minDistance);
	}

    protected void LateUpdate()
    {
		if(screenHeight != Screen.height) {
			orientation = Screen.orientation;
			calculateFactors();
		    CameraManager.share.reset();
		}
	}
	void FixedUpdate() {
		ballVelocity = _ball.velocity;

		Vector3 pos = _ball.transform.position;
		//pos.y = 0.015f;
		//_ballShadow.position = pos;
	}

	protected virtual void Update() {
		if(_isShooting) {		// neu banh chua vao luoi hoac trung thu mon, khung thanh thi banh duoc phep bay voi van toc dang co
			if( _enableTouch && !_isInTutorial ) {
				if(Input.GetMouseButtonDown(0)) {			// touch phase began
					mouseBegin(Input.mousePosition);
				}
				else if( Input.GetMouseButton(0) ) {			
					mouseMove(Input.mousePosition);
				}
				else if(Input.GetMouseButtonUp(0)) {	// touch ended
					mouseEnd();
				}
			}
            //Ham shoot Ball: Trong Update frame cua Unity
			if(_isShoot) {
				Vector3 speed = _ballParent.InverseTransformDirection(_ball.velocity);
				speed.z = _zVelocity;
				_ball.velocity = _ballParent.TransformDirection(speed);
			}
		}
	}

	public void mouseBegin(Vector3 pos) {
		_prePos = _curPos = pos;
		beginPos = _curPos;
	}

	public void mouseEnd() {
		if(_isShoot == true) {		// neu da sut' roi` thi ko cho dieu khien banh nua, tranh' truong` hop nguoi choi tao ra nhung cu sut ko the~ do~ noi~
            //_canControlBall = false;
		}
	}

	public void mouseMove(Vector3 pos) {
		if(_curPos != pos) {		// touch phase moved
			_prePos = _curPos;
			_curPos = pos;

			
			Vector3 distance = _curPos - beginPos;
			
			if(_isShoot == false) {				// CHUA SUT
				if(distance.y > 0 && distance.magnitude >= minDistance) {		
					if(Physics.Raycast( _mainCam.ScreenPointToRay(_curPos), out _hit, 100f) && _hit.transform != _cachedTrans){
						_isShoot = true;
						
						Vector3 point1 = _hit.point;		// contact point
						point1.y = 0;
						point1 = _ball.transform.InverseTransformPoint(point1);		// dua point1 ve he truc toa do cua ball, coi ball la goc toa do cho de~
						point1 -= Vector3.zero;			// vector tao boi point va goc' toa do
						
						Vector3 diff = point1;
						diff.Normalize();				// normalized rat' quan trong khi tinh' goc
						
						float angle = 90 - Mathf.Atan2(diff.z, diff.x) * Mathf.Rad2Deg;		// doi ra degree va lay 90 tru` vi nguoc
						//								Debug.Log("angle = " + angle);
						
						float x = _zVelocity * Mathf.Tan(angle * Mathf.Deg2Rad);				
						
						//							float x = distance.x * factorLeftRight;
						_ball.velocity = _ballParent.TransformDirection(new Vector3(x, distance.y * factorUp, _zVelocity));
						_ball.angularVelocity = new Vector3(0, x, 0f);
						
						if(EventShoot != null) {
							EventShoot();
						}
					}
				}
			}
			else {				// da~ sut' roi`, tuy theo do lech cua touch frame hien tai va truoc do' ma se lam cho banh xoay' trai', phai~, len va xuong' tuong ung'
				if(_canControlBall == true) {	// neu nhac ngon tay len khoi man hinh roi thi ko cho dieu khien banh nua

				    if (_cachedTrans.position.z < -_ballControlLimit)
				    {
				        // neu banh xa hon khung thanh 6m thi moi' cho dieu khien banh xoay, di vo trong khoang cach 6m thi ko cho nua~ de~ lam cho game can bang`

				        distance = _curPos - _prePos;

				        Vector3 speed = _ballParent.InverseTransformDirection(_ball.velocity);
				        speed.y += distance.y*((distance.y > 0) ? factorUp : factorDown);
				        speed.x += distance.x * factorLeftRight * factorLeftRightMultiply;
				        _ball.velocity = _ballParent.TransformDirection(speed);

				        speed = _ball.angularVelocity;
				        speed.y += distance.x*factorLeftRight;
				        _ball.angularVelocity = speed;
				    }
				    else
				    {
				        _canControlBall = false;
				    }
				}
			}
		}
	}

    protected void OnCollisionEnter(Collision other)
    {
		string tag = other.gameObject.tag;
		if(tag.Equals("Player") || tag.Equals("Obstacle") || tag.Equals("Net") || tag.Equals("Wall")) {	// banh trung thu mon hoac khung thanh hoac da vao luoi roi thi ko cho banh bay voi van toc nua, luc nay de~ cho physics engine tinh' toan' quy~ dao bay
			_isShooting = false;

            if (tag.Equals("Net"))
            {
                _ball.velocity /= 3f;
            }
            else { //Henry insert: Giam toc do bong sau khi va cham, note: check khi danh dau ?
                _ball.velocity /= 1.5f;
            };
		}
        
        EventOnCollisionEnter(other);
    }

	private void enableEffect() {
//		_effect.enabled = true;
		_effect.time = 1;
	}
    //Henry: Whenever reset, it call ramdom function to put the ball.
	public virtual void reset() {
		reset (- Random.Range(_distanceMinX, _distanceMaxX), - Random.Range(_distanceMinZ, _distanceMaxZ));

	}


    //Henry: Reset Ball to the corner, may be consider Leftcorner and Right corner. 
    public virtual void cornerReset()
    {
        reset(-44.5f, -0.3f); //Dat bong vao vi tri sut phat goc ben trai
        //reset(-44.5f, -0.3f); //Goc ben trai khung thanh

    }

    public virtual void reset(float x, float z)
	{
	    Debug.Log(string.Format("<color=#c3ff55>Reset Ball Pos, x = {0}, z = {1}</color>", x, z));

		_effect.time = 0;
//		_effect.enabled = false;
		Invoke("enableEffect", 0.1f);
    
		BallPositionX = x;
        EventChangeBallX(x);
		BallPositionZ = z;
	    EventChangeBallZ(z);


		_canControlBall = true;
		_isShoot = false;
		_isShooting = true;

		// reset ball
		_ball.velocity = Vector3.zero;
		_ball.angularVelocity = Vector3.zero;
		_ball.transform.localEulerAngles = Vector3.zero;


		Vector3 pos = new Vector3(BallPositionX, 0f, BallPositionZ);
		Vector3 diff = -pos;
		diff.Normalize();
		float angleRadian = Mathf.Atan2(diff.z, diff.x);		// tinh' goc' lech
		float angle = 90 - angleRadian * Mathf.Rad2Deg;

		_ball.transform.parent.localEulerAngles = new Vector3(0, angle, 0);		// set parent cua ball xoay 1 do theo truc y = goc lech

		_ball.transform.position = new Vector3(BallPositionX, 0.16f, BallPositionZ);

		pos = _ballTarget.position;
		pos.x = 0;
		_ballTarget.position = pos;

        float val = (Mathf.Abs(_ball.transform.localPosition.z) - _distanceMinZ) / (_distanceMaxZ - _distanceMinZ);
		_zVelocity =  Mathf.Lerp(_speedMin, _speedMax, val);

	    EventChangeSpeedZ(_zVelocity);

	    EventDidPrepareNewTurn();
	}

    public void enableTouch()
    {
        _enableTouch = true;
    }

    public void disableTouch()
    {
        StartCoroutine(_disableTouch());
    }

    private IEnumerator _disableTouch()
    {
        yield return new WaitForEndOfFrame();
        _enableTouch = false;
    }
}
