using System;
using System.Collections.Generic;
using Pulsar4X.ECSLib;

public static class OrderRegistry
{
    public static Dictionary<string, Func<IAction>> Actions = new Dictionary<string, Func<IAction>>()
    {
        { "Move to Nearest Colony", () => new MoveToNearestColonyAction() },
        { "Refuel", () => new RefuelAction() },
        { "Resupply", () => new ResupplyAction() }
    };

    public static Dictionary<Type, string> ActionDescriptions = new Dictionary<Type, string>()
    {
        { typeof(MoveToNearestColonyAction), "Move to Nearest Colony" },
        { typeof(RefuelAction), "Refuel" },
        { typeof(ResupplyAction), "Resupply" },
    };

    public static Dictionary<string, Func<ConditionItem>> Conditions = new Dictionary<string, Func<ConditionItem>>()
    {
        { "Fuel", () => new ConditionItem(new FuelCondition(30f, ComparisonType.LessThan))}
    };

    public static Dictionary<Type, string> ConditionDescriptions = new Dictionary<Type, string>()
    {
        { typeof(FuelCondition), "Fuel" }
    };
}