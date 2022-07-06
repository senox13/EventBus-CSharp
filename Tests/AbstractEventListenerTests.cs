using NUnit.Framework;
using EventBus.Api;

namespace EventBusTests{
    [TestFixture]
    public sealed class AbstractEventListenerTests{
        /*
         * Stub events
         */
        public abstract class AbstractSuperEvent : Event{}
        public class ConcreteSuperEvent : AbstractSuperEvent{}
        public abstract class AbstractSubEvent : ConcreteSuperEvent{}
        public class ConcreteSubEvent : AbstractSubEvent{}


        /*
         * Test methods
         */
        [Test]
        public void EventHandlersCanSubscribeToAbstractEvents(){
            bool abstractSuperEventHandled = false;
            bool concreteSuperEventHandled = false;
            bool abstractSubEventHandled = false;
            bool concreteSubEventHandled = false;

            IEventBus bus = BusBuilder.Builder().Build();
            bus.AddListener<AbstractSuperEvent>(EventPriority.NORMAL, false, ev => abstractSuperEventHandled = true);
            bus.AddListener<ConcreteSuperEvent>(EventPriority.NORMAL, false, ev => concreteSuperEventHandled = true);
            bus.AddListener<AbstractSubEvent>(EventPriority.NORMAL, false, ev => abstractSubEventHandled = true);
            bus.AddListener<ConcreteSubEvent>(EventPriority.NORMAL, false, ev => concreteSubEventHandled = true);

            bus.Post(new ConcreteSubEvent());

            Assert.Multiple(() => {
                Assert.IsTrue(abstractSuperEventHandled);
                Assert.IsTrue(concreteSuperEventHandled);
                Assert.IsTrue(abstractSubEventHandled);
                Assert.IsTrue(concreteSubEventHandled);
            });
        }
    }
}
