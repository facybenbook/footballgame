using UnityEngine;
using System.Collections;

public class GoalKeeperRootMotion : MonoBehaviour
{
    public static GoalKeeperRootMotion share;
    private Animator _animator;
    private LevelGK _levelGK;
    public float baseSpeed = 0.4f;
    public bool _shouldMove = true;

    void Awake()
    {
        share = this;
        GoalKeeperLevel.EventChangeLevel += OnChangeLevel;
    }

    void OnDestroy()
    {
        GoalKeeperLevel.EventChangeLevel -= OnChangeLevel;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OnChangeLevel(LevelGK newLevel, int level)
    {
        _levelGK = newLevel;
    }

    void OnAnimatorMove()
    {
        if (_animator != null &&_levelGK != null && _shouldMove)
        {
            Vector3 newPosition = transform.position;
            newPosition.x += _levelGK._moveSpeed * baseSpeed * Time.deltaTime * -_animator.GetInteger("move");
            transform.position = newPosition;
        }
    }
}
