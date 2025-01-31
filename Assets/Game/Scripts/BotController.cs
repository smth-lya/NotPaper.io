using UnityEngine;

public class BotController : MonoBehaviour
{
    [SerializeField] private float _speed = 4f;

    private Vector3 _targetPosition;
    private TrailDrawer _trailDrawer;
    private CollisionChecker _collisionChecker;
    private Territory _territory;
    private bool _isInTerritory = false;

    void Start()
    {
        _trailDrawer = GetComponent<TrailDrawer>();
        _collisionChecker = FindObjectOfType<CollisionChecker>();
        _territory = FindObjectOfType<Territory>();

        SetRandomTarget();
    }

    void Update()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        // Проверяем, столкнулся ли бот с собственным следом
        if (_collisionChecker.CheckTrailCollision(transform.position))
        {
            if (_isInTerritory)
            {
                // Если бот на своей территории и сталкивается с следом, то умирает
                Die();
                return;
            }
        }

        // Проверяем, находится ли бот в своей территории
        if (_collisionChecker.CheckTerritoryCollision(transform.position))
        {
            // Если бот на своей территории, он её захватывает
            if (!_isInTerritory)
            {
                CaptureTerritory();
            }
            _isInTerritory = true;
        }
        else
        {
            _isInTerritory = false;
        }

        // Если бот достиг цели, задаём новую случайную цель
        if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
        {
            SetRandomTarget();
        }
        else
        {
            // Двигаемся к цели
            Vector3 moveDirection = (_targetPosition - transform.position).normalized;
            transform.position += moveDirection * _speed * Time.deltaTime;

            // Добавляем точку в след
            _trailDrawer.AddPoint(transform.position);
        }
    }

    private void SetRandomTarget()
    {
        _targetPosition = new Vector3(Random.Range(-10f, 10f), transform.position.y, Random.Range(-10f, 10f));
    }

    private void CaptureTerritory()
    {
        // Захватываем территорию, когда бот заходит в свою область
        _territory.CaptureArea(_trailDrawer.GetTrail());
        _trailDrawer.ClearTrail(); // Очищаем след после захвата
        SetRandomTarget(); // Устанавливаем новую цель
    }

    private void Die()
    {
        Debug.Log($"Бот {gameObject.name} погиб!");
        Destroy(gameObject);
    }
}
