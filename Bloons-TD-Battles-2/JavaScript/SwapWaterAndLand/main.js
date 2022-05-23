function patchTowerBlueprint(data) {
    for (const node in data.nodes.filter(n => n.type == 0 && n.props?.placement !== null)) {
        node.props.placement.range = 0.0;
        node.props.placement.blocks_other_towers = false;

        if (node.props.placement.supported_area == "Land") {
            node.props.placement.supported_area = "Water";
        }
        else if (node.props.placement.supported_area == "Water") {
            node.props.placement.supported_area = "Land";
        }
    }

    return { successful: true, data: data };
}

souped.registerJsonPatcher(patchTowerBlueprint, "*.tower_blueprint");
souped.registerJsonPatcher(patchTowerBlueprint, "*.hero_blueprint");
