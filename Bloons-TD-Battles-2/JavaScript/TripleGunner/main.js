function patchDartling(data) {
    const projCount = data.variables.filter(v => v.key == "proj_count")[0];
    projCount.variable.value = 3;
    return { successful: true, data: data };
}

souped.registerJsonPatcher("*", "dartling_gunner.tower_blueprint", patchDartling);
