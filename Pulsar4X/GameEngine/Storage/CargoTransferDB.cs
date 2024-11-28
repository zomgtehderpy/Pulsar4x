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
    /// </summary>
    public class CargoTransferDB : BaseDataBlob
    {
        internal string TransferJobID { get; } = Guid.NewGuid().ToString();

        internal Entity PrimaryEntity { get; set; }
        internal Entity SecondaryEntity { get; set; }
        [JsonIgnore]
        internal CargoStorageDB ParentStorageDB { get; set; }
        
        /// <summary>
        /// This object is shared between two datablobs/entites 
        /// </summary>
        internal CargoTransferObject TransferData { get; private set; } 
        

        /// <summary>
        /// Threadsafe gets items left to transfer. don't call this every ui frame!
        /// (or you could cause deadlock slowdowns with the processing)tr
        /// </summary>
        /// <returns></returns>
        public List<(ICargoable item, long unitCount)> GetItemsToTransfer()
        {
             List<(ICargoable item, long unitCount)> list = new();
             foreach (var item in TransferData.EscroHeldInPrimary)
             {
                 var count = item.count;
                 if (OwningEntity == PrimaryEntity)
                     count *= -1;
                 list.Add((item.item, count));
             }
             foreach (var item in TransferData.EscroHeldInSecondary)
             {
                 var count = item.count;
                 if (OwningEntity == SecondaryEntity)
                     count *= -1;
                 list.Add((item.item, count));
             }
             return list;
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
