namespace WayfarerKit.Patterns.FSM.States
{
    public abstract class BaseState : IState
    {
        public virtual void OnEnter() {}
        public virtual void OnExit() {}

        public virtual void Update() {}
        public virtual void FixedUpdate() {}
    }
}