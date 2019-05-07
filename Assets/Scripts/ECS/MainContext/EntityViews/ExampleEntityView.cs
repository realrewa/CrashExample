using GameCode.ECS.MainContext.Components;
using Svelto.ECS;

namespace GameCode.ECS.MainContext.EntityViews
{
    public class ExampleEntityView : IEntityViewStruct
    {
        public EGID ID { get; set; }
        public IExampleComponent ExampleComponent;
    }
}