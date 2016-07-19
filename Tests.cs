using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace RetryInvoker.Tests
{
    [TestClass]
    public class InvokeRetrierTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryInvoke_NullActionDelegate_ThrowsArgumentNullException()
        {
            Invoker.TryInvoke(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryInvoke_NullFuncDelegate_ThrowsArgumentNullException()
        {
            int value;
            Invoker.TryInvoke<int>(null, out value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TryInvoke_AttemptsEqualToZero_ThrowsArgumentOutOfRangeException()
        {
            Invoker.TryInvoke(() => { }, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TryInvoke_AttemptsLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            Invoker.TryInvoke(() => { }, -1);
        }

        [TestMethod]
        public void TryInvoke_DelAlwaysFails_ReturnsFalse()
        {
            IList<Exception> exs;
            bool ret = Invoker.TryInvoke<Exception>(() => { throw new Exception(); }, out exs, 1);

            Assert.IsFalse(ret);
        }

        static int i = 0;
        [TestMethod]
        public void TryInvoke_FailsFirstPassesSecond_ReturnsTrue()
        {
            IList<Exception> exs;
            bool ret = Invoker.TryInvoke<Exception>(() => { if (i++ == 0) throw new Exception(); }, out exs, 2);

            Assert.IsTrue(ret);
            Assert.IsNotNull(exs);
            Assert.AreEqual(1, exs.Count); //one exception for the first failed attempt
        }

        static int j = 0;
        [TestMethod]
        public void TryInvoke_DelThrowsExceptionAndAlwaysFails_ReturnsFalseAndProvidesListOfExceptions()
        {
            IList<Exception> exs;
            bool ret = Invoker.TryInvoke<Exception>(() => { j++;  throw new Exception(); }, out exs, 3);

            Assert.IsFalse(ret);
            Assert.IsNotNull(exs);
            Assert.AreEqual(3, exs.Count);  //3 failed attempts therefore should contain exceptions
            Assert.AreEqual(3, j);  //check delegate is called 3 times
        }


        [TestMethod]
        public void TryInvoke_DelThrowsUnhandledException_FailsSubsequentAttemptsAndExpectsException()
        {
            IList<Exception> exs = null;
            bool ret = false;
            try
            {
                ret = Invoker.TryInvoke<ArgumentOutOfRangeException>(() => { throw new ArgumentNullException(); }, out exs, 3);
            }
            catch (ArgumentNullException) { }
            catch (Exception)
            {
                Assert.Fail();
            }

            Assert.IsFalse(ret);
            Assert.IsNotNull(exs);
            Assert.AreEqual(0, exs.Count);

        }

        class MockSleeper : Invoker.IInterval
        {
            public int SleepMethodCount;

            public void Sleep()
            {
                this.SleepMethodCount++;
            }
        }

        [TestMethod]
        public void TryInvoke_DelFails_SleeperIsCalledExpectedTimes()
        {
            MockSleeper mockSleeper = new MockSleeper();
            IList<Exception> exs;
            object value;
            Invoker.TryInvoke<Exception, object>(() => { throw new Exception(); return null; }, out value, out exs, 3, mockSleeper);

            Assert.AreEqual(3, mockSleeper.SleepMethodCount);
        }
    }
}
