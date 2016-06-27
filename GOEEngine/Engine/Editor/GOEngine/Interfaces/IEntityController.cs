using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine
{
    public interface IEntityController
	{
        IEntity Owner{get; set;}
        void CreateEntity();
        void OnDispose();
	}
}
