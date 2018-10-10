using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rondo.DnD5E.Networking {

    public class NetworkChunkData  {
        public byte[] DataArray { get { return m_CompleteArray; } }

        private int m_ExpectedChunks;
        private byte[][] m_ByteArrays;
        private byte[] m_CompleteArray;
        private int m_ChunksAdded = 0;

        public NetworkChunkData(int expectedChunks) {
            m_ExpectedChunks = expectedChunks;
            m_ByteArrays = new byte[m_ExpectedChunks][];
        }

        public bool AddByteArray(byte[] array, int chunkPart) {
            m_ByteArrays[chunkPart] = array;
            m_ChunksAdded++;

            if (m_ChunksAdded == m_ExpectedChunks) {
                MergeArrays();
                return true;
            } else {
                return false;
            }
        }

        private void MergeArrays() {
            int arraySize = 0;
            for (int i = 0; i < m_ByteArrays.Length; i++) {
                arraySize += m_ByteArrays[i].Length;
            }

            m_CompleteArray = new byte[arraySize];
            int nextIndex = 0;
            for (int i = 0; i < m_ByteArrays.Length; i++) {
                Array.Copy(m_ByteArrays[i], 0, m_CompleteArray, nextIndex, m_ByteArrays[i].Length);
                nextIndex += m_ByteArrays[i].Length;
            }
        }
    }

}