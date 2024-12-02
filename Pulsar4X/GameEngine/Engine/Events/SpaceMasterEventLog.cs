using Newtonsoft.Json;
using Pulsar4X.DataStructures;

namespace Pulsar4X.Events;

public class SpaceMasterEventLog : IEventLog
{
    [JsonProperty]
    private SafeList<Event> _events = new ();

    private SpaceMasterEventLog() { }

    public static SpaceMasterEventLog Create()
    {
        return new SpaceMasterEventLog();
    }

    public SafeList<Event> GetEvents() => _events;

    public void Subscribe()
    {
        var allEvents = EventTypeHelper.GetAllEventTypes();
        EventManager.Instance.Subscribe(allEvents, OnEvent);
    }

    public void Unsubscribe()
    {
        var allEvents = EventTypeHelper.GetAllEventTypes();
        EventManager.Instance.Unsubscribe(allEvents, OnEvent);
    }

    public void ToggleHaltsOn(EventType eventType)
    {
        // Do nothing, the SM log shouldn't ever halt the game
    }

    public bool HaltsOn(EventType eventType)
    {
        return false;
    }

    private void OnEvent(Event e)
    {
        _events.Add(e);
    }
}