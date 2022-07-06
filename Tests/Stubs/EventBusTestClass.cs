using System;
using EventBus.Api;

namespace EventBusTests.Stubs{
    public sealed class EventBusTestClass{
        /*
         * Properties
         */
        public bool Hit1{get; private set;}
        public bool Hit2{get; private set;}


        /*
         * Event listeners
         */
        [SubscribeEvent]
        public void EventMethod(DummyEvent evt){
            Hit1 = true;
        }

        [SubscribeEvent]
        public void EventMethod2(DummyEvent.GoodEvent evt){
            Hit2 = true;
        }

        [SubscribeEvent]
        public void EvtMethod3(DummyEvent.CancellableEvent evt){}

        [SubscribeEvent]
        public void EvtMethod4(DummyEvent.ResultEvent evt){}

        [SubscribeEvent]
        public void BadEventMethod(DummyEvent.BadEvent evt){
            throw new Exception("Throwing exception for DummyEvent.BadEvent");
        }
    }
}
