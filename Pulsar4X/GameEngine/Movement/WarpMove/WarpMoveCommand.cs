using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.WarpMove;
using Newtonsoft.Json;
using Pulsar4X.Orbital;
using Pulsar4X.Datablobs;
using Pulsar4X.Extensions;

namespace Pulsar4X.Engine.Orders
{
    public class WarpMoveCommand : EntityCommand
    {

        public override string Name
        {
            get
            {
                if(_targetEntity == null || _entityCommanding == null)
                    return "Warp Move";

                return "Warp Move to " + _targetEntity.GetName(_entityCommanding.FactionOwnerID);
            }
        }

        public override string Details
        {
            get
            {
                string targetName = _targetEntity.GetDataBlob<NameDB>().GetName(_factionEntity);
                return "Warp to + " + Stringify.Distance(EndpointRelitivePosition.Length()) + " from " + targetName;
            }
        }

        public override ActionLaneTypes ActionLanes => ActionLaneTypes.Movement;
        public override bool IsBlocking => true;

        [JsonProperty]
        public int TargetEntityGuid { get; set; }

        private Entity _targetEntity;


        [JsonIgnore]
        Entity _factionEntity;
        WarpMovingDB _warpingDB;


        Entity _entityCommanding;
        internal override Entity EntityCommanding { get { return _entityCommanding; } }

        public DateTime TransitStartDateTime;
        public Vector3 EndpointRelitivePosition { get; set; }
        public Vector3 EndpointTargetExpendDeltaV;
        /// <summary>
        /// the orbit we want to be in at the target.
        /// </summary>
        public KeplerElements EndpointTargetOrbit;
        public PositionDB.MoveTypes MoveTypeAtDestination;

        /// <summary>
        /// Creates the transit cmd.
        /// </summary>
        /// <param name="game">Game.</param>
        /// <param name="faction">Faction.</param>
        /// <param name="orderEntity">Order entity.</param>
        /// <param name="targetEntity">Target entity.</param>
        /// <param name="targetOffsetPos_m">Target offset position in au.</param>
        /// <param name="transitStartDatetime">Transit start datetime.</param>
        /// <param name="expendDeltaV">Amount of DV to expend to change the orbit in m/s</param>
        /// /// <param name="mass">mass of ship after warp (needed for DV calc)</param>
        public static (WarpMoveCommand, NewtonThrustCommand?) CreateCommand(
            CargoDefinitionsLibrary cargoLibrary, 
            int faction, 
            Entity orderEntity, 
            Entity targetEntity, 
            Vector3 targetOffsetPos_m, 
            DateTime transitStartDatetime, 
            Vector3 expendDeltaV, 
            double mass)
        {
            var cmd = new WarpMoveCommand()
            {
                RequestingFactionGuid = faction,
                EntityCommandingGuid = orderEntity.Id,
                CreatedDate = orderEntity.Manager.ManagerSubpulses.StarSysDateTime,
                TargetEntityGuid = targetEntity.Id,
                EndpointRelitivePosition = targetOffsetPos_m,
                TransitStartDateTime = transitStartDatetime,
                EndpointTargetExpendDeltaV = expendDeltaV,
            };

            orderEntity.Manager.Game.OrderHandler.HandleOrder(cmd);
            if (expendDeltaV.Length() != 0)
            {

                (Vector3 position, DateTime atDateTime) targetIntercept = WarpMath.GetInterceptPosition
                (
                    orderEntity,
                    targetEntity.GetDataBlob<OrbitDB>(),
                    orderEntity.StarSysDateTime,
                    targetOffsetPos_m
                );

                var burntime = TimeSpan.FromSeconds(OrbitMath.BurnTime(orderEntity, expendDeltaV.Length(), mass));
                var ntcmd = NewtonThrustCommand.CreateCommand(cargoLibrary, orderEntity, expendDeltaV, targetIntercept.atDateTime + burntime);

                return (cmd, ntcmd);
            }

            return (cmd, null);
        }
        public static WarpMoveCommand CreateCommand(
            Entity orderEntity, 
            Entity targetEntity, 
            DateTime transitStartDatetime, 
            Vector3 endpointRelativePos = new Vector3())
        {
            var datetimeArrive = WarpMath.GetInterceptPosition(orderEntity, targetEntity, transitStartDatetime, endpointRelativePos);

            var cmd = new WarpMoveCommand()
            {
                RequestingFactionGuid = orderEntity.FactionOwnerID,
                EntityCommandingGuid = orderEntity.Id,
                CreatedDate = orderEntity.Manager.ManagerSubpulses.StarSysDateTime,
                TargetEntityGuid = targetEntity.Id,
                EndpointRelitivePosition = endpointRelativePos,
                TransitStartDateTime = transitStartDatetime,
            };
            if (targetEntity.GetDataBlob<PositionDB>().MoveType == PositionDB.MoveTypes.None)
            {
                cmd.MoveTypeAtDestination = PositionDB.MoveTypes.None;
            }
            else
            {
                var sgp = GeneralMath.StandardGravitationalParameter(targetEntity.GetDataBlob<MassVolumeDB>().MassTotal + orderEntity.GetDataBlob<MassVolumeDB>().MassTotal);
                cmd.MoveTypeAtDestination = PositionDB.MoveTypes.Orbit;
                cmd.EndpointTargetOrbit = OrbitMath.KeplerCircularFromPosition(sgp, endpointRelativePos, datetimeArrive.Item2);;
            }

            orderEntity.Manager.Game.OrderHandler.HandleOrder(cmd);


            return cmd;
        }
        
        public static WarpMoveCommand CreateCommand(
            Entity orderEntity, 
            Entity targetEntity, 
            DateTime transitStartDatetime, 
            KeplerElements insertonTargetOrbit, 
            Vector3 exitPointRelative)
        {
            var targetOffsetPos_m = exitPointRelative;
            var datetimeArrive = WarpMath.GetInterceptPosition(orderEntity, targetEntity, transitStartDatetime, targetOffsetPos_m);

            var cmd = new WarpMoveCommand()
            {
                RequestingFactionGuid = orderEntity.FactionOwnerID,
                EntityCommandingGuid = orderEntity.Id,
                CreatedDate = orderEntity.Manager.ManagerSubpulses.StarSysDateTime,
                TargetEntityGuid = targetEntity.Id,
                EndpointRelitivePosition = targetOffsetPos_m,
                EndpointTargetOrbit = insertonTargetOrbit,
                TransitStartDateTime = transitStartDatetime,
            };
            if (targetEntity.GetDataBlob<PositionDB>().MoveType == PositionDB.MoveTypes.None)
            {
                cmd.MoveTypeAtDestination = PositionDB.MoveTypes.None;
            }
            else
            {
                var sgp = GeneralMath.StandardGravitationalParameter(targetEntity.GetDataBlob<MassVolumeDB>().MassTotal + orderEntity.GetDataBlob<MassVolumeDB>().MassTotal);
                cmd.MoveTypeAtDestination = PositionDB.MoveTypes.Orbit;
                cmd.EndpointTargetOrbit = OrbitMath.KeplerCircularFromPosition(sgp, targetOffsetPos_m, datetimeArrive.Item2);;
            }

            orderEntity.Manager.Game.OrderHandler.HandleOrder(cmd);


            return cmd;
        }
        
        internal override bool IsValidCommand(Game game)
        {
            if (CommandHelpers.IsCommandValid(game.GlobalManager, RequestingFactionGuid, EntityCommandingGuid, out _factionEntity, out _entityCommanding))
            {
                if (game.GlobalManager.TryGetGlobalEntityById(TargetEntityGuid, out _targetEntity))
                {
                    return true;
                }
            }
            return false;
        }

        internal override void Execute(DateTime atDateTime)
        {
            if (!IsRunning)
            {
                var warpDB = _entityCommanding.GetDataBlob<WarpAbilityDB>();
                var powerDB = _entityCommanding.GetDataBlob<EnergyGenAbilityDB>();
                string eType = warpDB.EnergyType;
                double estored = powerDB.EnergyStored[eType];
                double creationCost = warpDB.BubbleCreationCost;

                // FIXME: alert the player?
                if (creationCost > estored)
                    return;

                _warpingDB = new WarpMovingDB(_entityCommanding, _targetEntity, EndpointRelitivePosition);
                _warpingDB.EndpointTargetOrbit = EndpointTargetOrbit;
                _warpingDB.EndpointTargetExpendDeltaV = EndpointTargetExpendDeltaV;

                EntityCommanding.SetDataBlob(_warpingDB);

                WarpMoveProcessor.StartNonNewtTranslation(EntityCommanding);
                IsRunning = true;

                //debug code:
                double distance = (_warpingDB.EntryPointAbsolute - _warpingDB.ExitPointAbsolute).Length();
                double time = distance / _entityCommanding.GetDataBlob<WarpAbilityDB>().MaxSpeed;
                //Assert.AreEqual((_warpingDB.PredictedExitTime - _warpingDB.EntryDateTime).TotalSeconds, time, 1.0e-10);

            }
        }

        public override bool IsFinished()
        {
            if(_warpingDB != null)
                return _warpingDB.IsAtTarget;
            return false;
        }

        public override EntityCommand Clone()
        {
            throw new NotImplementedException();
        }
    }
    
    public class WarpFleetTowardsTargetOrder : EntityCommand
    {
        public override ActionLaneTypes ActionLanes => ActionLaneTypes.Movement;

        public override bool IsBlocking => true;

        public override string Name => "Move Fleet Towards Target";

        public override string Details => "";

        private Entity _entityCommanding;

        internal override Entity EntityCommanding => _entityCommanding;

        public Entity Target { get; set; }

        List<EntityCommand> _shipCommands = new List<EntityCommand>();

        public override EntityCommand Clone()
        {
            throw new NotImplementedException();
        }

        public override bool IsFinished()
        {
            if(!IsRunning) return false;

            foreach(var command in _shipCommands)
            {
                if(!command.IsFinished())
                    return false;
            }
            return true;
        }

        internal override void Execute(DateTime atDateTime)
        {
            if(IsRunning) return;
            if(!_entityCommanding.TryGetDatablob<FleetDB>(out var fleetDB)) return;
            // Get all the ships we need to add the movement command to
            var ships = fleetDB.Children.Where(c => c.HasDataBlob<ShipInfoDB>());

            
            foreach(var ship in ships)
            {
                var shipCommand = WarpMoveCommand.CreateCommand(ship, Target, atDateTime);

                _shipCommands.Add(shipCommand);
                ship.Manager.Game.OrderHandler.HandleOrder(shipCommand);
            }
            IsRunning = true;
        }

        public static WarpFleetTowardsTargetOrder CreateCommand(Entity entity, Entity target)
        {
            var order = new WarpFleetTowardsTargetOrder()
            {
                RequestingFactionGuid = entity.FactionOwnerID,
                EntityCommandingGuid = entity.Id,
                _entityCommanding = entity,
                Target = target,
            };

            return order;
        }

        internal override bool IsValidCommand(Game game)
        {
            return true;
        }
    }
}
