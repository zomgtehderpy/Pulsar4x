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
            return new List<(ICargoable item, long unitCount)>(TransferData.ItemsLeftToMove);
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
