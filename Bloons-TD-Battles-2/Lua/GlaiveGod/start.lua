mod_metadata("name", "Glaive God")

local function patch_glaive_count(tower)
    for _, node in pairs(tower["nodes"]) do
        if node["name"] == "Orbiting Glaives VFX Launcher" then
            node["props"]["emission_props"]["concurrent_projectile_limit"] = 10000
        end
    end

    return tower
end

jet_json_patch("game_data/game_project/assets/towers/boomerang_monkey/data/boomerang_monkey.tower_blueprint", patch_glaive_count)
