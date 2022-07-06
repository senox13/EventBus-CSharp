using System;


namespace EventBus.Api{
    /// <summary>
    /// Marks a method as being an event listener. Static methods with this
    /// attribute will be registered when their declaring type is passed into
    /// <see cref="EventBus.Register(object)"/>. Instance methods with this
    /// attribute will be registered when an instance of their declaring type
    /// is passed into <see cref="EventBus.Register(object)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscribeEventAttribute : Attribute{
        /// <summary>
        /// The priority with which to register this event listener. Defaults
        /// to <see cref="EventPriority.NORMAL"/>.
        /// </summary>
        public EventPriority Priority{get; set;} = EventPriority.NORMAL;
        /// <summary>
        /// Whether the event handler that this attribute is on should receive
        /// canceled events. Defaults to false.
        /// </summary>
        public bool ReceiveCanceled{get; set;} = false;

        /// <summary>
        /// Constructs a new <see cref="SubscribeEventAttribute"/>. This method
        /// should not be called directly as this class is meant to be used as
        /// an attribute on event handler methods.
        /// </summary>
        public SubscribeEventAttribute(){}
    }
}
