using WayfarerKit.Patterns.FSM.Predicates;
using WayfarerKit.Patterns.FSM.States;

namespace WayfarerKit.Patterns.FSM.Transitions
{
    public interface ITransition
    {
        IState To { get; }
        IPredicate Condition { get; }
    }
}