using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System;
using Random = UnityEngine.Random;

public enum Direction {
	Left = 0,
	Right,
	Both
}

public class ShootAI : Shoot {

	public static ShootAI shareAI;
  

    public static Action<float> EventChangeCurveRange = delegate {};

	private List<Vector3> _ballPath;
	public int _index;
	private bool _isShootByAI = false;

    private MyMultiSelection _multiSelection;
	public float _remainTimeToNextPointZ;
	private float yVelocityTarget;
	private float angleZY;
	private bool _isDown;

	public bool _isTestShootAI = false;


    public float _maxLeft = 0f;
    public float _maxRight = 0f;
	
	
	
	/*************** Debug ****************/
    public float _curveLevel = 0;
    public float _difficulty = 0.5f;
	public bool willBeShootByUser = true;
    public Direction _shootDirection = Direction.Left;
    //public Direction _cornerShootDirection = Direction.Left;
    private List<Vector3> _debugPath = new List<Vector3>();
	
	void OnDrawGizmos()
	{
		if(_ballPath != null && _ballPath.Count > 0) {
			iTween.DrawLineGizmos(_debugPath.ToArray(), Color.red);
		}
	}
	
	/*******************************/

	protected override void Awake() {
		base.Awake();
		shareAI = this;
		_ballPath = new List<Vector3>();
		_shootDirection = Direction.Both;
	}

	protected override void Start() {
		base.Start();
		_isShootByAI = false;
		_multiSelection = GetComponent<MyMultiSelection>();
		_multiSelection._gos = new List<GameObject>();
	}

	protected override void Update() {

        //Goi ham Shoot bong thong thuong
		if(GoalKeeperHorizontalFly.share.IsAIControl && willBeShootByUser) {
            //Goi ham Update cua Shoot.CS
			base.Update();

		}
        //Neu willBeShootByUser = false, IsAIControl =false, _isShootByAI = true, _isShooting = true
        else if (_isShooting && _isShootByAI) {

			while(_cachedTrans.position.z > _ballPath[_index].z && _index >= 1) {
				--_index;
			}

			if( _cachedTrans.position.z > _ballPath[_index].z && --_index < 0) {		// neu vi tri cua banh da~ vuot wa diem dang xet va khong con diem~ de xet' thi stop
				_isShootByAI = false;
			}
			else {
				// dai y' : lay' diem dang xet ra, tinh' toan' goc', tu do' tinh' toan van toc' theo x,y,z lam sao de banh bay duoc den' diem~ dang xet'


				Vector3 temp = (_ballParent.InverseTransformPoint(_ballPath[_index]) - _cachedTrans.localPosition).normalized;		// convert vi tri diem dang xet ve he toa do cua parent cua trai banh, lay vector chi phuong
				float angleZX = Mathf.Atan2(temp.x, temp.z);			// tinh goc'
				angleZY = Mathf.Atan2(temp.y, temp.z);

				float xVelocityTarget = _zVelocity * Mathf.Tan(angleZX);		// tinh van toc theo truc x
//				xVelocityTarget = Mathf.Clamp(xVelocityTarget, -factorLeftRightConstant * 0.7f, factorLeftRightConstant * 0.7f);		// clamp van toc theo truc x voi' cac' limit, nho` co' dong` nay` ma` ve mat ngu~ nghia~ may' ko choi an gian, no' cung bi gioi' han nhu nguoi` choi, ko fai muon' xoay' sao thi xoay'

				float tempVel = _zVelocity * Mathf.Tan(angleZY);			// tinh van toc theo truc y

				if(tempVel < yVelocityTarget) {			// check xem banh dang di xuong fai ko
					if(_isDown == false) {				// neu o~ frame truoc do' banh con dang di len
						_isDown = true;					
					}
				}
				else {					// banh dang di len
					_isDown = false;
				}

//				yVelocityTarget = Mathf.Clamp(tempVel, _yVelocityLimitDown, factorUpConstant);
				yVelocityTarget = tempVel;

				angleZY = angleZY * Mathf.Rad2Deg;

//				_ball.velocity = _ballParent.TransformDirection(new Vector3(xVelocityTarget, yVelocityTarget, _zVelocity));
				_remainTimeToNextPointZ = Mathf.Abs((_ballPath[_index].z - _cachedTrans.position.z)) / _ball.velocity.z;		// thoi gian con lai de trai banh di den diem dang xet voi' van toc' z hien tai

				Vector3 speed = _ballParent.InverseTransformDirection(_ball.velocity);			// convert van toc global cua trai banh ve he truc toa do cua cha trai' banh
				speed.z = _zVelocity;
				speed.x = Mathf.Lerp(speed.x, xVelocityTarget, _remainTimeToNextPointZ);
				speed.y = Mathf.Lerp(speed.y, yVelocityTarget, _remainTimeToNextPointZ);

				_ball.velocity = _ballParent.TransformDirection(speed);			// convert nguoc lai thanh van toc global roi gan' cho trai banh
//				_ball.angularVelocity = new Vector3(0, xVelocityTarget, 0f);

			}
		}
	}

	public override void goalEvent(bool isGoal, Area area) {
		base.goalEvent(isGoal, area);
	}

	public void shoot() {
		shoot (_shootDirection, _curveLevel, _difficulty);
	}
    //Henry add

    public void shoot(Direction shootDirection, float curveLevel, float difficulty) {
//		Debug.Log("AI shoot");
		EventShoot();

		_isShootByAI = true;

		_ballPath.Clear();

		_ballPath = createPath(shootDirection, curveLevel, difficulty, _cachedTrans.position); 
		_index = _ballPath.Count - 1;			// di nguoc tu cuoi' mang~ ve dau` mang~

		/*************** Debug ****************/
		_multiSelection.count = _ballPath.Count;
		for(int i = 0; i < _multiSelection._gos.Count; ++i) {
			Destroy(_multiSelection._gos[i]);
		}
		_multiSelection._gos.Clear();



		for(int i = 0; i < _ballPath.Count; ++i) {
			GameObject go = new GameObject();
			go.transform.position = _ballPath[i];
			_multiSelection._gos.Add(go);

		}
		/*******************************/
	}

 

    public float yEnd = 2.36f;			
	public float yMiddle = 2.8f;		
	public float xEnd = 3.5f;			

	public float yExtra = 0.5f;
	public Interpolate.EaseType easeType = Interpolate.EaseType.EaseInOutCubic;
	public int slideTest = 6;

	public AnimationCurve _animationCurve;

	public List<Vector3> createPath(Direction direction, float curveLevel, float difficulty, Vector3 ballPostion) {
		float ballDistance = new Vector2(Mathf.Abs(ballPostion.x), Mathf.Abs(ballPostion.z)).magnitude;
		int slide = 10;
		List<Vector3> listTemp = new List<Vector3>();

		Vector3 pointEnd = Vector3.zero;							// diem dich' den'


        Vector3 pointMiddle = Vector3.zero;

		Vector3 pointStart = ballPostion;			// diem bat dau
		
		if(direction == Direction.Both) 
			direction = (Direction) Random.Range(0, 2);



		// *********************** Tinh yMid va yEnd ************************
		float yMidMin = 0.145f;
		float yMidMax = 4.3f;
		float yEnd_Min_When_Mid_Min = 0f, yEnd_Max_When_Mid_Min = 0f;
		float yEnd_Min_When_Mid_Max = 0f, yEnd_Max_When_Mid_Max = 0f;
		float yMid = 0f;
		float yEnd = 0f;

		bool found = false;

		// y nghia: tu thuc nghiem ta thu duoc ket wa khi sut banh o khoang cach' tu` 35m ve` 16.5m
		// moi ket wa gom cac thong so:
		// distance : khoang cach
		// yMidMin : yMin cua pointMiddle 
		// yEnd_Min_When_Mid_Min	: yMin cua pointEnd khi yMiddle dat minimum
		// yEnd_Max_When_Mid_Min	: yMax cua pointEnd khi yMiddle dat minimum
		// yMidMax : yMax cua diem middle 
		// yEnd_Min_When_Mid_Min	: yMin cua pointEnd khi yMiddle dat maximum
		// yEnd_Max_When_Mid_Min	: yMax cua pointEnd khi yMiddle dat maximum

		//	Cach su dung:
		// co' vi tri dat banh, tinh duoc distance dat banh den goc toa do.
		// tu distance se suy ra duoc no' nam trong Range nao`. Duyet mang~ de lay ra cai' Range do'
		// Co' Range roi` tinh' t roi Lerp de ra duoc cac' thong so' nhu tren ung' voi khoang cach dat banh.
		// Co thong so roi` thi random de tinh yMid, roi tu yMid se tinh duoc yEndMin va yEndMax va roi la yEnd


		for(int i = 1; i < dataShootAI.Length; ++i) {		// dataShootAI la mang du~ lieu thu duoc tu` thuc nghiem
			DataShootAI data = dataShootAI[i];
			if(ballDistance <= data._distance) {			// check xem khoang cach fu hop de xet chua
				found = true;
				DataShootAI preMadeData = dataShootAI[i - 1];

				float t = (ballDistance - preMadeData._distance) / (data._distance - preMadeData._distance);

				yMidMin = Mathf.Lerp(preMadeData._yMid_Min, data._yMid_Min, t);
				yMidMax = Mathf.Lerp(preMadeData._yMid_Max, data._yMid_Max, t);
				yEnd_Min_When_Mid_Min = Mathf.Lerp(preMadeData._yEnd_Min_When_Mid_Min , data._yEnd_Min_When_Mid_Min, t);
				yEnd_Max_When_Mid_Min = Mathf.Lerp(preMadeData._yEnd_Max_When_Mid_Min , data._yEnd_Max_When_Mid_Min, t);
				yEnd_Min_When_Mid_Max = Mathf.Lerp(preMadeData._yEnd_Min_When_Mid_Max , data._yEnd_Min_When_Mid_Max, t);
				yEnd_Max_When_Mid_Max = Mathf.Lerp(preMadeData._yEnd_Max_When_Mid_Max , data._yEnd_Max_When_Mid_Max, t);

				yMid = Random.Range(yMidMin, yMidMax);

				t =  Mathf.Abs(yMid - yMidMin) / Mathf.Abs(yMidMax - yMidMin);
				float yEndMin = Mathf.Lerp(yEnd_Min_When_Mid_Min, yEnd_Min_When_Mid_Max, t);
				float yEndMax = Mathf.Lerp(yEnd_Max_When_Mid_Min, yEnd_Max_When_Mid_Max, t);

				yEnd = Random.Range(yEndMin, yEndMax);

				break;
			}
		}

		if(found == false) {		// neu for loop o tren ma ko thoa du chi 1 lan co' nghia~ la vi tri' da' banh >= 35m, nhu vay lay du lieu cua 35m ra doi` dung` lien`, ko can fai lerp nhu tren
			DataShootAI data = dataShootAI[dataShootAI.Length - 1];   // dataShootAI.Length - 1 la vi tri du~ lieu khi sut banh o vi tri 35m
			yMid = Random.Range(data._yMid_Min, data._yMid_Max);			// random ra yMid

			float t =  Mathf.Abs(yMid - data._yMid_Min) / Mathf.Abs(data._yMid_Max - data._yMid_Min);
			float yEndMin = Mathf.Lerp(data._yEnd_Min_When_Mid_Min, data._yEnd_Min_When_Mid_Max, t);			// co yMid roi se tinh duoc yEndMin
			float yEndMax = Mathf.Lerp(data._yEnd_Max_When_Mid_Min, data._yEnd_Max_When_Mid_Max, t);			// co yMid roi se tinh duoc yEndMax

			yEnd = Random.Range(yEndMin, yEndMax);			// random ra y end
		}
		// ***********************************************


		// *********************** Point End ************************
		float xTemp;
		float xMin = Mathf.Lerp(0, 3.3f, difficulty);
//		float xMax = Mathf.Lerp(3.6f, 3.5f, (ballDistance - 16f) / (22f - 16f));		// banh cang gan 16m thi cho xmax cang ra xa, tai vi cuoi ham nay` minh se fai remove nhung diem co' z gan` khung thanh 3m hoac nho hon
		float xMax = 3.45f;
		
		if(direction == Direction.Right)
			xTemp = Random.Range(xMin, xMax);
		else
			xTemp = Random.Range(-xMax, -xMin);
		
		pointEnd = new Vector3(xTemp, yEnd, 0f);		// diem dau tien duoc add vo List cung la diem cuoi' cung se duoc lay ra
//		pointEnd.y = yEnd;
//		pointEnd.x = xEnd;
		// ***********************************************


		// ********************** Tim x la do be~ cong  *************************
		float x = (pointEnd.x + pointStart.x) / 2f;

		if(Mathf.Abs(pointStart.z) <= 32f ) {
			// khi sut ra 2 goc' 2 ben cot doc thi fai co' gioi' han ve` do xoay' ra ngoai`. Do xoay' trong thi ko can care vi = 3 van an toan, banh ko bi vang ra ngoai
			// do xoay' ngoai duoc gioi' han lai theo cong thuc duoi' day khi z cua banh cach khung thanh <= 30m, > 30m thi do xoay' ngoai co' the bang maximum va van an toan, banh bao dam xoay' dzo goal
			// y tuong cua cong thuc la wa thuc nghiem ta biet duoc do xoay' ngoai an toan khi sut banh ra 2 goc (x = 3.4f hay = -3.4f) o khoang cach 30m la 3m, 16.5m la 0m. Ta se dung Mathf.Lerp de noi suy
			// khi sut banh gan` vi tri giua~ thi do xoay' ngoai` di~ nhien se duoc tang len cao hon ma ko so banh xoay' ra ngoai khung thanh, do' la ly' do tai sao co' 2 dong Lerp o duoi'

			float maxCurve = Mathf.Lerp(0, 3.5f, _animationCurve.Evaluate((Mathf.Abs(pointStart.z) - 16.5f) / (32f - 16.5f))); // maxcurve la do xoay ngoai` toi' da khi da' vao` diem~ co' x = 3.4 hoac -3.4
			_maxRight = Mathf.Lerp(3.5f, maxCurve, pointEnd.x / (3.4f));
			_maxLeft = Mathf.Lerp(-3.5f, -maxCurve, pointEnd.x / (-3.4f));
		}
		else {
			_maxLeft = -3.5f;
			_maxRight = 3.5f;
		}

		float curveFactor = Mathf.Lerp(0f, 0.95f, curveLevel);			// do kho cua xoay'
		float a = Random.Range(_maxLeft, _maxLeft * curveFactor);		// random xoay ben fai
		float b = Random.Range(_maxRight * curveFactor, _maxRight);		// random xoay ben trai
		float xCurve = (((int) Random.Range(0, 2)) == 0) ? a : b;				// random chon xoay ben fai hay ben trai

//		float xCurve = Mathf.Lerp(_maxLeft, _maxRight, curveLevel);					// do be cong duong di cua banh

	    EventChangeCurveRange(xCurve);
		x += xCurve;		// lam cho x cua diem giua~ bi lech se duoc ket qua la lam cho duong banh bi be cong xoay'
		// ***********************************************


		listTemp.Clear();
		if( pointStart.z >= -22f) {			// neu nhu diem dat banh cach' khung thanh 22m do~ lai thi tinh diem middle thuc su
			pointMiddle.x = x;
			pointMiddle.y = yMid;
			pointMiddle.z = (pointEnd.z + pointStart.z) / 2f;
		}
		else {	// neu nhu diem dat banh cach' khung thanh > 22m thi coi diem dat hang rao la diem middle, ket qua~ cug~ ok ko sao.
			pointMiddle.z = pointStart.z + 11f;		// z tai cho dat hang rao
			pointMiddle.x = x;
			pointMiddle.y = yMid;
		}

		// cac buoc tren la ap dung cho khi co' hang rao, neu' ko co' hang rao thi` chi can dzo if nay la ok, moi thu van dung. ko can chia 2 truong hop cho cac buoc tren
		if(Wall.share != null && !Wall.share.IsWall) {		// ko co hang rao
			pointEnd.y = Random.Range(0.145f, 2.8f);
			pointMiddle.y = Random.Range(0.145f, 4.3f);
			pointMiddle.z = (pointEnd.z + pointStart.z) / 2f;
		}

		listTemp.Clear();
		listTemp.Add(pointEnd);
		listTemp.Add(pointMiddle);
		listTemp.Add(pointStart);

		// ******************* tim diem cach khung thanh mot khoang cach mindistance *********************
		Vector3 closestPointToGoal = findPointInPathAtZ(Interpolate.Ease(easeType)  , listTemp.ToArray(), slide, -3f);
		// ****************************************


		// ******************** Final path ********************
		List<Vector3> retVal = new List<Vector3>();
		IEnumerator<Vector3> nodes = Interpolate.NewBezier(Interpolate.Ease(easeType), listTemp.ToArray(), slide).GetEnumerator() ;		// tao ra path gom cac diem
		_debugPath.Clear();

		while( nodes.MoveNext() ) {			// lay cac diem cua path ra
			_debugPath.Add(nodes.Current);

            //if(nodes.Current.z < -3f)
            if (nodes.Current.z < -_ballControlLimit)
				retVal.Add(nodes.Current);
		}
		retVal.Insert(0, closestPointToGoal); 
		retVal.RemoveAt(retVal.Count - 1);

        //if( Vector3.Distance(retVal[0], retVal[1]) <= 4f)
        //    retVal.RemoveAt(1);
		// ****************************************


		return retVal;
	}

	private Vector3 findPointInPathAtZ(Interpolate.Function ease, Vector3[] points, int slide, float z) {
		Vector3 pointBefore = Vector3.zero;
		Vector3 pointAfter = Vector3.zero;

		List<Vector3> temp = new List<Vector3>();

		IEnumerator<Vector3> nodes = Interpolate.NewBezier(ease, points, slide).GetEnumerator() ;		// tao ra path gom cac diem
		while( nodes.MoveNext() ) {			// lay cac diem cua path ra
			temp.Add(nodes.Current);
		}
		for(int i = 0; i < temp.Count; ++i) {			// tiem diem truoc' va sau diem hang rao
			if(temp[i].z < z) {
				pointBefore = temp[i];
				pointAfter = temp[i - 1];
				break;
			}
		}
		return Vector3.Lerp(pointBefore, pointAfter, (z - pointBefore.z) / (pointAfter.z - pointBefore.z));		
	}

	public override void reset(float x, float z) {
//		Debug.Log("Reser Shoot AI : x = " + x + "\t z = " + z);
		base.reset(x, z);
		_isShootByAI = false;
	}

	public override void reset() {
//		Debug.Log("Reser Shoot AI");
		base.reset();
		_isShootByAI = false;
	}


    [ System.Serializable ]
	public class DataShootAI {
		public float _distance;

		public float _yMid_Min;
		public float _yEnd_Min_When_Mid_Min;
		public float _yEnd_Max_When_Mid_Min;

		public float _yMid_Max;
		public float _yEnd_Min_When_Mid_Max;
		public float _yEnd_Max_When_Mid_Max;

		public DataShootAI(float distance, float yMid_Min, float yEnd_Min_When_Mid_Min, float yEnd_Max_When_Mid_Min, float yMid_Max, float yEnd_Min_When_Mid_Max, float yEnd_Max_When_Mid_Max) {
			_distance = distance;

			_yMid_Min = yMid_Min;
			_yEnd_Min_When_Mid_Min = yEnd_Min_When_Mid_Min;
			_yEnd_Max_When_Mid_Min = yEnd_Max_When_Mid_Min;

			_yMid_Max = yMid_Max;
			_yEnd_Min_When_Mid_Max = yEnd_Min_When_Mid_Max;
			_yEnd_Max_When_Mid_Max = yEnd_Max_When_Mid_Max;
		}
	}

	private DataShootAI[] dataShootAI = new DataShootAI[] {
		// distance, yMid_Min, yEnd_Min_When_Mid_Min, yEnd_Max_When_Mid_Min, yMid_Max, yEnd_Min_When_Mid_Max, yEnd_Max_When_Mid_Max
		
		new DataShootAI(16.5f, 2.6f, 2.1f, 2.4f, 4.3f, 0.145f, 0.76f)
		,new DataShootAI(19f, 2.8f, 2.2f, 2.4f, 4.3f, 0.145f, 1.5f)
		,new DataShootAI(21f, 2.8f, 2.2f, 2.4f, 4.3f, 0.145f, 1.8f)
		,new DataShootAI(23f, 2.8f, 2.3f, 2.55f, 4.3f, 0.145f, 2.2f)
		,new DataShootAI(27f, 3f, 1.8f, 2.6f, 4.3f, 0.145f, 2.6f)
		,new DataShootAI(35f, 3.2f, 1.6f, 2.8f, 4.3f, 0.145f, 2.8f)
	};	
}
