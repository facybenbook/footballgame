using UnityEngine;
using System.Collections;

public class BallTarget : MonoBehaviour
{
    public static BallTarget share;

	public Transform _ball;
	public Transform _ballTarget;
	private Vector3 _previousPos;
	public Transform _ballTargetPrevious;
	public Transform _endPoint;

	private Rigidbody _ballRigidBody;

	public bool _isShoot = false;


	private void event_shoot() {
		_isShoot = true;
	}

	private void shootFinish(bool isGoal, Area area) {
		_isShoot = false;
	}

    private void Awake()
    {
        share = this;
    }

    void Start() {
		_ballRigidBody = _ball.GetComponent<Rigidbody>();
		Shoot.EventShoot += event_shoot;
		GoalDetermine.EventFinishShoot += shootFinish;
	}

	void OnDestroy() {
		Shoot.EventShoot -= event_shoot;
		GoalDetermine.EventFinishShoot -= shootFinish;
	}

	// Update is called once per frame
	void FixedUpdate () {

		if(_isShoot) {		// neu da sut roi`
			if(_ballRigidBody.velocity.sqrMagnitude <= 0.001f) {	// banh dung yen, ko can tinh' diem~ giao cat'
				_ballTarget.position = Vector3.zero;
			}
			else {

				Vector3 intersection = Vector3.zero;
				if(Math3d.LinePlaneIntersection(out intersection, _ballRigidBody.position, (_ballRigidBody.velocity).normalized, _endPoint.up, _endPoint.position)) {	// tim giao diem giua~ mat phang gan` thu mon va duong di cua trai banh,
					intersection.y = Mathf.Clamp(intersection.y, 0, 3f);
					intersection.x = Mathf.Clamp(intersection.x, -5f, 5f);
					_ballTarget.position = intersection;		//  set vi tri ballTarget la vi tri giao diem tim duoc
				}
				else {
					// ball target la diem~ co' x,y = x,y cua ball. z = z cua _endpoint, xem endpoint nhu mat phang~ gan thu~ mon,
					// thu mon se~ bay nguoi can fa' khi banh giao cat' voi' mat phang~ nay`, va _ballTarget chinh' la diem~ giao cat'
					// giua~ duong` di cua~ ball va mat phang~ nay
					
					Vector3 posTemp = _ball.position;
					posTemp.z = _endPoint.position.z;
					_ballTarget.position = posTemp;		
				}
				
			}
			_previousPos = _ballRigidBody.position;
			_ballTargetPrevious.position = _previousPos;
		}
		else {

		}
	}
}
