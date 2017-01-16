namespace StateSystems
{
    /// <summary>
    /// Class is used like an interface by <see cref="Shooter.StateSystems.AIShooterStateSystemAnimator"/>
    /// You can inherite from this class or <see cref="Shooter.StateSystems.AIShooterStateSystemAnimator"/> if necessary to use with different <see cref="MonoBehaviour"/>'s-Agent's
    /// </summary>
    public class AIStateSystemAnimator : AIStateSystem
    {
        virtual public bool IsStartedAnimationFinished(string endAnimationShortName, string endAnimationTag)
        {
            return false;
        }

        virtual public float GetCurrentAnimationTime(AIBrain ai, int _lIndex = 0)
        {
            return 0;
        }

        virtual public float GetCurrentAnimationTime(AIBrain ai)
        {
            return 0;
        }

        virtual public bool ComparePlayingAnimation(string name)
        {
            return false;
        }

        virtual public bool ComparePlayingAnimation(string name, int lIndex)
        {
            return false;
        }

        virtual public void EnableLayer(AIBrain ai, int layerIndex, bool isContinuous, bool isImmediate)
        {
        }

        virtual public void InterruptAnimation(AIBrain ai)
        {
        }

        virtual public void DisableLayer(AIBrain ai, int layerIndex, bool isContinuous, bool isImmediate)
        {
        }

        virtual public void SetLayerTargetWeight(AIBrain ai, int layerIndex, float targetWeight, bool isContinuous, bool isImmediate)
        {
        }

        virtual public void AnimateTrigger(AIBrain ai, string paramName, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
        }

        virtual public void AnimateBool(AIBrain ai, string paramName, bool val = true, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
        }

        virtual public void AnimateFloat(AIBrain ai, string paramName, float val = 0, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
        }

        virtual public void AnimateInteger(AIBrain ai, string paramName, int val = 0, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
        }

        virtual public void ResetInterrupt(AIBrain ai)
        {
        }
    }
}