// LCD Panel to be used to display inventory
// Set panel to Mono font for best result
private string consoleName =  "Inventory LCD Panel";

// List of subtypes to filter the display for
// Only Ore and Ingot types supported
private List<string> subtypeFilter = new List<string>{ 
        "Stone",
        "Iron",
        "Silicon",
        "Nickel",
        "Cobalt",
        "Magensium",
        "Silver",
        "Gold",
        "Platinum",
        "Ice",
};

private List<IMyTerminalBlock> entityList = new List<IMyTerminalBlock>();
private Dictionary<string, MyFixedPoint> inventoryDict = new Dictionary<string, MyFixedPoint>();
private List<MyInventoryItem> itemList = new List<MyInventoryItem>();

public void Main(string argument, UpdateType updateSource)
{
    var console = GridTerminalSystem.GetBlockWithName(consoleName) as IMyTextPanel;
    if (console == null) {
        return;
    }

    // Get list of containers
    entityList.Clear();
    GridTerminalSystem.GetBlocksOfType<IMyEntity>(entityList);

    // Clear existing inventory dictionary.
    inventoryDict.Clear();

    // Loop over all entities
    for (var i = 0; i < entityList.Count; i++) {
        var entity = entityList[i] as IMyEntity;

        // Skip block if it does not have an inventory
        if (!entity.HasInventory) {
            continue;
        }

        // TODO: Look at all inventories in each entity
        itemList.Clear();
        entity.GetInventory(0).GetItems(itemList);

        // Loop over each item in the inventory
        foreach (MyInventoryItem item in itemList) {
            // Build an item type identifier to store in a dict
            var itemIdentifier = item.Type.TypeId + ":" + item.Type.SubtypeId;

            // If the subtype isn't one of the ones we are looking for, skip
            if (!subtypeFilter.Exists((filteredSubtype) => itemIdentifier.Contains(filteredSubtype))) {
                continue;
            }

            // initialize dict slot if we haven't seen this identifier yet
            if (!inventoryDict.ContainsKey(itemIdentifier)) {
                inventoryDict[itemIdentifier] = 0;
            }

            // add amount of this identifier in this invenetory to amount we've already seen
            inventoryDict[itemIdentifier] = inventoryDict[itemIdentifier] + item.Amount;
        }
        
    }

    string typePrefix = "MyObjectBuilder_";

    // Write heading row
    console.WriteText(String.Format("{0,-10}{1,7} {2,7}\n\n", "Material", "Ingots", "Ore"), false);

    // for each material in the subtype list
    foreach(string material in subtypeFilter) {
        // Initialize ore and ingot amount
        MyFixedPoint ore = 0;
        MyFixedPoint ingots = 0;

        // Build identifier strings for material Ore and Ingots
        string oreKey = typePrefix + "Ore:" + material;
        string ingotsKey = typePrefix + "Ingot:" + material;

        // If we have any, grab the amount of Ore we have
        if (inventoryDict.ContainsKey(oreKey)) {
            ore = inventoryDict[oreKey];
        }
        // If we have any grab the amount of ingots we have
        if (inventoryDict.ContainsKey(ingotsKey)) {
            ingots = inventoryDict[ingotsKey];
        }
        
        // Write a line for the material inlcuding the material name and amounts of ingots and ore
        string s = String.Format("{0,-10}{1,7} {2,7}\n", material, formatAmount(ingots), formatAmount(ore));
        console.WriteText(s, true);
    }
}

private string formatAmount(MyFixedPoint amount) {
    if (amount >= 1000000000) {
        return String.Format("{0:F1}g", amount.ToIntSafe() / 1000000000f);
    }
    if (amount >= 1000000) {
        return String.Format("{0:F1}m", amount.ToIntSafe() / 1000000f);
    }
    if (amount >= 1000) {
        return String.Format("{0:F1}k", amount.ToIntSafe() / 1000f);
    }
    return String.Format("{0}", amount.ToIntSafe());
}

public Program()
{
    // Run script once every 100 ticks
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Save()
{
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}
