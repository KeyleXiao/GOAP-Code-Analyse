namespace Player
{
    /// <summary>
    /// Listen <see cref="ThrowingEvents"/> using <see cref="ThrowCSMB.Events"/>
    /// </summary>
    public class ThrowingEvents
    {
        public delegate void SingleThrowParamHandler(Exploder exp);

        public delegate void SwitchThrowableHandler(Exploder oldExp, Exploder newExp);

        public delegate void NoParamHandler();

        public event SingleThrowParamHandler onPullOut;

        public event SingleThrowParamHandler onThrow;

        public event SwitchThrowableHandler onSwitch;

        public event NoParamHandler onNoGrenadeTry;

        public event SingleThrowParamHandler onThrowableExit;

        public void InvokeOnPullOut(Exploder exp)
        {
            if (onPullOut != null)
                onPullOut(exp);
        }

        public void InvokeOnThrow(Exploder exp)
        {
            if (onThrow != null)
                onThrow(exp);
        }

        public void InvokeOnSwitch(Exploder oldExp, Exploder newExp)
        {
            if (onSwitch != null)
                onSwitch(oldExp, newExp);
        }

        public void InvokeOnNoGrenadeTry()
        {
            if (onNoGrenadeTry != null)
                onNoGrenadeTry();
        }

        public void InvokeThrowableExit(Exploder exp)
        {
            if (onThrowableExit != null)
                onThrowableExit(exp);
        }
    }
}