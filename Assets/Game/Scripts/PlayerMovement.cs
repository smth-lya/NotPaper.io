using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private LayerMask _pointTargetLayer;
    [SerializeField] private Transform _pointMarker;

    private Rigidbody _rb;
    private Camera _camera;
    private Vector3 _moveDirection;

    private TrailDrawer _trailDrawer;
    private CollisionChecker _collisionChecker;
    private Territory _territory;

    private bool _isOnTerritory = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _camera = Camera.main;
        _trailDrawer = GetComponent<TrailDrawer>();
        _collisionChecker = FindObjectOfType<CollisionChecker>();
        _territory = FindObjectOfType<Territory>();
    }

    private void Update()
    {
        // �������� ����������� �������� � ������� ����
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 10000, _pointTargetLayer))
        {
            _moveDirection = (hit.point - transform.position).normalized;
            _moveDirection.y = 0;
            _pointMarker.position = hit.point;
        }

        // �������� �� ���� � ����������
        if (_collisionChecker.CheckTerritoryCollision(transform.position))
        {
            if (!_isOnTerritory)
            {
                _isOnTerritory = true;
                CaptureTerritory();
            }
        }
        else
        {
            _isOnTerritory = false;
        }
    }

    private void FixedUpdate()
    {
        // ������� ������
        var movePosition = transform.position + _moveDirection * _movementSpeed * Time.fixedDeltaTime;
        var moveRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDirection), _rotationSpeed * Time.fixedDeltaTime);

        _rb.MovePosition(movePosition);
        _rb.MoveRotation(moveRotation);

        // ������ ���� ������ ���� ����� �� �� ����� ����������
        if (!_isOnTerritory)
        {
            _trailDrawer.AddPoint(transform.position);
        }
    }

    private void CaptureTerritory()
    {
        // ����� ����� ������������ � ���� ����������, ����������� �
        _territory.CaptureArea(_trailDrawer.GetTrail());
        _trailDrawer.ClearTrail(); // ������� ���� ����� �������
    }
}
