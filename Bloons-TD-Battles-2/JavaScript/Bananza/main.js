function patchBloonsets(data) {
    for (let i = 0; i < data.length; i++) {
        data[i]["Eco Change"] *= 2;
    }

    return { successful: true, data: data };
}

function patchFarm(data) {
    const brf = data.nodes.filter(n => n.name == "LOC_UPGRADE_NAME_Banana_Research_Facility")[0];
    const cenMarket = data.nodes.filter(n => n.name == "LOC_UPGRADE_NAME_Central_Market")[0];

    const baseValue = data.variables.filter(v => v.key == "banana_value")[0];
    const brfValue = brf.variable_modifiers.filter(m => m.key == "banana_value")[0];
    const cenMarketValue = cenMarket.variable_modifiers.filter(m => m.key == "banana_value")[0];

    baseValue.variable.value *= 2;
    brfValue.expression *= 2;
    cenMarketValue.expression *= 2;

    return { successful: true, data: data };
}

function patchGameRules(data) {
    data.starting_cash *= 2;
    data.starting_income *= 2;
    return { successful: true, data: data };
}

souped.registerCsvPatcher("*", "bloon_sets.bloonset", patchBloonsets);
souped.registerJsonPatcher("*", "banana_farm.tower_blueprint", patchFarm);
souped.registerJsonPatcher("*", "*.gamerules", patchGameRules);
