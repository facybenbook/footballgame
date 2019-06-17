using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class GoalKeeperHorizontalFly : MonoBehaviour {

	public static GoalKeeperHorizontalFly share;

    public static Action<float> EventChangeFlyDistance = delegate {};
    public static Action<bool> EventChangeIsAIControl = delegate { };

	private GoalKeeper _goalKeeper;
	private Animator _animator;

	public float _deltaDistance = 1f;
	private bool _superGoalKeeper = false;
	private bool _isShoot = false;
    public AnimationCurve _curve;
    private Transform _cachedTrans;

    public bool _isAIControl;
    public float deltaX;
    public float beginX;
    public float endX;
    public float _postSaveMoveSpeed = 1f;
    public float _postSaveMoveDuration = 1f;
    private float _postSaveMoveDurationDeltaTime;
    private float _sign;
    private bool previousIsSaving = false;
    private bool isSaving = false;
    private bool justFinishSaving = false;

    public float normalizedTime;
    public float valX;

	public bool IsAIControl {
		get {
			return _isAIControl;
		}
		set {
			_isAIControl = value;
		    if (_isAIControl == false)
		    {
		        GoalKeeperLevel.share.setGKControlByHuman();
		    }
		    else
		    {
		        GoalKeeperLevel.share.setGKPreviousLevel();
		    }
		    EventChangeIsAIControl(_isAIControl);
		}
	}

	void Awake() {
		share = this;
		_isShoot = false;
        //Khoi tao gan _cachedTrans bang Goalkeeper fly transform
        _cachedTrans = transform;

	}

	// Use this for initialization
	IEnumerator Start () {
		_goalKeeper = GetComponent<GoalKeeper>();
		_animator = GetComponent<Animator>();

		setFlyDistance(0.5f);

		Shoot.EventShoot += eventShoot;
		GoalDetermine.EventFinishShoot += finishShoot;

	    yield return new WaitForEndOfFrame();
		IsAIControl = true;
	}

    void OnDestroy()
    {
        GoalDetermine.EventFinishShoot -= finishShoot;
    }

    public void MoveGKToLeft()
    {
        //Debug.Log("Move Left at frame : " + Time.frameCount);
		_animator.SetInteger("move", -1);
	}

    public void MoveGKToRight()
    {
        //Debug.Log("Move Right at frame : " + Time.frameCount);
		_animator.SetInteger("move", 1);
	}

	private void eventShoot() {
		_isShoot = true;
	}

	private void finishShoot(bool isGoal, Area area) {
		_isShoot = false;
	}

	public void setFlyDistance(float distance) {
//		Debug.Log("setFlyDistance : " + distance);
		_deltaDistance = distance;
        EventChangeFlyDistance(_deltaDistance / 4f);
	}

	void calculateFlyingDistance() {			// ham nay dung de tinh toa' xem thu mon fai~ bay sang ben trai hay phai~ bao xa, va tinh van toc' quan' tinh' sau do'

		float val = _goalKeeper._ballTarget.position.x;
		float distance = val - _cachedTrans.position.x; // khoang cach tu trai banh den thu mon
		_sign = Mathf.Sign(distance);												// lay' dau' cua khoang cach tu trai banh den thu mon
		float distanceMagnitude = Mathf.Abs(distance);								// lay do lon cua khoang cach tu trai banh den thu mon

		float distanceNear;
		if(GoalKeeper.share._direction == 0)
			distanceNear = GoalKeeperClone.share.nearDistanceRight;
		else 
			distanceNear = GoalKeeperClone.share.nearDistanceLeft;

		float distanceFar = distanceNear + _deltaDistance;

		if(_isAIControl) {
			_animator.SetInteger("move", 0);

			if(distanceMagnitude > distanceFar - _deltaDistance * 0.7f) { 	// khi x cua _balltarget xa hon tam bat distancefar - _deltaDistance * 0.7 cua thu mon thi se can nhac cho thu mon di chuyen,
				// tai sao ko chi la > _distanceFar ma fai la > distancefar - _deltaDistance * 0.7   ?????
				// tai vi doi' voi' thu mon cap' cao, _deltaDistance se la 2.5m, nhu vay khi banh cach' thu mon khoang 2.5m thi thu mon ko can` di chuyen
				// banh bay toi' noi thi thu mon bay cai' veo` ra can~ fa', nhin` cung~ hoi vo ly.
				// cho nen du cho thu mon co' _deltaDistance cao di chang nua thi cung ko nen chu quan khinh dich, banh o~ xa mac du van~ trong tam` voi'
				// nhung van~ bat' thu mon fai di chuyen, nhin` se~ hop ly' hon

				_animator.SetInteger("move", (int)-_sign);
			}
		}

		distanceMagnitude = Mathf.Clamp(distanceMagnitude, 0, distanceFar);

		
		deltaX = distanceMagnitude - distanceNear;									// tim` do chenh lech giua~ gioi' han bat bong gan` va khoang cach hien tai trai banh
		beginX = _cachedTrans.position.x;
		endX = beginX + deltaX * _sign;											//  tu` do' se~ suy ra duoc thu mon fai~ bay xa bao nhieu meter thi co' the~ voi' toi' trai banh
		
		_postSaveMoveSpeed = deltaX * 2 ;// / _goalKeeper._jumpDuration;		// tinh' van toc' quan' tinh' theo truc x ap dung khi thu mon roi xuong
		
//		Debug.Log("deltaX = " + deltaX + "\t beginX = " + beginX + "\t endX = " + endX);
	}

	// Update is called once per frame
	void Update () {
		AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);

		if( _isShoot ) {

			if(info.IsName("idle") || info.IsName("move left") || info.IsName("move right")) {

				calculateFlyingDistance();
			}
			else if(info.IsName("save")) { // || info.IsName("save center")) {

				if(info.normalizedTime < 0.5f && _superGoalKeeper)			// testing super goal keeper
					calculateFlyingDistance();

				Vector3 pos = _cachedTrans.position;
				normalizedTime = info.normalizedTime;
				valX = Mathf.Lerp(beginX, endX, _curve.Evaluate(info.normalizedTime));		// thu mon bay theo truc x theo nhung tham so' da duoc tinh' san~
				pos.x = valX;
				_cachedTrans.position = pos;

				isSaving = true;
				justFinishSaving = false;
			}
			else {
				isSaving = false;
			}

			if(previousIsSaving == true && isSaving == false) {		// neu o frame truoc' con dang cuu' bong ma frame nay da ko con thi co' nghia la moi' chuyen thu state save sang post-save
				justFinishSaving = true;				// justFinishSaving = true nghia la vua xong saving,
			}

			previousIsSaving = isSaving;
		}
		else if(_isAIControl && _animator.enabled)
		{
            //Debug.Log("Set animator move param = 0 at frame : " + Time.frameCount);
			_animator.SetInteger("move", 0);
		}

		if(justFinishSaving)
			moveAfterSave();

		if(_isAIControl == false && (info.IsName("idle") || info.IsName("move left") || info.IsName("move right")))
        {

#if UNITY_EDITOR || UNITY_WEBGL || UNITY_WEBPLAYER || UNITY_STANDALONE
            float h = Input.GetAxis("Horizontal");
			if(Mathf.Abs(h) > 0.001f) {
				_animator.SetInteger("move", (int)Mathf.Sign(h));
			}
			else if(Input.GetMouseButton(0) == false) {
				_animator.SetInteger("move", 0);
			}

#elif UNITY_IPHONE || UNITY_ANDROID
			if(Input.touchCount == 0 && _animator.enabled) {
				_animator.SetInteger("move", 0);
			}
#endif
        }
	}

	public void moveAfterSave() {		// sau khi bay nguoi` thi thu mon se tiep' tuc tinh tien' theo truc x nhu la quan' tinh' 1 khoang thoi gian nho~
//		Debug.Log("moveAfterSave");
		_postSaveMoveDurationDeltaTime += Time.deltaTime;
		if(_postSaveMoveDurationDeltaTime >= _postSaveMoveDuration) {
			reset();
		}
		Vector3 pos = _cachedTrans.position;
		pos.x += (Time.deltaTime * _postSaveMoveSpeed * _sign);
		_cachedTrans.position = pos;
	}

	public void reset() {
		_postSaveMoveDurationDeltaTime = 0;
		justFinishSaving = false;
		_isShoot = false;
	}

}
