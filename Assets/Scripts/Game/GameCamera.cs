using Rondo.DnD5E.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace Rondo.DnD5E.Game {

    public class GameCamera : MonoBehaviour {

        public static GameCamera Instance;

        public void Awake() {
            Instance = this;
        }

    }
}