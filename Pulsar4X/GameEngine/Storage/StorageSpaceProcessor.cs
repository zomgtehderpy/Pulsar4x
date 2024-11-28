using System.Collections.Generic;
using Pulsar4X.Datablobs;
using Pulsar4X.Engine;

namespace Pulsar4X.Storage
{
    public static class StorageSpaceProcessor
    {
       internal static void RecalcVolumeCapacityAndRates(Entity parentEntity, CargoDefinitionsLibrary cargoLibrary)
        {
            CargoStorageDB cargoStorageDB = parentEntity.GetDataBlob<CargoStorageDB>();
            Dictionary<string, double> calculatedMaxStorage = new ();

            var instancesDB = parentEntity.GetDataBlob<ComponentInstancesDB>();

            double transferRate = 0;
            double transferRange = 0;

            if( instancesDB.TryGetComponentsByAttribute<VolumeStorageAtb>(out var componentInstances))
            {

                foreach (var instance in componentInstances)
                {
                    var design = instance.Design;
                    var atbdata = design.GetAttribute<VolumeStorageAtb>();

                    if (instance.HealthPercent() > 0.75)
                    {
                        if(!calculatedMaxStorage.ContainsKey(atbdata.StoreTypeID))
                            calculatedMaxStorage[atbdata.StoreTypeID] = atbdata.MaxVolume;
                        else
                            calculatedMaxStorage[atbdata.StoreTypeID] += atbdata.MaxVolume;
                    }
                }
            }

            foreach (var kvp in calculatedMaxStorage)
            {
                if(!cargoStorageDB.TypeStores.ContainsKey(kvp.Key))
                    cargoStorageDB.TypeStores.Add(kvp.Key, new TypeStore(kvp.Value));

                else
                {
                    var stor = cargoStorageDB.TypeStores[kvp.Key];
                    var dif = kvp.Value - stor.MaxVolume;
                    cargoStorageDB.ChangeMaxVolume(kvp.Key, dif, cargoLibrary);
                }
            }

            int i = 0;
            if (instancesDB.TryGetComponentsByAttribute<StorageTransferRateAtb>(out var componentTransferInstances))
            {
                foreach (var instance in componentTransferInstances)
                {
                    var design = instance.Design;
                    if(!design.HasAttribute<StorageTransferRateAtb>())
                        continue;

                    var atbdata = design.GetAttribute<StorageTransferRateAtb>();
                    if (instance.HealthPercent() > 0.75)
                    {
                        transferRate += atbdata.TransferRate_kgs;
                        transferRange += atbdata.TransferRange_ms;
                        i++;
                    }
                }

                cargoStorageDB.TransferRate = (int)(transferRate / i);
                cargoStorageDB.TransferRangeDv_mps = transferRange / i;
            }
        }
    }
}


