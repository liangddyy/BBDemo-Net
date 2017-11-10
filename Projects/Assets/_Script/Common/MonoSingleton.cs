using UnityEngine;

namespace Babybus.Uno
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Ins
        {
            get { return instance; }
        }

        /// <summary>
        /// 值为true时当多余1个实例的时候,自动销毁最新的,否则放着不管.
        /// </summary>
        protected virtual bool DestroyWhenMultiple { get { return true; } }

        /// <summary>
        /// 值为true时当多余1个实例的时候,自动销毁旧的,用新的替代.
        /// </summary>
        protected virtual bool OverrideWhenMultiple { get { return false; } }

        protected void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                OnAwake();
            }
            else
            {
                if (OverrideWhenMultiple)
                {
                    Debug.Log("A new instance is replacing the old one: " + this.GetType());
                    Destroy(instance);
                    instance = this as T;
                    OnAwake();
                }
                else
                {
                    Debug.LogError("There's already an instance of Singleton \"" + this.GetType() + "\" present!");
                    if (DestroyWhenMultiple)
                    {
                        Destroy(this);
                    }
                }
            }
        }

        protected void OnDestroy()
        {
            OnDestroying();
            if (instance == this)
                instance = null;
        }

        /// <summary>
        /// 如需编写Awake相关代码,重写本方法
        /// </summary>
        protected virtual void OnAwake()
        {

        }

        /// <summary>
        /// 如需编写OnDestroy相关代码,重写本方法
        /// </summary>
        protected virtual void OnDestroying()
        {

        }
    }
}