using System;
using System.Collections.Generic;
using Fox.FSM;

namespace FSMExample
{
    /// <summary>
    /// 高级 FSM 示例：演示层级状态机和带回调的转换
    /// 场景：敌人 AI（巡逻、追逐、攻击）
    /// </summary>
    public class AdvancedUsageExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== FSM 高级使用示例 ===\n");

            // 创建敌人 AI
            var enemy = new EnemyAI("Enemy_001");
            enemy.IsEnabled = true;

            // 模拟游戏循环
            Console.WriteLine("--- 场景 1：正常巡逻 ---");
            EnemyContext.PlayerDistance = 20f;
            for (int i = 0; i < 3; i++)
            {
                enemy.Update();
            }

            Console.WriteLine("\n--- 场景 2：发现玩家 ---");
            EnemyContext.PlayerDistance = 5f;
            for (int i = 0; i < 3; i++)
            {
                enemy.Update();
            }

            Console.WriteLine("\n--- 场景 3：进入攻击范围 ---");
            EnemyContext.PlayerDistance = 1f;
            for (int i = 0; i < 3; i++)
            {
                enemy.Update();
            }

            Console.WriteLine("\n--- 场景 4：玩家逃跑 ---");
            EnemyContext.PlayerDistance = 15f;
            for (int i = 0; i < 3; i++)
            {
                enemy.Update();
            }

            Console.WriteLine("\n=== 示例结束 ===");
        }
    }

    #region 上下文

    /// <summary>
    /// 敌人共享上下文（实际应用中应为实例数据）
    /// </summary>
    public static class EnemyContext
    {
        public static float PlayerDistance { get; set; } = 20f;
        public const float ChaseRange = 10f;
        public const float AttackRange = 2f;
    }

    #endregion

    #region 敌人 AI 驱动器

    /// <summary>
    /// 敌人 AI 状态机
    /// </summary>
    public class EnemyAI : FSMDriver
    {
        public EnemyAI(string name) : base(name)
        {
        }

        protected override string InitialObject => "Patrol";

        protected override void InitObject()
        {
            new PatrolState(this);
            new ChaseState(this);
            new AttackState(this);
        }
    }

    #endregion

    #region 状态定义

    /// <summary>
    /// 巡逻状态
    /// </summary>
    public class PatrolState : FSMState
    {
        public PatrolState(FSMStateLayer layer) : base("Patrol", layer)
        {
            new PatrolToChaseTransition(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Patrol] 开始巡逻");
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("[Patrol] 巡逻中... (玩家距离: {0:F1})", EnemyContext.PlayerDistance);
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Patrol] 停止巡逻");
        }
    }

    /// <summary>
    /// 追逐状态
    /// </summary>
    public class ChaseState : FSMState
    {
        public ChaseState(FSMStateLayer layer) : base("Chase", layer)
        {
            new ChaseToAttackTransition(this);
            new ChaseToPatrolTransition(this);
        }

        internal override void OnStateEnter()
        {
            Console.WriteLine("[Chase] 发现目标，开始追逐！");
        }

        internal override void OnUpdate()
        {
            Console.WriteLine("[Chase] 追逐中... (玩家距离: {0:F1})", EnemyContext.PlayerDistance);
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Chase] 停止追逐");
        }
    }

    /// <summary>
    /// 攻击状态
    /// </summary>
    public class AttackState : FSMState
    {
        private int attackCount = 0;

        public AttackState(FSMStateLayer layer) : base("Attack", layer)
        {
            new AttackToChaseTransition(this);
        }

        internal override void OnStateEnter()
        {
            attackCount = 0;
            Console.WriteLine("[Attack] 进入攻击范围！");
        }

        internal override void OnUpdate()
        {
            attackCount++;
            Console.WriteLine("[Attack] 攻击！第 {0} 次", attackCount);
        }

        internal override void OnStateExit()
        {
            Console.WriteLine("[Attack] 目标离开攻击范围");
        }
    }

    #endregion

    #region 带回调的转换

    /// <summary>
    /// 巡逻 -> 追逐（带进入追逐时的回调）
    /// </summary>
    public class PatrolToChaseTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => EnemyContext.PlayerDistance <= EnemyContext.ChaseRange;
        public override string NextObject => "Chase";

        public PatrolToChaseTransition(FSMObject ob) : base(ob) { }

        internal override void OnTransition()
        {
            Console.WriteLine("  >> 触发警报！目标进入追逐范围");
        }
    }

    /// <summary>
    /// 追逐 -> 攻击
    /// </summary>
    public class ChaseToAttackTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => EnemyContext.PlayerDistance <= EnemyContext.AttackRange;
        public override string NextObject => "Attack";

        public ChaseToAttackTransition(FSMObject ob) : base(ob) { }

        internal override void OnTransition()
        {
            Console.WriteLine("  >> 目标进入攻击范围！");
        }
    }

    /// <summary>
    /// 追逐 -> 巡逻（玩家逃离）
    /// </summary>
    public class ChaseToPatrolTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => EnemyContext.PlayerDistance > EnemyContext.ChaseRange;
        public override string NextObject => "Patrol";

        public ChaseToPatrolTransition(FSMObject ob) : base(ob) { }

        internal override void OnTransition()
        {
            Console.WriteLine("  >> 目标丢失，返回巡逻");
        }
    }

    /// <summary>
    /// 攻击 -> 追逐
    /// </summary>
    public class AttackToChaseTransition : FSMObject.FSMTranslation
    {
        public override bool IsValid => EnemyContext.PlayerDistance > EnemyContext.AttackRange;
        public override string NextObject => "Chase";

        public AttackToChaseTransition(FSMObject ob) : base(ob) { }

        internal override void OnTransition()
        {
            Console.WriteLine("  >> 目标逃离，继续追逐");
        }
    }

    #endregion
}
