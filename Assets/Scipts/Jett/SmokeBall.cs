using System.Collections;
using UnityEngine;
using Photon.Pun;

public class SmokeBall : MonoBehaviourPun
{
    [SerializeField] float durationSeconds = 5f;

    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime >= durationSeconds)
        {
            Vanish();
        }
    }

    void Vanish()
    {
        if (photonView.IsMine)
        {
            Debug.Log("this is mine");
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
