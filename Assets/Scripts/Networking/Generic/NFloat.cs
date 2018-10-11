using System;
using Photon.Pun;
using UnityEngine;
using Rondo.Generic.Utils;

namespace Rondo.DnD5E.Networking.Generic {

    public class NFloat {

        public float Value { get { return m_Value; } set { m_Value = Mathf.Clamp(value, m_Min, m_Max); } }

        private float m_Value;
        private float m_Min;
        private float m_Max;
        private bool m_HighPrecision;
        private float m_HighPrecisionSectionSize;

        public NFloat(float min, float max, bool highPrecision, float value = 0) {
            m_Min = min;
            m_Max = max;
            Value = value;
            m_HighPrecision = highPrecision;
            m_HighPrecisionSectionSize = (m_Max - m_Min) / 255f;
        }

        public void Send(PhotonStream stream, bool debug = false) {
            float remapValue = Value.Remap(m_Min, m_Max, byte.MinValue, byte.MaxValue);
            if (m_HighPrecision) {
                int section = Mathf.FloorToInt(remapValue);
                remapValue -= section;
                byte b1 = Convert.ToByte(section);
                byte b2 = Convert.ToByte(remapValue * 255);
                stream.SendNext(b1);
                stream.SendNext(b2);
            } else {
                byte b = Convert.ToByte(remapValue);
                stream.SendNext(b);
            }
        }

        public void Receive(PhotonStream stream, bool debug = false) {
            if (m_HighPrecision) {
                byte b1 = (byte)stream.ReceiveNext();
                byte b2 = (byte)stream.ReceiveNext();
                float section = ((float)b1).Remap(byte.MinValue, byte.MaxValue, m_Min, m_Max);
                float part = (b2 / 255f) * m_HighPrecisionSectionSize;
                Value = section + part;
            } else {
                byte b = (byte)stream.ReceiveNext();
                Value = ((float)b).Remap(byte.MinValue, byte.MaxValue, m_Min, m_Max);
            }
        }

    }

}