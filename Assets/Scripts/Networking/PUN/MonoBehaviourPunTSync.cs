using System;
using Photon.Pun;
using UnityEngine;
using Rondo.DnD5E.Networking.Generic;

namespace Rondo.DnD5E.Networking.PUNExtensions {

    public class MonoBehaviourPunTSync : MonoBehaviourPun, IPunObservable {

        public bool syncPosition = true;
        public bool syncRotation = true;

        protected Rigidbody m_Body;

        private NVector3 m_Position = new NVector3(-5, 5, -4, 1, -5, 5, true);
        private NVector3 m_Rotation = new NVector3(0, 360);

        private Vector3 m_TargetPosition;
        private Quaternion m_TargetRotation;

        protected virtual void Awake() {
            m_Body = GetComponent<Rigidbody>();
        }

        protected virtual void Start() {
            if (m_Body != null && !photonView.IsMine) {
                m_Body.isKinematic = true;
            }

            m_TargetPosition = transform.position;
            m_TargetRotation = transform.localRotation;
        }

        protected virtual void Update() {
            if (photonView.IsMine) return;

            float lerpValue = Time.deltaTime * 10;
            if (syncPosition) transform.position = Vector3.Lerp(transform.position, m_TargetPosition, lerpValue);
            if (syncRotation) transform.localRotation = Quaternion.Lerp(transform.localRotation, m_TargetRotation, lerpValue);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                if (syncPosition) {
                    m_Position.Value = transform.position;
                    m_Position.Send(stream);
                }
                if (syncRotation) {
                    m_Rotation.Value = transform.rotation.eulerAngles;
                    m_Rotation.Send(stream);
                }
            } else {
                if (syncPosition) {
                    m_Position.Receive(stream);
                    m_TargetPosition = m_Position.Value;
                }
                if (syncRotation) {
                    m_Rotation.Receive(stream);
                    m_TargetRotation = Quaternion.Euler(m_Rotation.Value);
                }
            }
        }
    }

}