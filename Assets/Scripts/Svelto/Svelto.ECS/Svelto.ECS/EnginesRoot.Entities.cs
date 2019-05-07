﻿﻿using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot: IDisposable
    {
        /// <summary>
        /// Dispose an EngineRoot once not used anymore, so that all the
        /// engines are notified with the entities removed.
        /// It's a clean up process.
        /// </summary>
        public void Dispose()
        {
            foreach (var groups in _groupEntityDB)
                foreach (var entityList in groups.Value)
                    entityList.Value.RemoveEntitiesFromEngines(_entityEngines);
            
            foreach (var engine in _disposableEngines)
                engine.Dispose();
            
            GC.SuppressFinalize(this);
        }

        ~EnginesRoot()
        {
            Dispose();
        }

        ///--------------------------------------------

        public IEntityFactory GenerateEntityFactory()
        {
            return new GenericEntityFactory(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        public IEntityFunctions GenerateEntityFunctions()
        {
            return new GenericEntityFunctions(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        ///--------------------------------------------

        EntityStructInitializer BuildEntity<T>(EGID entityID, object[] implementors)
            where T : IEntityDescriptor, new()
        {
            return BuildEntity(entityID, EntityDescriptorTemplate<T>.descriptor, implementors);
        }

        EntityStructInitializer BuildEntity<T>(EGID entityID, 
                                T entityDescriptor,
                                object[] implementors) where T:IEntityDescriptor
        {
            var descriptorEntitiesToBuild = entityDescriptor.entitiesToBuild;
            
            CheckAddEntityID(entityID, entityDescriptor);

            var dic = EntityFactory.BuildGroupedEntities(entityID,
                                                  _groupedEntityToAdd.current,
                                                   descriptorEntitiesToBuild,
                                                   implementors);
            
            return new EntityStructInitializer(entityID, dic);
        }
      
        ///--------------------------------------------

        void Preallocate<T>(int groupID, int size) where T : IEntityDescriptor, new()
        {
            var entityViewsToBuild = EntityDescriptorTemplate<T>.descriptor.entitiesToBuild;
            var count              = entityViewsToBuild.Length;

            //reserve space in the database
            Dictionary<Type, ITypeSafeDictionary> @group;
            if (_groupEntityDB.TryGetValue(groupID, out group) == false)
                group = _groupEntityDB[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

            //reserve space in building buffer
            Dictionary<Type, ITypeSafeDictionary> @groupBuffer;
            if (_groupedEntityToAdd.current.TryGetValue(groupID, out @groupBuffer) == false)
                @groupBuffer = _groupedEntityToAdd.current[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

            ITypeSafeDictionary dbList;

            for (var index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityType();

                if (group.TryGetValue(entityViewType, out dbList) == false)
                    group[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);
                
                if (@groupBuffer.TryGetValue(entityViewType, out dbList) == false)
                    @groupBuffer[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);
            }
        }
        
        ///--------------------------------------------
        /// 
        void MoveEntity(IEntityBuilder[] entityBuilders, EGID entityGID, Type originalDescriptorType, int toGroupID = -1,
                        Dictionary<Type, ITypeSafeDictionary> toGroup = null)
        {
            //for each entity view generated by the entity descriptor
            Dictionary<Type, ITypeSafeDictionary> fromGroup;
            if (_groupEntityDB.TryGetValue(entityGID.groupID, out fromGroup) == false)
            {
                throw new ECSException("from group not found eid: ".FastConcat(entityGID.entityID).FastConcat(" group: ").FastConcat(entityGID.groupID));                
            }

            ITypeSafeDictionary entityInfoViewDic; EntityInfoView entityInfoView = default(EntityInfoView);
            
            //Check if there is an EntityInfoView linked to this entity, if so it's a DynamicEntityDescriptor!
            bool correctEntityDescriptorFound = true;
            if (fromGroup.TryGetValue(_entityInfoView, out entityInfoViewDic) == true
                && (entityInfoViewDic as TypeSafeDictionary<EntityInfoView>).TryGetValue
                    (entityGID.entityID, out entityInfoView) == true &&
                //I really need to improve this:
                (correctEntityDescriptorFound = entityInfoView.type == originalDescriptorType) == true)
            {
                var entitiesToMove = entityInfoView.entitiesToBuild;
                
                for (int i = 0; i < entitiesToMove.Length; i++)
                    MoveEntityView(entityGID, toGroupID, toGroup, fromGroup, entitiesToMove[i].GetEntityType());
            }
            //otherwise it's a normal static entity descriptor
            else
            {
                if (correctEntityDescriptorFound == false)
                    Utilities.Console.LogError(INVALID_DYNAMIC_DESCRIPTOR_ERROR.FastConcat(entityGID.entityID)
                                               .FastConcat(" group ID ").FastConcat(entityGID.groupID).FastConcat(
                                                " descriptor found: ", entityInfoView.type.Name, " descriptor Excepted ",
                                                originalDescriptorType.Name));
                
                for (var i = 0; i < entityBuilders.Length; i++)
                    MoveEntityView(entityGID, toGroupID, toGroup, fromGroup, entityBuilders[i].GetEntityType());
            }
        }

        void MoveEntityView(EGID entityGID, int toGroupID, Dictionary<Type, ITypeSafeDictionary> toGroup, 
                            Dictionary<Type, ITypeSafeDictionary> fromGroup, Type entityType)
        {
            ITypeSafeDictionary fromTypeSafeDictionary;
            if (fromGroup.TryGetValue(entityType, out fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no entities in from group eid: ".FastConcat(entityGID.entityID).FastConcat(" group: ").FastConcat(entityGID.groupID));                
            }
            
            ITypeSafeDictionary dictionaryOfEntities         = null;

            //in case we want to move to a new group, otherwise is just a remove
            if (toGroup != null)
            {
                if (toGroup.TryGetValue(entityType, out dictionaryOfEntities) == false)
                {
                    dictionaryOfEntities = fromTypeSafeDictionary.Create();
                    toGroup.Add(entityType, dictionaryOfEntities);
                }

                FasterDictionary<int, ITypeSafeDictionary> groupedGroup;
                if (_groupsPerEntity.TryGetValue(entityType, out groupedGroup) == false)
                    groupedGroup = _groupsPerEntity[entityType] = new FasterDictionary<int, ITypeSafeDictionary>();
                
                groupedGroup[toGroupID] = dictionaryOfEntities;
            }

            if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
            {
                throw new ECSException("entity not found eid: ".FastConcat(entityGID.entityID).FastConcat(" group: ").FastConcat(entityGID.groupID));                
            }
            fromTypeSafeDictionary.MoveEntityFromDictionaryAndEngines(entityGID, toGroupID, dictionaryOfEntities, _entityEngines);

            if (fromTypeSafeDictionary.Count == 0) //clean up
            {
                _groupsPerEntity[entityType].Remove(entityGID.groupID);

                //I don't remove the group if empty on purpose, in case it needs to be reused
                //however I trim it to save memory
                fromTypeSafeDictionary.Trim();
            }
        }

        void RemoveGroupAndEntitiesFromDB(int groupID)
        {
            var dictionariesOfEntities = _groupEntityDB[groupID];
            foreach (var dictionaryOfEntities in dictionariesOfEntities)
            {
                dictionaryOfEntities.Value.RemoveEntitiesFromEngines(_entityEngines);
                var groupedGroupOfEntities = _groupsPerEntity[dictionaryOfEntities.Key];
                groupedGroupOfEntities.Remove(groupID);
            }

            //careful, in this case I assume you really don't want to use this group anymore
            //so I remove it from the database
            _groupEntityDB.Remove(groupID);
        }

        ///--------------------------------------------

        void SwapEntityGroup(IEntityBuilder[] builders, Type originalEntityDescriptor, int entityID, int fromGroupID, int toGroupID)
        {
            DBC.ECS.Check.Require(fromGroupID != toGroupID, "the entity is already in this group");

            Dictionary<Type, ITypeSafeDictionary> toGroup;

            if (_groupEntityDB.TryGetValue(toGroupID, out toGroup) == false)
                toGroup = _groupEntityDB[toGroupID] = new Dictionary<Type, ITypeSafeDictionary>();

            MoveEntity(builders, new EGID(entityID, fromGroupID), originalEntityDescriptor, toGroupID, toGroup);
        }
        
        readonly Type  _entityInfoView = typeof(EntityInfoView);
        const string INVALID_DYNAMIC_DESCRIPTOR_ERROR = "Found an entity requesting an invalid dynamic descriptor, this "   +
                                                        "can happen only if you are building different entities with the " +
                                                        "same ID in the same group! id: ";
    }

    public struct EntityStructInitializer
    {
        public EntityStructInitializer(EGID id, Dictionary<Type, ITypeSafeDictionary> current)
        {
            _current = current;
            _id = id;
        }

        public void Init<T>(T initializer) where T: struct, IEntityStruct
        {
            var typeSafeDictionary = (TypeSafeDictionary<T>) _current[typeof(T)];

            initializer.ID = _id;

            int count;
            typeSafeDictionary.GetValuesArray(out count)[typeSafeDictionary.FindElementIndex(_id.entityID)] = initializer;
        }

        readonly Dictionary<Type, ITypeSafeDictionary> _current;
        readonly EGID                                  _id;
    }
}