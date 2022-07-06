using System;

//TODO: Test event filtering by generic type against the java version
namespace EventBus.Api{
    /// <summary>
    /// Provides a generic event - one that is able to be filtered based on
    /// the supplied Generic type. This is the non-generic version of this
    /// interface. In almost all cases, <see cref="IGenericEvent{T}"/> should
    /// be used instead.
    /// </summary>
    public interface IGenericEvent{
        /// <summary>
        /// Gets the generic type of this event
        /// </summary>
        /// <returns>The generic type of this event</returns>
        Type GetGenericType();
    }

    /// <summary>
    /// Provides a generic event - one that is able to be filtered based on
    /// the supplied Generic type.
    /// </summary>
    /// <typeparam name="T">The filtering type</typeparam>
    public interface IGenericEvent<T> : IGenericEvent{}
}
