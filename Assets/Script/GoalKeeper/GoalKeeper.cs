using UnityEngine;
using System.Collections;

using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Path;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;

using System;
using Random = UnityEngine.Random;

[System.Serializable]
public class ArmSystem {
	public Transform bone1;
	public Transform bone2;
	public Transform bone3;
}


public class GoalKeeper : MonoBehaviour {

	public static GoalKeeper share;
    public static Action EventBallHitGK = delegate {};

	public Transform _ball;
	public Transform _ballTarget;
	
	public float _weight = 1.0f;
	private Animator _animator;
	public Animator _animatorSave;
    private bool isLook = true;
	
	public AnimationCurve unityFreeHitWeight; // Workaround for Unity Free users that don't have access to Mecanim curves
	public AnimationCurve distanceWeight; // Workaround for Unity Free users that don't have access to Mecanim curves
	public AnimationCurve distanceWeight1;
	
	public ArmSystem armLeft;
	public ArmSystem armRight;

	public bool isShot = false;
	public bool useLimb = false;
	public bool useFBBIK = true;
	public bool isEvaluateTarget = true;
	public bool updateParams = true;
	
	public float hitWeight;
	
	private float _height = 0f;
	private float _distance = 0f;
	public float _direction = 0f;
	
	public float _minDistance = 0.4f;
	public float _maxDistance = 2.4f;
	
	public float _minHeight = 0.3f;
	public float _maxHeight = 1.8f;
	
	public Transform _endPoint;
	public Transform _cachedTrans;

	float _temp;
	float _freeTime = 0.5f;
	float _freeTimeCount = 0;
	public float _predictFactor = 2f;
	private float _startTime;
	public float _delayFactor = 1f;
	public float _goalKeeperZ = -0.5f;

	bool updateFrame;
	public float _speedBall;
	private Vector3 _previousPos;
	private float _deltaPos;
	public float _jumpDuration = 1f; // la the time it take cua hanh` dong bay nguoi can~ fa' cua thu mon, jump duration se duoc tinh' at runtime
	public float _timeLeftForBallToReachEndPoint;	// con bao nhieu giay nua la banh se den' diem~ giao cat'
	public bool _isJumped = false;	
	
	Vector3 posTemp;
	public bool _updateDirection = false;
	public bool _isShootEnd;
	private float trueLength;
	public int _isTouchedTheBall = 1; // = 1 nghia la chua cham banh, 0 nghia la cham roi
	public Material _matGK;


	void Awake() {
		share = this;

		_cachedTrans = transform;
		_animator = GetComponent<Animator>();
	}

	void Start() {
		Shoot.EventShoot += eventShootBegin;
		GoalDetermine.EventFinishShoot += eventShootFinish;

	    _ballTarget = BallTarget.share.transform;
	    _animatorSave = GoalKeeperClone.share.GetComponent<Animator>();
	    _ball = ShootAI.share.transform;

        _previousPos = _ball.position;
        _isShootEnd = true;
	}

	private void eventShootBegin() {
		_isJumped = false;
		_isShootEnd = false;
	}

	private void eventShootFinish(bool isGoal, Area area) {
		_isShootEnd = true;
	}


	void OnDestroy() {
		Shoot.EventShoot -= eventShootBegin;
        GoalDetermine.EventFinishShoot -= eventShootFinish;
	}

	void FixedUpdate() {
        if (_isShootEnd)
        {
            return;
        }

		updateFrame = true;
		
		if(isEvaluateTarget)		// tham~ dinh distance, height and direction cua ball
			evaluateTarget();     
		
		if(updateParams && 0f > _ball.GetComponent<Rigidbody>().position.z) 			// update parameter dzo animator va animator clone
			updateParamToAnimator();
		
		if(_isJumped == false)		// check xem khi nao thi fai~ jump
			checkJump();
	}
	
	void Update() {
		AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
		
		if(info.IsName("idle") || info.IsName("move left") || info.IsName("move right")) {// && _animator.IsInTransition(0) == false) {
			updateParams = true;
			_updateDirection = true;
//			_isJumped = false;
		}
		else if((info.IsName("save") || info.IsName("save center"))) {
			if(isShot) {
				isShot = false;
				trueLength = info.length;
			}

			updateParams = true;
			_updateDirection = false;

		}
		else {
			updateParams = false;
			_updateDirection = false;
		}
	}

    private AnimationCurve curveLinear = AnimationCurve.Linear(10f, 0f, 0f, 1f);

    private void OnAnimatorIK()
    {
        hitWeight = GetHitWeight(unityFreeHitWeight);
        
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, _ballTarget.position);
        _animator.SetIKPosition(AvatarIKGoal.RightHand, _ballTarget.position);

        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _weight * hitWeight * (_direction * 2));
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _weight * hitWeight * ((1 - _direction) * 2));

        //if (isLook)
        //{		// the head look at the ball
        //    _animator.SetLookAtPosition(_ball.position);
        //    _animator.SetLookAtWeight(curveLinear.Evaluate(Mathf.Abs(_ballTarget.position.z - _ball.position.z)));
        //}
    }
	// Update is called once per frame
    private void LateUpdate()
    {
        if (!updateFrame) return; // If no FixedUpdate has been called, do nothing
        updateFrame = false;

        UpdateIK();
    }

    public void UpdateIK()
    {
		hitWeight = GetHitWeight(unityFreeHitWeight);
	}

	private float GetHitWeight(AnimationCurve curve) {
		// Workaround for Unity Free users
		// If you have Unity Pro, use Mecanim curves instead: return animator.GetFloat("HitWeight");
		AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
		
		if(info.IsName("save") || info.IsName("save center")) {
// weight duoc xac dinh bang multiplication giua~ normalized time cua animation va khoang cach' cua trai banh den' diem~ giao cat' mat phang~ gan thu mon theo chieu z
			
            float max = Mathf.Max(GoalKeeperClone.share.nearDistanceLeft, GoalKeeperClone.share.nearDistanceRight);
			
			if(_ballTarget.position.y < 2.5f)	// neu x cua banh nam trong tam bat va y cua banh ko cao hon xa` ngang thi tinh toa' weight
				return _isTouchedTheBall * curve.Evaluate(info.normalizedTime) * distanceWeight.Evaluate(Mathf.Abs(_ballTarget.position.z - _ball.position.z));
			else 		// ko thao dieu kien thi cho weight = 0 luon, ko can IK focus dzo target, se lam model bi meo' mo' xau'
				return 0;
		}
		
		return 0;
	}

	private void checkJump() {
		AnimatorStateInfo info = _animatorSave.GetCurrentAnimatorStateInfo(0);
		_jumpDuration = info.length * _delayFactor; // + info.length * 0.07f;			// lay jump duration tu` clone wa
		_speedBall = _ball.GetComponent<Rigidbody>().velocity.z;
		
		if(_speedBall > 0) {		// van toc' ball > 0 thi moi' tinh'
			_timeLeftForBallToReachEndPoint = (_ballTarget.position.z - _ball.GetComponent<Rigidbody>().position.z) / _speedBall;		// tinh' thoi gian de~ trai' banh bay den' endpoint
			if(_timeLeftForBallToReachEndPoint >= 0 && _timeLeftForBallToReachEndPoint <= _jumpDuration) {		// neu' thoi gian nay` <= 1 giay thi` cho thu mon bay nguoi can~ fa'
				_isJumped = true;
				_startTime = (_jumpDuration - _timeLeftForBallToReachEndPoint) * _predictFactor;
				doJump(_startTime);
			}
		}
	}

	private void doJump(float startTime) {
		if(_distance < 0.1f) {
			_animator.Play("save center", 0, startTime);
		}
		else {
			_animator.Play("save", 0, startTime);
		}
		isShot = true;
	}

    public void evaluateTarget()
    {
		float val = _ballTarget.position.x - _cachedTrans.position.x;		// khoang cach giua _ballTarget va thu mon theo truc x (_balltarget la diem giao cat giua~ duong` di cua banh va mat phang ngay vi tri thu mon dung')
		float val1 = val;
		
		val1 = Mathf.Clamp(val1, -_minDistance, _minDistance);			// tinh' huong' coi left hay right
		_direction = (val1 + _minDistance) / (2 * _minDistance);		
		
		val = Mathf.Abs(val);
		val = Mathf.Clamp(val, _minDistance, _maxDistance); 			// tinh khoang cach coi xa hay gan`, thuc ra cung ko can thiet ve class GoalKeeperHorizontalFly da lam nhiem vu tinh khoang cach de tu do' xach dinh xem thu mon fai~ bay theo truc x bao xa
		_distance = (val - _minDistance) / (_maxDistance - _minDistance);
		
		val = Mathf.Clamp(_ballTarget.position.y, _minHeight, _maxHeight);		// tinh' height
		val = (val - _minHeight) / (_maxHeight - _minHeight);
		_height = val;
	}

    private void updateParamToAnimator()
    {
        if (_animator.enabled)
        {
            if (_updateDirection)
            {
			    _animator.SetFloat("direction", _direction);
			    _animatorSave.SetFloat("direction", _direction);
		    }
		
		    _animator.SetFloat("distance", _distance);
		    _animatorSave.SetFloat("distance", _distance);


		    if( _isTouchedTheBall == 1 ) {		// neu chua cham banh thi van cho cap nhat height
			    _animator.SetFloat("height", _height);
			    _animatorSave.SetFloat("height", _height);
		    }
        }
	}

	// ham nay dung de xac dinh giao diem duong` phan giac' tu` vi' tri' dat banh den' khung thanh
	// tam giac dang xet duoc tao thanh tu 3 diem : cot doc trai', cot doc fai, diem dat banh.
	// tim giao diem cua duong phan giac' tu` diem dat banh den' canh day' tao boi 2 cot doc
	// dung Vector3.lerp de lay diem dat thu mon khi biet gia tri cua _goalKeeperZ
	private void placeGK() {
        
        //Reset lai vi tri cua Ball
		Vector3 ballPosition = Shoot.share._ball.transform.position;
		ballPosition.y = 0.2f; //Dat Ball tren mat san
		Vector3 poleLeft = GoalDetermine.share._poleLeft.position;
        poleLeft.y = 0; //old value: 0
		Vector3 poleRight = GoalDetermine.share._poleRight.position;
        poleRight.y = 0; //old value: 0

		Vector3 intersection = Vector3.zero;
		intersection.x = ( poleRight.x + ((poleRight - ballPosition).magnitude / (poleLeft - ballPosition).magnitude) * poleLeft.x) / (1 +  ((poleRight - ballPosition).magnitude / (poleLeft - ballPosition).magnitude));
		intersection.z = ( poleRight.z + ((poleRight - ballPosition).magnitude / (poleLeft - ballPosition).magnitude) * poleLeft.z) / (1 +  ((poleRight - ballPosition).magnitude / (poleLeft - ballPosition).magnitude));
        intersection.y = 0.9f; //Reset lai vi tri thu mon giong thuoc tinh y = 0.9 cua Goalkeeper_rig

        //Reset vi tri cua Goal keeper
        //Tinh giao diem cua duong phan giac de chinh vi tri GoalKeeper vao giua.
        _cachedTrans.position = Vector3.Lerp(ballPosition, intersection , (_goalKeeperZ - ballPosition.z) / (intersection.z - ballPosition.z));


        Debug.Log(string.Format("<color=#c3ff44>Place GK Pos = {0}</color>", _cachedTrans.position.ToString()));
	}

    private void OnCollisionEnter(Collision other)
    {
		if(other.gameObject.tag.Equals("Ball")) {
			_isTouchedTheBall = 0;

			if(_isShootEnd == false) {		// neu chua het luot sut thi banh trung thu mon moi' cho sound, con het luot sut roi thi co' trung thu mon cung ko play sound gi het
			    EventBallHitGK();
			}

		}
	}

    public void reset()
    {
        //		Debug.Log("reset GoalKeeper");
        _isTouchedTheBall = 1;
        _isJumped = true;
        _animator.Play("idle");
        //Reset lai vi tri cua Goalkeeper
        placeGK();
    }

	public void setUniform(Country country) {
		_matGK.mainTexture = (Texture2D) Resources.Load( "Uniform/UniformFootball_" + country.ToString());
	}

	#region event methods
	void onAnimEnd_Idle() {
		reset();
	}
	#endregion
}
