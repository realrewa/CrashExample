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
        
        private int _uselessIterations = 0;

        public ExampleEngine2(IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
        }
        public void Ready()
        {
            Update().RunOnScheduler(Schedulers.exampleScheduler);
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
                    DoSomeWork(entityViews[i]);
                }
                
                yield return null;
            }
        }

        private void DoSomeWork(ExampleEntityView entityView)
        {
            if (entityView.ExampleComponent.State != ExampleEntityState.State_2)
            {
                _uselessIterations++;
                return;
            }

            if (_uselessIterations > 0)
            {
                Debug.Log($"{_uselessIterations} useless iterations in engine 2");
                _uselessIterations = 0;
            }
            entityView.ExampleComponent.State = ExampleEntityState.State_1;
            // without this ^ we get a "Only one entity operation per submission is allowed" while running on MultiThreadRunner
            
            //
            //work
            //
            
            Debug.Log($"Working in engine 2");
            
            _entityFunctions.SwapEntityGroup<ExampleEntityDescriptor>(entityView.ID, ExampleGroups.ExampleGroup1);
        }
        
    }
}