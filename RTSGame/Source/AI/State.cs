namespace RTSGame {

    public abstract class State<T> {
        // State Machine that contains this State
        protected StateMachine<T> Machine;
        // Context of the State (Needed information like a Unit, a list of Units, etc)
        protected T Context;

        public void Assign(StateMachine<T> Machine, T Context) {
            this.Machine = Machine;
            this.Context = Context;
            Initialize();
        }

        // Called after the Machine and Context are set
        public virtual void Initialize() { }

        // Called when the State becomes active
        public virtual void Begin() { }

        // Called every frame the State is active
        public abstract void Update(float DeltaTime);

        // Called when the State stops beign active
        public virtual void End() { }
    }
}
