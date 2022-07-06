using NUnit.Framework;
using EventBus.Api;

namespace EventBusTests{
    [TestFixture]
    public sealed class EventFiringEventTest{
        /*
         * Stub events
         */
        public class Event1 : Event{}
        public abstract class AbstractEvent : Event{
            public class Event2 : AbstractEvent{}
        }


        /*
         * Test methods
         */
        [Test]
        public void Test(){
            bool handled1 = false;
            bool handled2 = false;

            IEventBus bus = BusBuilder.Builder().Build();
            bus.AddListener<Event1>(EventPriority.NORMAL, false, e1 => {
                handled1 = true;
                bus.Post(new AbstractEvent.Event2());
            });
            bus.AddListener<AbstractEvent.Event2>(EventPriority.NORMAL, false, e2 => {
                handled2 = true;
            });

            bus.Post(new Event1());

            Assert.Multiple(() => {
                Assert.IsTrue(handled1);
                Assert.IsTrue(handled2);
            });
        }

    }
}
