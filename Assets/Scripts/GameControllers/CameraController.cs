using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _camHolder;
    private Camera _cam;
    
    public Transform target;

    public float moveSpeed;


    void Awake()
    {
        _camHolder = transform.GetChild(0);
        _cam = _camHolder.GetChild(0).GetComponent<Camera>();
    }

    void LateUpdate()
    {
        //_cam.transform.LookAt(target, Vector3.up);
    }

    void FixedUpdate()
    {
        Vector3 targetPos = target.position;
        Vector3 pos = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }
}
