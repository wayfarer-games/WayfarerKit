using WayfarerKit.Patterns.FSM.Predicates;
using WayfarerKit.Patterns.FSM.States;

namespace WayfarerKit.Patterns.FSM.Transitions
{
    public class Transition : ITransition
    {
        public Transition(IState to, IPredicate condition)
        {
            To = to;
            Condition = condition;
        }
        public IState To { get; }
        public IPredicate Condition { get; }
    }
}