using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using Rondo.DnD5E.Networking.PUNExtensions;

namespace Rondo.DnD5E.Game.Dice {

    public class GameDice : MonoBehaviourPunTSync {

        public List<Transform> diceNumbers;

        public Rigidbody Body { get { return m_Body; } }

        private MeshRenderer m_MeshRenderer;

        private Action<int> m_OnRolled;
        private bool m_IsRolled = false;
        private Vector3 m_StartScale;

        private bool m_DissappearSequence = false;
        private Vector3 m_DissappearPosition;
        private float m_DissappearRotateSpeed = 0;

        protected override void Awake() {
            base.Awake();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_StartScale = transform.localScale;
            transform.localScale = Vector3.one * 0.001f;
        }

        protected override void Update() {
            if (!m_DissappearSequence) {
                base.Update();
                transform.localScale = Vector3.Lerp(transform.localScale, m_StartScale, 0.5f * Time.deltaTime);
            } else {
                transform.position = Vector3.Lerp(transform.position, m_DissappearPosition, 1.5f * Time.deltaTime);
                transform.Rotate(Vector2.up, m_DissappearRotateSpeed, Space.World);

                if (m_DissappearRotateSpeed >= 20) {
                    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 2 * Time.deltaTime);
                    if (photonView.IsMine && transform.localScale.x <= 0.005f) {
                        PhotonNetwork.Destroy(photonView);
                    }
                }

                m_DissappearRotateSpeed += 10 * Time.deltaTime;
            }
        }

        private void FixedUpdate() {
            if (m_DissappearSequence) return;

            if(photonView.IsMine && m_Body.IsSleeping() && !m_IsRolled) {
                m_IsRolled = true;
                m_Body.isKinematic = true;
                if (m_OnRolled != null) m_OnRolled(GetRolledNumber());
            }
        }

        public void ListenToRoll(Action<int> onRollComplete) {
            m_OnRolled = onRollComplete;
        }

        public int GetRolledNumber() {
            Vector3 up = Vector3.up;
            int currentNumber = 1;
            float smallestAngle = float.MaxValue;
            Vector3 dicePos = transform.position;
            for (int i = 0; i < diceNumbers.Count; i++) {
                int number = i + 1;
                float angle = Vector3.Angle(up, diceNumbers[i].transform.position - dicePos);
                if (angle < smallestAngle) {
                    currentNumber = number;
                    smallestAngle = angle;
                }
            }
            return currentNumber;
        }

        public void ThrowDice(Vector3 force, Vector3 torque) {
            m_Body.AddForce(force, ForceMode.VelocityChange);
            m_Body.AddTorque(torque, ForceMode.VelocityChange);
        }

        public void SetDiceColor(Color c) {
            photonView.RPC("RPCSetDiceColor", RpcTarget.All, c.r, c.g, c.b);
        }

        [PunRPC]
        private void RPCSetDiceColor(float r, float g, float b) {
            Color c = new Color(r, g, b);
            m_MeshRenderer.material.color = c;
            Color numberColor = (c.r + c.g + c.b) / 3 >= 0.5f ? Color.black : Color.white;
            foreach (Transform tmp in diceNumbers) {
                foreach (TextMeshPro dn in tmp.GetComponentsInChildren<TextMeshPro>(true)) {
                    dn.color = numberColor;
                }
            }
        }

        public void RemoveDice() {
            photonView.RPC("RPCRemoveDice", RpcTarget.All);
        }

        [PunRPC]
        private void RPCRemoveDice() {
            m_Body.isKinematic = true;
            GetComponent<Collider>().enabled = false;
            m_DissappearSequence = true;
            m_DissappearPosition = transform.position + Vector3.up;
        }

    }
}