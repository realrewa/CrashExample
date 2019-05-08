using System;
using System.Collections;
using GameCode.ECS.MainContext.EntityDescriptors;
using GameCode.ECS.MainContext.EntityViews;
using GameCode.ECS.MainContext.Other;
using Svelto.ECS;
using UnityEngine;

namespace GameCode.ECS.MainContext.Engines
{
    public class ExampleEngine1 : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }
        
        private readonly IEntityFunctions _entityFunctions;

        public ExampleEngine1(IEntityFunctions entityFunctions)
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
                var entityViews = entitiesDB.QueryEntities<ExampleEntityView>(ExampleGroups.ExampleGroup1, out var count);
                if (count == 0)
                {
                    yield return null;
                }
                
                for (var i = 0; i < count; i++)
                {
                    switch (entityViews[i].ExampleComponent.State)
                    {
                        case ExampleEntityState.State_1:
                            entityViews[i].ExampleComponent.State = ExampleEntityState.Processing;
                            DoSomeWork(entityViews[i]).RunOnScheduler(Schedulers.exampleSchedulerMultiThread);
                            break;
                        case ExampleEntityState.Completed:
                            entityViews[i].ExampleComponent.State = ExampleEntityState.State_2;
                            _entityFunctions.SwapEntityGroup<ExampleEntityDescriptor>(entityViews[i].ID, ExampleGroups.ExampleGroup2);
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
            
            Debug.Log("Working in engine 1");
            
            yield break;
        }
        
    }
}