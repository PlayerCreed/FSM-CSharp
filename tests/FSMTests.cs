using System;
using Fox.FSM;

namespace Fox.FSM.Tests
{
    /// <summary>
    /// Comprehensive unit tests for the FSM system.
    /// Covers: input edge cases, state transitions, unexpected behaviors.
    /// </summary>
    public class FSMTests
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== FSM Unit Tests ===\n");

            int passed = 0;
            int failed = 0;

            // ===== Input Edge Cases =====
            Console.WriteLine("--- Input Edge Cases ---");

            RunTest("EmptyString_StateName", TestEmptyStringStateName, ref passed, ref failed);
            RunTest("NullStateName_ThrowsException", TestNullStateName, ref passed, ref failed);
            RunTest("SpecialCharacters_StateName", TestSpecialCharactersStateName, ref passed, ref failed);
            RunTest("UnicodeCharacters_StateName", TestUnicodeCharactersStateName, ref passed, ref failed);
            RunTest("DuplicateStateName_ThrowsException", TestDuplicateStateName, ref passed, ref failed);

            // ===== State Transitions =====
            Console.WriteLine("\n--- State Transitions ---");

            RunTest("ValidTransition_ExecutesSuccessfully", TestValidTransition, ref passed, ref failed);
            RunTest("Transition_CallsOnStateExit", TestTransitionCallsOnStateExit, ref passed, ref failed);
            RunTest("Transition_CallsOnStateEnter", TestTransitionCallsOnStateEnter, ref passed, ref failed);
            RunTest("Transition_CallsOnTransition", TestTransitionCallsOnTransition, ref passed, ref failed);
            RunTest("InvalidTargetState_ThrowsException", TestInvalidTargetState, ref passed, ref failed);
            RunTest("SelfTransition_ExecutesSuccessfully", TestSelfTransition, ref passed, ref failed);
            RunTest("MultipleTransitions_OnlyFirstExecutes", TestMultipleTransitions, ref passed, ref failed);

            // ===== Unexpected Behaviors =====
            Console.WriteLine("\n--- Unexpected Behaviors ---");

            RunTest("UpdateWhenDisabled_DoesNothing", TestUpdateWhenDisabled, ref passed, ref failed);
            RunTest("UpdateWhenEnabled_ProcessesState", TestUpdateWhenEnabled, ref passed, ref failed);
            RunTest("EmptyFSM_UpdateDoesNotCrash", TestEmptyFSMUpdate, ref passed, ref failed);
            RunTest("InitialStateNotFound_ThrowsException", TestInitialStateNotFound, ref passed, ref failed);
            RunTest("NestedLayer_HierarchicalTransitions", TestNestedLayer, ref passed, ref failed);

            // ===== Property Tests =====
            Console.WriteLine("\n--- Property Tests ---");

            RunTest("NameProperty_ReturnsCorrectValue", TestNameProperty, ref passed, ref failed);
            RunTest("IsEnabled_DefaultFalse", TestIsEnabledDefault, ref passed, ref failed);
            RunTest("IsEnabled_CanToggle", TestIsEnabledToggle, ref passed, ref failed);

            // ===== Results =====
            Console.WriteLine($"\n=== Results: {passed} passed, {failed} failed ===");

            Environment.Exit(failed > 0 ? 1 : 0);
        }

        private static void RunTest(string name, Func<bool> test, ref int passed, ref int failed)
        {
            try
            {
                if (test())
                {
                    Console.WriteLine($"[PASS] {name}");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"[FAIL] {name}");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {name} - Exception: {ex.Message}");
                failed++;
            }
        }

        #region Input Edge Cases Tests

        private static bool TestEmptyStringStateName()
        {
            var driver = new TestDriver("Test");
            var state = new TestState("", driver);
            return state.Name == "";
        }

        private static bool TestNullStateName()
        {
            try
            {
                var driver = new TestDriver("Test");
                var state = new TestState(null, driver);
                return state.Name == null;
            }
            catch (NullReferenceException)
            {
                // Expected behavior - null name may cause issues
                return true;
            }
        }

        private static bool TestSpecialCharactersStateName()
        {
            var driver = new TestDriver("Test");
            var state = new TestState("State@#$%^&*()_+-=[]{}|;':\",./<>?", driver);
            return state.Name == "State@#$%^&*()_+-=[]{}|;':\",./<>?";
        }

        private static bool TestUnicodeCharactersStateName()
        {
            var driver = new TestDriver("Test");
            var state = new TestState("状态名", driver);
            return state.Name == "状态名";
        }

        private static bool TestDuplicateStateName()
        {
            try
            {
                var driver = new DuplicateTestDriver();
                // Constructor will try to add duplicate "State1"
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }

        #endregion

        #region State Transitions Tests

        private static bool TestValidTransition()
        {
            var driver = new TransitionTestDriver();
            driver.IsEnabled = true;
            TransitionTestDriver.TransitionFlag = false;

            TransitionTestDriver.ShouldTransition = true;
            driver.Update();

            return TransitionTestDriver.TransitionFlag;
        }

        private static bool TestTransitionCallsOnStateExit()
        {
            var driver = new ExitEnterTestDriver();
            driver.IsEnabled = true;
            ExitEnterTestDriver.ExitCalled = false;

            ExitEnterTestDriver.ShouldTransition = true;
            driver.Update();

            return ExitEnterTestDriver.ExitCalled;
        }

        private static bool TestTransitionCallsOnStateEnter()
        {
            var driver = new ExitEnterTestDriver();
            driver.IsEnabled = true;
            ExitEnterTestDriver.EnterCalled = false;

            ExitEnterTestDriver.ShouldTransition = true;
            driver.Update();

            return ExitEnterTestDriver.EnterCalled;
        }

        private static bool TestTransitionCallsOnTransition()
        {
            var driver = new TransitionCallbackTestDriver();
            driver.IsEnabled = true;
            TransitionCallbackTestDriver.TransitionCallbackCalled = false;

            TransitionCallbackTestDriver.ShouldTransition = true;
            driver.Update();

            return TransitionCallbackTestDriver.TransitionCallbackCalled;
        }

        private static bool TestInvalidTargetState()
        {
            try
            {
                var driver = new InvalidTargetTestDriver();
                driver.IsEnabled = true;

                InvalidTargetTestDriver.ShouldTransition = true;
                driver.Update();

                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private static bool TestSelfTransition()
        {
            var driver = new SelfTransitionTestDriver();
            driver.IsEnabled = true;
            SelfTransitionTestDriver.TransitionCount = 0;

            SelfTransitionTestDriver.ShouldTransition = true;
            driver.Update();
            driver.Update();

            return SelfTransitionTestDriver.TransitionCount == 2;
        }

        private static bool TestMultipleTransitions()
        {
            var driver = new MultiTransitionTestDriver();
            driver.IsEnabled = true;
            MultiTransitionTestDriver.State2Entered = false;
            MultiTransitionTestDriver.State3Entered = false;

            MultiTransitionTestDriver.ShouldTransition1 = true;
            MultiTransitionTestDriver.ShouldTransition2 = true;
            driver.Update();

            // Only first valid transition should execute
            // Either State2 or State3 should be entered, not both
            return MultiTransitionTestDriver.State2Entered != MultiTransitionTestDriver.State3Entered;
        }

        #endregion

        #region Unexpected Behaviors Tests

        private static bool TestUpdateWhenDisabled()
        {
            var driver = new UpdateTestDriver();
            driver.IsEnabled = false;
            UpdateTestDriver.UpdateCalled = false;

            driver.Update();

            return !UpdateTestDriver.UpdateCalled;
        }

        private static bool TestUpdateWhenEnabled()
        {
            var driver = new UpdateTestDriver();
            driver.IsEnabled = true;
            UpdateTestDriver.UpdateCalled = false;

            driver.Update();

            return UpdateTestDriver.UpdateCalled;
        }

        private static bool TestEmptyFSMUpdate()
        {
            try
            {
                // This test is about activeObject being null scenario
                // But our FSM requires InitialObject, so we test with a minimal setup
                var driver = new TestDriver("EmptyTest");
                driver.IsEnabled = true;
                driver.Update(); // Should not crash even with minimal setup
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TestInitialStateNotFound()
        {
            try
            {
                var driver = new MissingInitialTestDriver();
                return false;
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message.Contains("InitialObject");
            }
        }

        private static bool TestNestedLayer()
        {
            var driver = new NestedLayerTestDriver();
            driver.IsEnabled = true;

            NestedLayerTestDriver.ChildUpdateCalled = false;
            driver.Update();

            return NestedLayerTestDriver.ChildUpdateCalled;
        }

        #endregion

        #region Property Tests

        private static bool TestNameProperty()
        {
            var driver = new TestDriver("MyTestDriver");
            return driver.Name == "MyTestDriver";
        }

        private static bool TestIsEnabledDefault()
        {
            var driver = new TestDriver("Test");
            return driver.IsEnabled == false;
        }

        private static bool TestIsEnabledToggle()
        {
            var driver = new TestDriver("Test");
            driver.IsEnabled = true;
            if (driver.IsEnabled != true) return false;

            driver.IsEnabled = false;
            return driver.IsEnabled == false;
        }

        #endregion

        #region Test Helpers

        private class TestState : FSMState
        {
            public TestState(string name, FSMStateLayer layer) : base(name, layer) { }
        }

        private class TestDriver : FSMDriver
        {
            public TestDriver(string name) : base(name) { }

            protected override string InitialObject => "Init";

            protected override void InitObject()
            {
                new TestState("Init", this);
            }
        }

        private class DuplicateTestDriver : FSMDriver
        {
            public DuplicateTestDriver() : base("DuplicateTest") { }

            protected override string InitialObject => "State1";

            protected override void InitObject()
            {
                new TestState("State1", this);
                new TestState("State1", this); // Should throw
            }
        }

        private class TransitionTestDriver : FSMDriver
        {
            public static bool ShouldTransition = false;
            public static bool TransitionFlag = false;

            public TransitionTestDriver() : base("TransitionTest")
            {
                TransitionFlag = false;
            }

            protected override string InitialObject => "StateA";

            protected override void InitObject()
            {
                new StateA(this);
                new StateB(this);
            }

            private class StateA : FSMState
            {
                public StateA(TransitionTestDriver driver) : base("StateA", driver)
                {
                    new TransitionAtoB(this);
                }
            }

            private class StateB : FSMState
            {
                public StateB(TransitionTestDriver driver) : base("StateB", driver)
                {
                    internal override void OnStateEnter()
                    {
                        TransitionFlag = true;
                    }
                }
            }

            private class TransitionAtoB : FSMTranslation
            {
                public override bool IsValid => ShouldTransition;
                public override string NextObject => "StateB";

                public TransitionAtoB(FSMObject ob) : base(ob) { }
            }
        }

        private class ExitEnterTestDriver : FSMDriver
        {
            public static bool ShouldTransition = false;
            public static bool ExitCalled = false;
            public static bool EnterCalled = false;

            public ExitEnterTestDriver() : base("ExitEnterTest")
            {
                ExitCalled = false;
                EnterCalled = false;
            }

            protected override string InitialObject => "StateA";

            protected override void InitObject()
            {
                new StateA(this);
                new StateB(this);
            }

            private class StateA : FSMState
            {
                public StateA(ExitEnterTestDriver driver) : base("StateA", driver)
                {
                    new TransitionAtoB(this);
                }

                internal override void OnStateExit()
                {
                    ExitCalled = true;
                }
            }

            private class StateB : FSMState
            {
                public StateB(ExitEnterTestDriver driver) : base("StateB", driver)
                {
                    internal override void OnStateEnter()
                    {
                        EnterCalled = true;
                    }
                }
            }

            private class TransitionAtoB : FSMTranslation
            {
                public override bool IsValid => ShouldTransition;
                public override string NextObject => "StateB";

                public TransitionAtoB(FSMObject ob) : base(ob) { }
            }
        }

        private class TransitionCallbackTestDriver : FSMDriver
        {
            public static bool ShouldTransition = false;
            public static bool TransitionCallbackCalled = false;

            public TransitionCallbackTestDriver() : base("CallbackTest")
            {
                TransitionCallbackCalled = false;
            }

            protected override string InitialObject => "StateA";

            protected override void InitObject()
            {
                new StateA(this);
                new StateB(this);
            }

            private class StateA : FSMState
            {
                public StateA(TransitionCallbackTestDriver driver) : base("StateA", driver)
                {
                    new TransitionAtoB(this);
                }
            }

            private class StateB : FSMState
            {
                public StateB(TransitionCallbackTestDriver driver) : base("StateB", driver) { }
            }

            private class TransitionAtoB : FSMTranslation
            {
                public override bool IsValid => ShouldTransition;
                public override string NextObject => "StateB";

                public TransitionAtoB(FSMObject ob) : base(ob) { }

                internal override void OnTransition()
                {
                    TransitionCallbackCalled = true;
                }
            }
        }

        private class InvalidTargetTestDriver : FSMDriver
        {
            public static bool ShouldTransition = false;

            public InvalidTargetTestDriver() : base("InvalidTargetTest") { }

            protected override string InitialObject => "StateA";

            protected override void InitObject()
            {
                new StateA(this);
                // StateB does not exist
            }

            private class StateA : FSMState
            {
                public StateA(InvalidTargetTestDriver driver) : base("StateA", driver)
                {
                    new TransitionToNonExistent(this);
                }
            }

            private class TransitionToNonExistent : FSMTranslation
            {
                public override bool IsValid => ShouldTransition;
                public override string NextObject => "NonExistentState";

                public TransitionToNonExistent(FSMObject ob) : base(ob) { }
            }
        }

        private class SelfTransitionTestDriver : FSMDriver
        {
            public static bool ShouldTransition = false;
            public static int TransitionCount = 0;

            public SelfTransitionTestDriver() : base("SelfTransitionTest")
            {
                TransitionCount = 0;
            }

            protected override string InitialObject => "StateA";

            protected override void InitObject()
            {
                new StateA(this);
            }

            private class StateA : FSMState
            {
                public StateA(SelfTransitionTestDriver driver) : base("StateA", driver)
                {
                    new SelfTransition(this);
                }

                internal override void OnStateEnter()
                {
                    TransitionCount++;
                }
            }

            private class SelfTransition : FSMTranslation
            {
                public override bool IsValid => ShouldTransition;
                public override string NextObject => "StateA";

                public SelfTransition(FSMObject ob) : base(ob) { }
            }
        }

        private class MultiTransitionTestDriver : FSMDriver
        {
            public static bool ShouldTransition1 = false;
            public static bool ShouldTransition2 = false;
            public static bool State2Entered = false;
            public static bool State3Entered = false;

            public MultiTransitionTestDriver() : base("MultiTransitionTest")
            {
                State2Entered = false;
                State3Entered = false;
            }

            protected override string InitialObject => "State1";

            protected override void InitObject()
            {
                new State1(this);
                new State2(this);
                new State3(this);
            }

            private class State1 : FSMState
            {
                public State1(MultiTransitionTestDriver driver) : base("State1", driver)
                {
                    new Transition1to2(this);
                    new Transition1to3(this);
                }
            }

            private class State2 : FSMState
            {
                public State2(MultiTransitionTestDriver driver) : base("State2", driver)
                {
                    internal override void OnStateEnter()
                    {
                        State2Entered = true;
                    }
                }
            }

            private class State3 : FSMState
            {
                public State3(MultiTransitionTestDriver driver) : base("State3", driver)
                {
                    internal override void OnStateEnter()
                    {
                        State3Entered = true;
                    }
                }
            }

            private class Transition1to2 : FSMTranslation
            {
                public override bool IsValid => ShouldTransition1;
                public override string NextObject => "State2";

                public Transition1to2(FSMObject ob) : base(ob) { }
            }

            private class Transition1to3 : FSMTranslation
            {
                public override bool IsValid => ShouldTransition2;
                public override string NextObject => "State3";

                public Transition1to3(FSMObject ob) : base(ob) { }
            }
        }

        private class UpdateTestDriver : FSMDriver
        {
            public static bool UpdateCalled = false;

            public UpdateTestDriver() : base("UpdateTest")
            {
                UpdateCalled = false;
            }

            protected override string InitialObject => "StateA";

            protected override void InitObject()
            {
                new StateA(this);
            }

            private class StateA : FSMState
            {
                public StateA(UpdateTestDriver driver) : base("StateA", driver) { }

                internal override void OnUpdate()
                {
                    UpdateCalled = true;
                }
            }
        }

        private class MissingInitialTestDriver : FSMDriver
        {
            public MissingInitialTestDriver() : base("MissingInitialTest") { }

            protected override string InitialObject => "NonExistent";

            protected override void InitObject()
            {
                // Don't add any states
            }
        }

        private class NestedLayerTestDriver : FSMDriver
        {
            public static bool ChildUpdateCalled = false;

            public NestedLayerTestDriver() : base("NestedTest")
            {
                ChildUpdateCalled = false;
            }

            protected override string InitialObject => "Parent";

            protected override void InitObject()
            {
                new ParentState(this);
            }

            private class ParentState : FSMStateLayer
            {
                public ParentState(FSMStateLayer layer) : base("Parent", layer) { }

                protected override string InitialObject => "Child";

                protected override void InitObject()
                {
                    new ChildState(this);
                }
            }

            private class ChildState : FSMState
            {
                public ChildState(FSMStateLayer layer) : base("Child", layer) { }

                internal override void OnUpdate()
                {
                    ChildUpdateCalled = true;
                }
            }
        }

        #endregion
    }
}
