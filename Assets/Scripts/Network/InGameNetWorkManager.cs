using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class InGameNetWorkManager : MonoBehaviourPunCallbacks
{
    public override void OnDisconnected(DisconnectCause cause)
    {
        leave();
    }

    public override void OnLeftRoom()
    {
        leave();
    }

    public override void OnLeftLobby()
    {
        leave();
    }

    private void OnApplicationQuit()
    {
        leave();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        leave();
    }
    private void leave()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
