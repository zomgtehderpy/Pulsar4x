using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pulsar4X.Engine;
using Pulsar4X.Interfaces;
using Pulsar4X.Datablobs;

namespace Pulsar4X.Storage
{

    //this object is shared between two CargoTransferDB.
    public class CargoTransferObject
    {
        internal VolumeStorageDB PrimaryStorageDB { get; private set; }
        internal VolumeStorageDB SecondaryStorageDB { get; private set; }

        internal List<(ICargoable item, long amount)> OrderedToTransfer { get; private set; }
        internal List<(ICargoable item, long amount)> ItemsLeftToMove { get; private set; }
        internal List<(ICargoable item, double amount)> ItemMassLeftToMove { get; private set; }
        
        internal CargoTransferObject(Entity primary, Entity secondary, List<(ICargoable item, long amount)> itemsToTransfer)
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

    /// <summary>
    /// this datablob is active on an entity that is or will be transfering cargo.
    /// this datablob should be on the entity that is SENDING cargo, not RECEVING 
    /// </summary>
    public class CargoTransferDB : BaseDataBlob
    {
        internal string TransferJobID { get; } = Guid.NewGuid().ToString();

        internal Entity PrimaryEntity { get; set; }
        internal Entity SecondaryEntity { get; set; }
        [JsonIgnore]
        internal VolumeStorageDB ParentStorageDB { get; set; }
        
        /// <summary>
        /// This object is shared between two datablobs/entites 
        /// </summary>
        internal CargoTransferObject TransferData { get; private set; } 
        

        /// <summary>
        /// Threadsafe gets items left to transfer. don't call this every ui frame!
        /// (or you could cause deadlock slowdowns with the processing)
        /// </summary>
        /// <returns></returns>
        public List<(ICargoable item, long unitCount)> GetItemsToTransfer()
        {
            ICollection  ic = TransferData.ItemsLeftToMove;
            lock (ic.SyncRoot)
            {
                return new List<(ICargoable item, long unitCount)>(TransferData.ItemsLeftToMove);
            }
        }

        public CargoTransferDB(CargoTransferObject transferObject)
        {
            TransferData = transferObject;
            PrimaryEntity = transferObject.PrimaryStorageDB.OwningEntity;
            SecondaryEntity = transferObject.SecondaryStorageDB.OwningEntity;
        }


        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }

}
