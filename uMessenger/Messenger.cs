using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace uMessenger
{
    public class Messenger : MonoBehaviour
    {
        // Thread lock.
        private static object m_MessengerLock = new object();

        // Messenger event dictionary.
        private Dictionary<string, MessengerEvent> m_EventDictionary = new Dictionary<string, MessengerEvent>();

        // Singleton messenger instance.
        private static Messenger m_Instance;
        public static Messenger Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (m_MessengerLock)
                    {
                        // Retrieve the instance(s) from the scene.
                        Messenger[] instances = FindObjectsOfType(typeof(Messenger)) as Messenger[];
                        if (instances.Length == 0) return null;

                        // More than one instance?
                        if (instances.Length > 1)
                        {
                            Debug.LogWarning($"uMessenger: Only one uMessenger instance can exist in the scene. Null will be returned.");
                            return null;
                        }

                        m_Instance = instances[0];
                    }
                }

                return m_Instance;
            }
        }

        public void Accept(string eventName, UnityAction<object[]> eventListener)
        {
            lock (m_MessengerLock)
            {
                if (m_EventDictionary.TryGetValue(eventName, out MessengerEvent thisEvent))
                    thisEvent.AddListener(eventListener);
                else
                {
                    thisEvent = new MessengerEvent();
                    thisEvent.AddListener(eventListener);
                    m_EventDictionary.Add(eventName, thisEvent);
                }
            }
        }

        public void Ignore(string eventName, UnityAction<object[]> eventListener)
        {
            lock (m_MessengerLock)
            {
                if (m_EventDictionary.TryGetValue(eventName, out MessengerEvent thisEvent))
                    thisEvent.RemoveListener(eventListener);
            }
        }

        public void Send(string eventName, object[] args = null)
        {
            lock (m_MessengerLock)
            {
                if (m_EventDictionary.TryGetValue(eventName, out MessengerEvent thisEvent))
                    thisEvent.Invoke(args);
            }
        }
    }
}