using System;
using System.Threading;

namespace Commons
{
    /// <summary>
    /// Three state lockfree latch that calls different delegates depending on state.
    /// One delegate is called when the latch is open,
    /// another is called the first time after trapping is armed
    /// and a third is called when the latch is closed.
    /// The latch will automatically change state to closed after first call after arming has been trapped.
    /// </summary>
    public class TrapLatch
    {
        public enum LatchState
        {
            Open,
            Trapping,
            Closed,
        }

        private int state;
        private readonly Action<object> openAction;
        private readonly Action<object> trapAction;
        private readonly Action<object> closedAction;

        public TrapLatch(Action<object> openAction, Action<object> trapAction, Action<object> closedAction)
        {
            this.openAction = openAction;
            this.trapAction = trapAction;
            this.closedAction = closedAction;
        }

        public LatchState State
        {
            get 
            {
                return (LatchState) state; 
            }
            set
            {
                Interlocked.Exchange(ref state, (int) value);
            }
        }

        public void Invoke(object value)
        {
            var previousState = (LatchState) Interlocked.CompareExchange(ref state, (int) LatchState.Closed, (int) LatchState.Trapping);
            switch (previousState)
            {
                case LatchState.Open:
                    openAction.Invoke(value);
                    break;
                case LatchState.Trapping:
                    trapAction.Invoke(value);
                    break;
                case LatchState.Closed:
                    closedAction.Invoke(value);
                    break;
            }
        }
    }
}
