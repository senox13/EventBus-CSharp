using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using EventBus.Api;

namespace EventBusTests{
    [TestFixture]
    public sealed class ParallelEventTests{
        /*
         * Fields
         */
        private const int BUS_COUNT = 16;
        private const int LISTENER_COUNT = 1000;
        private const int RUN_ITERATIONS = 1000;


        /*
         * Test methods
         */
        [Test]
        public void SingleBus(){
            int hitCounter = 0;
            IEventBus bus = BusBuilder.Builder().Build();

            //Add listeners in parallel
            Parallel.For(0, LISTENER_COUNT, i => {
                bus.AddListener<DummyEvent.GoodEvent>(e => Interlocked.Increment(ref hitCounter));
            });

            //Post events in parallel
            Parallel.For(0, RUN_ITERATIONS, i => {
                bus.Post(new DummyEvent.GoodEvent());
            });

            //Check that all listeners were invoked
            Assert.AreEqual(LISTENER_COUNT * RUN_ITERATIONS, hitCounter);
        }

        [Test]
        public void MultipleBusses(){
            int hitCounter = 0;
            HashSet<IEventBus> busSet = new HashSet<IEventBus>();
            for(int i=0; i<BUS_COUNT; i++){
                busSet.Add(BusBuilder.Builder().SetTrackPhases(false).Build());
            }

            //Add listeners in parallel
            Parallel.ForEach(busSet, bus => {
                for(int i=0; i<LISTENER_COUNT; i++){
                    bus.AddListener<DummyEvent.GoodEvent>(e => Interlocked.Increment(ref hitCounter));
                }
            });

            //Post events in parallel
            Parallel.ForEach(busSet, bus => {
                for(int i=0; i<RUN_ITERATIONS; i++){
                    bus.Post(new DummyEvent.GoodEvent());
                }
            });

            //Check that all listeners were invoked
            Assert.AreEqual(BUS_COUNT * LISTENER_COUNT * RUN_ITERATIONS, hitCounter);
        }
    }
}
