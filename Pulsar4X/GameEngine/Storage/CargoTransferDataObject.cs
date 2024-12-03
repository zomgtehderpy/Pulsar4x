using System;
using System.Collections.Generic;
using Pulsar4X.DataStructures;
using Pulsar4X.Engine;
using Pulsar4X.Galaxy;

namespace Pulsar4X.Storage;

public class CargoTransferDataObject
{
    internal Entity PrimaryEntity { get; set; }
    internal Entity SecondaryEntity { get; set; }
    
    /// <summary>
    /// positive amounts move INTO this entity, negitive amounts move OUT from this entity
    /// </summary>
    internal CargoStorageDB PrimaryStorageDB { get; private set; }
    /// <summary>
    /// positive amounts move OUT from this entity, negitive amounts move INTO this entity.
    /// </summary>
    internal CargoStorageDB SecondaryStorageDB { get; private set; }

    internal List<(ICargoable item, long amount)> OrderedToTransfer { get; private set; }

    internal SafeList<(ICargoable item, long count, double mass)> EscroHeldInPrimary { get; private set; } = new();
    internal SafeList<(ICargoable item, long count, double mass)> EscroHeldInSecondary { get; private set; } = new();

        
    internal CargoTransferDataObject(Entity primary, Entity secondary, List<(ICargoable item, long amount)> itemsToTransfer)
    {
        PrimaryEntity = primary;
        SecondaryEntity = secondary;
        PrimaryStorageDB = primary.GetDataBlob<CargoStorageDB>();
        SecondaryStorageDB = secondary.GetDataBlob<CargoStorageDB>();
        OrderedToTransfer = itemsToTransfer;
        
        PrimaryStorageDB.EscroItems.Add(this);
        SecondaryStorageDB.EscroItems.Add(this);
        
        for (int index = 0; index < OrderedToTransfer.Count; index++)
        {
            (ICargoable item, long amount) tuple = OrderedToTransfer[index];
            var cargoItem = tuple.item;
            var unitAmount = tuple.amount;
            TypeStore store;
            SafeList<(ICargoable item, long count, double mass)> itemsToRemove; //reference which list.
            long unitsStorable; 
            if (unitAmount < 0) //if we're removing items
            {
                unitAmount *= -1;
                store = PrimaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                itemsToRemove = EscroHeldInPrimary;
                unitsStorable = CargoMath.GetFreeUnitSpace(SecondaryStorageDB, cargoItem);
            }
            else
            {
                store = SecondaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                itemsToRemove = EscroHeldInSecondary;
                unitsStorable = CargoMath.GetFreeUnitSpace(PrimaryStorageDB, cargoItem);
            }
            if (store.CurrentStoreInUnits.ContainsKey(cargoItem.ID))
            {
                long amountInStore = store.CurrentStoreInUnits[cargoItem.ID];
                
                long amountToRemove = Math.Min(unitAmount, amountInStore);
                amountToRemove = Math.Min(unitsStorable, amountToRemove);
                store.CurrentStoreInUnits[cargoItem.ID] -= amountToRemove;
                double massToRemove = cargoItem.MassPerUnit * amountToRemove;
                itemsToRemove.Add((cargoItem,amountToRemove, massToRemove));
            }
            else
            {
                //in this case we're trying to remove items that don't exist. not sure how we should handle this yet.
                itemsToRemove.Add((cargoItem,0,0));
            }
        }
    }

    internal void UpdateEscro(ICargoable cargoItem, long unitAmount)
    {
        for (int index = 0; index < OrderedToTransfer.Count; index++)
        {
            (ICargoable item, long amount) tuple = OrderedToTransfer[index];
            if(tuple.item != cargoItem)
                continue;
            OrderedToTransfer[index] = (cargoItem, tuple.amount + unitAmount);
            TypeStore store;
            SafeList<(ICargoable item, long count, double mass)> itemsToRemove; //reference which list.
            long unitsStorable; 
            if (unitAmount < 0) //if we're removing items
            {
                unitAmount *= -1;
                store = PrimaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                itemsToRemove = EscroHeldInPrimary;
                unitsStorable = CargoMath.GetFreeUnitSpace(SecondaryStorageDB, cargoItem);
            }
            else
            {
                store = SecondaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                itemsToRemove = EscroHeldInSecondary;
                unitsStorable = CargoMath.GetFreeUnitSpace(PrimaryStorageDB, cargoItem);
            }
            if (store.CurrentStoreInUnits.ContainsKey(cargoItem.ID))
            {
                long amountInStore = store.CurrentStoreInUnits[cargoItem.ID];
                
                long amountToRemove = Math.Min(unitAmount, amountInStore);
                amountToRemove = Math.Min(unitsStorable, amountToRemove);
                store.CurrentStoreInUnits[cargoItem.ID] -= amountToRemove;
                double massToRemove = cargoItem.MassPerUnit * amountToRemove;
                itemsToRemove.Add((cargoItem,amountToRemove, massToRemove));
            }
            else
            {
                //in this case we're trying to remove items that don't exist. not sure how we should handle this yet.
                itemsToRemove.Add((cargoItem,0,0));
            }
            break;
        }
    }
}