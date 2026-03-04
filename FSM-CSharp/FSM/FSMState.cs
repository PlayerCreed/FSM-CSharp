using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fox.FSM
{
    public abstract class FSMState : FSMObject
    {
        public FSMState(string name, FSMStateLayer layer) : base(name, layer)
        {
        }
    }
}
