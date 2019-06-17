using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System;
using Random = UnityEngine.Random;


public enum cornerDirection
{
    Left = 0,
    Right,
    Both
}
public class ShootCorner : Shoot {

    public static ShootCorner shareCorner;//shareAI;



    public static Action<float> EventChangeCurveRange = delegate {};

	private List<Vector3> _ballPath;
	public int _index;
	public bool _isCornerShoot = false;
    private MyMultiSelection _multiSelection;
	public float _remainTimeToNextPointZ;
	private float yVelocityTarget;
	private float angleZY;
	private bool _isDown;

	public bool _isTestCornerShoot = false;


    public float _maxLeft = 0f;
    public float _maxRight = 0f;
	
	
	
	/*************** Debug ****************/
    public float _curveLevel = 0;
    public float _difficulty = 0.5f;
	//public bool willBeShootByUser = true;
    //public Direction _shootDirection = Direction.Left;
    public cornerDirection _cornerShootDirection = cornerDirection.Left;
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
		shareCorner = this;
		_ballPath = new List<Vector3>();
        _cornerShootDirection = cornerDirection.Both;
	}

	protected override void Start() {
		base.Start();
		_isCornerShoot = false;
		_multiSelection = GetComponent<MyMultiSelection>();
		_multiSelection._gos = new List<GameObject>();
	}

	protected override void Update() {

        //Goi ham Shoot bong thong thuong
        //if(GoalKeeperHorizontalFly.share.IsAIControl && willBeShootByUser) {
        //Goi ham Update cua Shoot.CS
        //	base.Update();

        //}
        //Henry edit: Tuy theo khu vuc cua Ball ma set lai Camera tuong ung
        //Neu Ball dang o goc trai sut phat
        //Neu Ball di duoc 50% quang duong -> Chuyen ve camera Corner

      /*
        if (Math.Abs(_cachedTrans.position.x) > 35f)
        {
            SwitchCamera.share.setActiveCamera("CameraMain");

        }
        else if (Math.Abs(_cachedTrans.position.x) < 35f)
        {
            SwitchCamera.share.setActiveCamera("CameraHeadKick");

        } */


        if (_isCornerShoot)
        {
        
        //Neu willBeShootByUser = false, IsAIControl =false, _isShootByAI = true, _isShooting = true
         if (_isShooting) {

			while(_cachedTrans.position.z < _ballPath[_index].z && _index >= 1) 
                { --_index; }

            if (_cachedTrans.position.z < _ballPath[_index].z && --_index < 0)
                {       // neu vi tri cua banh da~ vuot wa diem dang xet va khong con diem~ de xet' thi stop
                    _isCornerShoot = false; //old _isShootByAI
                }
             else
                {
                    // dai y' : lay' diem dang xet ra, tinh' toan' goc', tu do' tinh' toan van toc' theo x,y,z lam sao de banh bay duoc den' diem~ dang xet'


                    Vector3 temp = (_ballParent.InverseTransformPoint(_ballPath[_index]) - _cachedTrans.localPosition).normalized;      // convert vi tri diem dang xet ve he toa do cua parent cua trai banh, lay vector chi phuong
                    float angleZX = Mathf.Atan2(temp.x, temp.z);            // tinh goc'
                    angleZY = Mathf.Atan2(temp.y, temp.z);

                    float xVelocityTarget = _zVelocity * Mathf.Tan(angleZX);        // tinh van toc theo truc x
                     //				xVelocityTarget = Mathf.Clamp(xVelocityTarget, -factorLeftRightConstant * 0.7f, factorLeftRightConstant * 0.7f);		// clamp van toc theo truc x voi' cac' limit, nho` co' dong` nay` ma` ve mat ngu~ nghia~ may' ko choi an gian, no' cung bi gioi' han nhu nguoi` choi, ko fai muon' xoay' sao thi xoay'

                    float tempVel = _zVelocity * Mathf.Tan(angleZY);            // tinh van toc theo truc y

                    if (tempVel < yVelocityTarget)
                    {           // check xem banh dang di xuong fai ko
                        if (_isDown == false)
                        {               // neu o~ frame truoc do' banh con dang di len
                            _isDown = true;
                        }
                    }
                    else
                    {                   // banh dang di len
                        _isDown = false;
                    }

                    //				yVelocityTarget = Mathf.Clamp(tempVel, _yVelocityLimitDown, factorUpConstant);
                    yVelocityTarget = tempVel;

                    angleZY = angleZY * Mathf.Rad2Deg;

                    //				_ball.velocity = _ballParent.TransformDirection(new Vector3(xVelocityTarget, yVelocityTarget, _zVelocity));
                    _remainTimeToNextPointZ = Mathf.Abs((_ballPath[_index].z - _cachedTrans.position.z)) / _ball.velocity.z;        // thoi gian con lai de trai banh di den diem dang xet voi' van toc' z hien tai

                    Vector3 speed = _ballParent.InverseTransformDirection(_ball.velocity);          // convert van toc global cua trai banh ve he truc toa do cua cha trai' banh
                    speed.z = _zVelocity;
                    speed.x = Mathf.Lerp(speed.x, xVelocityTarget, _remainTimeToNextPointZ);
                    speed.y = Mathf.Lerp(speed.y, yVelocityTarget, _remainTimeToNextPointZ);
                    //Day la cong thuc quyet dinh Ball bay
                    _ball.velocity = _ballParent.TransformDirection(speed);         // convert nguoc lai thanh van toc global roi gan' cho trai banh
                                                                                    //				_ball.angularVelocity = new Vector3(0, xVelocityTarget, 0f);
                }

			}
		}
	}

	public override void goalEvent(bool isGoal, Area area) {
		base.goalEvent(isGoal, area);
	}


    //Henry add
    public void cornerShoot()
    {
        cornerShoot(_cornerShootDirection, _curveLevel, _difficulty);
    }

   
    public void cornerShoot(cornerDirection cornerShootDirection, float curveLevel, float difficulty)
    {
        //      Debug.Log("AI shoot");
        EventShoot();

        _isCornerShoot = true;

        _ballPath.Clear();

        _ballPath = createPath(cornerShootDirection, curveLevel, difficulty, _cachedTrans.position);
        _index = _ballPath.Count - 1;           // di nguoc tu cuoi' mang~ ve dau` mang~

        /*************** Debug ****************/
        _multiSelection.count = _ballPath.Count;
        for (int i = 0; i < _multiSelection._gos.Count; ++i)
        {
            Destroy(_multiSelection._gos[i]);
        }
        _multiSelection._gos.Clear();



        for (int i = 0; i < _ballPath.Count; ++i)
        {
            GameObject go = new GameObject();
            go.transform.position = _ballPath[i];
            _multiSelection._gos.Add(go);

        }
        /*******************************/
    }

    //Day la cac gia tri se thiet lap diem sut bong di den, tuy nhien x va y se tinh lai, z co dinh
    public float xEndC = 0f; //Se tinh lai bang ham Random
    public float yEndC = 3f; //Se tinh lai bang ham random
    public float zEndC = -13f;
    //Luu y: Ca tham so pointMiddle.z = zEnd (line 247) cung quyet dinh bong co di theo diem den cua Zend hay khong

    //public float yExtra = 0.5f;
	public Interpolate.EaseType easeType = Interpolate.EaseType.EaseInOutCubic;
	//public int slideTest = 6;

	public AnimationCurve _animationCurve;

	public List<Vector3> createPath(cornerDirection direction, float curveLevel, float difficulty, Vector3 ballPostion) {
		float ballDistance = new Vector2(Mathf.Abs(ballPostion.x), Mathf.Abs(ballPostion.z)).magnitude;
		int slide = 10;
		List<Vector3> listTemp = new List<Vector3>();

		Vector3 pointEnd, pointMiddle = Vector3.zero;	// diem dich' den'
        //henry add:
       // pointEnd.z = -3f; //Dat lui dich den ve phia truoc cau mon
       // pointEnd.z = -3f; //Dat lui dich den ve phia truoc cau mon
        Vector3 pointStart = ballPostion;			// diem bat dau
		
		if(direction == cornerDirection.Both) 
			direction = (cornerDirection) Random.Range(0, 2);

		// *********************** Tinh yMid va yEnd ************************
		//float yEndMin = 3f;
		//float yEndMax = 3.5f;
        //float xEndMin = 0f;
        //float xEndMax = 0.1f;
        //float xEnd, yEnd;
       

        xEndC = Random.Range(xEndC- 0.2f, xEndC + 0.2f);
        yEndC = Random.Range(yEndC -0.2f, yEndC + 0.2f);			// random ra y end

		
		//Gan diem cuoi cho dich den cua bong khi sut phat goc
		pointEnd = new Vector3(xEndC, yEndC, zEndC);       // diem dau tien duoc add vo List cung la diem cuoi' cung se duoc lay ra



        // ********************** Tim x la do be~ cong  *************************
        _maxLeft = -15f; // -3.5f
        _maxRight = 15f; //3.5f
      
		float curveFactor = Mathf.Lerp(0f, 0.95f, curveLevel);			// do kho cua xoay'
		float a = Random.Range(_maxLeft, _maxLeft * curveFactor);		// random xoay ben fai
		float b = Random.Range(_maxRight * curveFactor, _maxRight);		// random xoay ben trai
		float zCurve = (((int) Random.Range(0, 2)) == 0) ? a : b;				// random chon xoay ben fai hay ben trai

//		float xCurve = Mathf.Lerp(_maxLeft, _maxRight, curveLevel);					// do be cong duong di cua banh

	    EventChangeCurveRange(zCurve);
        float z = Math.Abs((pointEnd.z + pointStart.z) / 2f);
        z += zCurve;		// lam cho x cua diem giua~ bi lech se duoc ket qua la lam cho duong banh bi be cong xoay'
		// ***********************************************


		listTemp.Clear();
	
		pointMiddle.x = (pointEnd.x + pointStart.x) / 2f;
        pointMiddle.y = pointEnd.y* 2.5f; //(pointEnd.y + pointStart.y) / 2f;
        pointMiddle.z = zEndC ; //Gia tri nay quyet dinh huong bong tat tuong duong vi tri cua endpoint//pointEnd.z*2;//(pointEnd.z + pointStart.z) / 2f;
	

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

     

		return retVal; 
	}

	private Vector3 findPointInPathAtZ(Interpolate.Function ease, Vector3[] points, int slide, float x) {
		Vector3 pointBefore = Vector3.zero;
		Vector3 pointAfter = Vector3.zero;

		List<Vector3> temp = new List<Vector3>();

		IEnumerator<Vector3> nodes = Interpolate.NewBezier(ease, points, slide).GetEnumerator() ;		// tao ra path gom cac diem
		while( nodes.MoveNext() ) {			// lay cac diem cua path ra
			temp.Add(nodes.Current);
		}
		for(int i = 0; i < temp.Count; ++i) {			// tiem diem truoc' va sau diem hang rao
			if(temp[i].x < x) {
				pointBefore = temp[i];
				pointAfter = temp[i - 1];
				break;
			}
		}
		return Vector3.Lerp(pointBefore, pointAfter, (x - pointBefore.x) / (pointAfter.x - pointBefore.x));		
	}


    //Henry insert to reset Ball to the corner: Ham chong cua cornerReset trong Shoot.CS
    public override void cornerReset()
    {
        base.cornerReset();
        _isCornerShoot = false;
        //_isShootByAI = true;
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

	/*private DataShootAI[] dataShootAI = new DataShootAI[] {
		// distance, yMid_Min, yEnd_Min_When_Mid_Min, yEnd_Max_When_Mid_Min, yMid_Max, yEnd_Min_When_Mid_Max, yEnd_Max_When_Mid_Max
		
		new DataShootAI(16.5f, 2.6f, 2.1f, 2.4f, 4.3f, 0.145f, 0.76f)
		,new DataShootAI(19f, 2.8f, 2.2f, 2.4f, 4.3f, 0.145f, 1.5f)
		,new DataShootAI(21f, 2.8f, 2.2f, 2.4f, 4.3f, 0.145f, 1.8f)
		,new DataShootAI(23f, 2.8f, 2.3f, 2.55f, 4.3f, 0.145f, 2.2f)
		,new DataShootAI(27f, 3f, 1.8f, 2.6f, 4.3f, 0.145f, 2.6f)
		,new DataShootAI(35f, 3.2f, 1.6f, 2.8f, 4.3f, 0.145f, 2.8f)
	};	*/
}
