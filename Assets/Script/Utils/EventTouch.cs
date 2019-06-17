using UnityEngine;
using System.Collections;
using System;



public class EventTouch : MonoBehaviour {
	
	public delegate void EventTouchDelegate(EventTouch eventTouch); 

	public Vector3 _previousPos;
	public Vector3 _currentPos;
	public Vector3 _deltaPos;
	public float _deltaTime;
	public float _currentTime;
	public TouchPhase _phase;
	
	public EventTouchDelegate eventTouch;
	public EventTouchDelegate eventSwipeLeft;
	public EventTouchDelegate eventSwipeRight;
	public EventTouchDelegate eventSwipeUp;
	public EventTouchDelegate eventSwipeDown;
	public EventTouchDelegate eventTap;
	public EventTouchDelegate eventTouchBegan;
	public EventTouchDelegate eventTouchStationary;
	public EventTouchDelegate eventTouchMoved;
	public EventTouchDelegate eventTouchEnded;
	public EventTouchDelegate eventTouchMultiTouchBegan;
	public EventTouchDelegate eventTouchMultiTouchEnded;
	
	public float _touchThresHold = 50;
	
	// test for swipe gesture
	float _screenDiagonalSize;
	float _minSwipeDistancePixels;
	bool _isMoved;
	Vector2 _touchStartPos;
	public float _minSwipeDistance = .1f;
	
	//
	private static EventTouch _instance;
	public static EventTouch share() {
		if(_instance == null) {
			GameObject go = new GameObject("EventTouchSingleTon");
			_instance = go.AddComponent<EventTouch>();
		}
		return _instance;
	}
	
	void Start(){
		// test for swipe gesture
		_screenDiagonalSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
		_minSwipeDistancePixels = _minSwipeDistance * _screenDiagonalSize;	
		
	}
	
	public void OnApplicationQuit() {
//		Debug.Log("OnApplicationQuit");
		
		eventTouch = null;
		eventSwipeLeft = null;
		eventSwipeRight = null;
		eventSwipeUp = null;
		eventSwipeDown = null;
		eventTap = null;
		eventTouchBegan = null;
		eventTouchStationary = null;
		eventTouchMoved = null;
		eventTouchEnded = null;
		
		Destroy(_instance.gameObject);
	}
	
	private bool _isMultiTouchBegan = false;
	
	public void Update() {
		if( Input.touchCount > 1 ){
			_isMultiTouchBegan = true;
			if( eventTouchMultiTouchBegan != null ){
				eventTouchMultiTouchBegan( this );
				
			}
			return;
		}
		else {
			if(_isMultiTouchBegan == true ) {
				if( eventTouchMultiTouchEnded != null ){
					eventTouchMultiTouchEnded( this );
				}
				_currentPos = Input.mousePosition;
				_currentTime = Time.time;
				_deltaPos = Vector3.zero;
				_isMultiTouchBegan = false;
				_deltaTime = 0;
				
				
				_phase = TouchPhase.Began;
				_isMoved = false;
				return;
			}
			
		}
		

#if UNITY_EDITOR
		
#elif UNITY_IPHONE
		if( Input.touchCount > 1 ){
			if( eventTouchMultiTouchBegan != null ){
				eventTouchMultiTouchBegan( this );
			}
			return;
		}
#endif
	
		if(Input.GetMouseButtonDown(0)) {			// touch phase began
			_previousPos = _currentPos = Input.mousePosition;
			_currentTime = Time.time;
			_phase = TouchPhase.Began;
			_deltaPos = Vector3.zero;
			_deltaTime = 0;
			
			
			_isMoved = false;
			_touchStartPos = new Vector2( _currentPos.x, _currentPos.y );
			
			if( eventTouchBegan != null ){
				eventTouchBegan( this );
			}
		}
		else if( Input.GetMouseButton(0) ) {			
			if(_previousPos != Input.mousePosition) {		// touch phase moved
				_deltaTime = Time.time - _currentTime;
				_currentTime = Time.time;
				_deltaPos = Input.mousePosition - _currentPos;
				_previousPos = _currentPos;
				_currentPos = Input.mousePosition;
				_phase = TouchPhase.Moved;
				_isMoved = true;
				
				if( eventTouchMoved != null ){
					eventTouchMoved( this );
				}
			}
			else {							// touch stationary
				_phase = TouchPhase.Stationary;

				if( eventTouchStationary != null ){
					eventTouchStationary( this );
				}
			}
		}
		else if( Input.GetMouseButtonUp(0) ) {			// touch ended
			_phase = TouchPhase.Ended;
			_previousPos = _currentPos;
			_currentPos = Input.mousePosition;
			
			SwipeGesture();
			if(_isMoved == false && eventTap != null)
				
				eventTap(this);
			if( eventTouchEnded != null ){
				eventTouchEnded( this );
			}
		}
		else {					// touch cancel
			_phase = TouchPhase.Canceled;
		}
		
		if( eventTouch != null ){
			eventTouch( this );	
		}
	
}
	
	
	void SwipeGesture(){
		if( _phase == TouchPhase.Ended ){
				
			if( Mathf.Abs( _deltaPos.x ) > Mathf.Abs( _deltaPos.y ) ){
				float velocity = Mathf.Abs( _deltaPos.x / _deltaTime );
				if( velocity >= _touchThresHold ){ // swipe
					if( _deltaPos.x > 0 ){
						if( eventSwipeRight != null ){
							eventSwipeRight( this );
						}	
					}
					else if( _deltaPos.x < 0 ){
						if( eventSwipeLeft != null ){
							eventSwipeLeft( this );
						}	
					}
				}
			}
			else{
				float velocity = Mathf.Abs( _deltaPos.y / _deltaTime );
				if( velocity > _touchThresHold ){ // swipe
					if( _deltaPos.y > 0 ){
						if( eventSwipeUp != null ){
							eventSwipeUp( this );
						}
					}
					else if( _deltaPos.y < 0 ){
						if( eventSwipeDown != null ){
							eventSwipeDown( this );
						}
					}
				}
			}
		}
	}
}
