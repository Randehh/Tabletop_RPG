using Photon.Pun;
using Rondo.Generic.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rondo.DnD5E.Game.Dice {

    public class GameDiceGroup {

        public GameDice[] dice;
        private Dictionary<GameDice, Vector3> m_Offsets = new Dictionary<GameDice, Vector3>();
        private Dictionary<GameDice, Vector3> m_RotationSpeeds = new Dictionary<GameDice, Vector3>();
        private Action<int> m_OnRollComplete;

        private int m_DiceRolled = 0;
        private int m_DiceSum = 0;

        private Coroutine m_Routine;

        public GameDiceGroup(Action<int> onRollComplete, params GameDice[] dice) {
            m_OnRollComplete = onRollComplete;
            this.dice = dice;

            for (int i = 0; i < dice.Length; i++) {
                GameDice d = dice[i];
                d.ListenToRoll(OnDiceRolled);
                m_Offsets.Add(d, VectorUtils.GetRandomVector3(-dice.Length, dice.Length).normalized * (dice.Length * 0.05f));
                m_RotationSpeeds.Add(d, VectorUtils.GetRandomVector3(-0.5f, 0.5f));
            }
        }

        private void OnDiceRolled(int result) {
            m_DiceRolled++;
            m_DiceSum += result;

            if(m_DiceRolled == dice.Length) {
                if(m_OnRollComplete != null) m_OnRollComplete(m_DiceSum);
            }
        }

        public void SetColor(Color c) {
            foreach(GameDice d in dice) {
                d.SetDiceColor(c);
            }
        }

        public void DeleteGroup() {
            foreach (GameDice d in dice) d.RemoveDice();
        }

        public void AttractToPosition(Vector3 pos, float damping) {
            foreach (GameDice d in dice) {
                Vector3 targetPos = pos + m_Offsets[d];
                AddImpulse(d, targetPos - d.transform.position, m_RotationSpeeds[d]);
                d.Body.velocity *= damping;
                d.Body.angularVelocity *= damping;
            }
        }

        public void AddImpulse(Vector3 power, Vector3 torque, float randomPower = 0) {
            foreach (GameDice d in dice) {
                AddImpulse(d, power + VectorUtils.GetRandomVector3(-randomPower, randomPower), torque);
            }
        }

        private void AddImpulse(GameDice d, Vector3 power, Vector3 torque) {
            d.Body.AddForce(power, ForceMode.VelocityChange);
            d.Body.AddTorque(torque);
        }

        public void ThrowDice(Vector3 power, float torque) {
            foreach (GameDice d in dice) {
                d.ThrowDice(power, VectorUtils.GetRandomVector3(-torque, torque));
            }
        }
    }
}