using System;
using NUnit.Framework;
using EventBus.Api;

namespace EventBusTests{
    [TestFixture]
    public sealed class LambdaHandlerTests{
        /*
         * Stub events
         */
        public class SubEvent : Event{}
        [Cancelable]
        public class CancelableEvent : Event{}


        /*
         * Fields
         */
        private bool hit;
        private IEventBus bus;


        /*
         * Public methods
         */
        public void ConsumeEvent(Event e){
            hit = true;
        }

        public void ConsumeSubEvent(SubEvent e){
            hit = true;
        }

        public void RegisterSomeWrapper<T>(IEventBus bus, Func<T, bool> func) where T : Event{
            bus.AddListener<T>(EventPriority.NORMAL, false, e => {
                if(func.Invoke(e)){
                    e.SetCanceled(true);
                }
            });
        }

        private bool SubEventFunction(CancelableEvent e){
            return e is CancelableEvent;
        }


        /*
         * Setup
         */
        [SetUp]
        public void SetUp(){
            hit = false;
            bus = BusBuilder.Builder().Build();
        }


        /*
         * Test methods
         */
        [Test]
        public void LambdaInline(){
            bus.AddListener<Event>(e => hit = true);
            bus.Post(new Event());
            Assert.IsTrue(hit);
        }

        [Test]
        public void MethodReference(){
            bus.AddListener<Event>(ConsumeEvent);
            bus.Post(new Event());
            Assert.IsTrue(hit);
        }

        [Test]
        public void LambdaSubClass(){
            bus.AddListener<SubEvent>(e => hit = true);
            bus.Post(new SubEvent());
            bool subClassHit = hit;
            hit = false;
            bus.Post(new Event());
            bool superClassHit = hit;
            Assert.Multiple(() => {
                Assert.IsTrue(subClassHit);
                Assert.IsFalse(superClassHit);
            });
        }

        [Test]
        public void MethodReferenceSubClass(){
            bus.AddListener<SubEvent>(ConsumeSubEvent);
            bus.Post(new SubEvent());
            bool subClassHit = hit;
            hit = false;
            bus.Post(new Event());
            bool superClassHit = hit;
            Assert.Multiple(() => {
                Assert.IsTrue(subClassHit);
                Assert.IsFalse(superClassHit);
            });
        }

        [Test]
        public void LambdaGenerics(){
            RegisterSomeWrapper<CancelableEvent>(bus, SubEventFunction);
            CancelableEvent evt = new CancelableEvent();
            bus.Post(evt);
            Assert.IsTrue(evt.IsCanceled());
            SubEvent subEvt = new SubEvent();
            bus.Post(subEvt);
        }
    }
}
