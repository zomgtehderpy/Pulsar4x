using System.Collections.Generic;
using Pulsar4X.Engine;

namespace Pulsar4X.Storage;

public class CargoTransferObject
{
    /// <summary>
    /// positive amounts move INTO this entity, negitive amounts move OUT from this entity
    /// </summary>
    internal VolumeStorageDB PrimaryStorageDB { get; private set; }
    /// <summary>
    /// positive amounts move OUT from this entity, negitive amounts move INTO this entity.
    /// </summary>
    internal VolumeStorageDB SecondaryStorageDB { get; private set; }

    internal IReadOnlyList<(ICargoable item, long amount)> OrderedToTransfer { get; private set; }
    internal List<(ICargoable item, long amount)> ItemsLeftToMove { get; private set; }
    internal List<(ICargoable item, double amount)> ItemMassLeftToMove { get; private set; }
        
    internal CargoTransferObject(Entity primary, Entity secondary, IReadOnlyList<(ICargoable item, long amount)> itemsToTransfer)
    {
        PrimaryStorageDB = primary.GetDataBlob<VolumeStorageDB>();
        SecondaryStorageDB = secondary.GetDataBlob<VolumeStorageDB>();
        OrderedToTransfer = itemsToTransfer;
        ItemsLeftToMove = new List<(ICargoable item, long amount)>(itemsToTransfer);
        ItemMassLeftToMove = new List<(ICargoable item, double amount)>();
        foreach (var tuple in itemsToTransfer)
        {
            ICargoable cargoItem = tuple.item;
            double itemMassPerUnit = cargoItem.MassPerUnit;
            ItemMassLeftToMove.Add((cargoItem, (long)(tuple.amount * itemMassPerUnit)));
        }
    }
}