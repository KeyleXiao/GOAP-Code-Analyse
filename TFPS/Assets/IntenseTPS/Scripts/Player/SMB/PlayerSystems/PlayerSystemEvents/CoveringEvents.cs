namespace Player
{
    /// <summary>
    /// Listen <see cref="CoveringEvents"/> using <see cref="CoverCSMB.Events"/>
    /// </summary>
    public class CoveringEvents
    {
        public delegate void NoParamHandler();

        public event NoParamHandler onCoverEnter;

        public event NoParamHandler onCoverExit;

        public event NoParamHandler onUpPeek;

        public event NoParamHandler onEdgePeek;

        public event NoParamHandler onPeek;

        public event NoParamHandler onUnEdgePeek;

        public event NoParamHandler onUnUpPeek;

        public event NoParamHandler onUnPeek;

        public void InvokeOnCoverEnter()
        {
            if (onCoverEnter != null)
                onCoverEnter();
        }

        public void InvokeOnCoverExit()
        {
            if (onCoverExit != null)
                onCoverExit();
        }

        public void InvokeOnUpPeek()
        {
            if (onUpPeek != null)
                onUpPeek();
            if (onPeek != null)
                onPeek();
        }

        public void InvokeOnEdgePeek()
        {
            if (onEdgePeek != null)
                onEdgePeek();
            if (onPeek != null)
                onPeek();
        }

        public void InvokeOnUnEdgePeek()
        {
            if (onUnEdgePeek != null)
                onUnEdgePeek();
            if (onUnPeek != null)
                onUnPeek();
        }

        public void InvokeOnUnUpPeek()
        {
            if (onUnUpPeek != null)
                onUnUpPeek();
            if (onUnPeek != null)
                onUnPeek();
        }
    }
}