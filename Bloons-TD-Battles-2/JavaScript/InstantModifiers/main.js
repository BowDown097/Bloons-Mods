function patchGameRules(data) {
    data.bloon_send_round_unlock_camo = 1;
    data.bloon_send_round_unlock_fortified = 1;
    data.bloon_send_round_unlock_regen = 1;
    return { successful: true, data: data };
}

souped.registerJsonPatcher(patchGameRules, "*.gamerules");
