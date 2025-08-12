using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EarthWall : MonoBehaviour
{
    public void Init(float lifeTime)
    {
        StartCoroutine(DestroyAfterTime(lifeTime));
    }

    IEnumerator DestroyAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}
