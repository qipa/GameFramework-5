using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOESceneEntityContainer : GOESceneComponent
	{
        private List<Entity> mListEntity = new List<Entity>();

        internal void AddEntity(Entity obj)
        {
            if (_isUpdating)
            {
                _adding.Add(obj);
                return;
            }
            mListEntity.Add(obj);
        }

        internal Entity GetEntityByID(string id)
        {
            for (int i = 0; i < mListEntity.Count; i++)
            {
                if (mListEntity[i].ID == id)
                {
                    return mListEntity[i];
                }
            }
            return null;
        }
        internal Entity GetEntityByIndex( int index )
        {
            if( index >= 0 && index < mListEntity.Count)
            {
                return mListEntity[index];
            }
            return null;
        }

        internal int GetEntityCount()
        {
            return mListEntity.Count;
        }

        internal void DelEntity(string id)
        {
            for (int i = 0; i < mListEntity.Count; i++)
            {
                if (mListEntity[i].ID == id)
                {
                    DelEntity(mListEntity[i]);
                    return;
                }
            }
        }

        internal void DelEntity(Entity obj)
        {
            if (_isUpdating)
            {
                _deling.Add(obj);
                return;
            }
            if( mListEntity.Contains( obj ))
            {
                mListEntity.Remove(obj);
                obj.Destroy();
            }
        }

        internal override void Update()
        {
            base.Update();

            this.UpdateEntity();
        }

        internal void Clear()
        {
            mListEntity.Clear();
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

            for (int i = 0; i < mListEntity.Count; i++ )
            {
                mListEntity[i].LifeOver = true;
                mListEntity[i].Destroy();
            }
            mListEntity.Clear();
        }

        private bool _isUpdating = false;
        private List<Entity> _adding = new List<Entity>();
        private List<Entity> _deling = new List<Entity>();
        private void UpdateEntity()
        {
            if (mListEntity.Count == 0)
                return;
            _isUpdating = true;
            for (int i = 0; i < mListEntity.Count; i++)
            {
                mListEntity[i].Update();
            }
            _isUpdating = false;
            if (_adding.Count > 0)
            {
                mListEntity.AddRange(_adding);
                _adding.Clear();
            }
            if (_deling.Count > 0)
            {
                foreach (Entity dem in _deling)
                {
                    DelEntity(dem);
                }
                _deling.Clear();
            }
        }
	}
}
