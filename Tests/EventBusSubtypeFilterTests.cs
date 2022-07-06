using System;
using NUnit.Framework;
using EventBus.Api;

namespace EventBusTests{
    [TestFixture]
    public sealed class EventBusSubtypeFilterTests{
        /*
         * Stub events
         */
        public interface IEventMarker{}
        public class BaseEvent : Event, IEventMarker{}
        public class OtherEvent : Event{}


        /*
         * Private methods
         */
        private IEventBus Bus(){
            return BusBuilder.Builder().MarkerType(typeof(IEventMarker)).Build();
        }

        private IEventBus BusChecked(){
            return BusBuilder.Builder().MarkerType(typeof(IEventMarker)).CheckTypesOnDispatch().Build();
        }


        /*
         * Test methods
         */
        [Test]
        public void TestValidType(){
            IEventBus bus = BusChecked();
            Assert.Multiple(() => {
                Assert.DoesNotThrow(() => bus.AddListener<BaseEvent>(e => {}));
                Assert.DoesNotThrow(() => bus.Post(new BaseEvent()));
            });
        }

        [Test]
        public void TestInvalidType(){
            IEventBus bus = BusChecked();
            Assert.Multiple(() => {
                Assert.Throws<ArgumentException>(() => bus.AddListener<OtherEvent>(e => {}));
                Assert.Throws<ArgumentException>(() => bus.Post(new OtherEvent()));
            });
        }

        [Test]
        public void TestInvalidTypeNoDispatch(){
            IEventBus bus = Bus();
            Assert.Multiple(() => {
                Assert.Throws<ArgumentException>(() => bus.AddListener<OtherEvent>(e => {}));
                Assert.DoesNotThrow(() => bus.Post(new OtherEvent()));
            });
        }
    }
}
