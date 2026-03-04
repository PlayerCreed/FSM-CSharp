using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fox.FSM
{
    /// <summary>
    /// 有限状态机基类
    /// </summary>
    public abstract class FSMDriver : FSMStateLayer
    {

        public bool isEnable = false;

        protected FSMDriver(string name) : base(name, null)
        {
        }

        public void Update()
        {
            if (!isEnable)
                return;
            OnUpdate();
        }
    }
}
