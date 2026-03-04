using System.Collections;
using System.Collections.Generic;

namespace Fox.FSM
{

    public abstract class FSMStateLayer : FSMObject
    {
        Dictionary<string, FSMObject> objects = new Dictionary<string, FSMObject>();

        abstract protected string InitialObject { get; }

        protected FSMObject activeObject;

        public FSMStateLayer(string name, FSMStateLayer layer) : base(name, layer)
        {
            InitObject();
            if (objects.TryGetValue(InitialObject, out FSMObject fsmobject))
            {
                activeObject = fsmobject;
            }
            else
            {
                throw new System.InvalidOperationException($"InitialObject '{InitialObject}' not found in FSMStateLayer '{name}'. Ensure InitObject() adds the state before constructor completes.");
            }
        }

        /// <summary>
        /// 初始化所持状态机对象
        /// </summary>
        protected abstract void InitObject();

        internal void AddObject(string name, FSMObject state)
        {
            if (objects.ContainsKey(name))
            {
                throw new System.ArgumentException($"FSMObject with name '{name}' already exists in this layer.", nameof(name));
            }
            objects.Add(name, state);
        }

        public FSMObject GetObject(string name)
        {
            objects.TryGetValue(name, out FSMObject state);
            return state;
        }

        internal override void OnUpdate()
        {
            if (activeObject == null)
            {
                return;
            }
            foreach (FSMTranslation translation in activeObject.translations)
            {
                if (translation.IsValid)
                {
                    if (!objects.TryGetValue(translation.NextObject, out FSMObject fsmobject))
                    {
                        throw new System.InvalidOperationException($"Transition target state '{translation.NextObject}' not found in FSMStateLayer.");
                    }
                    activeObject.OnStateExit();
                    activeObject = fsmobject;
                    translation.OnTransition();
                    activeObject.OnStateEnter();
                    return;
                }
            }

            activeObject.OnUpdate();
        }
    }
}


