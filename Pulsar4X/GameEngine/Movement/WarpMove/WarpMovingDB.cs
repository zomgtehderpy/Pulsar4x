using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using GameEngine.WarpMove;
using Newtonsoft.Json;
using Pulsar4X.Orbital;
using Pulsar4X.Engine;
using Pulsar4X.Extensions;
using Pulsar4X.Orbits;

namespace Pulsar4X.Datablobs
{


    /// <summary>
    /// This datablob gets added to an entity when that entity is doing non-newtonion translation type movement.
    /// It gets removed from the entity once the entity has finished the translation.
    /// </summary>
    public class WarpMovingDB : BaseDataBlob
    {
        [JsonProperty]
        public bool HasStarted { get; internal set; } = false;

        [JsonProperty]
        public DateTime LastProcessDateTime = new DateTime();

        [JsonProperty]
        public Vector3 SavedNewtonionVector { get; internal set; }

        [JsonProperty]
        public Vector3 EntryPointAbsolute { get; internal set; }
        [JsonProperty]
        public Vector3 ExitPointAbsolute { get; internal set; }

        [JsonProperty]
        public Vector3 ExitPointrelative { get; internal set; }

        [JsonProperty]
        public float Heading_Radians { get; internal set; }
        [JsonProperty]
        public DateTime EntryDateTime { get; internal set; }
        [JsonProperty]
        public DateTime PredictedExitTime { get; internal set; }

        [JsonProperty]
        public Vector3 CurrentNonNewtonionVectorMS { get; internal set; }

        internal Vector2 _position;
        internal Entity _parentEnitity;
        public KeplerElements EndpointTargetOrbit { get; private set; }

        /// <summary>
        /// Newtonion Vector to burn once warp is complete.
        /// </summary>
        [JsonProperty]
        internal Vector3 EndpointTargetExpendDeltaV { get; set; }

        /// <summary>
        /// when true, will attempt a newton circularization burn after warp, if ExpendDelaV is 0
        /// </summary>
        [JsonProperty]
        internal bool AutoCirculariseAfterWarp { get; set; } = true;

        [JsonProperty]
        internal bool IsAtTarget { get; set; }

        [JsonProperty]
        internal Entity? TargetEntity;
        [JsonIgnore] //don't store datablobs, we catch this on deserialization.
        internal PositionDB TargetPositionDB;
        public PositionDB GetTargetPosDB
        {
            get { return TargetPositionDB; }
        }

        public WarpMovingDB()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Pulsar4X.ECSLib.TranslateMoveDB"/> class.
        /// Use this one to move to a specific postion vector.
        /// </summary>
        /// <param name="targetPosition_m">Target position in Meters.</param>
        public WarpMovingDB(Entity thisEntity, Vector3 targetPosition_m)
        {
            ExitPointAbsolute = targetPosition_m;

            var startState = MoveMath.GetAbsoluteState(thisEntity);

            ExitPointAbsolute = targetPosition_m;
            EntryPointAbsolute = startState.pos;
            EntryDateTime = thisEntity.Manager.ManagerSubpulses.StarSysDateTime;
            ExitPointrelative = Vector3.Zero;
            //PredictedExitTime = targetIntercept.atDateTime;
            SavedNewtonionVector = MoveMath.GetRelativeState(thisEntity).Velocity; //TODO: this needs to check GameSettings.UseRelativeVelocity
            TargetEntity = null;

            Heading_Radians = (float)Vector3.AngleBetween(startState.pos, ExitPointAbsolute);

            Heading_Radians = (float)Math.Atan2(targetPosition_m.Y, targetPosition_m.X);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetPositiondb"></param>
        /// <param name="offsetPosition">normaly you want to move to a position next to the entity, this is
        /// a position relative to the entity you're wanting to move to</param>
        public WarpMovingDB(Entity thisEntity, Entity targetEntity, Vector3 offsetPosition, KeplerElements endpointTargetOrbit)
        {
            EntryDateTime = thisEntity.Manager.ManagerSubpulses.StarSysDateTime;
            var targetIntercept = WarpMath.GetInterceptPosition(thisEntity, targetEntity, EntryDateTime, offsetPosition);

            var startState = MoveMath.GetAbsoluteState(thisEntity);
            ExitPointAbsolute = targetIntercept.position + offsetPosition;
            EntryPointAbsolute = startState.pos;
            ExitPointrelative = offsetPosition;
            PredictedExitTime = targetIntercept.etiDateTime;
            EndpointTargetOrbit = endpointTargetOrbit;
            SavedNewtonionVector = MoveMath.GetRelativeState(thisEntity).Velocity; //TODO: this needs to check GameSettings.UseRelativeVelocity
            TargetEntity = targetEntity;
            TargetPositionDB = targetEntity.GetDataBlob<PositionDB>();
            Heading_Radians = (float)Vector3.AngleBetween(startState.pos, ExitPointAbsolute);
        }

        public WarpMovingDB(WarpMovingDB db)
        {
            LastProcessDateTime = db.LastProcessDateTime;
            SavedNewtonionVector = db.SavedNewtonionVector;
            EntryPointAbsolute = db.EntryPointAbsolute;
            ExitPointAbsolute = db.ExitPointAbsolute;
            CurrentNonNewtonionVectorMS = db.CurrentNonNewtonionVectorMS;
            EndpointTargetOrbit = db.EndpointTargetOrbit;
            EndpointTargetExpendDeltaV = db.EndpointTargetExpendDeltaV;
            IsAtTarget = db.IsAtTarget;
            TargetEntity = db.TargetEntity;

            HasStarted = db.HasStarted;
            TargetPositionDB = db.TargetPositionDB;

        }
        // JSON deserialization callback.
        [OnDeserialized]
        private void Deserialized(StreamingContext context)
        {

            if (TargetEntity != null)
            {

                var game = (Game?)context.Context;
                game.PostLoad += (sender, args) =>
                {
                    TargetPositionDB = TargetEntity.GetDataBlob<PositionDB>();
                };
            }
        }

        public override object Clone()
        {
            return new WarpMovingDB(this);
        }

        internal override void OnSetToEntity()
        {
            if (OwningEntity.HasDataBlob<OrbitDB>())
            {
                OwningEntity.RemoveDataBlob<OrbitDB>();
            }
            if (OwningEntity.HasDataBlob<OrbitUpdateOftenDB>())
            {
                OwningEntity.RemoveDataBlob<OrbitUpdateOftenDB>();
            }
            if (OwningEntity.HasDataBlob<NewtonMoveDB>())
            {
                OwningEntity.RemoveDataBlob<NewtonMoveDB>();
            }
            if (OwningEntity.HasDataBlob<NewtonSimpleMoveDB>())
            {
                OwningEntity.RemoveDataBlob<NewtonSimpleMoveDB>();
            }

        }
    }
}
