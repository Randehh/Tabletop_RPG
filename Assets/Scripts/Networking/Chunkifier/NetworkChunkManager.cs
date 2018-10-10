using System;
using System.Collections.Generic;

namespace Rondo.DnD5E.Networking {

    public class NetworkChunkManager{

        private Action<byte[]> m_OnChunksReceived;
        private Dictionary<int, NetworkChunkData> m_Chunks = new Dictionary<int, NetworkChunkData>();
        private int m_LastShown = 0;

        public NetworkChunkManager() { }

        public virtual void RegisterAction(Action<byte[]> onChunksReceived) {
            m_OnChunksReceived = onChunksReceived;
        }

        public void CreateChunk(int chunkIndex, int expectedChunks) {
            m_Chunks.Add(chunkIndex, new NetworkChunkData(expectedChunks));
        }

        public void AddChunkData(byte[] data, int chunkIndex, int chunkPart) {
            if (m_Chunks[chunkIndex].AddByteArray(data, chunkPart)) {
                if (m_LastShown < chunkIndex) {
                    OnChunksReceived(m_Chunks[chunkIndex].DataArray);
                    m_LastShown = chunkIndex;
                }
                m_Chunks.Remove(chunkIndex);
            }
        }

        public bool HasChunkIndex(int chunkIndex) {
            return m_Chunks.ContainsKey(chunkIndex);
        }

        protected virtual void OnChunksReceived(byte[] data) {
            m_OnChunksReceived(data);
        }
    }
}
