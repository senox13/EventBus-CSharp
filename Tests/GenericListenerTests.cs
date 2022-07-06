using System;
using System.Collections.Generic;
using NUnit.Framework;
using EventBus.Api;

namespace EventBusTests{
    [TestFixture]
    public sealed class GenericListenerTests{
        /*
         * Fields
         */
        private IEventBus bus;
        private bool hit;


        /*
         * Setup
         */
        [SetUp]
        public void SetUp(){
            bus = BusBuilder.Builder().Build();
            hit = false;
        }


        /*
         * Private methods
         */
        private void HandleGenericEvent(GenericEvent<List<string>> e){
            hit = true;
        }


        /*
         * Test methods
         */
        [Test]
        public void TestGenericListener(){
            bus.AddGenericListener<GenericEvent<List<string>>, List<string>>(HandleGenericEvent);
            bus.Post(new GenericEvent<List<string>>());
            Assert.IsTrue(hit);
        }

        [Test]
        public void TestGenericListenerRegisteredIncorrectly(){
            Assert.Throws<ArgumentException>(() => bus.AddListener<GenericEvent<List<string>>>(HandleGenericEvent));
        }
    }
}
