using System;
using NUnit.Framework;
using EventBus.Api;
using EventBusTests.Stubs;

namespace EventBusTests{
    [TestFixture]
    public sealed class ParentHandlerTests{
        /*
         * Stub events
         */
        public class SuperEvent : Event{}
        public class SubEvent : SuperEvent{}


        /*
         * Fields
         */
        private IEventBus bus;


        /*
         * Setup
         */
        public void SetUp(){
            bus = BusBuilder.Builder().Build();
        }


        /*
         * Tests
         */
        public void ParentHandlersGetInvoked(){
            bool superEventHandled = false;
            bool subEventHandled = false;
            bus.AddListener<SuperEvent>(EventPriority.NORMAL, false, e => {
                Type eventType = e.GetType();
                if(eventType == typeof(SuperEvent)){
                    superEventHandled = true;
                }else if(eventType == typeof(SubEvent)){
                    subEventHandled = true;
                }
            });
            Assert.Multiple(() => {
                Assert.IsTrue(superEventHandled);
                Assert.IsTrue(subEventHandled);
            });
        }
        
        public void ParentHandlersGetInvokedDummy(){
            EventBusTestClass listener = new EventBusTestClass();
            bus.Register(listener);
            bus.Post(new DummyEvent.GoodEvent());
            Assert.Multiple(() => {
                Assert.IsTrue(listener.Hit1);
                Assert.IsTrue(listener.Hit2);
            });
        }
    }
}
