using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _rotationSpeed;

    [SerializeField] private LayerMask _pointTargetLayer;
    [SerializeField] private Transform _pointMarker;

    private Rigidbody _rb;
    private Camera _camera;

    private Vector3 _moveDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 10000, _pointTargetLayer))
        {
            _moveDirection = (hit.point - transform.position).normalized;
            _moveDirection.y = 0;
            _pointMarker.position = hit.point;
        }
    }

    private void FixedUpdate()
    {
        var movePosition = transform.position + _moveDirection * _movementSpeed * Time.fixedDeltaTime;
        var moveRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDirection), _rotationSpeed * Time.fixedDeltaTime);

        _rb.MovePosition(movePosition);
        _rb.MoveRotation(moveRotation);
    }
}
