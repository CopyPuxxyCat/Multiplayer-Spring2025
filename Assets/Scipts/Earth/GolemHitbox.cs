using UnityEngine;
using Photon.Pun;

public class GolemHitbox : MonoBehaviour
{
    public EarthGolem golem;

    public void ReceiveDamage(float dmg)
    {
        if (golem != null && golem.photonView != null)
        {
            golem.photonView.RPC("GolemReceiveDamage", RpcTarget.All, dmg);
        }
    }
}
