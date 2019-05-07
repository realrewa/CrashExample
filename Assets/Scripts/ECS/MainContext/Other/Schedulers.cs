using Svelto.Tasks;
using Svelto.Tasks.Unity;

namespace GameCode.ECS.MainContext.Other
{
    public class Schedulers
    {
//        public static readonly CoroutineMonoRunner exampleScheduler = new CoroutineMonoRunner("ExampleScheduler");
        public static readonly MultiThreadRunner exampleScheduler = new MultiThreadRunner("ExampleScheduler", true);

        public static void Stop()
        {
            exampleScheduler.Dispose();
        }
    }
}