using System;
using System.Collections.Generic;
using System.Threading;

namespace EventBus{
    internal sealed class LockHelper<K, V>{
        /*
         * Fields
         */
        private readonly ReaderWriterLock rwLock = new ReaderWriterLock();
        private readonly IDictionary<K, V> map;


        /*
         * Constructor
         */
        public LockHelper(IDictionary<K, V> mapIn){
            map = mapIn;
        }


        /*
         * Public methods
         */
        public V Get<I>(K key, Func<I> factory, Func<I, V> finalizer){
            rwLock.AcquireReaderLock(int.MaxValue);
            bool foundValue = map.TryGetValue(key, out V ret);
            rwLock.ReleaseReaderLock();

            if(foundValue)
                return ret;

            I intermediate = factory.Invoke();

            rwLock.AcquireReaderLock(int.MaxValue);
            LockCookie lc = rwLock.UpgradeToWriterLock(int.MaxValue);

            if(!map.TryGetValue(key, out ret)){
                ret = finalizer.Invoke(intermediate);
                map.Add(key, ret);
            }

            rwLock.DowngradeFromWriterLock(ref lc);
            rwLock.ReleaseReaderLock();

            return ret;
        }
    }
}
