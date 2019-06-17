using UnityEngine;
using System.Collections;

public class SlowMotion : MonoBehaviour {

	public static SlowMotion share;

    public bool _slowMotion;
	public float _slowMotionRange = 3f;
	public float _slowMotionTimeScale = 0.1f;

	private Transform _cachedTrans;

	public float _slowMotionTime = 6;
	public float _slowMotionTimeCount;

	private bool _slowMotionActive = false;

	private int _count = 0;
	public string _mode;

	public int _slowMotionAfterCount = 4;

	void Awake() {
		share = this;
		_cachedTrans = transform;
		_mode = "Sometimes";
		_cachedTrans.position = Vector3.zero;
	}

	void Start() {
//		GoalDetermine.share.eventFinishShoot += eventShootFinish;
		Shoot.EventShoot += eventShootStart;
	}

	private void eventShootStart() {
		++_count;
		if (_slowMotionAfterCount == 0) {
			_count = 0;
			_slowMotion = true;
			return;
		}

		if(_mode.Equals("Sometimes")) {
			if((_count / _slowMotionAfterCount) == 1) {
				_slowMotion = true;
			}
			else {
				_slowMotion = false;
			}
		}
		else if(_mode.Equals("Always")) {
			_slowMotion = true;
		}
		else 
			_slowMotion = false;

		_count = _count % _slowMotionAfterCount;
	}


	void FixedUpdate() {
		Time.timeScale = 1f;
		if(_slowMotion) {

			if( !_slowMotionActive && (GoalKeeper.share._ball.position - _cachedTrans.position).magnitude < _slowMotionRange) {
				_slowMotionTimeCount = 0;
				_slowMotionActive = true;
			}

			if(_slowMotionActive && _slowMotionTimeCount < _slowMotionTime) {
				_slowMotionTimeCount += (Time.deltaTime / _slowMotionTimeScale);
				Time.timeScale = _slowMotionTimeScale;
			}
		}

	}

	public void reset() {
		_slowMotionActive = false;
		enabled = true;
		Time.timeScale = 1f;
	}

	private float _currentTimeScale;
	public void pause() {
		_currentTimeScale = Time.timeScale;
		enabled = false;
		Time.timeScale = 0f;
	}

	public void unPause() {
		enabled = true;
		Time.timeScale = _currentTimeScale;
	}

//	public void onValueChanged_SlowMotion() {
//		_slowMotion = UIToggle.current.value;
//	}

	public void onChanged_SlowMotion() {
		/*
		_mode = UIPopupList.current.value;
		switch(_mode) {
		case "Sometimes":
			_slowMotion = true;
			break;
		case "Always":
			_slowMotion = true;
			break;
		case "None":
			_slowMotion = false;
			break;
		}
		*/
	}
}
