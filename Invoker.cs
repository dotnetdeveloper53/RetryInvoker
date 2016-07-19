using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RetryInvoker
{
    public static class Invoker
    {
        public interface IInterval
        {
            void Sleep();
        }

        class Interval : IInterval
        {
            TimeSpan _interval;
            public Interval(TimeSpan interval) { _interval = interval; }
            public void Sleep()
            {
                Thread.Sleep(_interval);
            }
        }

        public static bool TryInvoke<T>(Func<T> del, out T value, int attempts = 3)
        {
            IList<Exception> exceptions;
            return TryInvoke<Exception, T>(del, out value, out exceptions, attempts);
        }

        public static bool TryInvoke<T>(Func<T> del, out T value, out IList<Exception> exceptions, int attempts = 3)
        {
            return TryInvoke<Exception, T>(del, out value, out exceptions, attempts);
        }

        public static bool TryInvoke(Action del, int attempts = 3)
        {
            if (del == null) throw new ArgumentNullException("del");

            IList<Exception> exceptions;
            object value;
            return TryInvoke<Exception, object>(() =>
            {
                del();
                return null;
            }, out value, out exceptions, attempts);
        }

        public static bool TryInvoke<TException>(Action del, out IList<Exception> exceptions, int attempts = 3)
            where TException : Exception
        {
            object value;
            return TryInvoke<TException, object>(() =>
            {
                del();
                return null;
            }, out value, out exceptions, attempts);
        }

        public static bool TryInvoke<TException, T>(Func<T> del, out T value, out IList<Exception> exceptions, TimeSpan retryInterval, int attempts = 3)
            where TException : Exception
        {
            return TryInvoke<TException, T>(del, out value, out exceptions, attempts, new Interval(retryInterval));
        }

        public static bool TryInvoke<TException, T>(Func<T> del, out T value, out IList<Exception> exceptions, int attempts = 3, IInterval interval = null)
            where TException : Exception
        {
            if (del == null) throw new ArgumentNullException("del");
            if (attempts <= 0) throw new ArgumentOutOfRangeException("attempts");

            exceptions = new List<Exception>();

            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    value = del();
                    return true;
                }
                catch (TException ex)
                {
                    exceptions.Add(ex);
                    if (interval != null)
                        interval.Sleep();
                }
            }

            value = default(T);
            return false;
        }
    }
}
