using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pulsar4X.Datablobs;
using Pulsar4X.DataStructures;
using Pulsar4X.Engine;
using Pulsar4X.Engine.Orders;
using Pulsar4X.Extensions;
using Pulsar4X.Factions;
using Pulsar4X.Fleets;
using Pulsar4X.Interfaces;
using Pulsar4X.Names;
using Pulsar4X.Ships;

namespace Pulsar4X.Storage
{
    public class CargoLoadFromOrder : EntityCommand
    {
        public CargoUnloadToOrder Order;

        public override ActionLaneTypes ActionLanes { get; } = ActionLaneTypes.Movement | ActionLaneTypes.InteractWithExternalEntity;

        public override bool IsBlocking { get; } = true;

        public override string Name { get; } = "Cargo Transfer";

        public override string Details
        {
            get
            {
                string fromEntityName = _cargoFrom.GetDataBlob<NameDB>().GetName(_factionEntity);
                string toEntityName = _entityCommanding.GetDataBlob<NameDB>().GetName(_factionEntity);
                string detailStr = "From " + fromEntityName + " To " + toEntityName;
                return detailStr;
            }
        }

        Entity _cargoFrom;

        internal override Entity EntityCommanding { get{return _entityCommanding;} }
        Entity _entityCommanding;
        Entity _factionEntity;

        internal override bool IsValidCommand(Game game)
        {
            if (CommandHelpers.IsCommandValid(game.GlobalManager, RequestingFactionGuid, EntityCommandingGuid, out _factionEntity, out _entityCommanding))
            {
                return true;
            }
            return false;
        }

        public static void CreateCommand(int faction, Entity cargoFromEntity, Entity cargoToEntity, List<(ICargoable item, long amount)> itemsToMove )
        {
            var itemGuidAmounts = new List<(string, long)>();

            foreach (var tup in itemsToMove)
            {
                itemGuidAmounts.Add((tup.item.UniqueID, tup.amount));
            }

            var unloadcmd = new CargoUnloadToOrder()
            {
                RequestingFactionGuid = faction,
                EntityCommandingGuid = cargoFromEntity.Id,
                CreatedDate = cargoFromEntity.Manager.ManagerSubpulses.StarSysDateTime,
                SendCargoToEntityGuid = cargoToEntity.Id,
                ItemsGuidsToTransfer = itemGuidAmounts,
                ItemICargoablesToTransfer = itemsToMove
            };
            cargoFromEntity.Manager.Game.OrderHandler.HandleOrder(unloadcmd);

            var loadCmd = new CargoLoadFromOrder()
            {
                RequestingFactionGuid = faction,
                EntityCommandingGuid = cargoToEntity.Id,
                CreatedDate = cargoFromEntity.Manager.ManagerSubpulses.StarSysDateTime,
                Order = unloadcmd,
                _cargoFrom = cargoFromEntity
            };

            cargoFromEntity.Manager.Game.OrderHandler.HandleOrder(loadCmd);
        }


        public static void CreateRefuelFleetCommand(Entity cargoFromEntity, Entity fleet)
        {
            var fleetOwner = fleet.GetFactionOwner;
            var cargoLibrary = fleetOwner.GetDataBlob<FactionInfoDB>().Data.CargoGoods;
            if(fleet.TryGetDatablob<FleetDB>(out var fleetDB))
            {
                var ships = fleetDB.Children.Where(c => c.HasDataBlob<ShipInfoDB>());

                foreach (var ship in ships)
                {
                    var fuelInfo = ship.GetFuelInfo(cargoLibrary);
                    ICargoable fuel = fuelInfo.Item1;
                    long amountToMove = ship.GetDataBlob<VolumeStorageDB>().GetFreeUnitSpace(fuel);
                    var fuelAndAmount =(fuel, amountToMove);
                    var list = new List<(ICargoable, long)>();
                    list.Add(fuelAndAmount);
                    var unloadcmd = new CargoUnloadToOrder()
                    {
                        RequestingFactionGuid = fleetOwner.Id,
                        EntityCommandingGuid = cargoFromEntity.Id,
                        CreatedDate = cargoFromEntity.Manager.ManagerSubpulses.StarSysDateTime,
                        SendCargoToEntityGuid = ship.Id,
                        ItemICargoablesToTransfer = list
                    };
                    cargoFromEntity.Manager.Game.OrderHandler.HandleOrder(unloadcmd);

                    var loadCmd = new CargoLoadFromOrder()
                    {
                        RequestingFactionGuid = fleetOwner.Id,
                        EntityCommandingGuid = ship.Id,
                        CreatedDate = cargoFromEntity.Manager.ManagerSubpulses.StarSysDateTime,
                        Order = unloadcmd,
                        _cargoFrom = cargoFromEntity
                    };

                    cargoFromEntity.Manager.Game.OrderHandler.HandleOrder(loadCmd);
                }
            }
        }
        
        internal override void Execute(DateTime atDateTime)
        {
            //this needs to happen on a given trigger,ie a finished move command.

            Order.Execute(atDateTime);
        }

        public override bool IsFinished()
        {
            return Order.IsFinished();
        }

        public override EntityCommand Clone()
        {
            throw new NotImplementedException();
        }
    }
}