using Rondo.DnD5E.Gameplay;
using System.Collections.Generic;
using UnityEngine;

namespace Rondo.Generic.Utils {

    public static class VectorUtils {

        public static Vector3 GetRandomVector3(float min, float max) {
            return new Vector3(
                Random.Range(min, max),
                Random.Range(min, max),
                Random.Range(min, max)
                );
        }

    }
}