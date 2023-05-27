using System.Collections.Concurrent;
using OnlineChatV2.WebApi.Hubs.EventManagement.Base;

namespace OnlineChatV2.WebApi.Hubs.EventManagement;

public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<WeakReference<IBaseEventReceiver>>> _receivers;
    private readonly ConcurrentDictionary<int, WeakReference<IBaseEventReceiver>> _receiversHashToRef;

    public EventBus(ConcurrentDictionary<int, WeakReference<IBaseEventReceiver>> receiversHashToRef)
    {
        _receiversHashToRef = receiversHashToRef;
        _receivers = new ConcurrentDictionary<Type, List<WeakReference<IBaseEventReceiver>>>();
    }

    public void Register<T>(IEventReceiver<T> receiver) where T : struct, IEvent
    {
        var eventType = typeof(T);
        if (!_receivers.ContainsKey(eventType))
            _receivers[eventType] = new List<WeakReference<IBaseEventReceiver>>();
        var reference = new WeakReference<IBaseEventReceiver>(receiver);
        
        _receivers[eventType].Add(reference);
        _receiversHashToRef[receiver.GetHashCode()] = reference;
    }

    public void Unregister<T>(IEventReceiver<T> receiver) where T : struct, IEvent
    {
        var eventType = typeof(T);
        var receiverHash = receiver.GetHashCode();
        if (!_receivers.ContainsKey(eventType) || !_receiversHashToRef.ContainsKey(receiverHash))
            return;

        var reference = _receiversHashToRef[receiverHash];
        _receivers[eventType].Remove(reference);
        _receiversHashToRef.Remove(receiverHash, out _);
    }

    public void Invoke<T>(T @event) where T : struct, IEvent
    {
        var eventType = typeof(T);
        if (!_receivers.ContainsKey(eventType))
            return;

        foreach (var receiverRef in _receivers[eventType])
        {
            if (receiverRef.TryGetTarget(out var receiver))
            {
                (receiver as IEventReceiver<T>)?.OnEvent(@event);
            }
        }
    }
}