using System;
using Photon.Pun;
using UnityEngine;

namespace Rondo.DnD5E.Networking.PUNExtensions {

    public class MonoBehaviourPunTSync : MonoBehaviourPun, IPunObservable {

        public bool syncPosition = true;
        public bool syncRotation = true;

        protected Rigidbody m_Body;

        private float m_LastUpdateReceived;
        private Vector3 m_TargetPosition;
        private Quaternion m_TargetRotation;

        protected virtual void Awake() {
            m_Body = GetComponent<Rigidbody>();
        }

        protected virtual void Start() {
            if (m_Body != null && !photonView.IsMine) {
                m_Body.isKinematic = true;
            }
        }

        protected virtual void Update() {
            if (photonView.IsMine) return;

            float lerpValue = Time.deltaTime * 1;
            if (syncPosition) transform.position = Vector3.Lerp(transform.position, m_TargetPosition, lerpValue);
            if (syncRotation) transform.localRotation = Quaternion.Lerp(transform.localRotation, m_TargetRotation, lerpValue);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                if(syncPosition) stream.SendNext(transform.position);
                if(syncRotation) stream.SendNext(transform.rotation.eulerAngles);
            } else {
                if(syncPosition) m_TargetPosition = (Vector3)stream.ReceiveNext();
                if(syncRotation) m_TargetRotation = Quaternion.Euler((Vector3)stream.ReceiveNext());
                m_LastUpdateReceived = Time.time;
            }
        }
    }

}