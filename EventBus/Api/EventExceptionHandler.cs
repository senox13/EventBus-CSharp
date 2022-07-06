using System;


namespace EventBus.Api{
    /// <summary>
    /// Represents a method for handling an exception thrown by an
    /// <see cref="EventBus"/> while posting an <see cref="Event"/> to an
    /// <see cref="IEventListener"/>. After this function returns, the
    /// original exception will be propagated upwards.
    /// </summary>
    /// <param name="bus">The <see cref="EventBus"/> that posted this event</param>
    /// <param name="ev">The <see cref="Event"/> that was being handled when
    /// the exception was thrown</param>
    /// <param name="listeners">All of the <see cref="IEventListener"/>
    /// callbacks for this event type of this <see cref="EventBus"/></param>
    /// <param name="index">The index of the <see cref="IEventListener"/> that
    /// was being invoked when the exception was thrown</param>
    /// <param name="ex">The thrown <see cref="Exception"/></param>
    public delegate void EventExceptionHandler(IEventBus bus, Event ev, IEventListener[] listeners, int index, Exception ex);
}
