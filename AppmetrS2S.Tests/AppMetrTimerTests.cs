using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace AppmetrS2S.Tests
{
    public class AppMetrTimerTests
    {
        [Fact]
        public void ItShouldNotFallOnUnhandledException()
        {
            var steps = new Queue<Action>();
            var timer = new AppMetrTimer(1, () =>
            {
                if (steps.Count == 0) return;
                var step = steps.Dequeue();
                step();
            });

            var waitAllSteps = new ManualResetEvent(false);
            var execuptionsCount = 0;
            steps.Enqueue(() => execuptionsCount++);
            steps.Enqueue(() => execuptionsCount++);
            steps.Enqueue(() => { throw new InvalidOperationException(); });
            steps.Enqueue(() => execuptionsCount++);
            steps.Enqueue(() => waitAllSteps.Set());

            new Thread(() => timer.Start()).Start();
            waitAllSteps.WaitOne(500);
            timer.Stop();

            Assert.Equal(3, execuptionsCount);
        }
    }
}