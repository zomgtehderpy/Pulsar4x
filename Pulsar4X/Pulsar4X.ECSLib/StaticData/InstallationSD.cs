﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar4X.ECSLib
{
    public enum InstallationAbilityType
    {
        ShipMaintanance,
        InstalationConstruction,
        OrdnanceConstruction,
        FighterConstruction,
        FuelRefinary,
        Mine,
        AtmosphericModification,
        Research,
        Comercial, //ie aurora "Finance Center" 
        MassDriver,

    }

    public struct InstallationSD
    {
        public string Name;
        public string Description;
        /// <summary>
        /// the amount of pop required for this installation to operate.
        /// </summary>
        public int PopulationReqired;
        public int CargoSize;
        public JDictionary<InstallationAbilityType, int> BaseAbilityAmounts;
        
        /// <summary>
        /// research ID Requirements to build.
        /// </summary>
        public List<Guid> Requirements;

        /// <summary>
        /// The resources requred to build this facility
        /// </summary>
        public JDictionary<Guid,int> ResourceCosts;
        /// <summary>
        /// pricetag
        /// </summary>
        public int WealthCost;

        /// <summary>
        /// related to how long it will take to build this faclity, I think aurora uses mass for this, but hey why not be more flexable.
        /// </summary>
        public int BuildPoints;
    }
}
