using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PositionRotationData
{
    public Vector3 pos;
    public Quaternion rot;

    public PositionRotationData(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
        
}

[System.Serializable]
public class BoneData
{
    public PositionRotationData bone1;
    public PositionRotationData bone2;
    public PositionRotationData bone3;

    public BoneData(Transform boneTr1, Transform boneTr2, Transform boneTr3)
    {
        bone1 = new PositionRotationData(boneTr1.localPosition, boneTr1.localRotation);
        bone2 = new PositionRotationData(boneTr2.localPosition, boneTr2.localRotation);
        bone3 = new PositionRotationData(boneTr3.localPosition, boneTr3.localRotation);
    }
}

public class GoalKeeperRecord : MonoBehaviour
{

    public static GoalKeeperRecord share;

    public bool shouldCapture = false;
    public bool shouldReplay = false;
    private float ballNearLimit = 1f;
    private Animator _animatorGK;

    private List<Vector3> positionGK;
    private Transform _transGK;

    private ArmSystem armLeft;
	private ArmSystem armRight;

    private List<BoneData> boneLeftRecord;
    private List<BoneData> boneRightRecord;

    private List<float> _deltaTimeRecord;

    void Awake()
    {
        share = this;
        positionGK = new List<Vector3>();
        boneLeftRecord = new List<BoneData>();
        boneRightRecord = new List<BoneData>();
        _deltaTimeRecord = new List<float>();
    }

	// Use this for initialization
    void Start()
    {
        Shoot.EventDidPrepareNewTurn += OnPreparedNextTurn;
        Shoot.EventShoot += OnShoot;
        _animatorGK = GoalKeeper.share.GetComponent<Animator>();
        _transGK = GoalKeeper.share.GetComponent<Transform>();

        armLeft = GoalKeeper.share.armLeft;
        armRight = GoalKeeper.share.armRight;
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
        ballNearLimit = -Shoot.share._ballControlLimit / 2f;
        _animatorGK.StopRecording();
        Debug.Log("End Record time : " + _animatorGK.recorderStopTime);
        StopReplay();
    }

    void OnShoot()
    {
        shouldCapture = true;
        positionGK.Clear();
        boneLeftRecord.Clear();
        boneRightRecord.Clear();
        _deltaTimeRecord.Clear();
        _animatorGK.StartRecording(0);

        Debug.Log("Start Record time : " + _animatorGK.recorderStartTime);
    }

    private int _currentState;
    public void Replay()
    {
        GoalKeeper.share.enabled = false;
        GoalKeeperRootMotion.share._shouldMove = false;
        GoalKeeperHorizontalFly.share.enabled = false;
        _currentState = _animatorGK.GetCurrentAnimatorStateInfo(0).shortNameHash;
        _animatorGK.StartPlayback();
        shouldReplay = true;
        _timer = 0f;
        _index = 0;
    }

    public void StopReplay()
    {
        _animatorGK.StopPlayback();
        shouldReplay = false;
        GoalKeeper.share.enabled = true;
        GoalKeeperRootMotion.share._shouldMove = true;
        GoalKeeperHorizontalFly.share.enabled = true;
        //GoalKeeper.share.reset();
        StartCoroutine(Reset());
    }

    IEnumerator Reset()
    {
        yield return new WaitForEndOfFrame();
        //GoalKeeperLevel.share.setLevel(GoalKeeperLevel.share._level);
        _animatorGK.enabled = false;
        //_animatorGK.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        yield return new WaitForEndOfFrame();
        _animatorGK.enabled = true;
        //_animatorGK.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private float _timer;
    private int _index;
    void Update()
    {
        if (shouldReplay)
        {
            //DoReplay();
        }
    }
         
    void LateUpdate()
    {
        if (shouldCapture)
        {
            positionGK.Add(_transGK.position);

            boneLeftRecord.Add(new BoneData(armLeft.bone1.transform, armLeft.bone2.transform, armLeft.bone3.transform));
            boneRightRecord.Add(new BoneData(armRight.bone1.transform, armRight.bone2.transform, armRight.bone3.transform));
            _deltaTimeRecord.Add(Time.deltaTime);
        }

        if (shouldReplay)
        {
            DoReplay();
        }
    }

    private void DoReplay()
    {
        _animatorGK.playbackTime = _timer;

        if (_index < positionGK.Count)
        {
            _transGK.position = positionGK[_index];
        }

        if (_index < boneLeftRecord.Count)
        {
            BoneData boneData = boneLeftRecord[_index];
            ApplyBoneRecordedData(armLeft, boneData);
        }

        if (_index < boneRightRecord.Count)
        {
            BoneData boneData = boneRightRecord[_index];
            ApplyBoneRecordedData(armRight, boneData);
        }

        if (_index < _deltaTimeRecord.Count)
        {
            _timer += _deltaTimeRecord[_index];
        }

        ++_index;
        if (_timer >= _animatorGK.recorderStopTime)
        {
            _timer = _animatorGK.recorderStopTime;
            StopReplay();
        }
    }

    void ApplyBoneRecordedData(ArmSystem solver, BoneData boneData)
    {
        //solver.bone1.transform.localPosition = boneData.bone1.pos;
        solver.bone1.transform.localRotation = boneData.bone1.rot;

        //solver.bone2.transform.localPosition = boneData.bone2.pos;
        solver.bone2.transform.localRotation = boneData.bone2.rot;

        //solver.bone3.transform.localPosition = boneData.bone3.pos;
        solver.bone3.transform.localRotation = boneData.bone3.rot;
    }
}
