using System;
using System.Collections.Generic;
using System.Reflection;


namespace EventBus.Api{
    /// <summary>
    /// Provides the mapping of <see cref="Type"/>s to <see cref="ListenerList"/>
    /// instances containing the listeners for that type.
    /// </summary>
    public static class EventListenerHelper{
        /*
         * Fields
         */
        private static readonly LockHelper<Type, ListenerList> listeners = new LockHelper<Type, ListenerList>(new Dictionary<Type, ListenerList>());
        private static readonly ListenerList EVENTS_LIST = new ListenerList();
        private static readonly LockHelper<Type, bool> cancelable = new LockHelper<Type, bool>(new Dictionary<Type, bool>());
        private static readonly LockHelper<Type, bool> hasResult = new LockHelper<Type, bool>(new Dictionary<Type, bool>());


        /*
         * Public methods
         */
        /// <summary>
        /// Returns a <see cref="ListenerList"/> object that contains all
        /// listeners that are registered to this event class. This supports
        /// abstract classes that cannot be instantiated.
        /// </summary>
        /// <remarks>
        /// This is much slower than the instance method
        /// <see cref="Event.GetListenerList()"/>. For performance when
        /// emitting events, always call that method instead.
        /// </remarks>
        /// <param name="eventType">The type of <see cref="Event"/> to get the
        /// listeners for</param>
        /// <returns>A ListenerList for the given event type</returns>
        public static ListenerList GetListenerList(Type eventType){
            return GetListenerListInternal(eventType, false);
        }


        /*
         * Private/internal methods
         */
        internal static ListenerList GetListenerListInternal(Type eventClass, bool fromInstanceCall){
            if(eventClass == typeof(Event)) return EVENTS_LIST;
            return listeners.Get(eventClass, () => ComputeListenerList(eventClass, fromInstanceCall), list => list);
        }

        private static ListenerList ComputeListenerList(Type eventClass, bool fromInstanceCall){
            if(eventClass == typeof(Event)){
                return new ListenerList();
            }
            if(fromInstanceCall || eventClass.IsAbstract){
                Type superClass = eventClass.BaseType;
                ListenerList parentList = GetListenerList(superClass);
                return new ListenerList(parentList);
            }
            try{
                Event evt = (Event)Activator.CreateInstance(eventClass);
                return evt.GetListenerList();
            }catch(Exception e) when(e is MissingMethodException || e is MemberAccessException || e is TargetInvocationException){
                throw new EventListenerException($"Error computing listener list for {eventClass.Name}", e);
            }
        }

        internal static bool IsCancelable(Type eventClass){
            return HasAnnotation(eventClass, typeof(CancelableAttribute), cancelable);
        }

        internal static bool HasResult(Type eventClass){
            return HasAnnotation(eventClass, typeof(Event.HasResultAttribute), hasResult);
        }

        private static bool HasAnnotation(Type eventClass, Type annotation, LockHelper<Type, bool> lockHelper){
            if(eventClass == typeof(Event))
                return false;
            return lockHelper.Get(eventClass, () => {
                return Attribute.GetCustomAttribute(eventClass, annotation) != null;
            }, f => f);
        }
    }
}
