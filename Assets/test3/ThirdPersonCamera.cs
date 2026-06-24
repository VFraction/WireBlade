using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Объект, за которым следует камера.")]
    public Transform target;

    [Tooltip("Смещение камеры относительно цели в локальном пространстве.")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Header("Rotation")]
    [Tooltip("Чувствительность мыши.")]
    public Vector2 sensitivity = new Vector2(3f, 3f);

    [Tooltip("Инвертировать ось Y.")]
    public bool invertY = false;

    [Tooltip("Максимальный угол по вертикали (вверх).")]
    public float maxVerticalAngle = 80f;

    [Tooltip("Минимальный угол по вертикали (вниз).")]
    public float minVerticalAngle = -30f;

    [Header("Distance & Collision")]
    [Tooltip("Минимальное расстояние до цели после срабатывания коллизии.")]
    public float minDistance = 0.5f;

    [Tooltip("Максимальное расстояние до цели (при отсутствии препятствий).")]
    public float maxDistance = 10f;

    [Tooltip("Отступ от поверхности при обнаружении препятствия.")]
    public float collisionOffset = 0.25f;

    [Tooltip("Слои, с которыми проверяется коллизия.")]
    public LayerMask collisionMask = -1;

    [Tooltip("Режим обнаружения препятствий.")]
    public CollisionDetectionMode detectionMode = CollisionDetectionMode.SphereCast;

    [Tooltip("Радиус сферы для SphereCast (обычно равен Near Clip Plane камеры).")]
    public float sphereCastRadius = 0.2f;

    [Header("Look Target")]
    [Tooltip("Дополнительное смещение точки, в которую смотрит камера, относительно позиции цели.")]
    public float lookAtOffset = 1.5f;

    [Header("Smoothing")]
    [Tooltip("Время сглаживания позиции (меньше = резче).")]
    public float positionSmoothTime = 0.1f;

    [Tooltip("Скорость сглаживания поворота (экспоненциальное затухание).")]
    public float rotationSmoothSpeed = 12f;

    [Header("Zoom")]
    [Tooltip("Включить изменение дистанции колёсиком мыши.")]
    public bool enableScrollZoom = true;

    [Tooltip("Чувствительность зума.")]
    public float zoomSensitivity = 2f;

    [Tooltip("Захватывать курсор при старте.")]
    public bool lockCursorOnStart = true;

    private Transform _cameraTransform;
    private float _rotationX;
    private float _rotationY;
    private float _currentDistance;
    private float _targetDistance;
    private float _distanceVelocity;
    private Vector3 _positionVelocity;

    private float _manualDistanceOffset = 0f;

    public enum CollisionDetectionMode
    {
        Raycast,
        SphereCast
    }

    private void Start()
    {
        _cameraTransform = transform;

        Vector3 angles = _cameraTransform.eulerAngles;
        _rotationY = angles.y;
        _rotationX = angles.x;
        if (_rotationX > 180f) _rotationX -= 360f;
        _rotationX = Mathf.Clamp(_rotationX, minVerticalAngle, maxVerticalAngle);

        _currentDistance = offset.magnitude;
        _targetDistance = _currentDistance;

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateRotationInput();
        CalculateDesiredDistance();
        ApplySmoothing();
    }

    private void UpdateRotationInput()
    {
        Vector2 delta = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();
        float mouseX = delta.x * sensitivity.x;
        float mouseY = delta.y * sensitivity.y;

        if (invertY)
            mouseY = -mouseY;

        _rotationY += mouseX;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, minVerticalAngle, maxVerticalAngle);
    }

    private void CalculateDesiredDistance()
    {
        float baseDistance = offset.magnitude + _manualDistanceOffset;
        baseDistance = Mathf.Clamp(baseDistance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
        Vector3 desiredPosition = target.position + rotation * offset.normalized * baseDistance;
        Vector3 direction = desiredPosition - target.position;
        float fullDistance = direction.magnitude;

        if (fullDistance <= 0.001f)
        {
            _targetDistance = minDistance;
            return;
        }

        Vector3 normalizedDirection = direction / fullDistance;
        bool hitObstacle = false;
        float hitDistance = fullDistance;

        switch (detectionMode)
        {
            case CollisionDetectionMode.Raycast:
                if (Physics.Raycast(target.position, normalizedDirection, out RaycastHit hitInfo,
                    fullDistance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    hitDistance = hitInfo.distance;
                    hitObstacle = true;
                }
                break;

            case CollisionDetectionMode.SphereCast:
                if (Physics.SphereCast(target.position, sphereCastRadius, normalizedDirection,
                    out RaycastHit sphereHitInfo, fullDistance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    hitDistance = sphereHitInfo.distance;
                    hitObstacle = true;
                }
                break;
        }

        if (hitObstacle)
            _targetDistance = Mathf.Clamp(hitDistance - collisionOffset, minDistance, fullDistance);
        else
            _targetDistance = fullDistance;

        _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
    }

    private void ApplySmoothing()
    {
        _currentDistance = Mathf.SmoothDamp(
            _currentDistance, _targetDistance,
            ref _distanceVelocity, Mathf.Infinity, Time.deltaTime
        );
        _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
        Vector3 desiredPosition = target.position + rotation * offset.normalized * _currentDistance;

        Vector3 lookTarget = target.position + Vector3.up * lookAtOffset;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - desiredPosition);

        _cameraTransform.position = Vector3.SmoothDamp(
            _cameraTransform.position,
            desiredPosition,
            ref _positionVelocity,
            positionSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        _cameraTransform.rotation = Quaternion.Slerp(
            _cameraTransform.rotation,
            desiredRotation,
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime)
        );
    }

    public void SetTarget(Transform newTarget, bool resetSmoothing = true)
    {
        target = newTarget;
        if (resetSmoothing)
        {
            _positionVelocity = Vector3.zero;
            _distanceVelocity = 0f;
        }
    }

    public void AlignRotationToCamera()
    {
        Vector3 angles = _cameraTransform.eulerAngles;
        _rotationY = angles.y;
        _rotationX = angles.x;
        if (_rotationX > 180f) _rotationX -= 360f;
        _rotationX = Mathf.Clamp(_rotationX, minVerticalAngle, maxVerticalAngle);
    }
}