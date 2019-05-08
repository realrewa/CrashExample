using System.Collections;
using GameCode.ECS.MainContext.EntityDescriptors;
using GameCode.ECS.MainContext.EntityViews;
using GameCode.ECS.MainContext.Other;
using Svelto.ECS;
using UnityEngine;

namespace GameCode.ECS.MainContext.Engines
{
    public class ExampleEngine2 : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }
        
        private readonly IEntityFunctions _entityFunctions;

        public ExampleEngine2(IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
        }
        public void Ready()
        {
            Update().RunOnScheduler(Schedulers.exampleSchedulerMainThread);
        }

        private IEnumerator Update()
        {
            while (true)
            {
                var entityViews = entitiesDB.QueryEntities<ExampleEntityView>(ExampleGroups.ExampleGroup2, out var count);
                if (count == 0)
                {
                    yield return null;
                }
                
                for (var i = 0; i < count; i++)
                {
                    switch (entityViews[i].ExampleComponent.State)
                    {
                        case ExampleEntityState.State_2:
                            entityViews[i].ExampleComponent.State = ExampleEntityState.Processing;
                            DoSomeWork(entityViews[i]).RunOnScheduler(Schedulers.exampleSchedulerMultiThread);
                            break;
                        case ExampleEntityState.Completed:
                            entityViews[i].ExampleComponent.State = ExampleEntityState.State_1;
                            _entityFunctions.SwapEntityGroup<ExampleEntityDescriptor>(entityViews[i].ID, ExampleGroups.ExampleGroup1);
                            break;
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator DoSomeWork(ExampleEntityView entityView)
        {
            entityView.ExampleComponent.State = ExampleEntityState.Completed;

            //
            //work
            //
            
            Debug.Log("Working in engine 2");
            
            yield break;
        }
        
    }
}