using UnityEngine;
using Photon.Pun;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FlashBall : MonoBehaviourPun
{
    [SerializeField] float speed = 45f;
    [SerializeField] float explodeDelay = 1f;
    [SerializeField] float curveStrength = 10f;

    private Rigidbody rb;
    private Vector3 initialDirection;

    private bool hasExploded = false;

    public void Init(Vector3 direction)
    {
        initialDirection = direction.normalized;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = initialDirection * speed + Vector3.up * 4f;
        StartCoroutine(ExplodeAfterDelay());
    }

    void FixedUpdate()
    {
        if (hasExploded) return;

        Vector3 sideForce = Vector3.Cross(Vector3.up, initialDirection).normalized * curveStrength;
        rb.AddForce(sideForce, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || hasExploded) return;
        Explode();
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explodeDelay);
        if (!hasExploded) Explode();
    }

    void Explode()
    {
        hasExploded = true;

        // Kích hoạt vùng flash (child)
        Transform flashArea = transform.Find("FlashArea");
        if (flashArea != null && photonView.IsMine)
        {
            flashArea.gameObject.SetActive(true);
            flashArea.GetComponent<FlashArea>().Explode(photonView.Owner, photonView.ViewID);
        }

        // Delay nhỏ để đảm bảo RPC gửi xong
        Destroy(gameObject, 0.05f);
    }
}
