using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class ScoreBoard : MonoBehaviourPun
{
    public int score_1 = 0;
    public int score_2 = 0;

    public TextMeshProUGUI TB_ScoreB;
    public TextMeshProUGUI TB_ScoreR;

    public void ChangeScore(int target, int delta)
    {
        if (target == 1)
        {
            score_1 += delta;
        }
        else 
        {
            score_2 += delta;
        }
        photonView.RPC("RPCChangScore", RpcTarget.Others, target, delta);
        TB_ScoreB.text = score_1.ToString();
        TB_ScoreR.text = score_2.ToString();
        if (PhotonNetwork.IsMasterClient)
        {
            NetGameManager.Instance.m_CameraControl.m_Targets = new Transform[1];
            NetGameManager.Instance.SetGame();
        }
    }

    [PunRPC]
    public void RPCChangScore(int target, int delta)
    {
        if (target == 1)
        {
            score_1 += delta;
        }
        else
        {
            score_2 += delta;
        }
        TB_ScoreB.text = score_1.ToString();
        TB_ScoreR.text = score_2.ToString();
        NetGameManager.Instance.m_CameraControl.m_Targets = new Transform[1];
        NetGameManager.Instance.SetGame();
    }
}
