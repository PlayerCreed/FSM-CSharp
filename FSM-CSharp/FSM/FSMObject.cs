using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fox.FSM
{

    public abstract class FSMObject
    {

        public abstract class FSMTranslation
        {
            public abstract bool IsValid { get; }

            public abstract string NextObject { get; }

            public FSMTranslation(FSMObject ob)
            {
                ob.AddTranslation(this);
            }

            internal virtual void OnTransition()
            {

            }
        }

        protected string name;

        protected FSMStateLayer parent;

        protected FSMObject Root => GetRoot(this);

        internal readonly HashSet<FSMTranslation> translations = new HashSet<FSMTranslation>();

        public FSMObject(string name, FSMStateLayer layer)
        {
            layer?.AddObject(name, this);
            parent = layer;
            this.name = name;
        }

        internal void AddTranslation(FSMTranslation translation)
        {
            translations.Add(translation);
        }

        private FSMObject GetRoot(FSMObject layer)
        {
            if (layer == layer.parent)
            {
                return null;
            }
            if (layer.parent == null)
            {
                return layer;
            }
            else
            {
                return GetRoot(layer.parent);
            }
        }

        internal virtual void OnStateEnter() { }

        internal virtual void OnUpdate() { }

        internal virtual void OnStateExit() { }
    }

}
