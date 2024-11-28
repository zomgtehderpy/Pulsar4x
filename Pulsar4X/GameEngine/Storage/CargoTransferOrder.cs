using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pulsar4X.Engine;
using Pulsar4X.Names;
using Pulsar4X.Engine.Orders;
using Pulsar4X.Extensions;
using Pulsar4X.Factions;
using Pulsar4X.Fleets;
using Pulsar4X.Ships;


namespace Pulsar4X.Storage;

public class CargoTransferOrder : EntityCommand
{
    
    public bool IsPrimaryEntity { get; private set; }

    public override ActionLaneTypes ActionLanes => ActionLaneTypes.Movement | ActionLaneTypes.InteractWithExternalEntity;

    public override bool IsBlocking => true;

    public override string Name { get; } = "Cargo Transfer";

    public override string Details
    {
        get
        {
            string entity1 = _transferData.PrimaryStorageDB.OwningEntity.GetName(RequestingFactionGuid);
            string entity2 = _transferData.SecondaryStorageDB.OwningEntity.GetName(RequestingFactionGuid);
            string detailStr = "Between " + entity1 + " and " + entity2;
            return detailStr;
        }
    }

    Entity _entityCommanding;

    internal override Entity EntityCommanding { get { return _entityCommanding; } }

    private CargoTransferObject _transferData { get; }

    [JsonIgnore]
    Entity factionEntity;
    

    private CargoTransferOrder(CargoTransferObject transferData)
    {
        _transferData = transferData;
    }
    
    public static void CreateCommands(int faction, Entity primaryEntity, Entity secondaryEntity, List<(ICargoable item, long amount)> itemsToMove )
    {
        var itemsList = itemsToMove.AsReadOnly();
        CargoTransferObject cargoData = new(primaryEntity, secondaryEntity, itemsList);
        var cmd1 = new CargoTransferOrder(cargoData)
        {
            RequestingFactionGuid = faction,
            EntityCommandingGuid = primaryEntity.Id,
            CreatedDate = primaryEntity.Manager.ManagerSubpulses.StarSysDateTime,
            IsPrimaryEntity = true,
        };
        primaryEntity.Manager.Game.OrderHandler.HandleOrder(cmd1);
        
        var cmd2 = new CargoTransferOrder(cargoData)
        {
            RequestingFactionGuid = faction,
            EntityCommandingGuid = secondaryEntity.Id,
            CreatedDate = primaryEntity.Manager.ManagerSubpulses.StarSysDateTime,
            IsPrimaryEntity = false
        };
        secondaryEntity.Manager.Game.OrderHandler.HandleOrder(cmd2);
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

                CreateCommands(fleet.FactionOwnerID, ship, cargoFromEntity, list);
            }
        }
    }
    
    
    /// <summary>
    /// Validates and actions the command.
    /// may eventualy need to return a responce instead of void.
    /// This creates a CargoTranferDB from the command, which does all the work.
    /// the command is to create and enqueue a CargoTransferDB.
    /// </summary>
    internal override void Execute(DateTime atDateTime)
    {
        if (!IsRunning)
        {
            CargoTransferDB transferDB = new CargoTransferDB(_transferData);
            transferDB.ParentStorageDB = EntityCommanding.GetDataBlob<VolumeStorageDB>();
            EntityCommanding.SetDataBlob(transferDB);
             

            IsRunning = true;
        }
    }

    internal override bool IsValidCommand(Game game)
    {
        if (CommandHelpers.IsCommandValid(game.GlobalManager, RequestingFactionGuid, EntityCommandingGuid, out factionEntity, out _entityCommanding))
        {
            return true;
        }
        return false;
    }

    public override bool IsFinished()
    {
        if (AmountLeftToXfer() > 0)
            return false;
        else
            return true;
    }

    long AmountLeftToXfer()
    {
        long amount = 0;
        foreach (var tup in _transferData.EscroHeldInPrimary)
        {
            amount += tup.count;
        }
        foreach (var tup in _transferData.EscroHeldInSecondary)
        {
            amount += tup.count;
        }
        return amount;
    }
    
    public override EntityCommand Clone()
    {
        throw new NotImplementedException();
    }
    
}