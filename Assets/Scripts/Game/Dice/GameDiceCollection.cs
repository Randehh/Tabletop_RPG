using Rondo.DnD5E.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace Rondo.DnD5E.Game.Dice {

    [CreateAssetMenu(menuName = "Tabletop/Dice collection")]
    public class GameDiceCollection : ScriptableObject {
        public GameDice d4;
        public GameDice d6;
        public GameDice d8;
        public GameDice d10;
        public GameDice d12;
        public GameDice d20;
    }
}