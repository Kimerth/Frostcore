using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(AI_Motor))]
[RequireComponent(typeof(Seeker))]
public class AI_Controller : MonoBehaviour 
{
    public Transform Target;
    float Distance;
    [SerializeField] float m_LookAtRange = 25f;
    [SerializeField] float m_ChaseRange = 15f;
    [SerializeField] float m_AttackRange = 1.5f;

    [SerializeField] private float updateRate = 2f;
    private Seeker seeker;
    public Path path;

    [HideInInspector]
    public bool pathIsEnded = false;

    // The max distance from the AI to a waypoint for it to continue to the next waypoint	
    public float nextWaypointDistance = 3;

    // The waypoint we are currently moving towards
    private int currentWaypoint = 0;

    private bool searchingForPlayer = false;

    private AI_Motor _Motor;

    void Awake()
    {
        _Motor = GetComponent<AI_Motor>();
        _Motor.ImposedAwake();
    }

	void Start() 
    {
        seeker = GetComponent<Seeker>();

        if (seeker == null)
        {
            Debug.LogError("No Seeker component on this GameObject");
        }

        if (Target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }

            return;
        }

        // Start a new path to the target position, return the results on the OnPathComplete method
        seeker.StartPath(transform.position, Target.position, OnPathComplete);

        StartCoroutine(UpdatePath());
	}

    IEnumerator SearchForPlayer()
    {
        GameObject sResult = GameMaster.gm.m_Player.gameObject;
        if(sResult == null)
        {
            yield return new WaitForSeconds(1f/updateRate);
            StartCoroutine(SearchForPlayer());
        }
        else
        {
            Target = sResult.transform;
            searchingForPlayer = false;
            StartCoroutine(UpdatePath());

            yield break;
        }
    }

    IEnumerator UpdatePath()
    {
        if (Target == null)
        {
            if(!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }

            yield break;
        }

        // Start a new path to the target position, return the results on the OnPathComplete method
        seeker.StartPath(transform.position, Target.position, OnPathComplete);

        yield return new WaitForSeconds(1f / updateRate);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        if (Target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }

            return;
        }

        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            if (pathIsEnded)
                return;

            pathIsEnded = true;
            return;
        }

        float dis = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        Distance = Vector2.Distance(transform.position, Target.position);

        if(Distance < m_LookAtRange && !_Motor.HasAttacked)
        {
            _Motor.LookAt(Target.position);
        }

        if (Distance < m_AttackRange)
            if (!_Motor.IsAttacking && !_Motor.HasAttacked)
                StartCoroutine(_Motor.Attack(0.75f, 0.5f));

        if(Distance < m_ChaseRange && !_Motor.HasAttacked)
        {
            Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            _Motor.Move(dir);
        }
        else if(_Motor.HasAttacked)
        {
            Vector3 dir = (transform.position - path.vectorPath[currentWaypoint]).normalized;
            _Motor.Move(dir);
            _Motor.LookAt(dir);
        }

        if (dis < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        _Motor.ImposedFixedUpdate();
    }
}
