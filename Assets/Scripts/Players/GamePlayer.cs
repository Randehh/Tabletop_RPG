using Photon.Pun;
using Rondo.DnD5E.Game;
using Rondo.DnD5E.Networking;
using System.Collections.Generic;
using UnityEngine;
using System;
using Rondo.DnD5E.Game.Dice;
using Rondo.Generic.Utils;

namespace Rondo.DnD5E.Gameplay {

    public class GamePlayer : MonoBehaviourPunCallbacks {

        public bool useWebcam = true;
        public Transform handObj;
        public GameObject dicePocketParent;

        [Header("Layers")]
        public LayerMask maskFloor;

        public int PlayerID { get { return photonView.Owner.ActorNumber; } }
        public Vector3 DicePocketPosition { get {
                Vector3 p = transform.position;
                float pl = p.magnitude;
                return (p.normalized * (pl - 2)) - (Vector3.up * 2.1f);
            } }

        private Transform m_Pivot;
        private WebcamHandler m_WebcamHandler;
        private SpriteRenderer m_SpriteRenderer;
        private TextMesh m_TextMesh;
        private Transform m_Camera;
        private ParticleSystem m_DicePocketParticle;
        private SphereCollider m_DicePocketCollider;

        private List<GameDiceGroup> m_DiceOnTable = new List<GameDiceGroup>();
        private List<GameDiceGroup> m_DiceInPocket = new List<GameDiceGroup>();
        private List<GameDiceGroup> m_DiceInHand = new List<GameDiceGroup>();
        private bool m_IsMouseoverDicePocket = false;
        private Plane m_DiceHeightPlane;

        //Smoothing
        private const int UPDATE_RATE = 10;
        private float m_LastUpdateTime;
        private Quaternion m_TargetRotation;
        private Quaternion m_TargetRotationPivot;
        private float m_TargetPlayerDistance;
        private Vector3 m_TargetPositionHand;

        private void Awake() {
            m_Pivot = new GameObject().transform;
            transform.SetParent(m_Pivot);
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            m_TextMesh = GetComponentInChildren<TextMesh>();
            m_DicePocketParticle = dicePocketParent.GetComponentInChildren<ParticleSystem>();
            m_DicePocketCollider = dicePocketParent.GetComponentInChildren<SphereCollider>();

            m_LastUpdateTime = Time.time;
            m_TargetRotation = transform.rotation;
            m_TargetRotationPivot = Quaternion.identity;
            m_TargetPlayerDistance = 2;

            m_DiceHeightPlane = new Plane(Vector3.up, Vector3.up * -2);
        }

        private void Start() {
            if (photonView.IsMine) {
                m_Camera = GameManager.Instance.cameraToUse.transform;
                /*m_WebcamHandler = new WebcamHandler(this, 10);
                if (WebCamTexture.devices.Length == 0) {
                    useWebcam = false;
                } else {
                    useWebcam = true;
                    m_WebcamHandler.SetWebcamIndex(0);
                }*/
            }

            GameManager.Instance.RegisterPlayer(this);
            GameManager.WebcamChunkifier.GetPlayerChunkManager(PlayerID).RegisterPlayer(this, SetWebcamTexture);
        }

        private void Update() {
            if (photonView.IsMine) {
                UpdateCamera();
                UpdateHandObj();
                UpdatePlayerState();
                //UpdateWebcam();

                if (Input.GetKeyDown(KeyCode.Space)) {
                    SpawnDice(0, 0, 0, 0, 0, 1, (result) => { Debug.Log("Sum is " + result); });
                }

                if (Input.GetKeyDown(KeyCode.D)) {
                    foreach (GameDiceGroup g in m_DiceOnTable) g.DeleteGroup();
                    m_DiceOnTable.Clear();
                }

                if(Input.GetMouseButtonDown(0) && m_IsMouseoverDicePocket) {
                    m_DiceInHand.AddRange(m_DiceInPocket);
                    m_DiceInPocket.Clear();

                    m_DicePocketParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    m_DicePocketCollider.enabled = false;
                }
            } else {
                float lerpValue = (Time.time - m_LastUpdateTime) * UPDATE_RATE;
                transform.rotation = Quaternion.Lerp(transform.rotation, m_TargetRotation, lerpValue);
                handObj.position = Vector3.Lerp(handObj.position, m_TargetPositionHand, lerpValue);
            }

            m_Pivot.localRotation = Quaternion.Lerp(m_Pivot.rotation, m_TargetRotationPivot, 1 * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.forward * -m_TargetPlayerDistance, 1 * Time.deltaTime);
        }

        private void FixedUpdate() {
            if (photonView.IsMine) {

                if (m_DiceInPocket.Count != 0) {
                    Vector3 pocketPos = DicePocketPosition;
                    dicePocketParent.transform.position = pocketPos;

                    foreach (GameDiceGroup dc in m_DiceInPocket) {
                        dc.AttractToPosition(pocketPos, 0.5f);
                    }
                }

                if (m_DiceInHand.Count != 0) {
                    if (Input.GetMouseButton(0)) {
                        Ray ray = m_Camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                        float hitDistance;

                        if (m_DiceHeightPlane.Raycast(ray, out hitDistance)) {
                            Vector3 diceCenter = ray.GetPoint(hitDistance);
                            foreach (GameDiceGroup dc in m_DiceInHand) {
                                dc.AttractToPosition(diceCenter, 0.9f);
                            }
                        }
                    } else {
                        foreach (GameDiceGroup dc in m_DiceInHand) {
                            dc.ThrowDice(Vector3.up * 4, 500);
                        }

                        m_DiceOnTable.AddRange(m_DiceInHand);
                        m_DiceInHand.Clear();
                    }
                }
            }
        }

        private void UpdateCamera() {
            m_Camera.position = transform.position;

            if (!Input.GetMouseButton(1)) return;
            transform.Rotate(-Input.GetAxisRaw("Mouse Y") * 1.5f, 0, 0, Space.Self);
            transform.Rotate(0, Input.GetAxisRaw("Mouse X") * 1.5f, 0, Space.World);

            m_Camera.rotation = transform.rotation;
        }

        private void UpdateHandObj() {
            RaycastHit hit;
            Ray ray = m_Camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 10)) {
                handObj.position = hit.point;
                m_IsMouseoverDicePocket = hit.collider == m_DicePocketCollider;
            } else {
                handObj.position = Vector3.up * -1000;
                m_IsMouseoverDicePocket = false;
            }
        }

        private void UpdatePlayerState() {
            float nextUpdate = m_LastUpdateTime + (1f / UPDATE_RATE);
            if (nextUpdate < Time.time) return;
            m_LastUpdateTime = Time.time;

            Vector3 rot = m_Camera.rotation.eulerAngles;
            photonView.RPC("SetPlayerState", RpcTarget.Others, rot, handObj.transform.position);
        }

        [PunRPC]
        private void SetPlayerState(Vector3 rotation, Vector3 handObjPos) {
            m_TargetRotation = Quaternion.Euler(rotation);
            m_TargetPositionHand = handObjPos;
            m_LastUpdateTime = Time.time;
        }

        public void SetTableLocation(Quaternion rotation, float distance) {
            m_TargetRotationPivot = rotation;
            m_TargetPlayerDistance = distance;
        }

        public void SpawnDice(int d4, int d6, int d8, int d10, int d12, int d20, Action<int> onRollComplete) {
            List<GameDice> spawnedDice = new List<GameDice>();
            SpawnDice(GameManager.Instance.diceCollection.d4, d4, spawnedDice);
            SpawnDice(GameManager.Instance.diceCollection.d6, d6, spawnedDice);
            SpawnDice(GameManager.Instance.diceCollection.d8, d8, spawnedDice);
            SpawnDice(GameManager.Instance.diceCollection.d10, d10, spawnedDice);
            SpawnDice(GameManager.Instance.diceCollection.d12, d12, spawnedDice);
            SpawnDice(GameManager.Instance.diceCollection.d20, d20, spawnedDice);

            GameDiceGroup group = new GameDiceGroup(onRollComplete, spawnedDice.ToArray());
            group.SetColor(new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), 1));
            m_DiceInPocket.Add(group);

            if (m_DiceInPocket.Count == 1) {
                m_DicePocketParticle.Play(true);
                m_DicePocketCollider.enabled = true;
            }
        }

        private void SpawnDice(GameDice prefab, int amount, List<GameDice> l) {
            amount = Mathf.Clamp(amount, 0, int.MaxValue);
            if (amount == 0) return;
            for (int i = 0; i < amount; i++) {
                GameDice dice = GameManager.Instance.SpawnObjectOverNetwork(prefab, VectorUtils.GetRandomVector3(-1, 1), Quaternion.Euler(VectorUtils.GetRandomVector3(-100, 100)));
                l.Add(dice);
            }
        }

        private void UpdateWebcam() {
            if (!useWebcam) return;

            if (Input.GetKeyDown(KeyCode.UpArrow)) m_WebcamHandler.DecrementWebcamIndex();
            if (Input.GetKeyDown(KeyCode.DownArrow)) m_WebcamHandler.IncrementWebcamIndex();

            bool isWebcamUpdated = m_WebcamHandler.CheckUpdate();
            if (isWebcamUpdated) {
                SendWebcamChunks();
            }
        }

        private void SendWebcamChunks() {
            //byte[][] chunks = GameManager.WebcamChunkifier.GetChunks(m_WebcamHandler.RawData);

        }

        public void SetWebcamTexture(byte[] data, int width, int height) {
            Color32[] colors = WebcamHandler.DeserializeData(data, width, height);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(colors);
            texture.Apply(false);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            m_SpriteRenderer.sprite = sprite;
        }
    }
}
