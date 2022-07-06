namespace EventBus.Api{
    /// <summary>
    /// Defines a type which handles <see cref="Event"/>s. This should never be
    /// implemented yourself and is instead implemented using dynamic types as
    /// a wrapper for individual methods that handle events of more specific
    /// types. For more information, see <see cref="ASMEventHandler"/>.
    /// </summary>
    public interface IEventListener{
        /// <summary>
        /// Handles the given event as defined by the implementing type.
        /// </summary>
        /// <param name="e">The <see cref="Event"/> to handle</param>
        void Invoke(Event e);
    }
}
