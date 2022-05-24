function patch(data) {
    for (const node in data.nodes) {
        if node.type == 4 && node.props.reload_time !== null) {
            node.props.reload_time = "0.05";
        }
    }

    return { successful: true, data: data };
}

souped.registerJsonPatcher("*", "*.tower_blueprint", patch);
