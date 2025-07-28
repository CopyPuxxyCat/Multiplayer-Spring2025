using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FireBuffUtil : MonoBehaviourPun
{
    public void RunBuff(float duration, float speedMultiplier)
    {
        StartCoroutine(BuffRoutine(duration, speedMultiplier));
    }

    private IEnumerator BuffRoutine(float time, float multiplier)
    {
        var player = GetComponent<PlayerController>();
        float originalMove = player.MoveSpeed;
        float originalRun = player.RunSpeed;

        player.MoveSpeed *= multiplier;
        player.RunSpeed *= multiplier;

        yield return new WaitForSeconds(time);

        player.MoveSpeed = originalMove;
        player.RunSpeed = originalRun;
    }
}
