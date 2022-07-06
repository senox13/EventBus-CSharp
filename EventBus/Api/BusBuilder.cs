using System;


namespace EventBus.Api{
    /// <summary>
    /// Provides a class which can be used to define and instantiate an
    /// <see cref="EventBus"/> instance.
    /// </summary>
    public sealed class BusBuilder{
        /*
         * Fields
         */
        internal EventExceptionHandler exceptionHandler;
        internal bool trackPhases = true;
        internal bool startShutdown = false;
        internal bool checkTypesOnDispatch = false;
        internal Type markerType = typeof(Event);


        /*
         * Static methods
         */
        /// <summary>
        /// Creates a new <see cref="BusBuilder"/> instance with the following
        /// default values. Track phases: true, Start shutdown: false, Marker
        /// type: Event.
        /// </summary>
        /// <returns>The new <see cref="BusBuilder"/> instance</returns>
        public static BusBuilder Builder(){
            return new BusBuilder();
        }


        /*
         * Constructor
         */
        private BusBuilder(){}


        /*
         * Public instance methods
         */
        /// <summary>
        /// Sets the flag representing whether <see cref="EventBus"/> instances
        /// created by this BusBuilder will track the priority of the
        /// <see cref="IEventListener"/> currently being invoked.
        /// </summary>
        /// <param name="trackPhases"></param>
        /// <returns>This BusBuilder instance, for method chaining</returns>
        public BusBuilder SetTrackPhases(bool trackPhases){
            this.trackPhases = trackPhases;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="EventExceptionHandler"/> delegate that
        /// <see cref="EventBus"/> instances instantiated by this builder will
        /// use for handling exceptions thrown by <see cref="IEventListener"/>
        /// instances registered on this bus.
        /// </summary>
        /// <param name="handler">The <see cref="EventExceptionHandler"/> to assign to this bus</param>
        /// <returns>This BusBuilder instance, for method chaining</returns>
        public BusBuilder SetExceptionHandler(EventExceptionHandler handler){
            exceptionHandler = handler;
            return this;
        }

        /// <summary>
        /// Sets this builder to create <see cref="EventBus"/> instances with
        /// their shutdown flag set. EventBusses that are shut down silently
        /// ignore posted events.
        /// </summary>
        /// <returns>This BusBuilder instance, for method chaining</returns>
        public BusBuilder StartShutdown(){
            startShutdown = true;
            return this;
        }

        /// <summary>
        /// Sets this builder to create <see cref="EventBus"/> instances that
        /// verify valid event types are posted.
        /// </summary>
        /// <returns>This BusBuilder instance, for method chaining</returns>
        public BusBuilder CheckTypesOnDispatch(){
            checkTypesOnDispatch = true;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="Type"/> that all events handled by
        /// <see cref="EventBus"/> instances created by this builder must be
        /// derived from.
        /// </summary>
        /// <param name="type">The base event type for this EventBuilder</param>
        /// <returns>This BusBuilder instance, for method chaining</returns>
        public BusBuilder MarkerType(Type type){
            if(!type.IsInterface)
                throw new ArgumentException("Cannot specify a non-interface marker type", nameof(type));
            markerType = type;
            return this;
        }

        /// <summary>
        /// Instantiates and returns a new <see cref="IEventBus"/> instance
        /// with the parameters that have been set on this BusBuilder.
        /// </summary>
        /// <returns>The new IEventBus instance</returns>
        public IEventBus Build(){
            return new EventBus(this);
        }
    }
}
