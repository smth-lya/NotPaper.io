using System;
using System.Threading.Tasks;
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

    [SerializeField] private bool _isLocalPlayer;

    public event Func<Vector3, Task> OnDirectionChanged;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _camera = Camera.main;
    }

    public void SetMoveDirection(Vector3 direction)
    {
        _moveDirection = direction;
        Debug.Log(gameObject.name + " " + _moveDirection);
    }

    private async void Update()
    {
        //if (_isLocalPlayer && Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 10000, _pointTargetLayer))
        //{
        //    var newDirection = (hit.point - transform.position).normalized;
        //    newDirection.y = 0;

        //    if (_moveDirection != newDirection)
        //    {
        //        _moveDirection = newDirection;

        //        OnDirectionChanged?.Invoke(_moveDirection).ConfigureAwait(false);

        //        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        //        {
        //            _pointMarker.position = hit.point;
        //            Debug.Log("Направление обновлено");
        //        });
        //    }
        //}

        if (_isLocalPlayer)
        {
            var dirX = Input.GetAxisRaw("Horizontal");
            var dirZ = Input.GetAxisRaw("Vertical");

            var newDirection = new Vector3(dirX, 0, dirZ);
            newDirection.y = 0;

            if (_moveDirection != newDirection)
            {
                _moveDirection = newDirection;

                OnDirectionChanged?.Invoke(_moveDirection).ConfigureAwait(false);
            }
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
