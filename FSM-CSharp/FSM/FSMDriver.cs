namespace Fox.FSM
{
    /// <summary>
    /// Root driver class for the state machine. Entry point for game loop integration.
    /// </summary>
    public abstract class FSMDriver : FSMStateLayer
    {
        /// <summary>
        /// Controls whether Update() processes the state machine.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Creates a new FSMDriver with the specified name.
        /// </summary>
        /// <param name="name">Unique identifier for this driver.</param>
        protected FSMDriver(string name) : base(name, null)
        {
        }

        /// <summary>
        /// Updates the state machine. Call this method each frame from your game loop.
        /// Only processes when IsEnabled is true.
        /// </summary>
        public void Update()
        {
            if (!IsEnabled)
                return;
            OnUpdate();
        }
    }
}
