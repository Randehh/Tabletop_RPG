using Rondo.DnD5E.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rondo.DnD5E.Networking {

    public class NetworkChunkifier<T> where T : NetworkChunkManager {

        public int ChunkCount { get { return m_ChunkCount; } }
        public int ChunkSize { get { return m_ChunkSize; } set { m_ChunkSize = value; } }
        private Dictionary<int, T> m_PlayerChunkData = new Dictionary<int, T>();
        private int m_ChunkSize = 10000;
        private int m_ChunkCount = 0;

        public void BindPlayer(GamePlayer player, Action<byte[]> onTextureUpdate) {
            int playerIndex = player.PlayerID;
            CheckPlayerAvailable(playerIndex);
            m_PlayerChunkData[playerIndex].RegisterAction(onTextureUpdate);
        }

        public void CreateChunk(int playerIndex, int chunkIndex, int chunkAmount) {
            CheckPlayerAvailable(playerIndex);
            m_PlayerChunkData[playerIndex].CreateChunk(chunkIndex, chunkAmount);
        }

        public void AddChunkPart(int playerIndex, byte[] data, int chunkIndex, int chunkPart, int chunkAmount) {
            CheckPlayerAvailable(playerIndex);
            if (!m_PlayerChunkData[playerIndex].HasChunkIndex(chunkIndex)) CreateChunk(playerIndex, chunkIndex, chunkAmount);
            m_PlayerChunkData[playerIndex].AddChunkData(data, chunkIndex, chunkPart);
        }

        public T GetPlayerChunkManager(int playerIndex) {
            CheckPlayerAvailable(playerIndex);
            return m_PlayerChunkData[playerIndex];
        }

        private void CheckPlayerAvailable(int playerIndex) {
            if (m_PlayerChunkData.ContainsKey(playerIndex)) return;
            m_PlayerChunkData.Add(playerIndex, (T)Activator.CreateInstance(typeof(T)));
        }

        public byte[][] GetChunks(byte[] data) {
            byte[] allData = data;
            int index = 0;
            List<byte[]> chunks = new List<byte[]>();
            while (true) {
                bool breakNext = false;
                int nextSize = ChunkSize;
                if (index + nextSize > allData.Length) {
                    nextSize = allData.Length - index;
                    breakNext = true;
                }

                byte[] chunk = new byte[nextSize];
                Array.Copy(allData, index, chunk, 0, nextSize);
                chunks.Add(chunk);

                if (breakNext) break;
                index += ChunkSize;
            }

            m_ChunkCount++;
            return chunks.ToArray();
        }

    }

}