using Pulsar4X.Atb;
using Pulsar4X.Engine.Designs;
using Pulsar4X.Ships;

namespace Pulsar4X.Datablobs;

public class MissileLaunchersAbilityDB : BaseDataBlob
{
    public MissileLauncherAtb[] Launchers;
    public ShipDesign[] LoadedMissiles;


    public override object Clone()
    {
        throw new System.NotImplementedException();
    }
}