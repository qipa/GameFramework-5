using System;
using UnityEngine;
namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEEntityComponent : GOEBaseComponent
	{
        internal Entity Entity
		{
			get{ return this.Owner as Entity ;}
		}
		
		internal override void OnDestroy()
		{
			// 先不置空，FireEntityAct可能会调FireEntityEffect里的 //
			//mEntity = null;
		}

        internal virtual void OnLoadGameObject()
        {
        }

        static internal T Add<T>(Entity obj) where T : GOEEntityComponent, new()
		{
			T t = obj.GetComponent<T>();
			if ( t == null )
			{
                t = obj.AddComponent<T>();
			}
			
			return t;
		}

        static internal void EnableComponent<T>(Entity obj, bool bEnable) where T : GOEEntityComponent, new()
		{
			if ( bEnable )
			{
				T t = Add<T>( obj );
				t.Enable = bEnable;
			}
			else
			{
                T t = obj.GetComponent<T>();
				if ( t != null )
				{
					t.Enable = bEnable;
				}
			}				
		}
	}
}

