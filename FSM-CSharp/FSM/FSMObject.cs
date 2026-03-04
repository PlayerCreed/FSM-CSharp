using System.Collections.Generic;

namespace Fox.FSM
{
    /// <summary>
    /// Base abstract class for all FSM components.
    /// </summary>
    public abstract class FSMObject
    {
        /// <summary>
        /// Abstract class for defining state transitions.
        /// </summary>
        public abstract class FSMTranslation
        {
            /// <summary>
            /// Gets whether this transition is valid and should be executed.
            /// </summary>
            public abstract bool IsValid { get; }

            /// <summary>
            /// Gets the name of the target state for this transition.
            /// </summary>
            public abstract string NextObject { get; }

            /// <summary>
            /// Creates a new transition and automatically registers it with the parent object.
            /// </summary>
            /// <param name="ob">The parent FSMObject to register this transition with.</param>
            public FSMTranslation(FSMObject ob)
            {
                ob.AddTranslation(this);
            }

            /// <summary>
            /// Called when the transition is executed. Override to add custom logic.
            /// </summary>
            internal virtual void OnTransition()
            {
            }
        }

        /// <summary>
        /// The name identifier of this FSM object.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The name identifier of this FSM object.
        /// </summary>
        protected string name;

        /// <summary>
        /// The parent state layer that contains this object.
        /// </summary>
        protected FSMStateLayer parent;

        /// <summary>
        /// Gets the root object of the FSM hierarchy.
        /// </summary>
        protected FSMObject Root => GetRoot(this);

        /// <summary>
        /// Collection of transitions registered for this object.
        /// </summary>
        internal readonly HashSet<FSMTranslation> translations = new HashSet<FSMTranslation>();

        /// <summary>
        /// Creates a new FSMObject and registers it with the parent layer.
        /// </summary>
        /// <param name="name">Unique identifier for this object within its layer.</param>
        /// <param name="layer">Parent layer that manages this object (can be null for root).</param>
        public FSMObject(string name, FSMStateLayer layer)
        {
            layer?.AddObject(name, this);
            parent = layer;
            this.name = name;
        }

        private void AddTranslation(FSMTranslation translation)
        {
            translations.Add(translation);
        }

        private FSMObject GetRoot(FSMObject layer)
        {
            if (layer == null)
            {
                return null;
            }
            if (layer.parent == null)
            {
                return layer;
            }
            if (layer == layer.parent)
            {
                return null;
            }
            return GetRoot(layer.parent);
        }

        /// <summary>
        /// Called when this state becomes active.
        /// </summary>
        internal virtual void OnStateEnter() { }

        /// <summary>
        /// Called each frame while this state is active.
        /// </summary>
        internal virtual void OnUpdate() { }

        /// <summary>
        /// Called when transitioning away from this state.
        /// </summary>
        internal virtual void OnStateExit() { }
    }
}
