using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pulsar4X.Components;

namespace Pulsar4X.Sensors
{
    public class SensorReceiverAbility : ComponentAbilityState
    {
        [JsonProperty]
        public Dictionary<int, SensorReturnValues> CurrentContacts = new ();
        [JsonProperty]
        public Dictionary<int, SensorReturnValues> OldContacts = new ();
        [JsonConstructor]
        private SensorReceiverAbility(){}
        public SensorReceiverAbility(ComponentInstance componentInstance) : base(componentInstance)
        {
        }
    }
}