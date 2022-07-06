namespace EventBus.Api{
    /// <summary>
    /// Defines a method to which <see cref="IEventListener"/> invocation can
    /// be delegated. This allows for logic to be added before/after event
    /// posting.
    /// </summary>
    /// <param name="listener">The event listener that the given event is
    /// being dispatched to</param>
    /// <param name="ev">The event be3ing dispatched</param>
    public delegate void EventBusInvokeDispatcher(IEventListener listener, Event ev);
}
