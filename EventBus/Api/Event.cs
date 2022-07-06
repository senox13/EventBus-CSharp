using System;

namespace EventBus.Api{
    /// <summary>
    /// Base Event class that all other events are derived from
    /// </summary>
    public class Event{
        /*
         * Instance fields
         */
        private bool isCanceled = false;
        private Result result = Result.DEFAULT;
        private int phase = -1;


        /*
         * Constructor
         */
        /// <summary>
        /// Constructs a new <see cref="Event"/> instance.
        /// </summary>
        public Event(){}


        /*
         * Public methods
         */
        /// <summary>
        /// Determines if this function is cancelable, as indicated by the
        /// attribute <see cref="CancelableAttribute"/>.
        /// </summary>
        /// <returns><c>true</c> if this event can be canceled, otherwise
        /// <c>false</c></returns>
        public bool IsCancelable(){
            return EventListenerHelper.IsCancelable(GetType());
        }

        /// <summary>
        /// Determine if this event is canceled and should stop executing.
        /// </summary>
        /// <returns>The current canceled state</returns>
        public bool IsCanceled(){
            return isCanceled;
        }

        /// <summary>
        /// Sets the cancel state of this event. Note that not all events are
        /// cancelable, and any attempt to invoke this method on an event that
        /// is not cancelable (as determined by <see cref="IsCancelable()"/>
        /// will result in an <see cref="InvalidOperationException"/>. The
        /// functionality of setting the canceled state is defined on a
        /// per-event basis.
        /// </summary>
        /// <param name="cancel">The new cancellation value</param>
        public void SetCanceled(bool cancel){
            if(!IsCancelable()){
                throw new NotSupportedException($"Attempted to call Event.SetCanceled() on a non-cancelable event of type: {GetType().FullName}");
            }
            isCanceled = cancel;
        }

        /// <summary>
        /// Gets whether this event expects a significant result value, as
        /// indciated by the attribute <see cref="HasResultAttribute"/>.
        /// </summary>
        /// <returns><c>true</c> if this event expects a result, otherise
        /// <c>false</c></returns>
        /// <seealso cref="Result"/>
        public bool HasResult(){
            return EventListenerHelper.HasResult(GetType());
        }

        /// <summary>
        /// Returns the <see cref="Result"/> value set as the result of this event
        /// </summary>
        /// <returns>The result value of this event</returns>
        public Result GetResult(){
            return result;
        }

        /// <summary>
        /// Sets the result value for this event, not all events can have a
        /// result set, and any attempt to set a result for a event that isn't
        /// expecting it will result in a <see cref="ArgumentException"/>. The
        /// functionality of setting the result is defined on a per-event basis.
        /// </summary>
        /// <param name="value">The new result</param>
        public void SetResult(Result value){
            if(!HasResult())
                throw new ArgumentException($"Attempted to set result on event type which does not support results: {GetType()}");
            result = value;
        }

        /// <summary>
        /// Returns the priority of the <see cref="IEventListener"/> that this
        /// event is about to invoke/currently invoking. (Phase is updated
        /// immediately before listeners of that priority are invoked.)
        /// </summary>
        /// <returns>The current priority</returns>
        public EventPriority GetPhase(){
            return (EventPriority)phase;
        }

        /// <summary>
        /// Sets a new phase for this event. An event's phase is the
        /// <see cref="EventPriority"/> of the <see cref="IEventListener"/>
        /// that was last invoked. The new phase must be a lower or equal
        /// priority to the current phase.
        /// </summary>
        /// <param name="value">The new <see cref="EventPriority"/> for this event</param>
        public void SetPhase(EventPriority value){
            int prev = phase;
            int newValue = (int)value;
            if(prev >= newValue)
                throw new ArgumentException($"Attempted to set event phase to {value} when already {phase}", nameof(value));
            phase = newValue;
        }


        /*
         * Virtual methods
         */
        /// <summary>
        /// Returns a ListenerList object that contains all listeners that are
        /// registered to this event. If the default implementation is too slow
        /// for a given event subclass, that class can add a static constructor
        /// which calls <see cref="EventListenerHelper.GetListenerListInternal"/>
        /// a single time and caches the result in a private static readonly
        /// field.
        /// </summary>
        /// <returns>The <see cref="ListenerList"/> instance corresponding to
        /// this event</returns>
        public virtual ListenerList GetListenerList(){
            return EventListenerHelper.GetListenerListInternal(GetType(), true);
        }


        /*
         * Nested types
         */
        /// <summary>
        /// Represents the result status of an event. See <see cref="HasResult"/>
        /// for more details. The usage of this enum is largely up to the
        /// implementation of individual <see cref="Event"/> subclasses.
        /// </summary>
        public enum Result{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            DENY,
            DEFAULT,
            ALLOW
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// Attribute which can be added to an <see cref="Event"/> subclass to
        /// indicate that it has a result.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
        public sealed class HasResultAttribute : Attribute{}
    }
}
