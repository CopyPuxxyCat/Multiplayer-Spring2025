using UnityEngine;
using Photon.Pun;

public class FlashArea : MonoBehaviour
{
    [SerializeField] float flashRadius = 6f;
    [SerializeField] float flashDuration = 3f;
    [SerializeField] LayerMask buildingLayer;

    public void Explode(Photon.Realtime.Player owner, int viewId)
    {
        Collider[] players = Physics.OverlapSphere(transform.position, flashRadius);
        foreach (var col in players)
        {
            if (!col.CompareTag("Player")) continue;

            // Kiểm tra không bị che bởi Building
            Vector3 dir = col.transform.position - transform.position;
            if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, flashRadius, ~0))
            {
                if (!hit.collider.CompareTag("Player")) continue;

                PhotonView targetPV = col.GetComponent<PhotonView>();
                if (targetPV != null)
                {
                    targetPV.RPC("ApplyFlash", targetPV.Owner, flashDuration);
                }
            }
        }
    }
}
