﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UFramework.Pool
{
    public enum DespawnMode
    {
        Deactivate,
        Move
    }

    [System.Serializable]
    public class Pool
    {
        #region PublicFields

        public GameObject prefab = null;

        public GameObject Prefab
        {
            get { return prefab; }
            set
            {
                if (Application.isEditor)
                {
                    prefab = value;
                    if (prefab != null && m_Root != null)
                        m_Root.name = PoolName + "_Root";
                }
            }
        }

        public DespawnMode mode = DespawnMode.Deactivate;
        public Vector3 despawnPos = new Vector3(-100, -100, -100);
        public List<GameObject> despawned = new List<GameObject>();
        public List<GameObject> spawned = new List<GameObject>();
        public Transform m_Root;
        public bool allowMore = false;
        public bool debugMessages = true;
        public int size;

        #endregion

        #region Properties

        public int SpawnedCount
        {
            get { return spawned.Count; }
        }

        public int TotalCount
        {
            get { return spawned.Count + despawned.Count; }
        }

        public int LeftCount
        {
            get { return despawned.Count; }
        }

        public string PoolName
        {
            get { return prefab == null ? "None" : prefab.name; }
        }

        public bool Empty
        {
            get { return !despawned.Any(); }
        }

        #endregion

#if UNITY_EDITOR
        public bool foldout;
#endif

        /// <summary>
        /// Creates new pool
        /// </summary>
        /// <returns></returns>
        public Pool(GameObject _prefab)
        {
            GameObject root = new GameObject();
            this.Prefab = _prefab;
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(root, "root");
#endif
            root.name = prefab.name + "_Root_Object";
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            m_Root = root.transform;
            if (ObjectsPool.Instance == null)
            {
                Debug.Log("ObjectsPool is null");
            }
            root.transform.parent = ObjectsPool.Instance.transform;
            size = 1;
        }

        /// <summary>
        /// Preinstantiates all objects
        /// </summary>
        public void PreInstantiate()
        {
            for (int i = 0; i < size; i++)
            {
                if (TotalCount > size)
                    break;

                GameObject obj = GameObject.Instantiate(prefab, m_Root.position, m_Root.rotation) as GameObject;
#if UNITY_EDITOR
                UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "instantiated_obj");
#endif
                despawned.Add(obj);
                obj.transform.SetParent(m_Root);
                if (mode == DespawnMode.Deactivate)
                    obj.SetActive(false);
                else
                    obj.transform.position = despawnPos;
            }

            if (ObjectsPool.Instance.debugMessages && debugMessages)
                Debug.Log("PreInstantiate: Pool " + PoolName + " spawned");
        }

        /// <summary>
        /// Adds new object to pool if pool is empty
        /// </summary>
        public void AddNewObject()
        {
            if (!Empty || !allowMore)
                return;

            GameObject obj = GameObject.Instantiate(prefab, m_Root.position, m_Root.rotation) as GameObject;
            despawned.Add(obj);
            obj.transform.SetParent(m_Root);
            obj.SetActive(false);

            if (ObjectsPool.Instance.debugMessages && debugMessages)
                Debug.Log("New object of " + PoolName + "added");
        }

        /// <summary>
        /// 从pool获取一个实例
        /// </summary>
        /// <returns></returns>
        public GameObject GetItem()
        {
            if (Empty)
            {
                if (allowMore)
                {
                    AddNewObject();
                    size++;
                }
                else
                    return null;
            }

            GameObject obj = despawned[0];

            if (obj == null)
                return null;

            despawned.Remove(obj);
            spawned.Add(obj);
            if (mode == DespawnMode.Deactivate)
                obj.SetActive(true);
            obj.transform.parent = null;

            return obj;
        }

        /// <summary>
        /// 放回对象池
        /// </summary>
        /// <param name="obj"></param>
        public void PushItem(GameObject obj)
        {
            if (despawned.Contains(obj) || !spawned.Contains(obj))
                return;

            spawned.Remove(obj);
            despawned.Add(obj);
            if (mode == DespawnMode.Deactivate)
                obj.SetActive(false);
            else
                obj.transform.position = despawnPos;
            obj.transform.parent = m_Root;
        }

        /// <summary>
        /// Clears and deletes pool
        /// </summary>
        public void ClearAndDestroy()
        {
            for (int i = 0; i < despawned.Count; i++)
                Object.DestroyImmediate(despawned[i]);

            despawned.Clear();
        }
    }
}