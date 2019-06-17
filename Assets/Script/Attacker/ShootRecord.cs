using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShootRecord : MonoBehaviour
{
    public static ShootRecord share;

    struct BallDataRecord
    {
        public Vector3 speed;
        public Vector3 angularSpeed;
    }

    public bool shouldCapture = false;
    public bool shouldReplay = false;
    public bool fullRecord = false;
    private float ballNearLimit = 1f;

    private List<BallDataRecord> ballRecordPosRot;
    private List<BallDataRecord> ballRecordSpeed;

    public Vector3 posBallReplay;

    void Awake()
    {
        share = this;
        ballRecordPosRot = new List<BallDataRecord>();
        ballRecordSpeed = new List<BallDataRecord>();
    }

	// Use this for initialization
	void Start ()
	{
	    Shoot.EventDidPrepareNewTurn += OnPreparedNextTurn;
	    Shoot.EventShoot += OnShoot;
        
	}

    void OnDestroy()
    {
        Shoot.EventDidPrepareNewTurn -= OnPreparedNextTurn;
        Shoot.EventShoot -= OnShoot;
    }
	
	// Update is called once per frame
    void OnPreparedNextTurn()
    {
        shouldCapture = false;
        shouldReplay = false;
        _index = 0;
        ballNearLimit = -Shoot.share._ballControlLimit/2f;
    }

    void OnShoot()
    {
        posBallReplay = Shoot.share._ball.position;
        ballRecordPosRot.Clear();
        ballRecordSpeed.Clear();
        shouldCapture = true;
    }

    void OnShootFinished()
    {
        
    }

    private int _index = 0;
    void Update()
    {
        if (shouldReplay && _index < ballRecordPosRot.Count)
        {
            BallDataRecord data = ballRecordPosRot[_index++];


            Shoot.share._cachedTrans.position = data.speed;
            Shoot.share._cachedTrans.eulerAngles = data.angularSpeed;

            if (_index >= ballRecordPosRot.Count)
            {
                data = ballRecordSpeed[_index - 1];
                Shoot.share._ball.velocity = data.speed;
                Shoot.share._ball.angularVelocity = data.angularSpeed;

                shouldReplay = false;
                _index = 0;
            }
        }
    }

    void LateUpdate()
    {
        if (shouldCapture) // && Shoot.share._cachedTrans.position.z < ballNearLimit)
        {
            BallDataRecord data;

            data.speed = Shoot.share._cachedTrans.position;
            data.angularSpeed = Shoot.share._cachedTrans.eulerAngles;
            ballRecordPosRot.Add(data);

            data.speed = Shoot.share._ball.velocity;
            data.angularSpeed = Shoot.share._ball.angularVelocity;
            ballRecordSpeed.Add(data);

            if (Shoot.share._cachedTrans.position.z >= ballNearLimit && fullRecord == false)
            {
                shouldCapture = false;
            }
        }    
    }
}
