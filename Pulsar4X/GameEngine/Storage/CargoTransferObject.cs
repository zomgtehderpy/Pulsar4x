using System;
using System.Collections.Generic;
using Pulsar4X.DataStructures;
using Pulsar4X.Engine;

namespace Pulsar4X.Storage;

public class CargoTransferObject
{
    /// <summary>
    /// positive amounts move INTO this entity, negitive amounts move OUT from this entity
    /// </summary>
    internal CargoStorageDB PrimaryStorageDB { get; private set; }
    /// <summary>
    /// positive amounts move OUT from this entity, negitive amounts move INTO this entity.
    /// </summary>
    internal CargoStorageDB SecondaryStorageDB { get; private set; }

    internal IReadOnlyList<(ICargoable item, long amount)> OrderedToTransfer { get; private set; }

    internal SafeList<(ICargoable item, long count, double mass)> EscroHeldInPrimary { get; private set; } = new();
    internal SafeList<(ICargoable item, long count, double mass)> EscroHeldInSecondary { get; private set; } = new();

        
    internal CargoTransferObject(Entity primary, Entity secondary, IReadOnlyList<(ICargoable item, long amount)> itemsToTransfer)
    {
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
                
            if (unitAmount < 0) //if we're removing items
            {
                unitAmount *= -1;
                store = PrimaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                itemsToRemove = EscroHeldInPrimary;
            }
            else
            {
                store = SecondaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                itemsToRemove = EscroHeldInSecondary;
            }
            if (store.CurrentStoreInUnits.ContainsKey(cargoItem.ID))
            {
                long amountInStore = store.CurrentStoreInUnits[cargoItem.ID];
                long amountToRemove = Math.Min(unitAmount, amountInStore);
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
}