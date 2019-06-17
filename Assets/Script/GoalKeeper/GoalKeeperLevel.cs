using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class LevelGK {
	public float _responsive;
	public float _moveSpeed;
	public float _flyDistance;

	public LevelGK(float responsive, float flyDistance, float moveSpeed){
		_responsive = responsive;
		_moveSpeed = moveSpeed;
		_flyDistance = flyDistance;
	}
}

public class GoalKeeperLevel : MonoBehaviour {

	public static GoalKeeperLevel share;
    public static Action<LevelGK, int> EventChangeLevel = delegate {};

											// Responsive, flydistance, movespeed

	private LevelGK[] _levels = /*new LevelGK[]{ 
											   new LevelGK(1.2f, 0.5f, 0.2f)			// 0
											  ,new LevelGK(2.1f, 0.9f, 0.6f)			// 1
											  ,new LevelGK(2.3f, 1.1f, 1.2f)			// 2
											  ,new LevelGK(2.5f, 1.2f,  1.6f)			// 3
											  ,new LevelGK(2.7f, 1.3f, 2f)				// 4
											  ,new LevelGK(2.9f, 1.4f,  2.4f)			// 5
											  ,new LevelGK(3f,   1.5f, 2.8f)			// 6
											  ,new LevelGK(3f,   1.6f,    3f)			// 7
											  ,new LevelGK(3.2f, 1.7f, 3f)				// 8
											  ,new LevelGK(3.4f, 1.8f,  3f)			// 9
											  ,new LevelGK(3.6f, 1.9f, 3f)				// 10
											  ,new LevelGK(3.8f, 2f,  3f)			// 11
											  ,new LevelGK(4f,   2.1f,  3f)				// 12

											  ,new LevelGK(3f, 1.1f,   2.6f)		    // this data is suitable for goal keeper that is controlled by user
//											,new LevelGK(4.2f, 2.5f,  3f)				// 16
											};
                                 * */

                                new LevelGK[]{ 
											   new LevelGK(1.2f, 0.5f, 0.2f)			// 0
											  ,new LevelGK(1.5f, 0.9f, 0.5f)			// 1
											  ,new LevelGK(1.8f, 1.1f, 0.8f)			// 2
											  ,new LevelGK(2.1f, 1.2f,  1.1f)			// 3
											  ,new LevelGK(2.4f, 1.3f, 1.4f)				// 4
											  ,new LevelGK(2.7f, 1.4f,  1.7f)			// 5
											  ,new LevelGK(3f,   1.5f, 2.0f)			// 6
											  ,new LevelGK(3f,   1.6f,    2.3f)			// 7
											  ,new LevelGK(3.2f, 1.7f, 2.6f)				// 8
											  ,new LevelGK(3.4f, 1.8f,  2.9f)			// 9
											  ,new LevelGK(3.6f, 1.9f, 3f)				// 10
											  ,new LevelGK(3.8f, 2f,  3f)			// 11
											  ,new LevelGK(4f,   2.1f,  3f)				// 12

											  ,new LevelGK(3f, 1.1f,   2.6f)		    // this data is suitable for goal keeper that is controlled by user
//											,new LevelGK(4.2f, 2.5f,  3f)				// 16
											};

	public RuntimeAnimatorController[] _animatorControllers;
	public RuntimeAnimatorController[] _animatorControllerClones;


	void Awake() {
		share = this;
		_level = 2; //Henry update, old =0
		_responsive = 1f;
		_moveSpeed = 1f;
		_flyDistance = 0.5f;
	    _previousLevel = 0;
        //Henry add new
        setAllLevel();
    }

    void Start()
    {
      _animatorClone = GoalKeeperClone.share.GetComponent<Animator>();
    }


	public float _responsiveIncrease = 0.5f;
	public float _moveSpeedIncrease = 0.5f;
	public float _flyDistanceIncrease = 0.5f;

	private float _responsive;
	private float _moveSpeed;
	private float _flyDistance;

	public Animator _animator;
	public Animator _animatorClone;

	private RuntimeAnimatorController _animatorController;
	private RuntimeAnimatorController _animatorControllerClone;

	public int _level = 2; //Henry add = 6, defaul null
  


    private int _previousLevel;
	public void setGKControlByHuman()
	{
        setLevel(3); //Henry edit, old: 13
	}

    public void setGKPreviousLevel()
    {
        setLevel(_previousLevel);
    }

	public int getMaxLevel() {
		return _levels.Length - 1;
	}

	private LevelGK getLevelGK(int level) {
		if(level >= _levels.Length) {
			Debug.LogWarning("Level " + level + " excceed tha max level : " + _levels.Length);
			return null;
		}

		//return _levels[level];
        //Henry add new
        return _levels[2];

    }

	public void setLevel(int level) {
		LevelGK levelGK = getLevelGK(level);
       

        if (levelGK == null) {
			return;
		}

        if(_level != _levels.Length - 1)
            _previousLevel = _level;

		_level = level;

		if(_level == 0) {
			GoalKeeper.share._delayFactor = 0.3f;
		}
		else if(_level == 1) {
			GoalKeeper.share._delayFactor = 0.7f;
		}
		else {
			GoalKeeper.share._delayFactor = 1f;
		}

		GoalKeeperHorizontalFly.share.setFlyDistance(levelGK._flyDistance);
        //Henry remove //_animator.runtimeAnimatorController = _animatorControllers[level];
        //_animatorClone.runtimeAnimatorController = _animatorControllerClones[level];
        // EventChangeLevel(levelGK, level);
        //Henry add new
        _animator.runtimeAnimatorController = _animatorControllers[2];
        _animatorClone.runtimeAnimatorController = _animatorControllerClones[2];

        EventChangeLevel(levelGK, 2);



    }



#if UNITY_EDITOR

	void OnGUI() {
		if( GUILayout.Button("Set all Levels") ) {
			setAllLevel();
		}
	}


	public void setLevel(int level, RuntimeAnimatorController animatorController, RuntimeAnimatorController animatorControllerClone) {
		LevelGK levelGK = getLevelGK(level);
		if(levelGK == null) {
			return;
		}

		_flyDistance = levelGK._flyDistance;
		_moveSpeed = levelGK._moveSpeed;
		_responsive = levelGK._responsive;



		_animatorController = animatorController;
		_animatorControllerClone = animatorControllerClone;


		UnityEditor.Animations.AnimatorController ac = _animatorController as UnityEditor.Animations.AnimatorController;
        UnityEditor.Animations.AnimatorStateMachine sm = ac.layers[0].stateMachine;
        for (int i = 0; i < sm.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = sm.states[i].state;
			//			Debug.Log(string.Format("State: {0}", state.uniqueName));
			if(state.name.Equals("save") || state.name.Equals("save center")) {
				state.speed = _responsive;
			}
			else if(state.name.Equals("move left") || state.name.Equals("move right")) {
				state.speed = _moveSpeed;
			}
		}
		
		ac = _animatorControllerClone as UnityEditor.Animations.AnimatorController;
		sm = ac.layers[0].stateMachine;
		for (int i = 0; i < sm.states.Length; i++)
		{
		    UnityEditor.Animations.AnimatorState state = sm.states[i].state;
			//			Debug.Log(string.Format("State: {0}", state.uniqueName));
			if(state.name.Equals("save") || state.name.Equals("save center")) {
				state.speed = _responsive;
			}
		}


	}

	public void setAllLevel() {
        //Henry remove
        /*for(int i = 0; i < _levels.Length; ++i) {
			setLevel(i, _animatorControllers[i], _animatorControllerClones[i]);
		}*/

        //Henry add new: 
        for(int i = 0; i < 3; ++i) {
          setLevel(i, _animatorControllers[i], _animatorControllerClones[i]);
      }

    }
#endif
}
