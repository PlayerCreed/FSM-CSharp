using System;
using Fox.FSM;

namespace FSMExample
{
    /// <summary>
    /// Basic FSM example demonstrating a simple player state machine.
    /// States: Idle -> Run -> Jump -> (back to Idle or Run)
    /// </summary>
    public class BasicUsageExample
    {
        public static void Main(string[] args)
        {
            // Create and enable the FSM
            var playerFSM = new PlayerFSM("Player");
            playerFSM.isEnable = true;

            Console.WriteLine("=== FSM Basic Usage Example ===\n");

            // Simulate game loop
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"--- Frame {i + 1} ---");

                // Simulate input (in real usage, this would be actual input)
                PlayerInput.IsRunning = i >= 2 && i <= 5;
                PlayerInput.IsJumping = i == 4;

                // Update the FSM
                playerFSM.Update();
            }

            Console.WriteLine("\n=== Example Complete ===");
        }
    }

    #region Input Simulation

    /// <summary>
    /// Simulated input for demonstration purposes.
    /// In a real application, this would read from actual input devices.
    /// </summary>
    public static class PlayerInput
    {
        public static bool IsRunning { get; set; }
        public static bool IsJumping { get; set; }
    }

    #endregion

    #region FSM Driver

    /// <summary>
    /// Root driver for the player state machine.
    /// </summary>
    public class PlayerFSM : FSMDriver
    {
        public PlayerFSM(string name) : base(name)
        {
        }

        protected override string InitialObject => "Idle";

        protected override void InitObject()
        {
            // Create states
            new IdleState(this);
            new RunState(this);
            new JumpState(this);
        }
    }

    #endregion

    #region States

    /// <summary>
    /// Idle state - player is standing still.
    /// </summary>
    public class IdleState : FSMState
    {
        public IdleState(FSMStateLayer layer) : base("Idle", layer)
        {
            // Register transitions from Idle
            new IdleToRunTransition(this);
            new IdleToJumpTransition(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Idle] Entered idle state");
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("[Idle] Standing still...");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Idle] Leaving idle state");
        }
    }

    /// <summary>
    /// Run state - player is moving.
    /// </summary>
    public class RunState : FSMState
    {
        public RunState(FSMStateLayer layer) : base("Run", layer)
        {
            // Register transitions from Run
            new RunToIdleTransition(this);
            new RunToJumpTransition(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Run] Started running");
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("[Run] Running...");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Run] Stopped running");
        }
    }

    /// <summary>
    /// Jump state - player is in the air.
    /// </summary>
    public class JumpState : FSMState
    {
        private int jumpFrameCount = 0;
        private const int JumpDuration = 2;

        public JumpState(FSMStateLayer layer) : base("Jump", layer)
        {
            // Register transitions from Jump
            new JumpToIdleTransition(this);
            new JumpToRunTransition(this);
        }

        internal override void OnStateEnter()
        {
            jumpFrameCount = 0;
            Console.WriteLine("[Jump] Jumped!");
        }

        internal override void OnUpdate()
        {
            jumpFrameCount++;
            Console.WriteLine($"[Jump] In air (frame {jumpFrameCount})");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Jump] Landed");
        }

        public bool IsJumpComplete => jumpFrameCount >= JumpDuration;
    }

    #endregion

    #region Transitions

    // Idle -> Run
    public class IdleToRunTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => PlayerInput.IsRunning;
        public override string NextObject => "Run";

        public IdleToRunTransition(FSMObject ob) : base(ob) { }
    }

    // Idle -> Jump
    public class IdleToJumpTransition : FSMObject.FSMTransition
    {
        public override bool IsValid => PlayerInput.IsJumping && !PlayerInput.IsRunning;
        public override string NextObject => "Jump";

        public IdleToJumpTransition(FSMObject ob) : base(ob) { }
    }

    // Run -> Idle
    public class RunToIdleTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => !PlayerInput.IsRunning && !PlayerInput.IsJumping;
        public override string NextObject => "Idle";

        public RunToIdleTransition(FSMObject ob) : base(ob) { }
    }

    // Run -> Jump
    public class RunToJumpTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => PlayerInput.IsJumping;
        public override string NextObject => "Jump";

        public RunToJumpTransition(FSMObject ob) : base(ob) { }
    }

    // Jump -> Idle
    public class JumpToIdleTransition : FSMObject.FSMTranslation
    {
        private readonly JumpState jumpState;

        public JumpToIdleTransition(FSMObject ob) : base(ob)
        {
            jumpState = ob as JumpState;
        }

        public override bool IsValid => jumpState.IsJumpComplete && !PlayerInput.IsRunning;
        public override string NextObject => "Idle";
    }

    // Jump -> Run
    public class JumpToRunTransition : FSMObject.FSMTranslation
    {
        private readonly JumpState jumpState;

        public JumpToRunTransition(FSMObject ob) : base(ob)
        {
            jumpState = ob as JumpState;
        }

        public override bool IsValid => jumpState.IsJumpComplete && PlayerInput.IsRunning;
        public override string NextObject => "Run";
    }

    #endregion
}
