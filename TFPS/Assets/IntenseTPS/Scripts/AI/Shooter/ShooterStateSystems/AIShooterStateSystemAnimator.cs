using StateSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Shooter.StateSystems
{
    /// <summary>
    /// Used to execute <see cref="Animator"/> based animations and has a function to inform that animation is finished or not
    /// </summary>
    public class AIShooterStateSystemAnimator : AIStateSystemAnimator
    {
        private ShooterAnimatorSystemProps animProps;
        private Animator Animator;

        private class QLayerED
        {
            public int LayerIndex { get; private set; }
            private float layerTarget;

            public float LayerTarget
            {
                get { return layerTarget; }
                set { layerTarget = value; }
            }

            public float LastTarget { get; set; }
            public bool switchToLastTargetAtActionExit = false;

            public QLayerED(int _layerIndex, float _layerTarget)
            {
                LayerIndex = _layerIndex; LayerTarget = _layerTarget;
            }
        }

        private List<QLayerED> qLayerEDs = new List<QLayerED>();

        private class QAnims<T>
        {
            public string ParamName { get; private set; }
            public T Value { get; private set; }
            public bool IsNextStateContinuous { get; private set; }
            public int FinishAnimationNameHash { get; private set; }
            public int FinishAnimationTagHash { get; private set; }
            public T LastValue { get; private set; }
            public bool IsActivated { get; set; }
            public int LayerIndexOfNames { get; private set; }
            public bool SkipNextUpdateCheck { get; set; }

            public QAnims(string param, T _value, T _lastValue, bool isCont, string _nextAnimationShortName, string _nextAnimationTag,/* int _lastAnimationShortNameHash, int _lastAnimationTagHash,*/ int _layerIndexNames)
            {
                ParamName = param; Value = _value; IsNextStateContinuous = isCont;
                FinishAnimationNameHash = Animator.StringToHash(_nextAnimationShortName);
                FinishAnimationTagHash = Animator.StringToHash(_nextAnimationTag);
                LastValue = _lastValue;
                LayerIndexOfNames = _layerIndexNames;
            }
        }

        private List<QAnims<bool>> triggerQAnims = new List<QAnims<bool>>();
        private List<QAnims<bool>> boolQAnims = new List<QAnims<bool>>();
        private List<QAnims<float>> floatQAnims = new List<QAnims<float>>();
        private List<QAnims<int>> intQAnims = new List<QAnims<int>>();

        public AIShooterStateSystemAnimator(ShooterAnimatorSystemProps _animProps, Animator _animator)
        {
            Animator = _animator;
            animProps = _animProps;
            qLayerEDs.Add(new QLayerED(1, 0)); // There is only 2 extra layer is used // add another layer 3,4... if you use more
            qLayerEDs.Add(new QLayerED(2, 1)); // ik layer
            qLayerEDs.Add(new QLayerED(3, 1)); // legs layer
        }

        public override void InterruptAnimation(AIBrain ai)
        {
            Animator.SetTrigger("Interrupt");
        }

        public override void ResetInterrupt(AIBrain ai)
        {
            Animator.ResetTrigger("Interrupt");
        }

        private bool CheckAnimationEndCondition<T>(QAnims<T> qAnim)
        {
            if (qAnim.SkipNextUpdateCheck)
            {
                qAnim.SkipNextUpdateCheck = false;
                return false;
            }
            else
            {
                bool namesMatch = Animator.GetCurrentAnimatorStateInfo(qAnim.LayerIndexOfNames).shortNameHash == qAnim.FinishAnimationNameHash;
                bool tagsMatch = Animator.GetCurrentAnimatorStateInfo(qAnim.LayerIndexOfNames).tagHash == qAnim.FinishAnimationTagHash;
                bool isInTransition = Animator.IsInTransition(qAnim.LayerIndexOfNames);

                return (namesMatch || tagsMatch) && !isInTransition;
            }
        }

        public override bool IsStartedAnimationFinished(string endAnimationShortName, string endAnimationTag)
        {
            endAnimationShortName = endAnimationShortName == "" ? "XYZ" : endAnimationShortName;
            endAnimationTag = endAnimationTag == "" ? "ZYX" : endAnimationTag;
            int animationShortNameHash = Animator.StringToHash(endAnimationShortName); int animationTagHash = Animator.StringToHash(endAnimationTag);

            var triggerQAnim = triggerQAnims.Find(x => (x.FinishAnimationNameHash == animationShortNameHash || x.FinishAnimationTagHash == animationTagHash));
            var boolQAnim = boolQAnims.Find(x => (x.FinishAnimationNameHash == animationShortNameHash || x.FinishAnimationTagHash == animationTagHash));
            var floatQAnim = floatQAnims.Find(x => (x.FinishAnimationNameHash == animationShortNameHash || x.FinishAnimationTagHash == animationTagHash));
            var intQAnim = intQAnims.Find(x => (x.FinishAnimationNameHash == animationShortNameHash || x.FinishAnimationTagHash == animationTagHash));
            if (triggerQAnim != null)
                return CheckAnimationEndCondition(triggerQAnim);
            else if (boolQAnim != null)
                return CheckAnimationEndCondition(boolQAnim);
            else if (floatQAnim != null)
                return CheckAnimationEndCondition(floatQAnim);
            else if (intQAnim != null)
                return CheckAnimationEndCondition(intQAnim);
            else
            {
#if UNITY_EDITOR
                Debug.Log("Possible Wrong use of animator state system, Make sure animate & finishcheck ending Tag-Name matches");
#endif
                return false;
            }
        }

        #region Animate

        // if it is not immediate it will be activated in action activate - use immediate in action update function if u need...
        public override void AnimateTrigger(AIBrain ai, string name, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
            QAnims<bool> x;
            if (triggerQAnims.Find(y => y.ParamName == name) == null)
            {
                x = new QAnims<bool>(name, false, false, isContinuous, nextAnimationName, nextAnimationTag,
                   layerIndexNames);
                triggerQAnims.Add(x);
            }
            else
                x = triggerQAnims.Find(y => y.ParamName == name);

            if (isImmediate)
            {
                ActivateTriggerAnimation(x);
                x.IsActivated = true;
                x.SkipNextUpdateCheck = true;
            }
        }

        public override void AnimateBool(AIBrain ai, string name, bool val = true, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
            QAnims<bool> x;
            if (boolQAnims.Find(y => y.ParamName == name) == null)
            {
                x = new QAnims<bool>(name, val, Animator.GetBool(name), isContinuous, nextAnimationName, nextAnimationTag,
                 layerIndexNames);
                boolQAnims.Add(x);
            }
            else
                x = boolQAnims.Find(y => y.ParamName == name);

            if (isImmediate)
            {
                ActivateBoolAnimation(x);
                x.IsActivated = true;
                x.SkipNextUpdateCheck = true;
            }
        }

        public override void AnimateFloat(AIBrain ai, string name, float val = 0, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
            QAnims<float> x;
            if (floatQAnims.Find(y => y.ParamName == name) == null)
            {
                x = new QAnims<float>(name, val, Animator.GetFloat(name), isContinuous, nextAnimationName, nextAnimationTag,
                layerIndexNames);
                floatQAnims.Add(x);
            }
            else
                x = floatQAnims.Find(y => y.ParamName == name);

            if (isImmediate)
            {
                ActivateFloatAnimation(x);
                x.IsActivated = true;
                x.SkipNextUpdateCheck = true;
            }
        }

        public override void AnimateInteger(AIBrain ai, string name, int val = 0, bool isContinuous = false, bool isImmediate = false, string nextAnimationName = "", string nextAnimationTag = "", int layerIndexNames = 0)
        {
            QAnims<int> x;
            if (intQAnims.Find(y => y.ParamName == name) == null)
            {
                x = new QAnims<int>(name, val, Animator.GetInteger(name), isContinuous, nextAnimationName, nextAnimationTag,
                 layerIndexNames);
                intQAnims.Add(x);
            }
            else
                x = intQAnims.Find(y => y.ParamName == name);

            if (isImmediate)
            {
                ActivateIntAnimation(x);
                x.IsActivated = true;
                x.SkipNextUpdateCheck = true;
            }
        }

        private void ActivateTriggerAnimation(QAnims<bool> triggerQAnim)
        {
            if (triggerQAnim.IsActivated)
                return;
            Animator.SetTrigger(triggerQAnim.ParamName);
        }

        private void ActivateBoolAnimation(QAnims<bool> boolQAnim)
        {
            if (boolQAnim.IsActivated)
                return;
            Animator.SetBool(boolQAnim.ParamName, boolQAnim.Value);
        }

        private void ActivateFloatAnimation(QAnims<float> floatQAnim)
        {
            if (floatQAnim.IsActivated)
                return;
            Animator.SetFloat(floatQAnim.ParamName, floatQAnim.Value);
        }

        private void ActivateIntAnimation(QAnims<int> intQAnim)
        {
            if (intQAnim.IsActivated)
                return;
            Animator.SetInteger(intQAnim.ParamName, intQAnim.Value);
        }

        #endregion Animate

        public override bool ComparePlayingAnimation(string name, int _lIndex)
        {
            return Animator.GetCurrentAnimatorStateInfo(_lIndex).IsName(name);
        }

        public override float GetCurrentAnimationTime(AIBrain ai, int _lIndex = 0)
        {
            return Animator.GetCurrentAnimatorStateInfo(_lIndex).normalizedTime;
        }

        public override void EnableLayer(AIBrain ai, int layerIndex, bool isContinuous, bool isImmediate)
        {
            SetLayerTargetWeight(ai, layerIndex, 1, isContinuous, isImmediate);
        }

        public override void DisableLayer(AIBrain ai, int layerIndex, bool isContinuous, bool isImmediate)
        {
            SetLayerTargetWeight(ai, layerIndex, 0, isContinuous, isImmediate);
        }

        public override void SetLayerTargetWeight(AIBrain ai, int layerIndex, float targetWeight, bool isContinuous, bool isImmediate)
        {
            QLayerED layer = qLayerEDs.Find(x => x.LayerIndex == layerIndex);
            layer.LastTarget = layer.LayerTarget;
            layer.LayerTarget = targetWeight;
            layer.switchToLastTargetAtActionExit = !isContinuous;
        }

        public override void OnActionActivate(AIBrain ai, ET.StateType stateType)
        {
            for (int i = 0; i < triggerQAnims.Count; i++)
                ActivateTriggerAnimation(triggerQAnims[i]);
            for (int i = 0; i < boolQAnims.Count; i++)
                ActivateBoolAnimation(boolQAnims[i]);
            for (int i = 0; i < floatQAnims.Count; i++)
                ActivateFloatAnimation(floatQAnims[i]);
            for (int i = 0; i < intQAnims.Count; i++)
                ActivateIntAnimation(intQAnims[i]);

            Animator.ResetTrigger("Interrupt");
        }

        public override void OnUpdate(AIBrain ai, ET.StateType stateType)
        {
            foreach (var layer in qLayerEDs)
            {
                Animator.SetLayerWeight(layer.LayerIndex,
                    Mathf.Lerp(Animator.GetLayerWeight(layer.LayerIndex), layer.LayerTarget, Time.deltaTime * animProps.layerWeightChangeSpeed));
            }
        }

        public override void OnActionExit(AIBrain ai, ET.StateType stateType)
        {
            for (int i = 0; i < triggerQAnims.Count; i++)
                if (!triggerQAnims[i].IsNextStateContinuous)
                    Animator.ResetTrigger(triggerQAnims[i].ParamName);
            triggerQAnims.Clear();
            for (int i = 0; i < boolQAnims.Count; i++)
                if (!boolQAnims[i].IsNextStateContinuous)
                    Animator.SetBool(boolQAnims[i].ParamName, boolQAnims[i].LastValue);
            boolQAnims.Clear();
            for (int i = 0; i < floatQAnims.Count; i++)
                if (!floatQAnims[i].IsNextStateContinuous)
                    Animator.SetFloat(floatQAnims[i].ParamName, floatQAnims[i].LastValue);
            floatQAnims.Clear();
            for (int i = 0; i < intQAnims.Count; i++)
                if (!intQAnims[i].IsNextStateContinuous)
                    Animator.SetInteger(intQAnims[i].ParamName, intQAnims[i].LastValue);
            intQAnims.Clear();

            foreach (var layer in qLayerEDs)
            {
                if (layer.switchToLastTargetAtActionExit)
                    layer.LayerTarget = layer.LastTarget;
            }
        }
    }

    [System.Serializable]
    public class ShooterAnimatorSystemProps
    {
        public float layerWeightChangeSpeed = 4f;
    }
}