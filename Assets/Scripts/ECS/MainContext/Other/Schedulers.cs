using Svelto.Tasks;
using Svelto.Tasks.Unity;

namespace GameCode.ECS.MainContext.Other
{
    public class Schedulers
    {
        public static readonly CoroutineMonoRunner exampleSchedulerMainThread = new CoroutineMonoRunner("ExampleSchedulerMainThread");
        public static readonly MultiThreadRunner exampleSchedulerMultiThread = new MultiThreadRunner("ExampleSchedulerMultiThread", 0.016F);

        public static void Stop()
        {
            exampleSchedulerMainThread.Dispose();
            exampleSchedulerMultiThread.Dispose();
        }
    }
}