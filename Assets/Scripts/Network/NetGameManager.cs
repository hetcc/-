using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;

public class NetGameManager : MonoBehaviourPunCallbacks
{
    private static NetGameManager instance;

    public static NetGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetGameManager>();
            }
            return instance;
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game.
    public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
    public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
    public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
    public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
    public string TankPrefab;
    public string ScoreBoardPrefab = "ScoreBoard";

    public Transform[] SpawnPoints;

    private int m_RoundNumber;                  // Which round the game is currently on.
    private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
    private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
    private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
    private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.

    public List<GameObject> currentPlayerTanks = new List<GameObject>();

    const float k_MaxDepenetrationVelocity = float.PositiveInfinity;

    public ScoreBoard score;

    private void Start()
    {
        // This line fixes a change to the physics engine.
        Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;
            
        // Create the delays so they only have to be made once.
        m_StartWait = new WaitForSeconds (m_StartDelay);
        m_EndWait = new WaitForSeconds (m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            score = PhotonNetwork.Instantiate(ScoreBoardPrefab, Vector3.zero, Quaternion.identity).GetComponent<ScoreBoard>();
            int viewID = score.GetComponent<PhotonView>().ViewID;
            photonView.RPC("RPCAddScoreReference", RpcTarget.Others, viewID);
        }

        // Once the tanks have been created and the camera is using them as targets, start the game.
        m_MessageText.gameObject.SetActive(false);
    }

    [PunRPC]
    public void RPCAddScoreReference(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv != null)
        {
            score = pv.GetComponent<ScoreBoard>();
        }
    }

    public void SetGame()
    {
        foreach (GameObject i in currentPlayerTanks)
        {
            if (i.GetComponent<PhotonView>().IsMine)
            {
                PhotonNetwork.Destroy(i);
            }
        }
        currentPlayerTanks = new List<GameObject>();
        SpawnAllTanks();
        SetCameraTargets();
    }

    public void ChangeScore()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            score.ChangeScore(2, 1);
        }
        else 
        {
            score.ChangeScore(1, 1);
        }
    }

    private void SpawnAllTanks()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            int[] viewIDs = new int[PhotonNetwork.PlayerList.Length];
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                GameObject playerObj = PhotonNetwork.Instantiate(TankPrefab, SpawnPoints[i].position, SpawnPoints[i].rotation);
                PhotonView photonView = playerObj.GetComponent<PhotonView>();
                photonView.TransferOwnership(PhotonNetwork.PlayerList[i]);
                currentPlayerTanks.Add(playerObj);
                viewIDs[i] = photonView.ViewID;
            }

            photonView.RPC("AddTankReferences", RpcTarget.Others, viewIDs);
            photonView.RPC("ChangePlayerColor", RpcTarget.All);
        }
    }

    [PunRPC]
    public void AddTankReferences(int[] viewIDs)
    {
        currentPlayerTanks = new List<GameObject>();
        foreach (int id in viewIDs)
        {
            PhotonView pv = PhotonView.Find(id);
            if (pv != null)
            {
                currentPlayerTanks.Add(pv.gameObject);
            }
        }
        SetCameraTargets();
    }

    [PunRPC]
    public void ChangePlayerColor()
    {
        MeshRenderer[] renderers_1 = currentPlayerTanks[0].GetComponentsInChildren<MeshRenderer>();
        MeshRenderer[] renderers_2 = currentPlayerTanks[1].GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers_1)
        {
            renderer.material.color = Color.blue;
        }
        foreach (MeshRenderer renderer in renderers_2)
        {
            renderer.material.color = Color.red;
        }
    }

    private void SetCameraTargets()
    {
        // Create a collection of transforms the same size as the number of tanks.
        Transform[] targets = new Transform[currentPlayerTanks.Count];

        // For each of these transforms...
        for (int i = 0; i < currentPlayerTanks.Count; i++)
        {
            targets[i] = currentPlayerTanks[i].transform;
        }

        // These are the targets the camera should follow.
        m_CameraControl.m_Targets = targets;
    }

    // Returns a string message to display at the end of each round.
    private string EndMessage()
    {
        // By default when a round ends there are no winners so the default end message is a draw.
        string message = "DRAW!";

        // If there is a winner then change the message to reflect that.
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        // Add some line breaks after the initial message.
        message += "\n\n\n\n";

        // Go through all the tanks and add each of their scores to the message.


        // If there is a game winner, change the entire message to reflect that.
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }
}
