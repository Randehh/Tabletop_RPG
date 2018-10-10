using Rondo.DnD5E.Game;
using UnityEngine;

namespace Rondo.DnD5E.Gameplay {

    public class WebcamHandler {

        public int Width { get { return m_WebcamTexture.width; } }
        public int Height { get { return m_WebcamTexture.height; } }
        public byte[] RawData { get { return m_WebcamRaw; } }
        public Color32[] ColorData { get { return m_WebcamTexture.GetPixels32(); } }

        private GamePlayer m_Owner;
        private float m_WebcamUpdateRate;
        private float m_LastWebcamUpdate = 0;
        private int m_WebcamIndex = 0;
        private WebCamDevice m_Webcam;
        private WebCamTexture m_WebcamTexture;
        private byte[] m_WebcamRaw;

        public WebcamHandler(GamePlayer owner, float targetTickrate = 15) {
            m_Owner = owner;
            m_WebcamUpdateRate = 1f / targetTickrate;
            m_Webcam = WebCamTexture.devices[0];
        }

        public void IncrementWebcamIndex() {
            SetWebcamIndex(m_WebcamIndex + 1);
        }

        public void DecrementWebcamIndex() {
            SetWebcamIndex(m_WebcamIndex - 1);
        }

        public void SetWebcamIndex(int index) {
            if (m_WebcamTexture != null) m_WebcamTexture.Stop();

            if (index == -1) m_WebcamIndex = WebCamTexture.devices.Length - 1;
            else if (index == WebCamTexture.devices.Length) m_WebcamIndex = 0;
            else m_WebcamIndex = index;

            m_Webcam = WebCamTexture.devices[m_WebcamIndex];
            m_WebcamTexture = new WebCamTexture(m_Webcam.name, 10, 10);
            m_WebcamTexture.Play();

            m_WebcamRaw = new byte[m_WebcamTexture.width * m_WebcamTexture.height * 3];
            //GameManager.Instance.StoreWebcamSetting(Width, Height);

            Debug.Log("Set webcam device to " + m_Webcam.name);
        }

        public bool CheckUpdate() {
            if (m_LastWebcamUpdate + m_WebcamUpdateRate < Time.time) {
                m_LastWebcamUpdate = Time.time;

                //Serialize data
                Color32[] rawColors = m_WebcamTexture.GetPixels32();
                m_WebcamRaw = SerializeData(rawColors, Width, Height);
                return true;
            }
            return false;
        }

        public static byte[] SerializeData(Color32[] rawColors, int width, int height) {
            byte[] data = new byte[rawColors.Length * 3];
            for (int i = 0; i < rawColors.Length; i++) {
                Color32 c = rawColors[i];

                int startIndex = i * 3;
                data[startIndex + 0] = c.r;
                data[startIndex + 1] = c.g;
                data[startIndex + 2] = c.b;
            }

            return data;
            //return CompressionHelper.CompressBytes(data);
        }

        public static Color32[] DeserializeData(byte[] data, int width, int height) {
            //byte[] decompressedData = CompressionHelper.DecompressBytes(data);
            byte[] decompressedData = data;
            int colorCount = decompressedData.Length / 3;
            if(colorCount != width * height) {
                Debug.LogError("Data size doesn't match width and height!");
                return null;
            }

            Color32[] colors = new Color32[colorCount];
            for (int i = 0; i < colorCount; i++) {
                int index = i * 3;
                Color32 c = new Color32(
                    decompressedData[index],
                    decompressedData[index + 1],
                    decompressedData[index + 2],
                    (byte)255);
                colors[i] = c;
            }
            return colors;
        }
    }

}