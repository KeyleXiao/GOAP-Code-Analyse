using Messages;
using System.Collections.Generic;
using UnityEngine;

public class AIMemoryShooter : AIMemory
{
    private Shooter.AIMessageHandlerShooter messageHandlers;

    public AIMemoryShooter(GameObject gameObject, int maxItemCount, float updateListenedFriendsInterval, float listenRadius, LayerMask listenMessagesFrom)
        : base(gameObject, maxItemCount, updateListenedFriendsInterval, listenRadius, listenMessagesFrom)
    {
        Friends = new List<AIMemory>();
        onMessageSendToSelf += OnMessageReceived; // You can send messages to self if sender is not important
        messageHandlers = new Shooter.AIMessageHandlerShooter(this);
    }

    public override void UpdateListenedFriends()
    {
        Collider[] cols = Physics.OverlapSphere(GameObject.transform.position, ListenMessageRadius, FriendMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject == GameObject) continue;
            SharedProps sp = col.GetComponent<SharedProps>();
            if (sp && sp.memory != null)
            {
                if (Vector3.Distance(GameObject.transform.position, sp.transform.position) < ListenMessageRadius && sp.GetComponent<Health>() && sp.GetComponent<Health>().health > 0)
                {
                    // Listen
                    sp.memory.onMessageSendToListeners -= OnMessageReceived;
                    sp.memory.onMessageSendToListeners += OnMessageReceived;
                    if (!Friends.Contains(sp.memory))
                        Friends.Add(sp.memory);
                }
                else
                {
                    // Don't Listen
                    if (Friends.Contains(sp.memory))
                    {
                        Friends.Find(x => x == sp.memory).onMessageSendToListeners -= OnMessageReceived;
                        Friends.Remove(sp.memory);
                    }
                    sp.memory.onMessageSendToListeners -= OnMessageReceived;
                }
            }
        }
    }

    private void OnMessageReceived(object sender, InformationEventArgs eArgs)
    {
        AIMemory memSender = (AIMemory)sender;

        foreach (var message in eArgs.Messages)
            message.DispatchTo(messageHandlers, memSender);
    }

    public override void BroadcastToListeners(/*GameObject sender, */List<AIMessage> infoList)
    {
        OnMessageSendToListeners(new InformationEventArgs(infoList));
    }

    public override void BroadcastToListeners(/*GameObject sender,*/ AIMessage info)
    {
        OnMessageSendToListeners(new InformationEventArgs(info));
    }

    protected override void OnMessageSendToListeners(InformationEventArgs eArgs)
    {
        // Shooter specific(before send)...

        // Call actual sender
        base.OnMessageSendToListeners(eArgs);
    }

    public override void BroadcastToSelf(AIMessage information)
    {
        OnMessageSendToSelf(new InformationEventArgs(information));
    }

    protected override void OnMessageSendToSelf(InformationEventArgs eArgs)
    {
        // Shooter specific(before send)...

        // Call actual sender
        base.OnMessageSendToListeners(eArgs);
    }
}