using System;
using Fox.FSM;

namespace FSMExample
{
    /// <summary>
    /// 层级状态机示例：演示嵌套状态
    /// 场景：角色战斗系统（地面状态和空中状态各有子状态）
    /// </summary>
    public class HierarchicalUsageExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== 层级状态机示例 ===\n");

            var character = new CharacterFSM("Hero");
            character.IsEnabled = true;

            // 模拟游戏循环
            Console.WriteLine("--- 阶段 1：地面状态 ---");
            CharacterContext.IsGrounded = true;
            CharacterContext.IsMoving = true;
            CharacterContext.WantsJump = false;

            for (int i = 0; i < 2; i++)
            {
                character.Update();
            }

            Console.WriteLine("\n--- 阶段 2：跳跃 ---");
            CharacterContext.WantsJump = true;
            character.Update();

            CharacterContext.WantsJump = false;
            CharacterContext.IsGrounded = false;
            CharacterContext.IsMoving = false;

            for (int i = 0; i < 3; i++)
            {
                character.Update();
            }

            Console.WriteLine("\n--- 阶段 3：落地 ---");
            CharacterContext.IsGrounded = true;
            character.Update();

            Console.WriteLine("\n=== 示例结束 ===");
        }
    }

    #region 上下文

    public static class CharacterContext
    {
        public static bool IsGrounded { get; set; } = true;
        public static bool IsMoving { get; set; } = false;
        public static bool WantsJump { get; set; } = false;
        public static bool JumpComplete { get; set; } = false;
    }

    #endregion

    #region 角色 FSM

    /// <summary>
    /// 角色状态机（包含地面和空中两个层级）
    /// </summary>
    public class CharacterFSM : FSMDriver
    {
        public CharacterFSM(string name) : base(name)
        {
        }

        protected override string InitialObject => "Grounded";

        protected override void InitObject()
        {
            new GroundedState(this);
            new AirborneState(this);
        }
    }

    #endregion

    #region 地面状态层级

    /// <summary>
    /// 地面状态层级（包含 Idle 和 Run 子状态）
    /// </summary>
    public class GroundedState : FSMStateLayer
    {
        public GroundedState(FSMStateLayer layer) : base("Grounded", layer)
        {
        }

        protected override string InitialObject => "Idle";

        protected override void InitObject()
        {
            new GroundedIdleState(this);
            new GroundedRunState(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Grounded] 进入地面状态");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Grounded] 离开地面状态");
        }
    }

    public class GroundedIdleState : FSMState
    {
        public GroundedIdleState(FSMStateLayer layer) : base("Idle", layer)
        {
            new IdleToRunTransition(this);
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("  [Grounded.Idle] 站立");
        }
    }

    public class GroundedRunState : FSMState
    {
        public GroundedRunState(FSMStateLayer layer) : base("Run", layer)
        {
            new RunToIdleTransition(this);
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("  [Grounded.Run] 跑步");
        }
    }

    // 地面层内的转换
    public class IdleToRunTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => CharacterContext.IsMoving;
        public override string NextObject => "Run";

        public IdleToRunTransition(FSMObject ob) : base(ob) { }
    }

    public class RunToIdleTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => !CharacterContext.IsMoving;
        public override string NextObject => "Idle";

        public RunToIdleTransition(FSMObject ob) : base(ob) { }
    }

    #endregion

    #region 空中状态层级

    /// <summary>
    /// 空中状态层级（包含 Rise 和 Fall 子状态）
    /// </summary>
    public class AirborneState : FSMStateLayer
    {
        public AirborneState(FSMStateLayer layer) : base("Airborne", layer)
        {
        }

        protected override string InitialObject => "Rise";

        protected override void InitObject()
        {
            new AirborneRiseState(this);
            new AirborneFallState(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Airborne] 进入空中状态");
            CharacterContext.JumpComplete = false;
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Airborne] 离开空中状态");
        }
    }

    public class AirborneRiseState : FSMState
    {
        private int frameCount = 0;

        public AirborneRiseState(FSMStateLayer layer) : base("Rise", layer)
        {
            new RiseToFallTransition(this);
        }

        internal override void OnStateEnter()
        {
            frameCount = 0;
        }

        internal override void OnUpdate()
        {
            frameCount++;
            Console.WriteLine("  [Airborne.Rise] 上升中 (帧 {0})", frameCount);
            if (frameCount >= 2)
            {
                CharacterContext.JumpComplete = true;
            }
        }
    }

    public class AirborneFallState : FSMState
    {
        public AirborneFallState(FSMStateLayer layer) : base("Fall", layer) { }

        internal override void OnUpdate()
        {
            Console.WriteLine("  [Airborne.Fall] 下落中");
        }
    }

    // 空中层内的转换
    public class RiseToFallTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => CharacterContext.JumpComplete;
        public override string NextObject => "Fall";

        public RiseToFallTransition(FSMObject ob) : base(ob) { }
    }

    #endregion

    #region 层级间转换

    // Grounded -> Airborne（从跳跃触发）
    public class GroundedToAirborneTransition : FSMObject.FSMTranslation
    {
        public GroundedToAirborneTransition(FSMObject ob) : base(ob) { }

        public override bool IsValid => CharacterContext.WantsJump && CharacterContext.IsGrounded;
        public override string NextObject => "Airborne";
    }

    // Airborne -> Grounded（落地）
    public class AirborneToGroundedTransition : FSMObject.FSMTranslation
    {
        public AirborneToGroundedTransition(FSMObject ob) : base(ob) { }

        public override bool IsValid => CharacterContext.IsGrounded;
        public override string NextObject => "Grounded";
    }

    // 为 GroundedState 添加层级转换
    public class GroundedStateWithTransition : GroundedState
    {
        public GroundedStateWithTransition(FSMStateLayer layer) : base(layer)
        {
            new GroundedToAirborneTransition(this);
        }
    }

    // 为 AirborneState 添加层级转换
    public class AirborneStateWithTransition : AirborneState
    {
        public AirborneStateWithTransition(FSMStateLayer layer) : base(layer)
        {
            new AirborneToGroundedTransition(this);
        }
    }

    #endregion
}
