using System;
using System.Collections.Generic;

namespace RTSGame {

    public class StateMachine<T> {
        // Active State
        public State<T> CurrentState { get; protected set; }
        // Last active State
        public State<T> PreviousState;

        // Time the current State has been active
        public float ElapsedTime;

        // Context of the State Machine
        protected T Context;
        // Holds all States of the machine in a Dictionary to avoid creating them more than once
        protected Dictionary<Type, State<T>> States;

        public StateMachine(T Context, State<T> InitialState) {
            ElapsedTime = 0f;

            States = new Dictionary<Type, State<T>>();

            this.Context = Context;

            // Setup initial State
            AddState(InitialState);
            CurrentState = InitialState;
            CurrentState.Begin();
        }

        // Adds a new State to the machine
        public void AddState(State<T> State) {
            State.Assign(this, Context);
            States[State.GetType()] = State;
        }

        // Updates the State Machine
        public virtual void Update(float DeltaTime) {
            ElapsedTime += DeltaTime;
            CurrentState.Update(DeltaTime);
        }

        // Changes the current State
        public R ChangeState<R>() where R : State<T> {
            // Don't change to the same State
            var NewType = typeof(R);

            if (NewType == CurrentState.GetType())
                return CurrentState as R;

            // End the current State if any
            if (CurrentState != null)
                CurrentState.End();

            // Swap State
            ElapsedTime = 0f;
            PreviousState = CurrentState;

            CurrentState = States[NewType];
            CurrentState.Begin();

            return CurrentState as R;
        }
    }
}
