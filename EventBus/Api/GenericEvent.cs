using System;


namespace EventBus.Api{
    /// <summary>
    /// Implements <see cref="IGenericEvent{T}"/> to provide filterable events
    /// based on generic type data. Subclasses should extend this if they wish
    /// to expose a secondary type based filter (the generic type).
    /// </summary>
    /// <typeparam name="T">The type to filter this generic event for</typeparam>
    public class GenericEvent<T> : Event, IGenericEvent<T>{
        /*
         * Instance fields
         */
        // A type, which is guaranteed to be derived from T, that this event will be filtered for
        private readonly Type type;


        /*
         * Constructors
         */
        /// <summary>
        /// Constructs a new <see cref="GenericEvent{T}"/>.
        /// </summary>
        public GenericEvent() : this(typeof(T)){}
        
        /// <summary>
        /// Constructs a new <see cref="GenericEvent{T}"/> with the given type
        /// that this event should be filtererd for. The given type must be a
        /// subtype of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="type">The type to filter this generic event for</param>
        protected GenericEvent(Type type){
            if(!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException($"Given type '{type}' cannot be used as a filter as it is not assignable from base type '{typeof(T)}'", nameof(type));
            this.type = type;
        }


        /*
         * Interface implementation
         */
        /// <inheritdoc/>
        public Type GetGenericType(){
            return type;
        }
    }
}
