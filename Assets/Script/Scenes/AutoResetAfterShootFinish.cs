using UnityEngine;
using System.Collections;


public class AutoResetAfterShootFinish : MonoBehaviour
{

    public float resetAfter = 1.5f;
    public float shootAfter = 1.5f;
    public bool RandomNewPos { get; set; }
	// Use this for initialization
	void Start ()
	{
	    RandomNewPos = true;
	    GoalDetermine.EventFinishShoot += OnShootFinished;
	    Shoot.EventDidPrepareNewTurn += OnNewTurn;
	}

    void OnDestroy()
    {
        GoalDetermine.EventFinishShoot -= OnShootFinished;
        Shoot.EventDidPrepareNewTurn -= OnNewTurn;
    }

    void OnNewTurn()
    {
        RunAfter.removeTasks(gameObject);
    }

    void OnShootFinished(bool isGoal, Area area)
    {
        RunAfter.runAfter(gameObject, () =>
        {
            DemoShoot.share.Reset(RandomNewPos);
        }, resetAfter);
    }
}
