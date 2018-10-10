using Photon.Pun;
using Rondo.DnD5E.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using Rondo.DnD5E.Networking.Comms;
using Rondo.DnD5E.Networking;
using Rondo.DnD5E.Game.Dice;

namespace Rondo.DnD5E.Game {

    public class GameManager : MonoBehaviourPunCallbacks {

        public static GameManager Instance;
        public static NetworkChunkifier<WebcamChunkHandler> WebcamChunkifier;

        public GamePlayer playerPrefab;
        public GameCamera cameraToUse;
        public Transform tableObj;

        [Header("Generic objects")]
        public GameDiceCollection diceCollection;

        private List<GamePlayer> m_Players = new List<GamePlayer>();
        private GamePlayer m_LocalPlayer;

        public void Awake() {
            Instance = this;
            WebcamChunkifier = new NetworkChunkifier<WebcamChunkHandler>();
        }

        public void Start() {
            m_LocalPlayer = SpawnObjectOverNetwork(playerPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(this);
        }

        public override void OnDisable() {
            base.OnDisable();
            CloseConnections();
            Debug.Log("On disable");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer) {
            base.OnPlayerEnteredRoom(newPlayer);

            if (PhotonNetwork.IsMasterClient) {
                Debug.Log("Player entered room");
            }
        }

        public void RegisterPlayer(GamePlayer player) {
            if (m_Players.Contains(player)) return;
            m_Players.Add(player);
            m_Players.Sort((p1, p2) => p1.PlayerID.CompareTo(p2.PlayerID));
            UpdateTablePositions();
        }

        private void UpdateTablePositions() {
            float distance = Mathf.Clamp(2.5f + (m_Players.Count / 4f), 5f, 7.5f);
            float currentAngle = 0;
            float rotateAngle = 360f / m_Players.Count;

            float tableSize = distance * 1.7f;
            tableObj.localScale = new Vector3(tableSize, 1, tableSize);

            for (int i = 0; i < m_Players.Count; i++) {
                m_Players[i].SetTableLocation(Quaternion.Euler(0, currentAngle, 0), distance);
                currentAngle += rotateAngle;
            }
        }

        public void CloseConnections() {
            List<Connection> oldConnections = NetworkComms.GetExistingConnection(ApplicationLayerProtocolStatus.Enabled);
            foreach (Connection c in oldConnections) {
                c.CloseConnection(false);
            }
            CommsNetworking.CloseConnection();
        }

        public T SpawnObjectOverNetwork<T>(T prefab, Vector3 pos, Quaternion rot)
            where T : MonoBehaviour {
            GameObject obj = PhotonNetwork.Instantiate(prefab.name, pos, rot, 0);
            return obj.GetComponent<T>();
        }
    }

}