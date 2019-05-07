using GameCode.ECS.MainContext.Engines;
using GameCode.ECS.MainContext.EntityDescriptors;
using GameCode.ECS.MainContext.Implementors;
using GameCode.ECS.MainContext.Other;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Schedulers.Unity;
using Svelto.Tasks;

namespace GameCode.ECS.MainContext
{
    public class MainCompositionRoot : ICompositionRoot
    {
        EnginesRoot                    _enginesRoot;
        IEntityFactory                 _entityFactory;
        UnityEntitySubmissionScheduler _unityEntitySubmissionScheduler;

        public MainCompositionRoot()
        {
            SetupEngines();
            SetupEntities();
        }

        public void OnContextCreated<T>(T contextHolder) {  }
        public void OnContextInitialized() {}

        public void OnContextDestroyed()
        {
            _enginesRoot.Dispose();
            TaskRunner.StopAndCleanupAllDefaultSchedulers();
            Schedulers.Stop();
        }
        
        void SetupEngines()
        {
            _unityEntitySubmissionScheduler = new UnityEntitySubmissionScheduler();
            _enginesRoot                    = new EnginesRoot(_unityEntitySubmissionScheduler);
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            _enginesRoot.AddEngine(new ExampleEngine1(entityFunctions));
            _enginesRoot.AddEngine(new ExampleEngine2(entityFunctions));
        }

        void SetupEntities()
        {
            _entityFactory.BuildEntity<ExampleEntityDescriptor>(1, ExampleGroups.ExampleGroup1, new object[]{ new ExampleImplementor() });
        }
    }
}