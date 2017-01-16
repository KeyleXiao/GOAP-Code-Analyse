using UnityEngine;

namespace Player
{
    /// <summary>
    /// Listen <see cref="LookIKEvents"/> using <see cref="LookIKCSMB.LookEvents"/>
    /// </summary>
    public class LookIKEvents
    {
        public delegate void LookAtObjectEventHandler(Transform transform);

        public delegate void LookAtPositionEventHandler(Vector3 position);

        public delegate void StopLookingAtEventHandler();

        public event LookAtObjectEventHandler onStartToLookAtObject;

        public event LookAtPositionEventHandler onStartToLookAtPosition;

        public event StopLookingAtEventHandler onStopLookingAt;

        #region Invoke Events

        public void InvokeStartToLookAtObject(Transform transform)
        {
            if (onStartToLookAtObject != null)
                onStartToLookAtObject(transform);
        }

        public void InvokeStartToLookAtPosition(Vector3 position)
        {
            if (onStartToLookAtPosition != null)
                onStartToLookAtPosition(position);
        }

        public void InvokeStopLookingAt()
        {
            if (onStopLookingAt != null)
                onStopLookingAt();
        }

        #endregion Invoke Events
    }
}