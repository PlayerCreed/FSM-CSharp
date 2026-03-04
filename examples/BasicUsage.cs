using System;
using Fox.FSM;

namespace FSMExample
{
    /// <summary>
    /// 基础 FSM 示例：演示简单的玩家状态机
    /// 状态流转：Idle -> Run -> Jump -> (回到 Idle 或 Run)
    /// </summary>
    public class BasicUsageExample
    {
        public static void Main(string[] args)
        {
            // 创建并启用 FSM
            var playerFSM = new PlayerFSM("Player");
            playerFSM.IsEnabled = true;

            Console.WriteLine("=== FSM 基础使用示例 ===\n");

            // 模拟游戏循环
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"--- 帧 {i + 1} ---");

                // 模拟输入（实际使用时从输入设备读取）
                PlayerInput.IsRunning = i >= 2 && i <= 5;
                PlayerInput.IsJumping = i == 4;

                // 更新 FSM
                playerFSM.Update();
            }

            Console.WriteLine("\n=== 示例结束 ===");
        }
    }

    #region 输入模拟

    /// <summary>
    /// 模拟输入类（演示用）
    /// 实际应用中应从输入设备读取
    /// </summary>
    public static class PlayerInput
    {
        public static bool IsRunning { get; set; }
        public static bool IsJumping { get; set; }
    }

    #endregion

    #region FSM 驱动器

    /// <summary>
    /// 玩家状态机的根驱动器
    /// </summary>
    public class PlayerFSM : FSMDriver
    {
        public PlayerFSM(string name) : base(name)
        {
        }

        protected override string InitialObject => "Idle";

        protected override void InitObject()
        {
            // 创建所有状态
            new IdleState(this);
            new RunState(this);
            new JumpState(this);
        }
    }

    #endregion

    #region 状态定义

    /// <summary>
    /// 空闲状态 - 玩家站立不动
    /// </summary>
    public class IdleState : FSMState
    {
        public IdleState(FSMStateLayer layer) : base("Idle", layer)
        {
            // 注册从 Idle 出发的转换
            new IdleToRunTransition(this);
            new IdleToJumpTransition(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Idle] 进入空闲状态");
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("[Idle] 站立中...");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Idle] 离开空闲状态");
        }
    }

    /// <summary>
    /// 跑步状态 - 玩家正在移动
    /// </summary>
    public class RunState : FSMState
    {
        public RunState(FSMStateLayer layer) : base("Run", layer)
        {
            // 注册从 Run 出发的转换
            new RunToIdleTransition(this);
            new RunToJumpTransition(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Run] 开始跑步");
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("[Run] 跑步中...");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Run] 停止跑步");
        }
    }

    /// <summary>
    /// 跳跃状态 - 玩家在空中
    /// </summary>
    public class JumpState : FSMState
    {
        private int jumpFrameCount = 0;
        private const int JumpDuration = 2;

        public JumpState(FSMStateLayer layer) : base("Jump", layer)
        {
            // 注册从 Jump 出发的转换
            new JumpToIdleTransition(this);
            new JumpToRunTransition(this);
        }

        internal override void OnStateEnter()
        {
            jumpFrameCount = 0;
            Console.WriteLine("[Jump] 跳起！");
        }

        internal override void OnUpdate()
        {
            jumpFrameCount++;
            Console.WriteLine($"[Jump] 空中 (第 {jumpFrameCount} 帧)");
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Jump] 落地");
        }

        public bool IsJumpComplete => jumpFrameCount >= JumpDuration;
    }

    #endregion

    #region 转换定义

    // Idle -> Run
    public class IdleToRunTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => PlayerInput.IsRunning;
        public override string NextObject => "Run";

        public IdleToRunTransition(FSMObject ob) : base(ob) { }
    }

    // Idle -> Jump
    public class IdleToJumpTransition : FSMObject.FSMTranslation
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
