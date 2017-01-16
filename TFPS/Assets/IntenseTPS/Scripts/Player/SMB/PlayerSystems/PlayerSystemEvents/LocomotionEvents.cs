namespace Player
{
    /// <summary>
    /// Listen <see cref="LocomotionEvents"/> using <see cref="LocomotionCSMB.Locomotion_Events"/>
    /// </summary>
    public class LocomotionEvents
    {
        public delegate void OnLocomotionStyleChanged(LocomotionStyle oldStyle, LocomotionStyle newStyle);

        public event OnLocomotionStyleChanged onLocomotionStyleChanged;

        #region Invoke Events

        public void InvokeLocomotionStyleChanged(LocomotionStyle oldStyle, LocomotionStyle newStyle)
        {
            if (onLocomotionStyleChanged != null)
                onLocomotionStyleChanged(oldStyle, newStyle);
        }

        #endregion Invoke Events
    }
}