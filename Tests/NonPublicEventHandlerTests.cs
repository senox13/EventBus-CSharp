using NUnit.Framework;
using EventBus.Api;
using System;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter
namespace EventBusTests{
    [TestFixture]
    public sealed class NonPublicEventHandlerTests{
        /*
         * Stub event subscribers
         */
        public sealed class PublicListener{
            public bool Hit{get; private set;}

            [SubscribeEvent]
            public void Handler(Event e){
                Hit = true;
            }
        }

        public class ProtectedListener{
            public bool Hit{get; private set;}

            [SubscribeEvent]
            protected void Handler(Event e){
                Hit = true;
            }
        }

        public sealed class InternalListener{
            public bool Hit{get; private set;}

            [SubscribeEvent]
            internal void Handler(Event e){
                Hit = true;
            }
        }

        public sealed class PrivateListener{
            public bool Hit{get; private set;}

            [SubscribeEvent]
            private void Handler(Event e){
                Hit = true;
            }
        }


        /*
         * Fields
         */
        private IEventBus bus;


        /*
         * Setup
         */
        [SetUp]
        public void SetUp(){
            bus = BusBuilder.Builder().Build();
        }


        /*
         * Test methods
         */
        [Test]
        public void PublicEventListener(){
            PublicListener listener = new PublicListener();
            bus.Register(listener);
            bus.Post(new Event());
            Assert.IsTrue(listener.Hit);
        }

        [Test]
        public void ProtectedEventListener(){
            ProtectedListener listener = new ProtectedListener();
            Assert.Throws<ArgumentException>(() => bus.Register(listener));
        }

        [Test]
        public void InternalEventListener(){
            InternalListener listener = new InternalListener();
            Assert.Throws<ArgumentException>(() => bus.Register(listener));
        }

        [Test]
        public void PrivateEventListener(){
            PrivateListener listener = new PrivateListener();
            Assert.Throws<ArgumentException>(() => bus.Register(listener));
        }
    }
}
