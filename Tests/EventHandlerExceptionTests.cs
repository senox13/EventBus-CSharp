using System;
using NUnit.Framework;
using EventBus.Api;
using EventBusTests.Stubs;

namespace EventBusTests{
    [TestFixture]
    public sealed class EventHandlerExceptionTests{
        /*
         * Test methods
         */
        [Test]
        public void TestEventHandlerException(){
            IEventBus bus = BusBuilder.Builder().Build();
            EventBusTestClass listener = new EventBusTestClass();
            bus.Register(listener);
            Assert.Throws<Exception>(() => bus.Post(new DummyEvent.BadEvent()));
        }
    }
}
