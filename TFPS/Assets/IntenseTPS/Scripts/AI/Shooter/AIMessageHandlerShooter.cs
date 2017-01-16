using Information;
using Messages;
using System.Linq;

namespace Shooter
{
    public class AIMessageHandlerShooter :
        IMessageHandler<AIMessageSuspicionFound>,
        IMessageHandler<AIMessageTargetDead>,
        IMessageHandler<AIMessageSuspicionPosition>,
        IMessageHandler<AIMessageSuspicionLost>

    {
        private AIMemory memory;

        public AIMessageHandlerShooter(AIMemory _aiMemory)
        {
            memory = _aiMemory;
        }

        public void ProcessMessage(AIMessageTargetDead message, AIMemory memSender)
        {
            var memInfo = memory.GetSuspicionWithBaseTransform<InformationAlive>(message.deadTarget);
            var relatedInfos = memory.Items.OfType<InformationSuspicion>().Where(x => x != memInfo).ToList();
            memory.Remove(relatedInfos);
            if (memInfo != null)
                memInfo.IsDead = true;
        }

        public void ProcessMessage(AIMessageSuspicionFound message, AIMemory memSender)
        {
            var memInfo = memory.GetSuspicionWithBaseTransform<InformationAlive>(message.Info.BaseTransform);
            if (memInfo != null)
            {
                if (memInfo.UpdateTime < message.Info.UpdateTime)
                    memInfo.Update(message.Info.lastKnownPosition.Value, memInfo.lastKnownPosition.Confidence);
            }
            else
            {
                var n = new InformationSuspicion(message.Info);
                //n.SuspicionFirm = 1;
                //n.IsSure = true;
                memory.Add(n);
            }
        }

        public void ProcessMessage(AIMessageSuspicionLost message, AIMemory memSender)
        {
            var susps = memory.Items.OfType<InformationSuspicion>().Where(x => x.BaseTransform != null && x.BaseTransform == message.lostTarget).ToList().OrderByDescending(x => x.UpdateTime).ToList();
            if (susps.Count > 0)
                memory.BroadcastToListeners(new AIMessageSuspicionPosition(message.lostTarget, susps[0].lastKnownPosition.Value, susps[0].UpdateTime));
        }

        public void ProcessMessage(AIMessageSuspicionPosition message, AIMemory memSender)
        {
            var susps = memory.Items.OfType<InformationSuspicion>().Where(x => x.BaseTransform != null && x.BaseTransform == message.FoundTransform).ToList();
            foreach (var susp in susps)
                if (message.FoundTime > susp.UpdateTime)
                    susp.Update(message.FoundPos, susp.lastKnownPosition.Confidence);
        }
    }
}