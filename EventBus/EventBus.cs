using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using EventBus.Api;

namespace EventBus{
    /// <summary>
    /// Provides an implementation of <see cref="IEventBus"/>. This class
    /// should generally be instantiated via <see cref="BusBuilder.Build"/>.
    /// </summary>
    public class EventBus : IEventBus{
        /*
         * Fields
         */
        //Static fields
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(EventBus));
        private static readonly bool CHECK_TYPES_ON_DISPATCH_ENV_VAR = ReadCheckTypesEnvVar();
        private static int maxID = 0;
        //Instance fields
        private readonly ConcurrentDictionary<object, IList<IEventListener>> listeners = new ConcurrentDictionary<object, IList<IEventListener>>();
        private readonly int busID = Interlocked.Increment(ref maxID);
        private readonly bool trackPhases;
        private readonly EventExceptionHandler exceptionHandler;
        private readonly bool checkTypesOnDispatch = CHECK_TYPES_ON_DISPATCH_ENV_VAR;
        private readonly Type baseType;
        private volatile bool shutdown = false;

    

        /*
         * Constructors
         */
        private EventBus(){
            ListenerList.Resize(busID + 1);
            exceptionHandler = HandleException;
            trackPhases = true;
            baseType = typeof(Event);
            checkTypesOnDispatch = CHECK_TYPES_ON_DISPATCH_ENV_VAR;
        }

        private EventBus(EventExceptionHandler handler, bool trackPhase, bool startShutdown, Type baseTypeIn, bool checkTypesOnDispatchIn){
            ListenerList.Resize(busID + 1);
            exceptionHandler = handler == null ? HandleException : handler;
            trackPhases = trackPhase;
            shutdown = startShutdown;
            baseType = baseTypeIn;
            checkTypesOnDispatch = checkTypesOnDispatchIn || CHECK_TYPES_ON_DISPATCH_ENV_VAR;
        }

        /// <summary>
        /// Constructs a new EventBus from the given <see cref="BusBuilder"/>
        /// </summary>
        /// <param name="busBuilder">The BusBuilder to use to construct this EventBus</param>
        public EventBus(BusBuilder busBuilder)
            :this(busBuilder.exceptionHandler,
                busBuilder.trackPhases,
                busBuilder.startShutdown,
                busBuilder.markerType,
                busBuilder.checkTypesOnDispatch
            ){}


        /*
         * Static methods
         */
        /// <summary>
        /// Reads the environment variable <c>EVENTBUS__CHECK_TYPES_ON_DISPATCH</c>
        /// and attempts to parse its value as a boolean. If it is not present
        /// in the environment or cannot be parsed, defaults to true.
        /// </summary>
        /// <returns>The value of the environment variable if found, otherwise true</returns>
        private static bool ReadCheckTypesEnvVar(){
            string envVar = Environment.GetEnvironmentVariable("EVENTBUS__CHECK_TYPES_ON_DISPATCH");
            if(envVar == null)
                return false;
            try{
                return bool.Parse(envVar);
            }catch(FormatException){
                LOGGER.Warn($"Failed to parse value of EVENTBUS__CHECK_TYPES_ON_DISPATCH, defaulting to false");
                return false;
            }
        }


        /*
         * Private methods
         */
        /// <summary>
        /// Scans a given <see cref="Type"/> for static methods that have a
        /// <see cref="SubscribeEventAttribute"/>, and registers them as event
        /// listeners on this bus.
        /// </summary>
        /// <param name="type">The type to scan for static methods</param>
        private void RegisterClass(Type type){
            foreach(MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)){
                if(method.GetCustomAttribute<SubscribeEventAttribute>(false) != null){
                    RegisterListener(type, method);
                }
            }
        }

        /// <summary>
        /// Scans a given arbitrary <see cref="object"/> for public instance
        /// methods that have a <see cref="SubscribeEventAttribute"/>, and
        /// registers them as event listeners on this bus.
        /// </summary>
        /// <param name="obj">The object to scan for instance methods</param>
        private void RegisterObject(object obj){
            foreach(MethodInfo method in obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)){
                if(method.GetCustomAttributes(typeof(SubscribeEventAttribute), true).Length > 0){
                    RegisterListener(obj, method);
                }
            }
        }

        private void RegisterListener(object target, MethodInfo method){
            ParameterInfo[] parameters = method.GetParameters();
            //Verify that this method has a single argument
            if(parameters.Length != 1){
                string fullMethodName = method.DeclaringType.FullName + "." + method.Name;
                throw new ArgumentException($"Method {fullMethodName} has SubscribeEvent annotation. It has {parameters.Length} arguments, but event handler methods require a single argument only.");
            }

            Type eventType = parameters[0].ParameterType;
            //Verify that this method's argument is assignable from Event
            if(!typeof(Event).IsAssignableFrom(eventType)){
                string fullMethodName = method.DeclaringType.FullName + "." + method.Name;
                throw new ArgumentException($"Method {fullMethodName} has SubscribeEvent annotation, but takes an argument that is not an Event subtype : {eventType}");
            }

            //If this bus' base type is not Event, verify that this method's argument is assignable from it
            if(baseType != typeof(Event) && !baseType.IsAssignableFrom(eventType)){
                string fullMethodName = $"{method.DeclaringType.FullName}.{method.Name}";
                throw new ArgumentException($"Method {fullMethodName} has SubscribeEvent annotation, but takes an argument that is not a subtype of the base type {baseType}: {eventType} ");
            }

            //Verify that this method is public
            if(!method.IsPublic){
                throw new ArgumentException($"Cannot register non-public method {method.ReflectedType.FullName}.{method.Name}", nameof(method));
            }

            Register(eventType, target, method);
        }

        private Predicate<T> PassCancelled<T>(bool ignored) where T : Event{
            return e => ignored || !e.IsCancelable() || !e.IsCanceled();
        }

        private Predicate<T> PassGenericFilter<T, F>(Type type) where T : GenericEvent<F>{
            return e => e.GetGenericType().Equals(type);
        }

        private void CheckNotGeneric(Type eventType){
            Type type = eventType;
            while(type != null){
                if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(GenericEvent<>)){
                    throw new ArgumentException("Cannot register a generic event listener with AddListener, use AddGenericListener");
                }
                type = type.BaseType;
            }
        }

        private void AddListener<T>(EventPriority priority, Predicate<T> filter, Action<T> consumer) where T: Event{
            if(typeof(T).Equals(typeof(Event))){
                LOGGER.Warn("Attempting to add a Lambda listener with computed generic type of Event. Are you sure this is what you meant?");
            }
            AddListener(priority, filter, typeof(T), consumer);
        }

        private void AddListener<T>(EventPriority priority, Predicate<T> filter, Type eventClass, Action<T> consumer) where T : Event{
            if(baseType != typeof(Event) && !baseType.IsAssignableFrom(eventClass)){
                throw new ArgumentException($"Listener for event {eventClass} takes an argument that is not a subtype of the base type {baseType}");
            }
            AddToListeners(consumer, eventClass, new ListenerWrapper(e => DoCastFilter(filter, consumer, e)), priority);
        }

        private void DoCastFilter<T>(Predicate<T> filter, Action<T> consumer, Event e) where T : Event{
            T cast = (T)e;
            if(filter(cast)){
                consumer(cast);
            }
        }

        private void Register(Type eventType, object target, MethodInfo method){
            try{
                ASMEventHandler asm = new ASMEventHandler(target, method, typeof(IGenericEvent).IsAssignableFrom(eventType));
                AddToListeners(target, eventType, asm, asm.GetPriority());
            }catch(Exception e) when(e is MemberAccessException || e is TargetInvocationException){
                LOGGER.Error($"Error registering event handler: {eventType} {method} {e}");
            }
        }

        private void AddToListeners(object target, Type eventType, IEventListener listener, EventPriority priority){
            ListenerList listenerList = EventListenerHelper.GetListenerList(eventType);
            listenerList.Register(busID, priority, listener);
            IList<IEventListener> others = listeners.GetOrAdd(target, k=> new List<IEventListener>());
            others.Add(listener);
        }

        private void HandleException(IEventBus bus, Event ev, IEventListener[] listeners, int index, Exception ex){
            LOGGER.Error(new EventBusErrorMessage(ev, index, listeners, ex));
        }


        /*
         * Public methods
         */
        /// <inheritdoc/>
        public void Register(object target){
            if(listeners.ContainsKey(target)){
                return;
            }
            if(target is Type typeToReg){
                RegisterClass(typeToReg);
            }else{
                RegisterObject(target);
            }
        }

        /// <inheritdoc/>
        public void AddListener<T>(Action<T> consumer) where T : Event{
            AddListener(EventPriority.NORMAL, consumer);
        }
        
        /// <inheritdoc/>
        public void AddListener<T>(EventPriority priority, Action<T> consumer) where T : Event{
            AddListener(priority, false, consumer);
        }
        
        /// <inheritdoc/>
        public void AddListener<T>(EventPriority priority, bool receiveCancelled, Action<T> consumer) where T : Event{
            CheckNotGeneric(typeof(T));
            AddListener(priority, PassCancelled<T>(receiveCancelled), consumer);
        }
        
        /// <inheritdoc/>
        public void AddGenericListener<T, F>(Action<T> consumer) where T : GenericEvent<F>{
            AddGenericListener<T, F>(EventPriority.NORMAL, consumer);
        }
        
        /// <inheritdoc/>
        public void AddGenericListener<T, F>(EventPriority priority, Action<T> consumer) where T : GenericEvent<F>{
            AddGenericListener<T, F>(priority, false, consumer);
        }
        
        /// <inheritdoc/>
        public void AddGenericListener<T, F>(EventPriority priority, bool receiveCancelled, Action<T> consumer) where T : GenericEvent<F>{
            bool filter(T e){
                return PassGenericFilter<T, F>(typeof(F)).Invoke(e)
                    && PassCancelled<T>(receiveCancelled).Invoke(e);
            }
            AddListener(priority, filter, consumer);
        }

        /// <inheritdoc/>
        public void Unregister(object obj){
            if(listeners.TryRemove(obj, out IList<IEventListener> list)){
                foreach(IEventListener listener in list){
                    ListenerList.UnregisterAll(busID, listener);
                }
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If the environment variable <c>EVENTBUS__CHECK_TYPES_ON_DISPATCH</c>
        /// is set to <c>true</c> (case-insensitive) or is not provided,
        /// posting events of types that do not match this eventbus's base
        /// type will generate an <see cref="ArgumentException"/>. If the value
        /// of the environment variable cannot be parsed as a boolean, a
        /// warning will be logged and this value will default to <c>false</c>.
        /// </remarks>
        public bool Post(Event ev){
            return Post(ev, (el, ev) => el.Invoke(ev));
        }

        /// <inheritdoc/>
        public bool Post(Event ev, EventBusInvokeDispatcher wrapper){
            if(shutdown)return false;
            if(checkTypesOnDispatch && !baseType.IsAssignableFrom(ev.GetType())){
                throw new ArgumentException($"Cannot post event of type {ev.GetType().Name} to this event. Must match type: {baseType.Name}", nameof(ev));
            }
            IEventListener[] listeners = ev.GetListenerList().GetListeners(busID);
            int index = 0;
            try{
                for(; index<listeners.Length; index++){
                    if(!trackPhases && listeners[index].GetType().Equals(typeof(EventPriority))) continue;
                    wrapper.Invoke(listeners[index], ev);
                }
            }catch(Exception ex){
                exceptionHandler(this, ev, listeners, index, ex);
                throw ex;
            }
            return ev.IsCancelable() && ev.IsCanceled();
        }

        /// <inheritdoc/>
        public void Start(){
            shutdown = false;
        }

        /// <inheritdoc/>
        public void Shutdown(){
            LOGGER.Error($"EventBus {busID} shutting down - future events will not be posted.");
            shutdown = true;
        }


        /*
         * Nested types
         */
        /// <summary>
        /// A simple wrapper from <see cref="Action{Event}"/> to <see cref="IEventListener"/>
        /// </summary>
        private class ListenerWrapper : IEventListener{
            private readonly Action<Event> listener;

            public ListenerWrapper(Action<Event> listenerIn){
                listener = listenerIn;
            }

            public void Invoke(Event e){
                listener(e);
            }
        }
    }
}