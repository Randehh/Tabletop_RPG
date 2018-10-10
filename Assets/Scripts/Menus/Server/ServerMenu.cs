using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rondo.DnD5E.Menu.Server {

    public class ServerMenu : MonoBehaviourPunCallbacks {

        private static string GAME_MODE_NAME = "DND5E";
        private static string LOBBY_NAME = "DND_Lobby";
        private static byte MAX_PLAYER_COUNT = 15;
        private static string GAME_SCENE_NAME = "GameScene";

        public RectTransform serverListParent;
        public ServerEntry serverTemplate;
        public GameObject connectingScreen;

        private string m_HostServerName = "";
        private TextMeshProUGUI m_BlockText;

        private void Awake() {
            serverTemplate.gameObject.SetActive(false);
            m_BlockText = connectingScreen.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start() {
            PhotonNetwork.ConnectUsingSettings();
            SetBlockingMessage("Connecting to master server...");
        }

        public override void OnConnectedToMaster() {
            base.OnConnectedToMaster();
            connectingScreen.SetActive(false);
            TypedLobby defaultLobby = new TypedLobby(LOBBY_NAME, LobbyType.Default);
            PhotonNetwork.JoinLobby(defaultLobby);
        }

        public override void OnJoinedLobby() {
            base.OnJoinedLobby();
            SetBlockingMessage("Connecting to lobby...");
        }

        #region Server listing
        public override void OnRoomListUpdate(List<RoomInfo> roomList) {
            base.OnRoomListUpdate(roomList);
            connectingScreen.SetActive(false);

            Debug.Log("Lobby updated, room count = " + roomList.Count);

            foreach (RectTransform child in serverListParent) {
                if (child == serverTemplate.GetComponent<RectTransform>()) continue;
                DestroyImmediate(child.gameObject);
            }

            foreach (RoomInfo room in roomList) {
                ServerEntry nEntry = Instantiate(serverTemplate);
                nEntry.transform.SetParent(serverListParent, false);
                nEntry.SetServerData(room, OnServerJoinButton);
            }
        }
        #endregion

        #region Room joining
        public void OnServerJoinButton(ServerEntry entry) {
            PhotonNetwork.JoinRoom(entry.GetServerName());
            SetBlockingMessage("Joining room " + entry.GetServerName() + "...");
        }

        public override void OnJoinedRoom() {
            base.OnJoinedRoom();
            connectingScreen.SetActive(false);
            Debug.Log("Joined room!");

            PhotonNetwork.LoadLevel(GAME_SCENE_NAME);
        }

        public override void OnJoinRoomFailed(short returnCode, string message) {
            base.OnJoinRoomFailed(returnCode, message);
            connectingScreen.SetActive(false);
            Debug.LogWarning("Failed to join: " + message);
        }
        #endregion

        #region Server hosting
        public void OnHostServerButton() {
            if (string.IsNullOrEmpty(m_HostServerName)) return;

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = MAX_PLAYER_COUNT;

            //Create room with the lobby
            TypedLobby sqlLobby = new TypedLobby(LOBBY_NAME, LobbyType.Default);
            PhotonNetwork.CreateRoom(m_HostServerName, roomOptions, sqlLobby);

            SetBlockingMessage("Creating room...");
        }

        public override void OnCreatedRoom() {
            base.OnCreatedRoom();
            Debug.Log("Room successfully created!");
            connectingScreen.SetActive(false);
        }

        public override void OnCreateRoomFailed(short returnCode, string message) {
            base.OnCreateRoomFailed(returnCode, message);
            connectingScreen.SetActive(false);
            Debug.LogWarning("Room not created: " + message);
        }

        public void OnServerNameChange(TMP_InputField field) {
            m_HostServerName = field.text;
        }
        #endregion

        private void SetBlockingMessage(string s) {
            connectingScreen.SetActive(true);
            m_BlockText.text = s;
        }
    }

}