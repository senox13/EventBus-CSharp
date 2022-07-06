using EventBus.Api;


namespace EventBus{
    /// <summary>
    /// Provides a wrapper for <see cref="EventPriority"/> instances which
    /// implements <see cref="IEventListener"/>. When invoked, updates the
    /// current phase of the <see cref="Event"/> it is invoked on.
    /// </summary>
    public class EventPriorityListener : IEventListener{
        private readonly EventPriority priority;


        /// <summary>
        /// Constructs a new <see cref="EventPriorityListener"/> with the given
        /// priority. This class is for internal use and should never be
        /// instantiated by the end user.
        /// </summary>
        /// <param name="priorityIn"></param>
        internal EventPriorityListener(EventPriority priorityIn){
            priority = priorityIn;
        }
        

        /// <summary>
        /// Gets the priority this <see cref="EventPriorityListener"/> was
        /// constructed with. This method has no equivalent setter as the
        /// priority is read-only.
        /// </summary>
        /// <returns></returns>
        public EventPriority GetPriority(){
            return priority;
        }
        
        /// <summary>
        /// Updates the current phase of the given <see cref="Event"/>.
        /// </summary>
        /// <param name="e">The event to update to this instance's priority</param>
        public void Invoke(Event e){
            e.SetPhase(priority);
        }
    }
}
