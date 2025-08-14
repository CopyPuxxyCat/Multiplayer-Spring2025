using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EarthSmoke : MonoBehaviourPun
{
    private float duration;

    // Gọi khi spawn
    public void Init(float smokeDuration)
    {
        duration = smokeDuration;
        StartCoroutine(DestroyAfterSeconds());
    }

    private IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(duration);
        // Nếu là prefab Photon thì dùng PhotonNetwork.Destroy để mọi người đều thấy nó mất
        if (photonView != null && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else if (photonView == null)
        {
            // Nếu không phải object network, chỉ hủy local
            Destroy(gameObject);
        }
    }
}
