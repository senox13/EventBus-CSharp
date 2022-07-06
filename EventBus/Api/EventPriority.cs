namespace EventBus.Api{
    /// <summary>
    /// Represents the priority with which to execute an event listener.
    /// This class has no public constructor and can only be referred to via
    /// its public static instances. Note that this class is an event listener
    /// of itself, and when invoked will change the priority stored in the
    /// event instance it is invoked upon.
    /// </summary>
    public enum EventPriority{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        HIGHEST = 0,
        HIGH = 1,
        NORMAL = 2,
        LOW = 3,
        LOWEST = 4
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
