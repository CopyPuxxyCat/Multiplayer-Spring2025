using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EarthWall : MonoBehaviourPun
{
    [System.Serializable]
    public class CubeData
    {
        public string name;
        public float hp = 100f;
        public bool scaledUp = false;
        public bool tookDamage = false;
    }

    public List<CubeData> cubes = new List<CubeData>();
    private float scaleDelay = 3f;

    public void Init(float wallLifetime)
    {
        // Lấy danh sách cube con
        cubes.Clear();
        foreach (Transform child in transform)
        {
            CubeData data = new CubeData();
            data.name = child.name;
            cubes.Add(data);
        }

        StartCoroutine(DestroyAfterTime(wallLifetime));
        StartCoroutine(HPScaleAfterDelay(scaleDelay));
    }

    private IEnumerator HPScaleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var c in cubes)
        {
            if (!c.scaledUp)
            {
                c.scaledUp = true;
                if (c.tookDamage)
                    c.hp *= 4f;
                else
                    c.hp = 500f;
            }
        }
    }

    [PunRPC]
    public void ReceiveDamage(int cubeIndex, float dmg)
    {
        if (cubeIndex < 0 || cubeIndex >= cubes.Count) return;

        CubeData cube = cubes[cubeIndex];
        cube.hp -= dmg;
        cube.tookDamage = true;

        if (cube.hp <= 0)
        {
            // Gửi RPC cho tất cả client ẩn cube này
            photonView.RPC("HideCube", RpcTarget.All, cubeIndex);

            // Kiểm tra toàn bộ cube
            bool allDestroyed = true;
            foreach (var c in cubes)
            {
                if (c.hp > 0) { allDestroyed = false; break; }
            }
            if (allDestroyed && photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public void HideCube(int cubeIndex)
    {
        Transform cubeTf = transform.GetChild(cubeIndex);
        if (cubeTf != null)
            cubeTf.gameObject.SetActive(false);
    }



    private IEnumerator DestroyAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
