namespace Fox.FSM
{
    /// <summary>
    /// Abstract class representing a leaf state in the state machine.
    /// </summary>
    public abstract class FSMState : FSMObject
    {
        /// <summary>
        /// Creates a new FSMState with the specified name.
        /// </summary>
        /// <param name="name">Unique identifier for this state within its layer.</param>
        /// <param name="layer">The parent state layer.</param>
        public FSMState(string name, FSMStateLayer layer) : base(name, layer)
        {
        }
    }
}
