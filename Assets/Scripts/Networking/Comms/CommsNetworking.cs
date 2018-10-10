
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Rondo.DnD5E.Networking.Comms {

    public static class CommsNetworking {

        public const int WEBCAM_PORT = 12345;

        public static void OpenListenConnection() {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, WEBCAM_PORT);
            Connection.StartListening(ConnectionType.TCP, endpoint);
            Debug.Log("Listening to webcam messages");
        }

        public static void CloseConnection() {
            Connection.StopListening();
        }

        public static void SendMessage(string mType, TCPConnection con, byte[] obj) {
            Debug.Log("Sending a message to...");
            con.SendObject(mType, obj);
        }

        public static void AddMessageListener<T>(string mType, NetworkComms.PacketHandlerCallBackDelegate<T> onReceived) {
            NetworkComms.AppendGlobalIncomingPacketHandler(mType, onReceived);
        }

        public static void RemoveMessageListener<T>(string mType, NetworkComms.PacketHandlerCallBackDelegate<T> onReceived) {
            NetworkComms.RemoveGlobalIncomingPacketHandler(mType, onReceived);
        }

        private class CommsNetworkConnection {
            public int playerID;
            public string ip;
        }
    }
}
