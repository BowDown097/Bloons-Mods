mod_metadata("name", "Speed Battles")
mod_metadata("author", "BowDown097")
mod_metadata("version", "1.0.1")

local function patch_bloonsets(data)
    for i = 2, #data do
        if tonumber(data[i][4]) >= 3 then
            data[i][4] = tostring(tonumber(data[i][4]) - 2)
        end
    end

    return data
end

local function patch_components(document)
    local children = document:get("/root/entity_children")
    for id, child in pairs(children) do
        local speed = document:get("/root/entity_children/" .. tostring(id - 1) .. "/bloon/blueprint/props/speed")
        if speed ~= nil then
            document:set("/root/entity_children/" .. tostring(id - 1) .. "/bloon/blueprint/props/speed", speed * 1.4)
        end
    end
end

local function patch_game_rules(document)
    document:set("/eco_tick_time", 4.0)
end

local function patch_loadouts(data)
    table.remove(data, 2)
    table.remove(data, 2)
    for i = 2, #data do
        data[i][1] = tostring(tonumber(data[i][1]) - 2)
    end

    return data
end

jet_csv_patch("game_data/game_project/assets/data/bloon_sets.bloonset", patch_bloonsets)
jet_csv_patch("game_data/game_project/assets/data/bloon_set_loadouts.bloonset_loadouts", patch_loadouts)
jet_json_advanced_patch("game_data/game_project/assets/bloons/scenes/bloon_components.scene", patch_components)
jet_json_advanced_patch("game_data/game_project/assets/data/default.gamerules", patch_game_rules)
jet_json_advanced_patch("game_data/game_project/assets/data/default_private.gamerules", patch_game_rules)
