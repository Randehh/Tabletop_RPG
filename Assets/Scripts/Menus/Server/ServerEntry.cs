using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rondo.DnD5E.Menu.Server {

    public class ServerEntry : MonoBehaviour {

        public TextMeshProUGUI nameText;
        public TextMeshProUGUI playersText;

        private Button m_Button;

        public void SetServerData(RoomInfo room, Action<ServerEntry> onClick) {
            m_Button = GetComponent<Button>();

            nameText.text = room.Name;
            playersText.text = room.PlayerCount + "/" + room.MaxPlayers;

            m_Button.onClick.AddListener(() => {
                onClick(this);
            });

            gameObject.SetActive(true);
        }

        public string GetServerName() {
            return nameText.text;
        }
    }

}