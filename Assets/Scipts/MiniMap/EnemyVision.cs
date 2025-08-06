using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class EnemyVision : MonoBehaviourPun
{
    public float viewRadius = 20f;
    [Range(0, 360)] public float viewAngle = 100f;
    public Transform viewPoint; // Viewpoint của chính mình
    public LayerMask blockingLayers; // layer chặn như Building

    private void Update()
    {
        if (!PhotonNetwork.InRoom || !photonView.IsMine || viewPoint == null)
            return;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv == null || pv.IsMine) continue;

            Transform targetViewPoint = player.transform.Find("MinimapViewPoint");
            if (targetViewPoint == null)
            {
                Debug.LogWarning($"Player {player.name} thiếu MinimapViewPoint");
                continue;
            }

            Vector3 dirToEnemy = targetViewPoint.position - viewPoint.position;
            float distance = dirToEnemy.magnitude;

            if (distance > viewRadius) continue;

            Vector3 dirNormalized = dirToEnemy.normalized;
            float angle = Vector3.Angle(transform.forward, dirNormalized);

            // Debug vùng nhìn
            Debug.DrawLine(viewPoint.position, viewPoint.position + Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward * viewRadius, Color.yellow);
            Debug.DrawLine(viewPoint.position, viewPoint.position + Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewRadius, Color.yellow);

            if (angle < viewAngle / 2f)
            {
                // Raycast đến targetViewPoint
                if (Physics.Raycast(viewPoint.position, dirNormalized, out RaycastHit hit, distance, ~0))
                {
                    Debug.DrawLine(viewPoint.position, hit.point, Color.cyan);

                    if (hit.collider.CompareTag("Building"))
                    {
                        MiniMapController.instance.SetIconVisibility(pv.ViewID, false);
                    }
                    else if (hit.collider.CompareTag("MinimapViewPoint")) // đến đúng điểm nhìn
                    {
                        MiniMapController.instance.SetIconVisibility(pv.ViewID, true);
                    }
                    else
                    {
                        MiniMapController.instance.SetIconVisibility(pv.ViewID, false);
                    }
                }
                else
                {
                    // Không bị chặn
                    MiniMapController.instance.SetIconVisibility(pv.ViewID, true);
                }
            }
            else
            {
                MiniMapController.instance.SetIconVisibility(pv.ViewID, false);
            }
        }
    }
}
