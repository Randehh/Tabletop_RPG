using Rondo.DnD5E.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace Rondo.Generic.Utils {

    public static class FloatUtils {

        public static float Remap(this float value, float min, float max, float targetMin, float targetMax) {
            value = Mathf.Clamp(value, min, max);
            float result = targetMin + (value - min) * (targetMax - targetMin) / (max - min);
            if (Mathf.Approximately(result, 0))
                return 0;

            return result;
        }
    }
}