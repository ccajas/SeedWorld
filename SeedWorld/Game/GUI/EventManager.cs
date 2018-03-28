using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeedWorld.GUI
{
    using EventType = UInt64;

    /// <summary>
    /// Base Event data class
    /// </summary>
    public interface IEventData
    {
	    EventType GetEventType();
    }

    public delegate void EventListenerDelegate(IEventData data);

    /// <summary>
    /// Global event manager class
    /// </summary>
    public static class EventManager
    {
        public static void AddListener(EventListenerDelegate eventDelegate, EventType type )
        {
            List<EventListenerDelegate> listenerDelegates = null;

            if (!eventListeners.TryGetValue(type, out listenerDelegates))
            {
                eventListeners.Add(type, new List<EventListenerDelegate>());
            }

	        List<EventListenerDelegate> eventListenerList = eventListeners[type];

	        //Check for dupes etc...

	        //Push the event onto the list for this type
	        eventListenerList.Add(eventDelegate);
        }

        public static void RemoveListener(EventType type)
        {
            eventListeners.Remove(type);
        }

        public static void TriggerEvent(IEventData eventData)
        {
	        // Get the list of all bound events for this type
            List<EventListenerDelegate> listenerDelegates = null;

            if (eventListeners.TryGetValue(eventData.GetEventType(), out listenerDelegates))
            {
                foreach (var item in listenerDelegates)
                {
                    //Call the delegate, passing in event data for each bound event
                    item(eventData);
                }
            }
        }

        // Mapping of event types with each list of listening delegates
        static Dictionary<EventType, List<EventListenerDelegate>> eventListeners = 
            new Dictionary<EventType, List<EventListenerDelegate>>();
    }

    //Speced class to store arbitrary event specific data
    public class EventData_ConsoleEvent : IEventData
    {
        /// Event ID for ConsoleEvent
	    public static readonly EventType sk_EventType = 0x2042012;

        /// Command string
	    public string ConsoleCommand;

        /// <summary>
        /// Receive console command
        /// </summary>
        /// <param name="command"></param>
	    public EventData_ConsoleEvent(string command)
	    {
		    ConsoleCommand = command;
	    }

	    public EventType GetEventType()
	    {
		    return sk_EventType;
	    }
    }
    
    /// <summary>
    /// Concrete EventManager implementation
    /// </summary>
    public class EntitySpawner 
    {
        public EntitySpawner()
        {
            RegisterEventDelegates();
        }

        public void RegisterEventDelegates()
        {
            EventManager.AddListener(ConsoleEventDelegate, EventData_ConsoleEvent.sk_EventType);
        }

	    //Delegate for when console command event recieved
	    void ConsoleEventDelegate(IEventData eventData)
	    {
		    string command = ((EventData_ConsoleEvent)eventData).ConsoleCommand;

            string[] words = command.Split(' ');
		    //Do stuff based off the command

            if (words[0] == "test")
            {

            }
	    }
    }

    /// <summary>
    /// Console interface for inputting event commands
    /// </summary>
    public class ConsoleManager
    {
	    public void Command(string command)
	    {
		    var newevent = new EventData_ConsoleEvent(command);

		    //This kicks off the event
		    EventManager.TriggerEvent(newevent);
	    }
    }
}
