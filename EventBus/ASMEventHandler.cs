using System;
using System.Reflection;
using EventBus.Api;


namespace EventBus{
    /// <summary>
    /// Wrapper type for a single method which is to be used as an event
    /// listener. Implements <see cref="IEventListener"/> by invoking the
    /// method passed into the constructor.
    /// </summary>
    public class ASMEventHandler : IEventListener{
        /*
         * Instance fields
         */
        //An instance of the dynamic type generated for this instance's callback method
        private readonly IEventListener handler;
        //The subscribe attribute from the callback method
        private readonly SubscribeEventAttribute subInfo;
        //A human-readable representation of this instance, used by ToString()
        private readonly string readable;
        //A type definition to be used as a filter for generic events. Remains null for non-generic events
        private readonly Type filter = null;


        /*
         * Constructor
         */
        /// <summary>
        /// Constructs a new <see cref="ASMEventHandler"/> instance which
        /// wraps the given <see cref="MethodInfo"/> and object instance (for
        /// non-static methods) to dynamically create an
        /// <see cref="IEventListener"/> implementation.
        /// </summary>
        /// <param name="target">The instance that the passed in MethodInfo
        /// should be called on</param>
        /// <param name="method">The method to use as an event listener. Must
        /// take a single argument of type <see cref="Event"/></param>
        /// <param name="isGeneric">Should be set to true if the
        /// <see cref="Event"/> type passed in is generic</param>
        public ASMEventHandler(object target, MethodInfo method, bool isGeneric){
            handler = ASMEventListenerFactory.Create(method, target);
            subInfo = method.GetCustomAttribute<SubscribeEventAttribute>();
            readable = $"ASM: {target} {method}";
            if(isGeneric){
                Type type = method.GetParameters()[0].ParameterType;
                if(type.IsGenericType){
                    filter = type.GetGenericArguments()[0];
                    if(filter.IsGenericType){
                        filter = type.GetGenericTypeDefinition();
                    }
                }
            }
        }
        

        /*
         * Public methods
         */
        /// <summary>
        /// Returns the priority of this event as specified by the
        /// <see cref="SubscribeEventAttribute"/> on the method passed into
        /// the constructor. If not specified, defaults to
        /// <see cref="EventPriority.NORMAL"/>.
        /// </summary>
        /// <returns>An <see cref="EventPriority"/> instance representing the
        /// priority of this event listener</returns>
        public EventPriority GetPriority(){
            return subInfo.Priority;
        }
        
        
        /*
         * IEventListener implementation
         */
        /// <summary>
        /// Checks the cancelation state of the given <see cref="Event"/> against
        /// the <see cref="SubscribeEventAttribute"/> on this instance's callback
        /// method and invokes the callback if appropriate. Additionally verifies
        /// that the generic type of the given event matches that of the callback
        /// method.
        /// </summary>
        /// <param name="e">The event to pass to the listener callback, if
        /// appropriate</param>
        public void Invoke(Event e){
            if(handler != null){
                if(!e.IsCancelable() || !e.IsCanceled() || subInfo.ReceiveCanceled){
                    if(filter == null || filter == ((IGenericEvent)e).GetGenericType()){
                        handler.Invoke(e);
                    }
                }
            }
        }


        /*
         * Object override methods
         */
        /// <summary>
        /// Returns a string of the format: <c>ASM: {target} {callback}</c>
        /// where target is the object instance passed into the constructor,
        /// and callback is the signature of the callback method.
        /// </summary>
        /// <returns>A string representation of this event handler wrapper instance</returns>
        public override string ToString(){
            return readable;
        }
    }
}
