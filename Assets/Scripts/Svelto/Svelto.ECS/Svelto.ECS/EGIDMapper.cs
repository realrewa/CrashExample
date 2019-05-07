using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : IEntityStruct
    {
        internal TypeSafeDictionary<T> map;

        public T[] entities(EGID id, out uint index)
        {
                int count;
                index = map.FindElementIndex(id.entityID); 
                return map.GetValuesArray(out count);
        }
        
        public T[] TryFind(EGID id, out uint index)
        {
            return TryFind(id.entityID, out index);
        }
        
        public T[] TryFind(int id, out uint index)
        {
            if (map.TryFindElementIndex(id, out index))
            {
                int count;
                return map.GetValuesArray(out count);
            }

            return null;
        }
    }
}