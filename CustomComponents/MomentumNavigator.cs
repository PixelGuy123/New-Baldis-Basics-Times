using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MomentumNavigator : MonoBehaviour
{
	public EnvironmentController ec;

	public Cell _startTile;
	public Cell _targetTile;

	protected NavMeshPath _navMeshPath;
	protected NavMeshHit _startHit;
	protected NavMeshHit _targetHit;
	protected NavMeshHit _dummyHit;

	[SerializeField]
	protected float radius = 10f;

	public float maxSpeed = 15f;
	public float accel = 15f;
	public float speed;
	public float _distanceToTravel;

	protected Cell currentTargetTile;
	protected Cell currentStartTile;
	protected Cell currentTile;

	protected bool beenMoved;

	[SerializeField]
	public bool preciseTarget = true;

	[SerializeField]
	public bool autoRotate = true;

	[SerializeField]
	public bool decelerate = false;

	[SerializeField]
	public bool useAcceleration = false;

	[SerializeField]
	public float height = 5f;

	protected bool recalculatePath;

	protected Vector3 destination;
	protected Vector3 velocity;
	protected Vector3 realVelocity;

	private Vector3 _startPos;

	protected List<Vector3> destinationPoints = [];

	protected IntVector2 _gridPosition;
	protected IntVector2 _avoidPosition;

	protected Vector3 expectedPosition;
	protected IntVector2 _travelVector;

	public List<Direction> currentDirs = [];
	protected List<Direction> _potentialDirs = [];
	protected List<Direction> _potentialDirsCheck = [];
	protected List<Cell> _path = [];

	public float TimeScaleMultiplier { get; set; } = 1f; // Replaces ec.NpcTimeScale

	public bool HasDestination => destinationPoints.Count > 0;

	public Vector3 NextPoint => HasDestination ? destinationPoints[0] : transform.position;

	public Vector3 CurrentDestination => HasDestination ? destinationPoints[destinationPoints.Count - 1] : Vector3.zero;

	public Vector3 Velocity => realVelocity;

	public float Acceleration => decelerate ?
		accel * Time.deltaTime * TimeScaleMultiplier * Mathf.Sign(maxSpeed - speed) :
		accel * Time.deltaTime * TimeScaleMultiplier;

	public float Radius => radius;
	public float Speed => speed;

	public event System.Action<Vector3, Vector3> OnMove;

	private void Awake()
	{
		_navMeshPath = new NavMeshPath();
	}

	public void Initialize(EnvironmentController ec)
	{
		this.ec = ec;
		expectedPosition = transform.position;
		_startPos = transform.position;
	}

	private void Update()
	{
		if (!ec || Time.timeScale <= 0f || Time.deltaTime <= 0f) return;

		_startPos = transform.position;
		if (expectedPosition != transform.position) beenMoved = true;
		else beenMoved = false;

		HandleMovement();
		UpdateSpeed();
	}

	private void LateUpdate()
	{
		if (ec && Time.timeScale > 0f && Time.deltaTime > 0f)
		{
			realVelocity = transform.position - _startPos;
			if (autoRotate && realVelocity.magnitude > 0f)
			{
				transform.rotation = Quaternion.LookRotation(
					realVelocity.z * Vector3.forward + realVelocity.x * Vector3.right,
					Vector3.up
				);
			}
		}
	}

	private void HandleMovement()
	{
		if (destinationPoints.Count == 0)
		{
			expectedPosition = transform.position;
			currentTargetTile = null;
			return;
		}

		destination = destinationPoints[0];
		_distanceToTravel = speed * TimeScaleMultiplier * Time.deltaTime;

		while ((destination - transform.position).magnitude <= _distanceToTravel && destinationPoints.Count > 0)
		{
			_distanceToTravel -= (destination - transform.position).magnitude;
			transform.position = destination;
			destinationPoints.RemoveAt(0);
			if (destinationPoints.Count > 0) destination = destinationPoints[0];
		}

		Vector3 velDir = (destination - transform.position).normalized;

		if (destinationPoints.Count > 0)
		{
			velocity = velDir * _distanceToTravel;
			transform.position += velocity;
			expectedPosition = transform.position;
		}

		OnMove?.Invoke(transform.position, velDir);
	}

	private void UpdateSpeed() =>
		speed = !useAcceleration ? maxSpeed :
			decelerate ? Mathf.Clamp(speed + Acceleration, 0, maxSpeed) :
			Mathf.Min(speed + Acceleration, maxSpeed);


	protected IntVector2 GetGridPosition(Vector3 position)
	{
		_gridPosition.x = Mathf.FloorToInt(position.x / 10f);
		_gridPosition.z = Mathf.FloorToInt(position.z / 10f);
		currentTile = ec.CellFromPosition(_gridPosition);
		return _gridPosition;
	}

	public void FindPath(Vector3 targetPosition) =>
		FindPath(transform.position, targetPosition);


	private void FindPath(Vector3 startPos, Vector3 targetPos)
	{
		targetPos.y = height;
		_startTile = ec.CellFromPosition(GetGridPosition(startPos));
		_targetTile = ec.CellFromPosition(GetGridPosition(targetPos));

		if (_targetTile == currentTargetTile && !recalculatePath) return;

		//TempOpenObstacles();
		ec.FindPath(_startTile, _targetTile, PathType.Nav, out _path, out bool success);
		//TempCloseObstacles();

		if (success)
		{
			ConvertPath(_path, targetPos);
			currentTargetTile = _targetTile;
			currentStartTile = _startTile;
		}
		else
		{
			ClearDestination();
		}
	}

	protected void ConvertPath(List<Cell> path, Vector3 targetPos)
	{
		targetPos.y = height;
		Cell currentCell = null;
		Cell lastOpenCell = null;
		bool inOpenGroup = false;

		destinationPoints.Clear();

		while (path.Count > 0)
		{
			if (path[0].open)
			{
				if (inOpenGroup)
				{
					if (path[0].openTiles.Contains(lastOpenCell))
					{
						lastOpenCell = path[0];
					}
					else
					{
						BuildNavPath(currentCell, lastOpenCell, targetPos);
						currentCell = path[0];
						lastOpenCell = path[0];
					}
				}
				else
				{
					inOpenGroup = true;
					currentCell = path[0];
					lastOpenCell = path[0];
				}

				if (path.Count == 1)
				{
					BuildNavPath(currentCell, lastOpenCell, targetPos);
				}
			}
			else
			{
				if (inOpenGroup)
				{
					inOpenGroup = false;
					BuildNavPath(currentCell, lastOpenCell, targetPos);
				}
				destinationPoints.Add(path[0].FloorWorldPosition + Vector3.up * height);
			}
			path.RemoveAt(0);
		}

		if (preciseTarget)
		{
			destinationPoints.Add(new Vector3(targetPos.x, height, targetPos.z));
		}
	}

	protected void BuildNavPath(Cell firstOpenTile, Cell lastOpenTile, Vector3 targetPosition)
	{
		if (ec.CellFromPosition(transform.position) != firstOpenTile ||
			!NavMesh.SamplePosition(transform.position.ZeroOutY(), out _dummyHit, 1f, NavMesh.AllAreas))
		{
			NavMesh.SamplePosition(firstOpenTile.FloorWorldPosition, out _startHit, 10f, NavMesh.AllAreas);
		}
		else
		{
			NavMesh.SamplePosition(transform.position.ZeroOutY(), out _startHit, 10f, NavMesh.AllAreas);
		}

		if (ec.CellFromPosition(targetPosition) != lastOpenTile ||
			!preciseTarget ||
			!NavMesh.SamplePosition(targetPosition.ZeroOutY(), out _dummyHit, 1f, NavMesh.AllAreas))
		{
			NavMesh.SamplePosition(lastOpenTile.FloorWorldPosition, out _targetHit, 10f, NavMesh.AllAreas);
		}
		else
		{
			NavMesh.SamplePosition(targetPosition.ZeroOutY(), out _targetHit, 10f, NavMesh.AllAreas);
		}

		NavMesh.CalculatePath(_startHit.position, _targetHit.position, NavMesh.AllAreas, _navMeshPath);

		foreach (Vector3 corner in _navMeshPath.corners)
		{
			destinationPoints.Add(new Vector3(corner.x, height, corner.z));
		}
	}

	public void ClearDestination()
	{
		destinationPoints.Clear();
		currentTargetTile = null;
		currentStartTile = null;
	}

	public void SkipCurrentDestinationPoint()
	{
		if (destinationPoints.Count > 1) destinationPoints.RemoveAt(0);
	}
}