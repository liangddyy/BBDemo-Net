using UnityEngine;
using System.Collections;

namespace UFramework.Pool
{
    public static class Extentions
    {
        public static GameObject Spawn(this GameObject obj)
        {
            return ObjectsPool.Spawn(obj, Vector3.zero, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject obj, Vector3 pos, Quaternion rot)
        {
            return ObjectsPool.Spawn(obj, pos, rot);
        }

        public static void Despawn(this GameObject obj)
        {
            ObjectsPool.Despawn(obj);
        }
    }
}