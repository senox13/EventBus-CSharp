using System;


namespace EventBus.Api{
    /// <summary>
    /// Represents an error related to an <see cref="EventBus"/>'s
    /// <see cref="IEventListener"/> callbacks.
    /// </summary>
    public class EventListenerException : Exception{
        /// <summary>
        /// Constructs a new EventListenerException.
        /// </summary>
        public EventListenerException(){}

        /// <summary>
        /// Constructs a new EventListenerException with the given message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for
        /// the exception</param>
        public EventListenerException(string message) : base(message){}

        /// <summary>
        /// Constructs a new EventListenerException with the given message and a
        /// reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="inner">The exception that is the cause of the current exception</param>
        public EventListenerException(string message, Exception inner) : base(message, inner){}
    }
}
