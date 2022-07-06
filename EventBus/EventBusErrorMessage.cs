using System;
using System.Text;
using EventBus.Api;

namespace EventBus{
    /// <summary>
    /// Represents the state of the <see cref="EventBus"/> when an
    /// <see cref="Exception"/> was thrown. Used for error logging and
    /// debugging.
    /// </summary>
    public class EventBusErrorMessage{
        /*
         * Properties
         */
        /// <summary>
        /// The event which was being handled when the exception was thrown
        /// </summary>
        public Event Evt{get;}
        /// <summary>
        /// The index of the <see cref="IEventListener"/> which threw the
        /// exception. This is the index of the listener in <see cref="Listeners"/>
        /// which threw the exception.
        /// </summary>
        public int Index{get;}
        /// <summary>
        /// The array of all <see cref="IEventListener"/>s which were registered
        /// to the event being handled when the exception was thrown.
        /// </summary>
        public IEventListener[] Listeners{get;}
        /// <summary>
        /// The exception which was thrown by an <see cref="IEventListener"/>
        /// </summary>
        public Exception Exception{get;}


        /*
         * Constructor
         */
        /// <summary>
        /// Constructs a new <see cref="EventBusErrorMessage"/> with the given
        /// event, index, listeners, and exception.
        /// </summary>
        /// <param name="evtIn">The event being handled when the exception was thrown</param>
        /// <param name="indexIn">The index of the listener which threw the exception</param>
        /// <param name="listenersIn">The array of all listeners registered to this event</param>
        /// <param name="exceptionIn">The exception thrown by the listener at the given index</param>
        public EventBusErrorMessage(Event evtIn, int indexIn, IEventListener[] listenersIn, Exception exceptionIn){
            Evt = evtIn;
            Index = indexIn;
            Listeners = listenersIn;
            Exception = exceptionIn;
        }


        /*
         * Override methods
         */
        /// <summary>
        /// Returns a string containing all of the information containined in
        /// this <see cref="EventBusErrorMessage"/>. Includes the exception
        /// message, list of listeners and the index of the one which threw
        /// the exception, and the exception's stacktrace.
        /// </summary>
        /// <returns>A string representation of this <see cref="EventBusErrorMessage"/></returns>
        public override string ToString(){
            StringBuilder buffer = new StringBuilder();
            buffer.
                    Append("Exception caught during firing event: ").Append(Exception.Message).Append('\n').
                    Append("\tIndex: ").Append(Index).Append('\n').
                    Append("\tListeners:\n");
            for(int x = 0; x < Listeners.Length; x++){
                buffer.Append("\t\t").Append(x).Append(": ").Append(Listeners[x]).Append('\n');
            }
            buffer.Append(Exception.StackTrace);
            return buffer.ToString();
        }
    }
}
