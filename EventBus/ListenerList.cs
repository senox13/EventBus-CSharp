using System;
using System.Collections.Generic;
using EventBus.Api;


namespace EventBus{
    /// <summary>
    /// Holds a list of <see cref="IEventListener"/> instances for a given
    /// <see cref="Event"/> type. Additionally keeps track of the
    /// <see cref="EventPriority"/> for each event and which
    /// <see cref="EventBus"/> it is registetred to.
    /// </summary>
    public class ListenerList{
        /*
         * Static fields
         */
        // List of all ListenerList instances constructed
        private static readonly List<ListenerList> allLists = new List<ListenerList>();
        private static readonly object allListsLock = new object();
        // Holds the length that new ListenerList instances should initialize their lists array to.
        // Equivalent to the ID of the most recently initialized EventBus plus one.
        private static int maxSize = 0;
        

        /*
         * Instance fields
         */
        // A list to inherit event handlers from. This is largely used for event inheritance.
        // i.e. If type B is derived from type A, a listener for type B will also receive all events fired for type A.
        private readonly ListenerList parent;
        // Each ListenerListInst in this array corresponds to the EventBus with an ID matching its index
        private ListenerListInst[] lists = new ListenerListInst[0];
        private readonly object listsLock = new object();


        /*
         * Constructors
         */
        /// <summary>
        /// Constructs a new ListenerList instance with no listeners and no parent list.
        /// </summary>
        public ListenerList() : this(null){}

        /// <summary>
        /// Constructs a new ListenerList instance with the given parent. A
        /// list inherits listeners from its parent, even if they are added to
        /// the parent list after the child list is created.
        /// </summary>
        /// <param name="parentIn">The ListenerList instance to inherit listeners from</param>
        public ListenerList(ListenerList parentIn){
            // parent needs to be set before resize!
            parent = parentIn;
            ExtendMasterList(this);
            ResizeLists(maxSize);
        }


        /*
         * Static methods
         */
        /// <summary>
        /// Adds a ListenerList instance to the private list allLists, which
        /// contains every ListenerList instance which has been instantiated.
        /// This method is called in the ListenerList constructor and should
        /// never have to be called otherwise.
        /// </summary>
        /// <param name="inst">The new ListenerList instance</param>
        private static void ExtendMasterList(ListenerList inst){
            lock(allListsLock){
                allLists.Add(inst);
            }
        }

        /// <summary>
        /// Sets a new max size for the lists field of all ListenerLists. If
        /// the given max is less then the current size, does nothing.
        /// </summary>
        /// <param name="max">The new size of the lists field</param>
        internal static void Resize(int max){
            if(max <= maxSize){
                return;
            }
            lock(allListsLock){
                allLists.ForEach(list => list.ResizeLists(max));
            }
            maxSize = max;
        }

        /// <summary>
        /// Unregisters a given <see cref="IEventListener"/> from the
        /// <see cref="ListenerList"/> for the given id.
        /// </summary>
        /// <param name="id">The ID of the <see cref="EventBus"/> to unregister
        /// the given listener from</param>
        /// <param name="listener">The <see cref="IEventListener"/> to unregister</param>
        public static void UnregisterAll(int id, IEventListener listener){
            lock(allListsLock){
                foreach(ListenerList list in allLists){
                    list.Unregister(id, listener);
                }
            }
        }

        
        /*
         * Private instance methods
         */
        /// <summary>
        /// Sets a new size for the lists field. This represents the number of
        /// separate ListenerListInst instanes that this ListenerList will hold
        /// references to. Practically, this defines the new maximum id value
        /// that can be passed into <see cref="GetInstance(int)"/>,
        /// <see cref="GetListeners(int)"/>,
        /// <see cref="Register(int, EventPriority, IEventListener)"/>,
        /// and <see cref="Unregister(int, IEventListener)"/>. If the given
        /// max is less then the current size, does nothing. This method is
        /// thread-safe.
        /// </summary>
        /// <param name="max">The new length for the lists field</param>
        private void ResizeLists(int max){
            if(parent != null){
                parent.ResizeLists(max);
            }
            if(lists.Length >= max){
                return;
            }
            lock(listsLock){
                ListenerListInst[] newList = new ListenerListInst[max];
                int x = 0;
                for(; x<lists.Length; x++){
                    newList[x] = lists[x];
                }
                for(; x<max; x++){
                    if(parent != null){
                        newList[x] = new ListenerListInst(parent.GetInstance(x));
                    }else{
                        newList[x] = new ListenerListInst();
                    }
                }
                lists = newList;
            }
        }

        /// <summary>
        /// Gets the <see cref="ListenerListInst"/> corresponding to
        /// the <see cref="EventBus"/> instance with the specified ID.
        /// </summary>
        /// <param name="id">The ID corresponding to the <see cref="ListenerListInst"/>
        /// that will be returned</param>
        /// <returns>The ListenerListInst corresponding to the given ID</returns>
        private ListenerListInst GetInstance(int id){
            return lists[id];
        }


        /*
         * Public instance methods
         */
        /// <summary>
        /// Gets all <see cref="IEventListener"/>s for the given id.
        /// </summary>
        /// <param name="id">The ID of the <see cref="EventBus"/> to get
        /// listeners for</param>
        /// <returns>The listeners corresponding to the given id</returns>
        public IEventListener[] GetListeners(int id){
            return lists[id].GetListeners();
        }

        /// <summary>
        /// Registers the given <see cref="IEventListener"/> to the
        /// <see cref="EventBus"/> corresponding to the given ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="EventBus"/> to
        /// register this event listener to</param>
        /// <param name="priority">The <see cref="EventPriority"/> with which
        /// to register this event listener</param>
        /// <param name="listener">The new event listener to register</param>
        public void Register(int id, EventPriority priority, IEventListener listener){
            lists[id].Register(priority, listener);
        }

        /// <summary>
        /// Unregisters the given <see cref="IEventListener"/> from the
        /// <see cref="EventBus"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the EventBus to remove the event
        /// listener from</param>
        /// <param name="listener">The IEventListener to unregister</param>
        public void Unregister(int id, IEventListener listener){
            lists[id].Unregister(listener);
        }


        /*
         * Nested types
         */
        /// <summary>
        /// Holds <see cref="IEventListener"/> instances for a specific <see cref="EventBus"/> instance.
        /// </summary>
        private class ListenerListInst{
            /*
             * Fields
             */
            //If true, signifies that this instance's listeners array needs to be rebuilt
            private bool rebuild = true;
            //Acts as a cached, flattened version of the priorities nested list,
            //with all priorities in order and each group of listeners of a given
            //priority prefaced by the instance of that priority. Includes
            //listeners from the parent ListenerListInst.
            private IEventListener[] listeners;
            //Holds a list of listeners for each priority level. The list at each
            //index corresponds to the priority of the same index.
            private readonly List<List<IEventListener>> priorities;
            //The ListenerListInst to inherit listeners from
            private readonly ListenerListInst parent;
            //List of ListenerListInst that inherit listeners from this one
            private List<ListenerListInst> children;
            private readonly object writeLock = new object();


            /*
             * Constructor
             */
            /// <summary>
            /// Constructs a new ListenerListInst with no parent and no
            /// listeners.
            /// </summary>
            public ListenerListInst(){
                int count = Enum.GetValues(typeof(EventPriority)).Length;
                priorities = new List<List<IEventListener>>(count);
                for(int i=0; i<count; i++){
                    priorities.Add(new List<IEventListener>());
                }
            }
            
            /// <summary>
            /// Constructs  a new ListenerListInst with the specified parent
            /// and no listeners.
            /// </summary>
            /// <param name="parentIn">The parent ListenerListInst to inherit
            /// event listeners from</param>
            public ListenerListInst(ListenerListInst parentIn) : this(){
                parent = parentIn;
                parent.AddChild(this);
            }


            /*
             * Public methods
             */
            /// <summary>
            /// Returns a List containing all listeners for this event, and
            /// all parent events for the specified priority. The list is
            /// returned with the listeners for the children events first.
            /// </summary>
            /// <param name="priority">The priority of events to get</param>
            /// <returns>A List containing all of this ListenerList's listeners for the given priority</returns>
            public List<IEventListener> GetListeners(EventPriority priority){
                List<IEventListener> ret;
                lock(writeLock){
                    ret = new List<IEventListener>(priorities[(int)priority]);
                }
                if(parent != null){
                    ret.AddRange(parent.GetListeners(priority));
                }
                return ret;
            }

            /// <summary>
            /// Returns a full list of all <see cref="IEventListener"/>s for all priority levels,
            /// including all parent listeners, in proper priority order. The
            /// list includes <see cref="EventPriorityListener"/> instances immediately preceeding
            /// events of the corresponding priority. This method's return
            /// value will remain cached until the listeners on this or a
            /// parent list change.
            /// </summary>
            /// <returns>An array of <see cref="IEventListener"/> instances,
            /// in priority order</returns>
            public IEventListener[] GetListeners(){
                if(ShouldRebuild())
                    BuildCache();
                return listeners;
            }

            /// <summary>
            /// Adds the given <see cref="IEventListener"/> instance to this
            /// listener with the specified priority.
            /// </summary>
            /// <param name="priority">The priority at which to add the listener</param>
            /// <param name="listener">The new event listener</param>
            public void Register(EventPriority priority, IEventListener listener){
                lock(writeLock){
                    priorities[(int)priority].Add(listener);
                }
                ForceRebuild();
            }

            /// <summary>
            /// Remove the given <see cref="IEventListener"/> instance from
            /// this ListenerList. This method is thread safe.
            /// </summary>
            /// <param name="listener">The event listener to remove from this list</param>
            public void Unregister(IEventListener listener){
                lock(writeLock){
                    foreach(List<IEventListener> listeners in priorities){
                        if(listeners.Remove(listener)){
                            ForceRebuild();
                        }
                    }
                }
            }


            /*
             * Private methods
             */
            /// <summary>
            /// Adds the given <see cref="ListenerListInst"/> to this
            /// ListenerListInst's list of children.
            /// </summary>
            /// <param name="child">The new child list</param>
            private void AddChild(ListenerListInst child){
                if(children == null)
                    children = new List<ListenerListInst>();
                children.Add(child);
            }

            /// <summary>
            /// Check if this list needs its cache rebuilt.
            /// </summary>
            /// <returns>Returns true if a rebuild is required, otherwise false</returns>
            private bool ShouldRebuild(){
                return rebuild;
            }

            /// <summary>
            /// Forces a rebuild. This triggers a rebuild on all child lists as well.
            /// </summary>
            private void ForceRebuild(){
                rebuild = true;
                if(children != null){
                    children.ForEach(c => c.ForceRebuild());
                }
            }

            /// <summary>
            /// Rebuild the local Array of listeners, returns early if there is no work to do.
            /// </summary>
            private void BuildCache(){
                if(parent != null && parent.ShouldRebuild()){
                    parent.BuildCache();
                }
                List<IEventListener> ret = new List<IEventListener>();
                foreach(EventPriority priority in Enum.GetValues(typeof(EventPriority))){
                    List<IEventListener> listeners = GetListeners(priority);
                    if(listeners.Count > 0){
                        ret.Add(new EventPriorityListener(priority));
                        ret.AddRange(listeners);
                    }
                }
                listeners = ret.ToArray();
                rebuild = false;
            }
        }
    }
}
