mod_metadata("name", "Hypersonic Towers")
mod_metadata("author", "Vadmeme")
mod_metadata("version", "1.0.0")

local function patch_tower_blueprint(tower)
    for _, node in pairs(tower["nodes"]) do
        if node["type"] == 4 and node["props"]["reload_time"] ~= nil then
            node["props"]["reload_time"] = tostring(0.05)
        end
    end

    return tower
end

for _, asset in pairs(database.towers) do
    jet_json_patch(asset, patch_tower_blueprint)
end

for _, asset in pairs(database.heros) do
    jet_json_patch(asset, patch_tower_blueprint)
end

