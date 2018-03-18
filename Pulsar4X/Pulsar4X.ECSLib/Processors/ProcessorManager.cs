﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pulsar4X.ECSLib
{
    //consdisder making this a singleton
    internal class ProcessorManager
    {

        private readonly Dictionary<Type, IHotloopProcessor> _hotloopProcessors = new Dictionary<Type, IHotloopProcessor>();
        private readonly List<IRecalcProcessor> _recalcProcessors = new List<IRecalcProcessor>();
        //private readonly Dictionary<PulseActionEnum, IHotloopProcessor> _hotloopProcessorsByEnum = new Dictionary<PulseActionEnum, IHotloopProcessor>();
        private readonly Dictionary<string, IInstanceProcessor> _instanceProcessors = new Dictionary<string, IInstanceProcessor>();
        private StaticDataStore _staticData;
        internal ProcessorManager(Game game)
        {
            _staticData = game.StaticData;
            CreateProcessors(game);
        }

        internal void AddHotloopProcessor<T>(IHotloopProcessor processor) where T : BaseDataBlob
        {
            _hotloopProcessors.Add(typeof(T), processor);
        }

        internal void AddRecalcProcessor(IRecalcProcessor processor)
        {
            if (processor.ProcessPriority <= _recalcProcessors.Count)
                _recalcProcessors.Insert(processor.ProcessPriority, processor);
            else
                _recalcProcessors.Add(processor);
        }

        internal void Hotloop<T>(EntityManager manager, int deltaSeconds) where T : BaseDataBlob
        {
            var type = typeof(T);

            _hotloopProcessors[type].ProcessManager(manager, deltaSeconds);
        }

        internal IHotloopProcessor GetProcessor<T>() where T : BaseDataBlob
        {
            return _hotloopProcessors[typeof(T)];
        }

        internal void RunProcessOnEntity<T>(Entity entity, int deltaSeconds)
            where T : BaseDataBlob
        {
            var type = typeof(T);
            _hotloopProcessors[type].ProcessEntity(entity, deltaSeconds);
        }

        internal void RecalcEntity(Entity entity)
        {
            foreach (var processor in _recalcProcessors)
            {
                processor.RecalcEntity(entity);
            }
        }

        internal IInstanceProcessor GetInstanceProcessor(string typeName)
        {
            return _instanceProcessors[typeName];
        }


        internal void InitializeMangerSubpulse(EntityManager _entityManager)
        {
            ManagerSubPulse managerSubPulse = _entityManager.ManagerSubpulses;

            /*
            //we offset some of these to spread the load out a bit more. 
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime, GetProcessor<OrbitDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime, GetProcessor<NewtonBalisticDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime, GetProcessor<EntityResearchDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime + TimeSpan.FromMinutes(5), GetProcessor<OrderableDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime + TimeSpan.FromMinutes(10), GetProcessor<TranslateMoveDB>());
            //AddSystemInterupt(_entityManager.Game.CurrentDateTime + TimeSpan.FromMinutes(10.1), _processManager.GetProcessor<SensorProfileDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime + TimeSpan.FromHours(1), GetProcessor<MiningDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime + TimeSpan.FromHours(2), GetProcessor<RefiningDB>());
            managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime + TimeSpan.FromHours(3), GetProcessor<ConstructionDB>());
            */

            foreach (var item in _hotloopProcessors)
            {
                managerSubPulse.AddSystemInterupt(_entityManager.Game.CurrentDateTime + item.Value.FirstRunOffset, item.Value);
            }
        }

        private void CreateProcessors(Game game)
        {
            /*
            AddHotloopProcessor<EntityResearchDB>(new ResearchProcessor(game.StaticData));
            AddHotloopProcessor<MiningDB>(new MineResourcesProcessor(_staticData));
            AddHotloopProcessor<RefiningDB>(new RefineResourcesProcessor(_staticData.ProcessedMaterials));
            AddHotloopProcessor<ConstructionDB>(new ConstructEntitiesProcessor());
            AddHotloopProcessor<PropulsionDB>(new ShipMovement());
            AddHotloopProcessor<OrbitDB>(new OrbitProcessor());
            AddHotloopProcessor<NewtonBalisticDB>(new NewtonBalisticProcessor());
            AddHotloopProcessor<OrderableDB>(new OrderableProcessor(game));
            AddHotloopProcessor<TranslateMoveDB>(new TranslateMoveProcessor());
            //AddHotloopProcessor<SensorProfileDB>(new SetReflectedEMProfile());
            */

            var hotloopTypes = GetDerivedTypesFor(typeof(IHotloopProcessor));
            foreach (var hotloopType in hotloopTypes)
            {  
                IHotloopProcessor processor = (IHotloopProcessor)Activator.CreateInstance(hotloopType);
                processor.Init(game);
                Type type = processor.GetParameterType;
                _hotloopProcessors.Add(type, processor);
            }

            var instanceTypes = GetDerivedTypesFor(typeof(IInstanceProcessor));
            foreach (var itemType in instanceTypes)
            {
                IInstanceProcessor processor = (IInstanceProcessor)Activator.CreateInstance(itemType);
                _instanceProcessors.Add(processor.TypeName, processor);
            }

            ///AddRecalcProcessor
        }

        private static IEnumerable<Type> GetDerivedTypesFor(Type baseType)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                .Where(baseType.IsAssignableFrom)
                .Where(t => baseType != t);
        }
    }

    /// <summary>
    /// Hotloop processor - this proccessor is fired at a specific regular time interval
    /// </summary>
    internal interface IHotloopProcessor
    {
        void Init(Game game);
        void ProcessEntity(Entity entity, int deltaSeconds);
        void ProcessManager(EntityManager manager, int deltaSeconds);
        TimeSpan RunFrequency { get; }
        TimeSpan FirstRunOffset { get; }
        Type GetParameterType { get; }
    }


    /// <summary>
    /// Instance processor. - This processor is fired at a specific timedate or on command. 
    /// </summary>
    public abstract class IInstanceProcessor
    {
        internal string TypeName { get { return GetType().Name; } }
        internal abstract void ProcessEntity(Entity entity, int deltaSeconds);
    }

    internal abstract class AInstanceProcessor
    {
        
    }

    /// <summary>
    /// Recalc processor. - this processor is called when something on the entity changes. 
    /// </summary>
    internal interface IRecalcProcessor
    {
        byte ProcessPriority { get; set; }
        void RecalcEntity(Entity entity);
    }
}