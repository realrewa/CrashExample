﻿﻿﻿using System;
using System.Collections.Generic;
 
namespace Svelto.ECS
{
    /// <summary>
    /// Exclusive Groups guarantee that the GroupID is unique.
    ///
    /// The best way to use it is like:
    ///
    /// public static class MyExclusiveGroups //(can be as many as you want)
    /// {
    ///     public static ExclusiveGroup MyExclusiveGroup1 = new ExclusiveGroup();
    ///
    ///     public static ExclusiveGroup[] GroupOfGroups = { MyExclusiveGroup1, ...}; //for each on this!
    /// }
    /// </summary>
    ///
    public class ExclusiveGroup
    {
        public ExclusiveGroup()
        {
            _group = ExclusiveGroupStruct.Generate();
        }
        
        public ExclusiveGroup(ushort range)
        {
            _group = new ExclusiveGroupStruct(range);
        }
        
        public static implicit operator ExclusiveGroupStruct (ExclusiveGroup group)
        {
            return group._group;
        }
        
        public static explicit operator int (ExclusiveGroup group) 
        {
            return @group._group;
        }

        public static ExclusiveGroupStruct operator + (ExclusiveGroup a, int b)
        {
            return a._group + b;
        }

        readonly ExclusiveGroupStruct _group;
        
        //I use this as parameter because it must not be possible to pass null Exclusive Groups.
        public struct ExclusiveGroupStruct : IEquatable<ExclusiveGroupStruct>, IComparable<ExclusiveGroupStruct>,
                                IEqualityComparer<ExclusiveGroupStruct>
        {
            public static bool operator == (ExclusiveGroupStruct c1, ExclusiveGroupStruct c2)
            {
                return c1.Equals(c2);
            }

            public static bool operator != (ExclusiveGroupStruct c1, ExclusiveGroupStruct c2)
            {
                return c1.Equals(c2) == false;
            }

            public bool Equals (ExclusiveGroupStruct other)
            {
                return other._id == _id;
            }

            public int CompareTo (ExclusiveGroupStruct other)
            {
                return other._id.CompareTo(_id);
            }

            public bool Equals (ExclusiveGroupStruct x, ExclusiveGroupStruct y)
            {
                return x._id == y._id;
            }

            public int GetHashCode (ExclusiveGroupStruct obj)
            {
                return _id.GetHashCode();
            }

            internal static ExclusiveGroupStruct Generate()
            {
                ExclusiveGroupStruct groupStruct;

                groupStruct._id = (int) _globalId;
                DBC.ECS.Check.Require(_globalId + 1 < ushort.MaxValue, "too many exclusive groups created");
                _globalId++;

                return groupStruct;
            }

            /// <summary>
            /// Use this constructor to reserve N groups
            /// </summary>
            public ExclusiveGroupStruct(ushort range)
            {
                _id = (int) _globalId;
                DBC.ECS.Check.Require(_globalId + range < ushort.MaxValue, "too many exclusive groups created");
                _globalId += range;
            }

            public static implicit operator int(ExclusiveGroupStruct groupStruct)
            {
                return groupStruct._id;
            }
            
            public static ExclusiveGroupStruct operator+(ExclusiveGroupStruct a, int b)
            {
                var group = new ExclusiveGroupStruct();

                group._id = a._id + b;

                return group;
            }

            int         _id;
            static uint _globalId;
        }
    }
}