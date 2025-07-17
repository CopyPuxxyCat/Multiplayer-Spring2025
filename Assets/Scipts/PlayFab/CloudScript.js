// Grant 500 currency at the start of match
handlers.grantInitialMoney = function (args, context) {
    var request = {
        PlayFabId: currentPlayerId,
        VirtualCurrency: "CO",
        Amount: 500
    };
    var result = server.AddUserVirtualCurrency(request);
    return {
        message: "Granted 500 CO to player",
        details: result
    };
};

// Spend currency during upgrade
handlers.spendMoney = function (args, context) {
    if (!args.amount) {
        throw "Missing 'amount' argument.";
    }

    var request = {
        PlayFabId: currentPlayerId,
        VirtualCurrency: "CO",
        Amount: args.amount
    };

    var result = server.SubtractUserVirtualCurrency(request);
    return {
        message: `Spent ${args.amount} CO`,
        details: result
    };
};

// Reset currency to 500
handlers.resetMoney = function (args, context) {
    var getBalance = server.GetUserInventory({ PlayFabId: currentPlayerId });
    var currentBalance = getBalance.VirtualCurrency["CO"] || 0;

    if (currentBalance > 500) {
        server.SubtractUserVirtualCurrency({
            PlayFabId: currentPlayerId,
            VirtualCurrency: "CO",
            Amount: currentBalance - 500
        });
    } else if (currentBalance < 500) {
        server.AddUserVirtualCurrency({
            PlayFabId: currentPlayerId,
            VirtualCurrency: "CO",
            Amount: 500 - currentBalance
        });
    }

    return {
        message: "Reset CO to 500"
    };
};

// Check balance (can be used if needed)
handlers.getMoney = function (args, context) {
    var result = server.GetUserInventory({ PlayFabId: currentPlayerId });
    return {
        CO: result.VirtualCurrency["CO"] || 0
    };
};
