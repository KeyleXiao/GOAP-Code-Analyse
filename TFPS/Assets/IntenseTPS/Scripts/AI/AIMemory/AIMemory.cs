using Information;
using Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to send messages to friends or to agent itself
/// </summary>
public class InformationEventArgs : EventArgs
{
    protected List<AIMessage> messages;

    public ReadOnlyCollection<AIMessage> Messages
    {
        get { return messages.AsReadOnly(); }
    }

    public InformationEventArgs(List<AIMessage> _messages)
    {
        messages = _messages;
    }

    public InformationEventArgs(AIMessage _message)
    {
        messages = new List<AIMessage>();
        messages.Add(_message);
    }
}

public abstract class AIMemory
{
    /// <summary>
    /// Used to limit max <see cref="InformationP"/> count in memory
    /// </summary>
    public int MaxItemCount { get; private set; }

    /// <summary>
    /// The <see cref="GameObject"/> that this memory is attached to
    /// </summary>
    public GameObject GameObject { get; private set; }

    /// <summary>
    /// Used to determine which <see cref="AIMemory"/>'s are friends to listen to
    /// </summary>
    public LayerMask FriendMask { get; set; }

    /// <summary>
    /// Check time interval of friends (listened sources)
    /// </summary>
    public float UpdateListenedFriendsInterval { get; set; } // to listen them

    /// <summary>
    /// Only listen friends within this radius
    /// </summary>
    public float ListenMessageRadius { get; set; }

    /// <summary>
    /// Memory informations
    /// </summary>
    protected List<InformationP> items;

    /// <summary>
    /// Used to send messages
    /// </summary>
    public ReadOnlyCollection<InformationP> Items
    {
        get { return items.AsReadOnly(); }
    }

    private int _curItemCount = 0;

    private float _lastListenedFriendsUpdatedAt = -Mathf.Infinity;

    #region Events

    /// <summary>
    /// Message sending to friends
    /// </summary>
    public virtual event EventHandler<InformationEventArgs> onMessageSendToListeners;

    /// <summary>
    /// Message sending to self
    /// </summary>
    public virtual event EventHandler<InformationEventArgs> onMessageSendToSelf;

    #endregion Events

    /// <summary>
    /// <see cref="InformationP"/>'s (Messages) will be listened from these <see cref="AIMemory"/>'s
    /// </summary>
    public List<AIMemory> Friends { get; protected set; }

    public bool isFriend(Transform transform)
    {
        foreach (AIMemory mem in Friends)
            if (mem.GameObject && mem.GameObject == transform.gameObject)
                return true;
        return false;
    }

    public AIMemory(GameObject gameObject, int maxItemCount, float updateListenedFriendsInterval, float listenRadius, LayerMask listenMessagesFrom)
    {
        items = new List<InformationP>();
        GameObject = gameObject;
        MaxItemCount = maxItemCount;
        ListenMessageRadius = listenRadius;
        FriendMask = listenMessagesFrom;
        UpdateListenedFriendsInterval = updateListenedFriendsInterval;
    }

    /// <summary>
    /// Sending multiple messages to listeners at once
    /// </summary>
    /// <param name="messages"></param>
    public abstract void BroadcastToListeners(/*GameObject sender,*/ List<AIMessage> messages);

    /// <summary>
    /// Sending single message to listeners
    /// </summary>
    /// <param name="message"></param>
    public abstract void BroadcastToListeners(/*GameObject sender,*/ AIMessage message);

    /// <summary>
    /// You can override to specialize messages before sending
    /// </summary>
    /// <param name="eArgs"></param>
    protected virtual void OnMessageSendToListeners(InformationEventArgs eArgs)
    {
        EventHandler<InformationEventArgs> handler = onMessageSendToListeners;
        if (handler != null)
            handler(this, eArgs);
    }

    /// <summary>
    /// Sending single message to self
    /// </summary>
    /// <param name="information"></param>
    public abstract void BroadcastToSelf(AIMessage information);

    /// <summary>
    /// You can override to specialize messages before sending
    /// </summary>
    /// <param name="information"></param>
    protected virtual void OnMessageSendToSelf(InformationEventArgs eArgs)
    {
        EventHandler<InformationEventArgs> handler = onMessageSendToSelf;
        if (handler != null)
            handler(this, eArgs);
    }

    public abstract void UpdateListenedFriends();

    public virtual void Update()
    {
        if (Time.time - _lastListenedFriendsUpdatedAt > UpdateListenedFriendsInterval)
        {
            UpdateListenedFriends();
            _lastListenedFriendsUpdatedAt = Time.time;
        }
    }

    /// <summary>
    /// Returns informations of type <see cref="InformationAlive"/> or of derived types from items with the <see cref="Transform"/>
    /// </summary>
    public virtual T GetAliveWithTransform<T>(Transform trnsfrm) where T : InformationAlive
    {
        foreach (InformationAlive info in Items.OfType<T>())
        {
            if (info.transform != null && info.transform == trnsfrm)
            {
                return info as T;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns informations of type <see cref="InformationDanger"/> or of derived types from items with the <see cref="Transform"/>
    /// </summary>
    public virtual T GetDangerWithTransform<T>(Transform trnsfrm) where T : InformationDanger
    {
        if (trnsfrm == null)
            return null;
        foreach (InformationDanger info in Items.OfType<T>())
        {
            if (info.dangerTransform != null && info.dangerTransform == trnsfrm)
            {
                return info as T;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns informations of type <see cref="InformationSuspicion"/> or of derived types from items with the <see cref="Transform"/>
    /// </summary>
    public virtual T GetSuspicionWithBaseTransform<T>(Transform trnsfrm) where T : InformationSuspicion
    {
        if (trnsfrm == null)
            return null;
        foreach (InformationSuspicion info in Items.OfType<T>())
        {
            if (info.BaseTransform != null && info.BaseTransform == trnsfrm)
            {
                return info as T;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns first information of type from <see cref="Items"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T GetFirstItemOfTypeFromMemory<T>() where T : InformationP
    {
        foreach (InformationP info in Items.OfType<T>())
        {
            return info as T;
        }
        return null;
    }

    /// <summary>
    /// Returns highest confidence of type information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T GetHighestOverall<T>() where T : InformationP
    {
        float highestOverall = -Mathf.Infinity;
        T retVal = null;
        foreach (InformationP info in Items.OfType<T>())
        {
            if (info.OverallConfidence > highestOverall)
            {
                highestOverall = info.OverallConfidence;
                retVal = info as T;
            }
        }
        return retVal;
    }

    /// <summary>
    /// Add an <see cref="InformationP"/> to memory
    /// </summary>
    /// <param name="info"></param>
    public virtual void Add(InformationP info)
    {
        _curItemCount++;
        if (_curItemCount > MaxItemCount)
        {
#if UNITY_EDITOR
            //Debug.Log("Reached Max Information Count in Memory, new Informations will be added after releasing old items, you can increase max item count from AIMemory script");
#endif
            return;
        }
        items.Add(info);
    }

    /// <summary>
    /// Add and <see cref="InformationP"/> to memory and get the reference of it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <returns></returns>
    public virtual T AddNReturn<T>(T info) where T : InformationP
    {
        Add(info);
        return info as T;
    }

    /// <summary>
    /// Add multiple <see cref="InformationP"/>'s to memory at once
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="infos"></param>
    public virtual void Add<T>(List<T> infos) where T : InformationP
    {
        foreach (T info in infos)
            Add(info);
    }

    /// <summary>
    /// Remove multiple <see cref="InformationP"/>'s from memory at once
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="infos"></param>
    public virtual void Remove<T>(List<T> infos) where T : InformationP
    {
        if (infos == null)
            return;
        foreach (T info in infos)
            Remove(info);
    }

    /// <summary>
    /// Remove single <see cref="InformationP"/> from memory
    /// </summary>
    /// <param name="info"></param>
    public virtual void Remove(InformationP info)
    {
        if (Items.Contains(info))
        {
            items.Remove(info);

            _curItemCount--;
        }
    }

    /// <summary>
    /// See if memory has an <see cref="InformationP"/> of type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual bool MemoryContainsObjectOfType(Type type)
    {
        foreach (InformationP infoP in Items)
            if (infoP.GetType() == type)
                return true;
        return false;
    }
}