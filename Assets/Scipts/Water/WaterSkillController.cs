using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSkillController : MonoBehaviourPun
{
    public bool isSkillEnabled { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (!isSkillEnabled) return;

        Debug.Log("bat skill nuoc len");
    }
}
