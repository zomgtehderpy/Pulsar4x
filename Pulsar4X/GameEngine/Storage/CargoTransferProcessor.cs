using System;
using System.Collections.Generic;
using Pulsar4X.Orbital;
using Pulsar4X.Extensions;
using Pulsar4X.Interfaces;
using Pulsar4X.Engine;
using Pulsar4X.Galaxy;
using Pulsar4X.Movement;

namespace Pulsar4X.Storage
{
    public class CargoTransferProcessor : IHotloopProcessor
    {
        public static CargoDefinitionsLibrary CargoDefs;

        public TimeSpan RunFrequency
        {
            get { return TimeSpan.FromMinutes(1); }
        }

        public TimeSpan FirstRunOffset => TimeSpan.FromHours(0);

        public Type GetParameterType => typeof(CargoTransferDB);

        public void Init(Game game)
        {
            //unneeded
        }

        public void ProcessEntity(Entity entity, int deltaSeconds)
        {
            ProcessEntity(entity.GetDataBlob<CargoTransferDB>(), deltaSeconds);
        }


        public void ProcessEntity(CargoTransferDB transferDB, int deltaSeconds)
        {
            var transferData = transferDB.TransferData;
            var transferRange = transferDB.ParentStorageDB.TransferRangeDv_mps;
            var transferRate = transferDB.ParentStorageDB.TransferRateInKgHr;
            double dv_mps = CalcDVDifference_m(transferDB.PrimaryEntity, transferDB.SecondaryEntity);
            
            if(dv_mps > transferRange)
                return;//early out if we're out of range. 
            
            
            double massTransferable = transferRate * deltaSeconds;
            //each of the items to transfer...
            for (int i = 0; i < transferData.ItemMassLeftToMove.Count; i++)
            {
                (ICargoable item, double mass) xferItems = transferData.ItemMassLeftToMove[i];
                ICargoable cargoItem = xferItems.item;
                //string cargoTypeID = cargoItem.CargoTypeID;
                double itemMassPerUnit = cargoItem.MassPerUnit;
                double netMass = xferItems.mass;
                var massToXfer = Math.Min(massTransferable, netMass);
                
                
                //remove from transferData (escro)
                //mass is a double here to signify larger objects taking longer to move 
                var massLeft = xferItems.mass += massToXfer;
                transferData.ItemMassLeftToMove[i] = (cargoItem, massLeft);
                //we use Floor here to signify whole part items not fully moved yet. 
                int itemsMoved = (int)Math.Floor(massToXfer / itemMassPerUnit);
                //long itemsLeft = (long)Math.Ceiling(transferData.ItemMassLeftToMove[i].amount / itemMassPerUnit);
                long itemsLeft = transferData.ItemsLeftToMove[i].amount - itemsMoved;
                transferData.ItemsLeftToMove[i] = (cargoItem, itemsLeft);


                //if we're ADDING to the PRIMARY
                if (itemsMoved > 0)
                {
                    transferData.PrimaryStorageDB.AddCargoByUnit(cargoItem, itemsMoved);
                }
                else //We're Removing from the Secondary
                {
                    //update mass and volume of secondary entity store.
                    double volumeStoring = itemsMoved * cargoItem.VolumePerUnit;
                    double massStoring = itemsMoved * cargoItem.MassPerUnit;
                    TypeStore store = transferData.SecondaryStorageDB.TypeStores[cargoItem.CargoTypeID];
                    store.FreeVolume += volumeStoring;
                    transferData.SecondaryStorageDB.TotalStoredMass += massStoring;
                }
                
                
                massTransferable -= Math.Abs(massToXfer);
                transferDB.OwningEntity.GetDataBlob<MassVolumeDB>().UpdateMassTotal();
                UpdateFuelAndDeltaV(transferDB.OwningEntity);

                if(massTransferable <= 0)
                    break;//early out of loop if we've hit the limit of mass moveable this tick.
            }
        }

        /// <summary>
        /// Add cargo and updates the entites MassTotal
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="item"></param>
        /// <param name="amountInMass"></param>
        internal static double AddCargoItems(Entity entity, ICargoable item, int amount)
        {
            VolumeStorageDB cargo = entity.GetDataBlob<VolumeStorageDB>();
            double amountSuccess = cargo.AddCargoByUnit(item, amount);
            MassVolumeDB mv = entity.GetDataBlob<MassVolumeDB>();
            mv.UpdateMassTotal(cargo);
            UpdateFuelAndDeltaV(entity);
            return amountSuccess;
        }

        /// <summary>
        /// Removes cargo and updates the entites MassTotal
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="item"></param>
        /// <param name="amountInMass"></param>
        internal static double RemoveCargoItems(Entity entity, ICargoable item, int amount)
        {
            VolumeStorageDB cargo = entity.GetDataBlob<VolumeStorageDB>();
            double amountSuccess = cargo.RemoveCargoByUnit(item, amount);
            MassVolumeDB mv = entity.GetDataBlob<MassVolumeDB>();
            mv.UpdateMassTotal(cargo);
            UpdateFuelAndDeltaV(entity);
            return amountSuccess;
        }

        /// <summary>
        /// Add or Removes cargo and updates the entites MassTotal
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="item"></param>
        /// <param name="amountInMass"></param>
        internal static double AddRemoveCargoMass(Entity entity, ICargoable item, double amountInMass)
        {
            VolumeStorageDB cargo = entity.GetDataBlob<VolumeStorageDB>();
            double amountSuccess = cargo.AddRemoveCargoByMass(item, amountInMass);
            MassVolumeDB mv = entity.GetDataBlob<MassVolumeDB>();
            mv.UpdateMassTotal(cargo);
            UpdateFuelAndDeltaV(entity);
            return amountSuccess;
        }
        /// <summary>
        /// Add or Removes cargo and updates the entites MassTotal
        /// </summary>
        /// <param name="storeDB"></param>
        /// <param name="item"></param>
        /// <param name="amountInMass"></param>
        internal static double AddRemoveCargoMass(VolumeStorageDB storeDB, ICargoable item, double amountInMass)
        {
            double amountSuccess = storeDB.AddRemoveCargoByMass(item, amountInMass);
            MassVolumeDB mv = storeDB.OwningEntity.GetDataBlob<MassVolumeDB>();
            mv.UpdateMassTotal(storeDB);
            UpdateFuelAndDeltaV(storeDB.OwningEntity);
            return amountSuccess;
        }


        
        /// <summary>
        /// Add or Removes cargo and updates the entites MassTotal
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="item"></param>
        /// <param name="amountInVolume"></param>
        internal static double AddRemoveCargoVolume(Entity entity, ICargoable item, double amountInVolume)
        {
            VolumeStorageDB cargo = entity.GetDataBlob<VolumeStorageDB>();
            double amountSuccess = cargo.AddRemoveCargoByVolume(item, amountInVolume);
            MassVolumeDB mv = entity.GetDataBlob<MassVolumeDB>();
            mv.UpdateMassTotal(cargo);
            UpdateFuelAndDeltaV(entity);
            return amountSuccess;
        }

        internal static void UpdateFuelAndDeltaV(Entity entity)
        {
            if(!entity.TryGetDatablob(out NewtonThrustAbilityDB newtdb))
                return;
            if (!entity.TryGetDatablob(out MassVolumeDB massdb))
                return;
            if(!entity.TryGetDatablob(out VolumeStorageDB storedb))
                return;

            var cargoLib = entity.GetFactionCargoDefinitions();
            var fuelTypeID = newtdb.FuelType;
            var fuelType = cargoLib.GetAny(fuelTypeID);
            var fuelMass = storedb.GetMassStored(fuelType);
            newtdb.SetFuel(fuelMass, massdb.MassTotal);
        }

        /// <summary>
        /// Calculates a simplified difference in DeltaV between two enties who have the same parent
        /// for the purposes of calculating cargo transfer rate
        /// </summary>
        /// <param name="entity1"></param>
        /// <param name="entity2"></param>
        /// <returns></returns>
        public static double CalcDVDifference_m(Entity entity1, Entity entity2)
        {
            double dvDif = 0;

            Entity parent;
            double parentMass;
            double sgp;
            double r1;
            double r2;

            Entity? soi1 = entity1.GetSOIParentEntity();
            Entity? soi2 = entity2.GetSOIParentEntity();


            if(soi1 is not null && soi2 is not null && soi1 == soi2)
            {
                parent = soi1;
                parentMass = parent.GetDataBlob<MassVolumeDB>().MassDry;
                sgp = GeneralMath.StandardGravitationalParameter(parentMass);

                (Vector3 pos, Vector3 Velocity) state1 = MoveMath.GetRelativeState(entity1);
                (Vector3 pos, Vector3 Velocity) state2 = MoveMath.GetRelativeState(entity2);
                r1 = state1.pos.Length();
                r2 = state2.pos.Length();
            }
            else
            {
                //StaticRefLib.EventLog.AddEvent(new Event("Cargo calc failed, entities must have same soi parent"));
                return double.PositiveInfinity;
            }

            var hohmann = OrbitalMath.Hohmann(sgp, r1, r2);
            dvDif = hohmann[0].deltaV.Length() + hohmann[1].deltaV.Length();
            return dvDif;


        }



        /// <summary>
        /// Calculates a simplified difference in DeltaV between two enties who have the same parent
        /// for the purposes of calculating cargo transfer rate
        /// </summary>
        /// <param name="sgp"></param>
        /// <param name="state1"></param>
        /// <param name="state2"></param>
        /// <returns></returns>
        public static double CalcDVDifference_m(double sgp, (Vector3 pos, Vector3 Velocity) state1, (Vector3 pos, Vector3 Velocity) state2)
        {
            var r1 = state1.pos.Length();
            var r2 = state2.pos.Length();
            var hohmann = OrbitalMath.Hohmann(sgp, r1, r2);
            return hohmann[0].deltaV.Length() + hohmann[1].deltaV.Length();
        }


        /// <summary>
        /// Calculates the transfer rate.
        /// </summary>
        /// <returns>The transfer rate.</returns>
        /// <param name="dvDifference_mps">Dv difference in m/s</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public static int CalcTransferRate(double dvDifference_mps, VolumeStorageDB from, VolumeStorageDB to)
        {
            //var from = transferDB.CargoFromDB;
            //var to = transferDB.CargoToDB;
            var fromDVRange = from.TransferRangeDv_mps;
            var toDVRange = to.TransferRangeDv_mps;

            double maxRange;
            double maxXferAtMaxRange;
            double bestXferRange_ms = Math.Min(fromDVRange, toDVRange);
            double maxXferAtBestRange = from.TransferRateInKgHr + to.TransferRateInKgHr;

            double transferRate;

            if (from.TransferRangeDv_mps > to.TransferRangeDv_mps)
            {
                maxRange = fromDVRange;
                if (from.TransferRateInKgHr > to.TransferRateInKgHr)
                    maxXferAtMaxRange = from.TransferRateInKgHr;
                else
                    maxXferAtMaxRange = to.TransferRateInKgHr;
            }
            else
            {
                maxRange = toDVRange;
                if (to.TransferRateInKgHr > from.TransferRateInKgHr)
                    maxXferAtMaxRange = to.TransferRateInKgHr;
                else
                    maxXferAtMaxRange = from.TransferRateInKgHr;
            }

            if (dvDifference_mps < bestXferRange_ms)
                transferRate = (int)maxXferAtBestRange;
            else if (dvDifference_mps < maxRange)
                transferRate = (int)maxXferAtMaxRange;
            else
                transferRate = 0;
            return (int)transferRate;
        }
        

        public static (double bestDVRange, double bestRate) GetBestRangeRate(Entity from, Entity to)
        {
            var fromdb = from.GetDataBlob<VolumeStorageDB>();
            var todb = to.GetDataBlob<VolumeStorageDB>();
            var fromDVRange = fromdb.TransferRangeDv_mps;
            var toDVRange = todb.TransferRangeDv_mps;
            double bestXferRange_ms = Math.Min(fromDVRange, toDVRange);
            double maxXferAtBestRange = fromdb.TransferRateInKgHr + todb.TransferRateInKgHr;
            return (maxXferAtBestRange, bestXferRange_ms);
        }

        public static (double maxDVRange, double lowRate) GetMaxRangeRate(Entity from, Entity to)
        {
            var fromdb = from.GetDataBlob<VolumeStorageDB>();
            var todb = to.GetDataBlob<VolumeStorageDB>();
            var fromDVRange = fromdb.TransferRangeDv_mps;
            var toDVRange = todb.TransferRangeDv_mps;
            double maxRange;
            double maxXferAtMaxRange;

            if (fromdb.TransferRangeDv_mps > todb.TransferRangeDv_mps)
            {
                maxRange = fromDVRange;
                if (fromdb.TransferRateInKgHr > todb.TransferRateInKgHr)
                    maxXferAtMaxRange = fromdb.TransferRateInKgHr;
                else
                    maxXferAtMaxRange = todb.TransferRateInKgHr;
            }
            else
            {
                maxRange = toDVRange;
                if (todb.TransferRateInKgHr > fromdb.TransferRateInKgHr)
                    maxXferAtMaxRange = todb.TransferRateInKgHr;
                else
                    maxXferAtMaxRange = fromdb.TransferRateInKgHr;
            }

            return(maxRange, maxXferAtMaxRange);
        }



        public int ProcessManager(EntityManager manager, int deltaSeconds)
        {
            List<CargoTransferDB> dblist = manager.GetAllDataBlobsOfType<CargoTransferDB>();
            foreach(var db in dblist)
            {
                ProcessEntity(db, deltaSeconds);
            }
            return dblist.Count;
        }
    }

}
