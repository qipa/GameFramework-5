using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine
{
	public class EntityController : IEntityController
	{
        public IEntity Owner { get; set; }
        public virtual void OnDispose() { }
        public virtual void CreateEntity()
        {
            Owner = GOERoot.Scene.AddEntity();
        }
	}
}
