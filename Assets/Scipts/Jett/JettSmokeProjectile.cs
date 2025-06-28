using UnityEngine;
using Photon.Pun;

public class JettSmokeProjectile : MonoBehaviourPun
{
    [SerializeField] float particleMovementSpeed = 20.0f;
    [SerializeField] float maxDistance = 70.0f;
    [SerializeField] float lifeTime = 10.0f;

    private Vector3 startingPosition;
    private float distanceTraveled = 0f;
    private float timeAlive = 0f;

    private Camera playerCamera;
    private bool isControlled = false;
    private bool wasControlled = false;

    private float downwardForce = -2.0f;
    private float downwardForceIncrement = -3.8f;

    public System.Action<Vector3, Quaternion> OnExplode;

    void Start()
    {
        startingPosition = transform.position;
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifeTime)
        {
            Explode();
            return;
        }

        if (isControlled && playerCamera != null)
        {
            transform.rotation = playerCamera.transform.rotation;
            wasControlled = true;
        }

        Vector3 movementVector = transform.forward * particleMovementSpeed * Time.deltaTime;

        if (!isControlled && wasControlled)
        {
            downwardForce += downwardForceIncrement * Time.deltaTime;
            movementVector += transform.up * downwardForce * Time.deltaTime;
        }

        transform.position += movementVector;
        distanceTraveled = Vector3.Distance(startingPosition, transform.position);

        if (distanceTraveled >= maxDistance)
        {
            Explode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        OnExplode?.Invoke(transform.position, transform.rotation);
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    public void Initialize(bool control, Camera cam)
    {
        isControlled = control;
        wasControlled = control;
        playerCamera = cam;
    }

    public void SetIsControlled(bool control)
    {
        isControlled = control;
    }
}



