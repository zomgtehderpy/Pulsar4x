using System;
using Pulsar4X.Engine;
using Pulsar4X.Extensions;
using Pulsar4X.Orbital;

namespace Pulsar4X.Datablobs;

/// <summary>
/// Databolob/movetype agnostic position and movement math for entites
/// </summary>
public static class MoveMath
{
    /// <summary>
    /// Gets future velocity for this entity, datablob agnostic.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="atDateTime"></param>
    /// <returns>Velocity in m/s relative to SOI parent</returns>
    /// <exception cref="Exception"></exception>
    public static Vector3 GetRelativeFutureVelocity(Entity entity, DateTime atDateTime)
    {

        if (entity.HasDataBlob<OrbitDB>())
        {
            return entity.GetDataBlob<OrbitDB>().InstantaneousOrbitalVelocityVector_m(atDateTime);
        }
        if (entity.HasDataBlob<OrbitUpdateOftenDB>())
        {
            return entity.GetDataBlob<OrbitUpdateOftenDB>().InstantaneousOrbitalVelocityVector_m(atDateTime);
        }
        else if (entity.HasDataBlob<NewtonMoveDB>())
        {
            return NewtonionMovementProcessor.GetRelativeState(entity, entity.GetDataBlob<NewtonMoveDB>(), atDateTime).vel;
        }
        else if (entity.HasDataBlob<NewtonSimpleMoveDB>())
        {
            return NewtonSimpleProcessor.GetRelativeState(entity, atDateTime).vel;
        }
        else if (entity.HasDataBlob<WarpMovingDB>())
        {
            return entity.GetDataBlob<WarpMovingDB>().SavedNewtonionVector;
        }
        else
        {
            throw new Exception("Entity has no velocity");
        }
    }

    /// <summary>
    /// Gets future velocity for this entity, datablob agnostic.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="atDateTime"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Vector3 GetAbsoluteFutureVelocity(Entity entity, DateTime atDateTime)
    {
        PositionDB posDB = entity.GetDataBlob<PositionDB>();
        
        
        if (entity.HasDataBlob<OrbitDB>())
        {
            return entity.GetDataBlob<OrbitDB>().AbsoluteOrbitalVector_m(atDateTime);
        }
        if (entity.HasDataBlob<OrbitUpdateOftenDB>())
        {
            return entity.GetDataBlob<OrbitUpdateOftenDB>().AbsoluteOrbitalVector_m(atDateTime);
        }
        else if (entity.HasDataBlob<NewtonMoveDB>())
        {
            var vel = NewtonionMovementProcessor.GetRelativeState(entity, entity.GetDataBlob<NewtonMoveDB>(), atDateTime).vel;
            var parentEntity = posDB.Parent;
            if(parentEntity == null) throw new NullReferenceException("parentEntity cannot be null");
            //recurse
            return GetAbsoluteFutureVelocity(parentEntity, atDateTime) + vel;
        }
        else if (entity.HasDataBlob<NewtonSimpleMoveDB>())
        {
            return NewtonSimpleProcessor.GetAbsoluteState(entity, atDateTime).vel;
        }
        else if (entity.HasDataBlob<WarpMovingDB>())
        {
            return entity.GetDataBlob<WarpMovingDB>().SavedNewtonionVector;
        }
        else
        {
            throw new Exception("Entity has no velocity");
        }
    }

    public static Vector2 GetAbsoluteFuturePosition(Entity entity, DateTime atDateTime)
    {
        PositionDB position = entity.GetDataBlob<PositionDB>();
        Vector2 pos = new Vector2(0,0);
        switch (position.MoveType)
        {
            case PositionDB.MoveTypes.None:
            {
                pos = position.AbsolutePosition2;
                break;
            }
            case PositionDB.MoveTypes.Orbit:
            {
                if(entity.TryGetDatablob<OrbitDB>(out var orbitDB))
                {
                    pos = (Vector2)OrbitMath.GetAbsolutePosition(orbitDB, atDateTime);
                }
                else if (entity.TryGetDatablob<OrbitUpdateOftenDB>(out var orbitDB2))
                {
                    pos = (Vector2)OrbitMath.GetAbsolutePosition(orbitDB2, atDateTime);
                }
            }
                break;
            case PositionDB.MoveTypes.NewtonSimple:
            {
                pos = (Vector2)NewtonSimpleProcessor.GetAbsoluteState(entity, atDateTime).pos;
            }
                break;

            case PositionDB.MoveTypes.NewtonComplex:
            {
                var db = entity.GetDataBlob<NewtonMoveDB>();
                pos = (Vector2)NewtonionMovementProcessor.GetAbsoluteState(entity, db, atDateTime).pos;
            }
                break;
            case PositionDB.MoveTypes.Warp:
            {
                var db = entity.GetDataBlob<WarpMovingDB>();
                if (atDateTime < db.PredictedExitTime)
                {
                    var t = (atDateTime - db.LastProcessDateTime).TotalSeconds;
                    pos = db._position + (Vector2)(db.CurrentNonNewtonionVectorMS * t);
                }
                else
                {
                    var endOrbit = db.TargetEndpointOrbit;
                    var rpos = (Vector2)OrbitalMath.GetPosition(endOrbit, atDateTime);
                    var ppos = GetAbsoluteFuturePosition(db.TargetEntity, atDateTime);
                    pos = ppos + rpos;
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return pos;
    }

    public static Vector2 GetRelativeFuturePosition(Entity entity, DateTime atDateTime)
    {
        PositionDB position = entity.GetDataBlob<PositionDB>();
        Vector2 pos = new Vector2(0,0);
        switch (position.MoveType)
        {
            case PositionDB.MoveTypes.None:
            {
                pos = position.RelativePosition2;
                break;
            }
            case PositionDB.MoveTypes.Orbit:
            {
                if(entity.TryGetDatablob<OrbitDB>(out var orbitDB))
                {
                    pos = (Vector2)OrbitMath.GetPosition(orbitDB, OrbitMath.GetTrueAnomaly(orbitDB, atDateTime));
                }
                else if (entity.TryGetDatablob<OrbitUpdateOftenDB>(out var orbitDB2))
                {
                    pos = (Vector2)OrbitMath.GetPosition(orbitDB2, OrbitMath.GetTrueAnomaly(orbitDB2, atDateTime));
                }
            }
                break;
            case PositionDB.MoveTypes.NewtonSimple:
            {
                pos = (Vector2)NewtonSimpleProcessor.GetRelativeState(entity, atDateTime).pos;
            }
                break;

            case PositionDB.MoveTypes.NewtonComplex:
            {
                var db = entity.GetDataBlob<NewtonMoveDB>();
                pos = (Vector2)NewtonionMovementProcessor.GetRelativeState(entity, db, atDateTime).pos;
            }
                break;
            case PositionDB.MoveTypes.Warp:
            {
                var db = entity.GetDataBlob<WarpMovingDB>();
                if (atDateTime < db.PredictedExitTime)
                {
                    var t = (atDateTime - db.LastProcessDateTime).TotalSeconds;
                    pos = db._position + (Vector2)(db.CurrentNonNewtonionVectorMS * t);
                }
                else
                {
                    var endOrbit = db.TargetEndpointOrbit;
                    pos = (Vector2)OrbitMath.GetPosition(endOrbit, atDateTime);
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return pos;
    }
}