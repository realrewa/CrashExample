﻿using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class DoubleBufferedEntitiesToAdd<T> where T : FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>, new()
        {
            readonly T _entityViewsToAddBufferA = new T();
            readonly T _entityViewsToAddBufferB = new T();

            internal DoubleBufferedEntitiesToAdd()
            {
                this.other = _entityViewsToAddBufferA;
                this.current = _entityViewsToAddBufferB;
            }

            internal T other;
            internal T current;
            
            internal void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }

            public void ClearOther()
            {
                foreach (var item in other)
                {
                    foreach (var subitem in item.Value)
                    {
                        subitem.Value.Clear();
                    }
                    
                    item.Value.Clear();
                }
                
                other.Clear();
            }
        }
    }
}