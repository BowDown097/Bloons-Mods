function patchBloonsets(data) {
    for (let i = 0; i < data.length; i++) {
        const bloon = data[i];
        if (bloon["Min Round"] >= 3) {
            bloon["Min Round"] -= 2;
        }
    }

    return { successful: true, data: data };
}

function patchLoadouts(data) {
    data.shift();
    data.shift();

    for (let i = 0; i < data.length; i++) {
        const loadout = data[i];
        loadout["Round Number"] -= 2;
    }

    return { successful: true, data: data };
}

function patchComponents(data) {
    for (const child in data.root.entity_children) {
        const speed = child.bloon.blueprint.props.speed;
        if (speed !== null) {
            speed *= 1.4;
        }
    }

    return { successful: true, data: data };
}

function patchGameRules(data) {
    data.eco_tick_time = 4.0;
    return { successful: true, data: data };
}

souped.registerCsvPatcher(patchBloonsets, "bloon_sets.bloonset");
souped.registerCsvPatcher(patchLoadouts, "bloon_set_loadouts.bloonset_loadouts");
souped.registerJsonPatcher(patchComponents, "bloon_components.scene");
souped.registerJsonPatcher(patchGameRules, "*.gamerules");
