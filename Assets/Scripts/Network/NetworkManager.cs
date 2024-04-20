using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager instance;

    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkManager>();
            }
            return instance;
        }
    }

    public TextMeshProUGUI TB_ConnectionStatus;
    public GameObject P_Lobby;
    public GameObject Btn_JoinMatch;
    public GameObject Btn_StartGame;
    public GameObject TB_RoomInfo;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        print("Connecting...");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnConnectedToMaster()
    {
        print("Connected");
        TB_ConnectionStatus.text = "Connected";
        print(PhotonNetwork.LocalPlayer.NickName);
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            P_Lobby.SetActive(true);
            Btn_JoinMatch.SetActive(true);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        TB_ConnectionStatus.text = "Disconnected";
        print("Disconnected: " + cause.ToString());
        Refresh();
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Refresh();
            return;
        }
        // Btn_StartGame.SetActive(true);
        if (!PhotonNetwork.IsMasterClient)
        {
            TB_RoomInfo.GetComponent<TextMeshProUGUI>().text = "Awaiting the Host to start the game";
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Refresh();
            return;
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            TB_RoomInfo.GetComponent<TextMeshProUGUI>().text = "Awaiting the Host to start the game";
        }
        else
        {
            TB_RoomInfo.GetComponent<TextMeshProUGUI>().text = "Searching for players...";
        }
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            Btn_StartGame.SetActive(true);
            TB_RoomInfo.SetActive(false);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Refresh();
            return;
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            TB_RoomInfo.GetComponent<TextMeshProUGUI>().text = "Awaiting the Host to start the game";
        }
        else
        {
            TB_RoomInfo.GetComponent<TextMeshProUGUI>().text = "Searching for players...";
        }
    }

    public override void OnLeftRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Refresh();
            return;
        }
        Btn_JoinMatch.SetActive(false);
        TB_RoomInfo.SetActive(true);
    }

    public override void OnLeftLobby()
    {
        Refresh();
    }

    public void OnClick_Btn_JoinMatch()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, null, null, null, options);
        Btn_JoinMatch.SetActive(false);
        TB_RoomInfo.SetActive(true);
    }

    public void OnClick_Btn_StartGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Refresh();
            return;
        }
        PhotonNetwork.LoadLevel("GameMain");
    }

    private void Refresh()
    {
        TB_RoomInfo.GetComponent<TextMeshProUGUI>().text = "Searching for players...";
        P_Lobby.SetActive(false);
        Btn_JoinMatch.SetActive(false);
        Btn_StartGame.SetActive(false);
        TB_RoomInfo.SetActive(false);
    }
}
