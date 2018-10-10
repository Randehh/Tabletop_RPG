using System;
using Rondo.DnD5E.Gameplay;
using Rondo.DnD5E.Game;
using UnityEngine;

namespace Rondo.DnD5E.Networking {

    public class WebcamChunkHandler : NetworkChunkManager {

        private GamePlayer m_Player;
        private Action<byte[], int, int> m_OnChunksReceived;

        public WebcamChunkHandler() { }

        public void RegisterPlayer(GamePlayer player, Action<byte[], int, int> onChunksReceived) {
            m_Player = player;
            m_OnChunksReceived = onChunksReceived;
        }

        protected override void OnChunksReceived(byte[] data) {
            //Vector2Int dimensions = GameManager.Instance.GetWebcamSettings(m_Player.PlayerID);
            //m_OnChunksReceived(data, dimensions.x, dimensions.y);
        }
    }

}