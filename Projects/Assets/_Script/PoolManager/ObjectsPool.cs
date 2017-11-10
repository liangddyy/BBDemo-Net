using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace UFramework.Pool
{
    /// <summary>
    /// 对象池
    /// 参考来源 store/trace
    /// 挂此脚本 ObjectsPool
    /// </summary>
    public class ObjectsPool : MonoBehaviour
    {
        #region 单例

        protected static ObjectsPool instance;

        public static ObjectsPool Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null)
                {
                    // 保证编辑器模式单例可用
                    instance = (ObjectsPool) FindObjectOfType(typeof(ObjectsPool));
                }
#endif
                if (instance == null)
                {
                    Debug.Log("创建缓存池");
                    GameObject animGameObject = new GameObject("[ObjectsPool]");
                    instance = animGameObject.AddComponent<ObjectsPool>();
                }
                return instance;
            }
        }


        protected virtual void Awake()
        {
            if (instance != null)
            {
                if (instance != this)
                    Debug.LogError(GetType() + " 多个实例存在！");
                return;
            }
            instance = this;
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }

        #endregion

        public List<Pool> pools = new List<Pool>();
        public bool debugMessages = true;
        public bool spawnDespawnMessages = true;

#if UNITY_EDITOR
        public bool foldout;
#endif
        public List<GameObject> Prefabs
        {
            get { return pools.Select(pool => pool.Prefab).ToList(); }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < pools.Count; i++)
                pools[i].PreInstantiate();
        }

        public static GameObject Spawn(string name)
        {
            return Spawn(name, Vector3.zero, Quaternion.identity);
        }

        public static GameObject Spawn(string name, Vector3 pos, Quaternion rot)
        {
            //ObjectsPool.instance.pools.Count;
            Pool targetPool = Instance.pools.Where(pool => pool.PoolName == name).FirstOrDefault();

            if (targetPool == null)
                return null;

            GameObject obj = targetPool.GetItem();

            if (obj == null)
            {
                if (Instance.debugMessages)
                    Debug.Log("No such object left");
                return null;
            }

            obj.SetActive(true);
            obj.transform.position = pos;
            obj.transform.rotation = rot;

            if (Instance.spawnDespawnMessages)
                obj.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);

            return obj;
        }

        public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            Pool targetPool = Instance.pools.Where(pool => pool.Prefab == prefab).FirstOrDefault();

            if (targetPool == null)
            {
                targetPool = new Pool(prefab);
                targetPool.allowMore = true;
                Instance.pools.Add(targetPool);
            }


            GameObject obj = targetPool.GetItem();

            if (obj == null)
                return null;

            obj.transform.position = pos;
            obj.transform.rotation = rot;

            if (Instance.spawnDespawnMessages)
                obj.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
            return obj;
        }

        public static void Despawn(GameObject target)
        {
            if (ObjectsPool.Instance.spawnDespawnMessages)
                target.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);

            Pool targetPool = ObjectsPool.Instance.pools.Where(pool => pool.spawned.Contains(target)).FirstOrDefault();

            targetPool.PushItem(target);
        }
    }
}