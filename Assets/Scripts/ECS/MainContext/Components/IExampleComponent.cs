using GameCode.ECS.MainContext.Other;

namespace GameCode.ECS.MainContext.Components
{
    public interface IExampleComponent
    {
        ExampleEntityState State { get; set; }
    }
}