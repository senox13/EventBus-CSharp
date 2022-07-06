using System;


namespace EventBus.Api{
    /// <summary>
    /// Defines a manager for registering and posting events.
    /// </summary>
    public interface IEventBus{
        /// <summary>
        /// Register an instance object or a Class, and add listeners for all
        /// methods with a <see cref="SubscribeEventAttribute"/> that are found.
        /// Depending on what is passed as an argument, different listener
        /// creation behaviour is performed. If a <see cref="Type"/> is passed
        /// in, it will be scanned for static methods with the subscribe
        /// attribute. If an instance is passed in, it will be scanned for
        /// non-static methods with the subscribe attribute. All methods found
        /// are registered as event handlers.
        /// </summary>
        /// <param name="target">Either a <see cref="Type"/> instance or an
        /// arbitrary object, for scanning and event listener creation</param>
        void Register(object target);

        /// <summary>
        /// Add an <see cref="Action{T}"/> as a listener with the default
        /// priority of <see cref="EventPriority.NORMAL"/>, which will not
        /// recieve cancelled events.
        /// </summary>
        /// <typeparam name="T">The <see cref="Event"/> subclass to listen for</typeparam>
        /// <param name="callback">Callback to invoke when a matching event is received</param>
        void AddListener<T>(Action<T> callback) where T : Event;

        /// <summary>
        /// Add an <see cref="Action{T}"/> as a listener with the specified
        /// <see cref="EventPriority"/>, which will not recieve cancelled
        /// events.
        /// </summary>
        /// <typeparam name="T">The <see cref="Event"/> subclass to listen for</typeparam>
        /// <param name="priority">The <see cref="EventPriority"/> to use for this listener</param>
        /// <param name="callback">Callback to invoke when a matching event is received</param>
        void AddListener<T>(EventPriority priority, Action<T> callback) where T : Event;
        
        /// <summary>
        /// Add an <see cref="Action{T}"/> as a listener with the specified
        /// <see cref="EventPriority"/> and potentially cancelable events.
        /// </summary>
        /// <typeparam name="T">The <see cref="Event"/> subclass to listen for</typeparam>
        /// <param name="priority">The <see cref="EventPriority"/> for this listener</param>
        /// <param name="receiveCancelled">Indicate if this listener should receive events that have been cancelled</param>
        /// <param name="callback">Callback to invoke when a matching event is received</param>
        void AddListener<T>(EventPriority priority, bool receiveCancelled, Action<T> callback) where T : Event;

        /// <summary>
        /// Add an <see cref="Action{T}"/> as a listener for a
        /// <see cref="GenericEvent{F}"/> subclass, filtered to only be called
        /// for the specified filter <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="GenericEvent{F}"/> subclass to listen for</typeparam>
        /// <typeparam name="F">The <see cref="Type"/> to filter the <see cref="GenericEvent{F}"/> for</typeparam>
        /// <param name="callback">Callback to invoke when a matching event is received</param>
        void AddGenericListener<T, F>(Action<T> callback) where T : GenericEvent<F>;

        /// <summary>
        /// Add an <see cref="Action{T}"/> as a listener with the specified
        /// <see cref="EventPriority"/>, which will not recieve cancelled
        /// events, for a <see cref="GenericEvent{T}"/> subclass, filtered to only
        /// be called for the specified filter <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="GenericEvent{F}"/> subclass to listen for</typeparam>
        /// <typeparam name="F">The <see cref="Type"/> to filter the <see cref="GenericEvent{F}"/> for</typeparam>
        /// <param name="priority">The <see cref="EventPriority"/> to use for this listener</param>
        /// <param name="callback">Callback to invoke when a matching event is received</param>
        void AddGenericListener<T, F>(EventPriority priority, Action<T> callback) where T : GenericEvent<F>;

        /// <summary>
        /// Add an <see cref="Action{T}"/> as a listener with the specified
        /// <see cref="EventPriority"/> and potentially cancelled events, for a
        /// <see cref="GenericEvent{F}"/> subclass, filtered to only be called
        /// for the specified filter <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="GenericEvent{F}"/> subclass to listen for</typeparam>
        /// <typeparam name="F">The <see cref="Type"/> to filter the <see cref="GenericEvent{F}"/> for</typeparam>
        /// <param name="priority">The <see cref="EventPriority"/> to use for this listener</param>
        /// <param name="receiveCancelled">Indicate if this listener should receive events that have been cancelled</param>
        /// <param name="callback">Callback to invoke when a matching event is received</param>
        void AddGenericListener<T, F>(EventPriority priority, bool receiveCancelled, Action<T> callback) where T : GenericEvent<F>;

        /// <summary>
        /// Unregister the supplied listener from this EventBus. Removes all
        /// listeners from events. NOTE: <see cref="Action{T}"/> callbacks can
        /// be stored in a variable if unregistration is required for the
        /// callback.
        /// </summary>
        /// <param name="obj">The object, <see cref="Type"/> or
        /// <see cref="Action{T}"/> to unsubscribe</param>
        void Unregister(object obj);

        /// <summary>
        /// Submits an event for dispatch to the appropriate listeners.
        /// </summary>
        /// <param name="ev">The event to dispatch to listeners</param>
        /// <returns><c>true</c> if the event was cancelled, otherwise <c>false</c></returns>
        /// <seealso cref="Post(Event, EventBusInvokeDispatcher)"/>
        bool Post(Event ev);

        /// <summary>
        /// Submits an event for dispatch to the appropriate listeners using
        /// the provided <see cref="EventBusInvokeDispatcher"/>. This allows
        /// for logic to be added before/after event posting.
        /// </summary>
        /// <param name="ev">The event to dispatch to listeners</param>
        /// <param name="wrapper">The method to delegate listener invocation to</param>
        /// <returns><c>true</c> if the event was cancelled, otherwise <c>false</c></returns>
        /// <seealso cref="Post(Event)"/>
        bool Post(Event ev, EventBusInvokeDispatcher wrapper);

        /// <summary>
        /// Shuts down this event bus. No future events will be fired on this
        /// event bus, so any call to <see cref="Post(Event)"/> will be a no op
        /// after this method has been invoked. This operation can be reversed
        /// using <see cref="Start"/>.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Starts this event bus. This is the inverse of <see cref="Shutdown"/>.
        /// </summary>
        void Start();
    }
}
