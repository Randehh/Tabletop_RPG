using System;
using Photon.Pun;
using UnityEngine;

namespace Rondo.DnD5E.Networking.Generic {

    public class NVector3 {

        public Vector3 Value { get { return m_Raw; } set { m_Raw = value; m_X.Value = value.x; m_Y.Value = value.y; m_Z.Value = value.z; } }

        private Vector3 m_Raw;
        private NFloat m_X;
        private NFloat m_Y;
        private NFloat m_Z;
        private bool m_HighPrecision;

        public NVector3(float min, float max, bool highPrecision = false) {
            m_X = new NFloat(min, max, highPrecision, 0);
            m_Y = new NFloat(min, max, highPrecision, 0);
            m_Z = new NFloat(min, max, highPrecision, 0);
            m_HighPrecision = highPrecision;
        }

        public NVector3(
                float minX, float maxX,
                float minY, float maxY,
                float minZ, float maxZ,
                bool highPrecision = false) {
            m_X = new NFloat(minX, maxX, highPrecision, 0);
            m_Y = new NFloat(minY, maxY, highPrecision, 0);
            m_Z = new NFloat(minZ, maxZ, highPrecision, 0);
        }

        public void Send(PhotonStream stream) {
            m_X.Send(stream, true);
            m_Y.Send(stream);
            m_Z.Send(stream);
        }

        public void Receive(PhotonStream stream) {
            m_X.Receive(stream, true);
            m_Y.Receive(stream);
            m_Z.Receive(stream);
            Value = new Vector3(m_X.Value, m_Y.Value, m_Z.Value);
        }

    }

}