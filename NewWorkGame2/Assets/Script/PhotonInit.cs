using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class PhotonInit : Photon.PunBehaviour
{
    public InputField playerInput;
    bool isGameStart = false;
    string playerName = "";

    public string chatMessege;
    Text chatText;
    ScrollRect scroll_rect = null;

    PhotonView pv;


    void Awake()
    {
        PhotonNetwork.ConnectUsingSettings("MyFps 1.0");

        chatText = GameObject.Find("ChatText").GetComponent<Text>();
        scroll_rect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("No Room");
        PhotonNetwork.CreateRoom("MyRoom");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Finish make a room");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        StartCoroutine(this.CreatePlayer());
    }

    IEnumerator CreatePlayer()
    {
        while(!isGameStart)
        {
            yield return new WaitForSeconds(0.5f);
        }
        GameObject tempPlayer = PhotonNetwork.Instantiate("Player", new Vector3(0, 0, 0), Quaternion.identity, 0);
        tempPlayer.GetComponent<PlayerCtrl>().SetPlayerName(playerName);

        pv = GetComponent<PhotonView>();
        yield return null;
    }

    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }

    public void SetPlayerName()
    {
        Debug.Log(playerInput.text + "를 입력 하셨습니다!");

        if(isGameStart==false)
        {
            playerName = playerInput.text;
            playerInput.text = string.Empty;
            isGameStart = true;
        }
        else
        {
            chatMessege = playerInput.text;
            pv.RPC("ChatInfo", PhotonTargets.All, chatMessege);
            playerInput.text = string.Empty;
        }
    }

    [PunRPC]
    public void ChatInfo(string sChat,PhotonMessageInfo info)
    {
        showChat(sChat);
    }

    public void showChat(string chat)
    {
        chatText.text += chat + "\n";
        scroll_rect.verticalNormalizedPosition = 0.0f;
    }
}
