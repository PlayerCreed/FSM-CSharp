using System.Collections.Generic;

namespace Fox.FSM
{
    /// <summary>
    /// Abstract class that manages a collection of states and handles transitions.
    /// </summary>
    public abstract class FSMStateLayer : FSMObject
    {
        private readonly Dictionary<string, FSMObject> objects = new Dictionary<string, FSMObject>();

        /// <summary>
        /// Gets the name of the initial active state.
        /// </summary>
        abstract protected string InitialObject { get; }

        /// <summary>
        /// The currently active state object.
        /// </summary>
        protected FSMObject activeObject;

        /// <summary>
        /// Creates a new FSMStateLayer and initializes its states.
        /// </summary>
        /// <param name="name">Unique identifier for this layer.</param>
        /// <param name="layer">Parent layer (can be null for root).</param>
        public FSMStateLayer(string name, FSMStateLayer layer) : base(name, layer)
        {
            InitObject();
            if (objects.TryGetValue(InitialObject, out FSMObject fsmobject))
            {
                activeObject = fsmobject;
            }
            else
            {
                throw new System.InvalidOperationException(
                    $"InitialObject '{InitialObject}' not found in FSMStateLayer '{Name}'. " +
                    "Ensure InitObject() adds the state before constructor completes.");
            }
        }

        /// <summary>
        /// Initializes the state objects contained in this layer.
        /// </summary>
        protected abstract void InitObject();

        /// <summary>
        /// Registers a state object with this layer.
        /// </summary>
        /// <param name="name">Unique name for the state.</param>
        /// <param name="state">The state object to register.</param>
        /// <exception cref="System.ArgumentException">Thrown if a state with the same name already exists.</exception>
        internal void AddObject(string name, FSMObject state)
        {
            if (objects.ContainsKey(name))
            {
                throw new System.ArgumentException(
                    $"FSMObject with name '{name}' already exists in this layer.", nameof(name));
            }
            objects.Add(name, state);
        }

        /// <summary>
        /// Gets a state object by its name.
        /// </summary>
        /// <param name="name">The name of the state to retrieve.</param>
        /// <returns>The state object, or null if not found.</returns>
        public FSMObject GetObject(string name)
        {
            objects.TryGetValue(name, out FSMObject state);
            return state;
        }

        /// <summary>
        /// Processes transitions and updates the active state.
        /// </summary>
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
                        throw new System.InvalidOperationException(
                            $"Transition target state '{translation.NextObject}' not found in FSMStateLayer '{Name}'.");
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
