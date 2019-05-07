using GameCode.ECS.MainContext.Components;
using GameCode.ECS.MainContext.Other;

namespace GameCode.ECS.MainContext.Implementors
{
    public class ExampleImplementor : IExampleComponent
    {
        public ExampleEntityState State { get; set; } = ExampleEntityState.State_1;
    }
}