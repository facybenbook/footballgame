using UnityEngine;
using System.Collections;

public class GoalKeeperClone : MonoBehaviour {

	public static GoalKeeperClone share;

	public Transform _pointLeft;
	public Transform _pointRight;

	public float nearDistanceLeft;
	public float nearDistanceRight;

	void Awake() {
		share = this;
		cacheTrans = transform;
	}

	private Transform cacheTrans;

	void FixedUpdate() {
		nearDistanceLeft = Mathf.Abs(_pointLeft.position.x - cacheTrans.position.x);
		nearDistanceRight = Mathf.Abs(_pointRight.position.x - cacheTrans.position.x);
	}
}
