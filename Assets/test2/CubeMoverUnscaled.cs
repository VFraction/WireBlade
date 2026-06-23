using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class CubeMoverUnscaled : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private Vector3 pointA = new Vector3(-5f, 0f, 0f);
    [SerializeField] private Vector3 pointB = new Vector3(5f, 0f, 0f);
    [SerializeField] private float duration = 2f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Управление")]
    [SerializeField] private bool startAutomatically = true;

    private Transform _transform;
    private Coroutine _moveRoutine;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }

    private void Start()
    {
        if (startAutomatically)
        {
            StartMovement();
        }
    }

    public void StartMovement()
    {
        if (_moveRoutine != null)
            StopMovement();

        _moveRoutine = StartCoroutine(MoveLoop());
    }

    public void StopMovement()
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
    }

    private void OnDisable()
    {
        StopMovement();
    }

    private IEnumerator MoveLoop()
    {
        Vector3 start = pointA;
        Vector3 end = pointB;

        while (true)
        {
            yield return MoveBetweenPoints(start, end);
            yield return MoveBetweenPoints(end, start);

            start = pointA;
            end = pointB;
        }
    }

    private IEnumerator MoveBetweenPoints(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float evaluatedT = curve.Evaluate(t);
            _transform.position = Vector3.Lerp(from, to, evaluatedT);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _transform.position = to;
    }

    public void ResetToPointA()
    {
        _transform.position = pointA;
    }

    public void ResetToPointB()
    {
        _transform.position = pointB;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB, 0.3f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA, pointB);
    }
#endif
}