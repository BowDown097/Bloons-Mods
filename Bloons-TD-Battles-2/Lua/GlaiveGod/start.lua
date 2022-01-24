mod_metadata("name", "Glaive God")
mod_metadata("author", "BowDown097")
mod_metadata("version", "1.0.1")

local function patch_orbiting_glaives(tower)
    for _, variable in pairs(tower["variables"]) do
        if variable["key"] == "orbiting_glaives_damage" then
            variable["variable"]["value"] = 10.0
        end
    end
    for _, node in pairs(tower["nodes"]) do
        if node["name"] == "Orbiting Glaives VFX Launcher" then
            node["props"]["emission_props"]["concurrent_projectile_limit"] = 100
        end
        if node["name"] == "LOC_UPGRADE_NAME_Red_Hot_Rangs" then
            for _, modifier in pairs(node["variable_modifiers"]) do
                if modifier["key"] == "orbiting_glaives_damage" then
                    modifier["expression"] = tostring(11.0)
                end
            end
        end
    end

    return tower
end

jet_json_patch("game_data/game_project/assets/towers/boomerang_monkey/data/boomerang_monkey.tower_blueprint", patch_orbiting_glaives)
