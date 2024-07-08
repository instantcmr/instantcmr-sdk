using System;
using System.Threading;
using System.Threading.Tasks;

namespace Icmr.Integration
{
    public class StoppableTask
    {
        private readonly Task task;
        private readonly CancellationTokenSource ctoks;

        public StoppableTask(Task task, CancellationTokenSource ctoks)
        {
            this.task = task;
            this.ctoks = ctoks;
        }

        public void StopAsync()
        {
            ctoks.Cancel();
        }

        public async Task Task()
        {
            try
            {
                await task;
            }
            catch (TaskCanceledException) { }
            catch (AggregateException er) when (er.InnerExceptions.Count == 1 && er.InnerException is TaskCanceledException) { }
        }
    }
}