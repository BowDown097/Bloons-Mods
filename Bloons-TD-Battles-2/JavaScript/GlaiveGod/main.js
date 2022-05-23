function patch(data) {
    let glaiveDamage = data.variables.filter((v) => v.key == "orbiting_glaives_damage")[0];
    let glaiveLauncher = data.nodes.filter((n) => n.name == "Orbiting Glaives VFX Launcher")[0];
    let rhr = data.nodes.filter((n) => n.name == "LOC_UPGRADE_NAME_Red_Hot_Rangs")[0];
    let rhrGlaiveDamage = rhr.variable_modifiers.filter((m) => m.key == "orbiting_glaives_damage")[0];

    glaiveDamage.variable.value = 10.0;
    glaiveLauncher.props.emission_props.concurrent_projectile_limit = 100;
    rhrGlaiveDamage.expression = "11.0";

    return { successful: true, data: data };
}

souped.registerJsonPatcher(patch, "boomerang_monkey.tower_blueprint");
