// --- MessageBox ----

function MessageBox()
{ }

MessageBox.alert = function(title, icon, body, callback) {
    title = title || "Alert";
    icon = icon || "info";
    body = body || "";

    $("#alertDialogTitle").html(title);
    var alertIconClass;
    switch (icon.toLowerCase()) {
        case "info":
            alertIconClass = "fa-info-circle";
            break;
        case "warning":
            alertIconClass = "fa-exclamation-triangle";
            break;
        default:
            alertIconClass = "";
            break;
    }
    $("#alertDialogIcon").addClass("fa").addClass(alertIconClass);
    $("#alertDialogBody").html(body);

    if (callback !== undefined) {
        var okButton = $("#alertDialogOkButton");
        okButton.click(function () {
            callback();
            okButton.off("click");
        });
    }
    $("#alertDialog").modal("show");
}

MessageBox.confirm = function (title, body, callback) {
    title = title || "Confirm";
    body = body || "";
    callback = callback || function() {};

    $("#confirmDialogTitle").html(title);
    $("#confirmDialogBody").html(body);

    var yesButton = $("#confirmDialogYesButton");
    yesButton.click(function () {
        callback(true);
        yesButton.off("click");
    });
    var noButton = $("#confirmDialogNoButton");
    noButton.click(function () {
        callback(false);
        noButton.off("click");
    });

    var dialog = $("#confirmDialog");
    dialog.modal("show");
}

// --- End of MessageBox ----

// --- Clipboard ---

function Clipboard()
{ }

Clipboard.setText = function(text, callbackOk, callbackFail) {
    var tempInput = $("<input>").css({
        position: "absolute",
        left: "-999px"
    }).appendTo($(document.body));
    tempInput.val(text).select();
    try {
        document.execCommand("copy");
        if (callbackOk !== undefined)
            callbackOk();
    } catch (e) {
        if (callbackFail !== undefined)
            callbackFail();
    } finally {
        tempInput.remove();
    }
}

// --- End of Clipboard

// --- Notification ---

function Notification(body) {
    this.body = body;
}

Notification.prototype.success = function() {
    $.notify({
        message: this.body
    },{
        type: "success"
    });
}

Notification.prototype.info = function() {
    $.notify({
        message: this.body
    },{
        type: "info"
    });
}

Notification.prototype.warning = function() {
    $.notify({
        message: this.body
    },{
        type: "warning"
    });
}

Notification.prototype.danger = function() {
    $.notify({
        message: this.body
    },{
        type: "danger"
    });
}

// -- End of Notification

// -- AjaxOperationControl --

function AjaxOperationControl(element) {
    this.element = element;
    this.previousHtml = null;
}

AjaxOperationControl.prototype.disable = function() {
    if (this.element.prop("disabled"))
        return false;
    this.previousHtml = this.element.html();
    this.element.css({
        width: this.element.outerWidth(),
        height: this.element.outerHeight()
    });
    var throbber = $("<img>")
        .attr({
            alt: "...",
            src: "/images/ajax-loader-small.gif"
        })
        .css({
            width: "14px",
            height: "14px"
        });
    this.element.empty().append(throbber).prop("disabled", true);
    return true;
}

AjaxOperationControl.prototype.enable = function() {
    if (!this.element.prop("disabled"))
        return false;
    this.element.css({
        width: null,
        height: null
    });
    this.element.html(this.previousHtml).prop("disabled", false);
    return true;
}

// -- End of AjaxOperationControl --

// *** DOMContentLoaded handler

$(function () {
    // Enable confirmation dialogs on forms which require confirmation
    $("form[data-confirm-body]").submit(function(e) {
        var self = $(this);
        if (self.data("confirmed"))
            return;
        MessageBox.confirm(
            self.data("confirm-title") || "Confirmation",
            self.data("confirm-body"),
            function(result) {
                if (result) {
                    self.data("confirmed", true);
                    self.submit();
                }
            });
        e.preventDefault();
    });

    // Enable sticky behavior of table headers
    $("table.sticky-header").floatThead({
        //useAbsolutePositioning: false,
        scrollingTop: 48
    });

    // Enable throbber on AJAX requests
    $(document).ajaxSend(function (ev, xhr, options) {
        if (!options.disableThrobber)
            $("#throbber").show();
    });

    $(document).ajaxComplete(function (ev, xhr, options) {
        if (!options.disableThrobber)
            $("#throbber").hide();
    });

    // Cancel clicks on the disabled list items
    $("li.disabled, li.active").click(function (e) { e.preventDefault(); });

    // Enable copy to clipboard button behavior
    $("button[data-long-id]").click(function() {
        var self = $(this);
        Clipboard.setText(self.data("long-id"),
            function() {
                self.addClass("btn-success");
                setTimeout(function() { self.removeClass("btn-success"); }, 1200);
            },
            function() {
                self.addClass("btn-danger");
                setTimeout(function() { self.removeClass("btn-danger"); }, 1200);
            });
    });

    // Page-specific event handlers

    // ** Coins Index
    bindDisableButton($("tbody#coins-table"), "coin-name", "coin");
    bindDeleteButton($("tbody#coins-table"), "coin-name", "coin");

    // ** Coins Edit
    $("select#NetworkInfoApiType").change(function() {
        var apiUrlField = $("input#NetworkApiUrl");
        var urlDescription = $("#apiUrlDescription");
        var apiCoinNameField = $("input#NetworkApiName");
        var coinNameDescription = $("#apiCoinNameDescription");

        urlDescription.empty();
        coinNameDescription.empty();
        if (this.selectedIndex < 0)
            return;

        var apiUrlEnabled = false, apiCoinNameEnabled = false;
        var urlDescriptionText = null, coinNameDescriptionText = null;
        switch (this.options[this.selectedIndex].value) {
            case "JsonRpc":
                urlDescriptionText = "Local node URL will be used";
                break;
            case "BchainInfo":
            case "ChainRadar":
            case "ChainzCryptoid":
            case "MinerGate":
            case "Special":
            case "SpecialMulti":
                //do nothing - we already have all required info
                break;
            case "Insight":
                apiUrlEnabled = true;
                urlDescriptionText =
                    "Load the main page of block explorer and find through Fiddler or browser instruments the URL ending with <i>status?q=getInfo</i>";
                break;
            case "Iquidus":
            case "IquidusWithPos":
                apiUrlEnabled = true;
                urlDescriptionText = "URL of the block explorer's main page";
                break;
            case "OpenEthereumPool":
                apiUrlEnabled = true;
                urlDescriptionText = "URL of the 'stats' API method (extract it with Fiddler or browser's network activity recorder)";
                break;
            case "ProHashing":
                apiCoinNameEnabled = true;
                coinNameDescriptionText = "Coin name from selector on <a href='https://prohashing.com/explorer/'>this</a> page";
                break;
            case "TheBlockFactory":
                apiCoinNameEnabled = true;
                coinNameDescriptionText =
                    "First part of the pool host name, example: <i><b>orb</b>.theblocksfactory.com</i>";
                break;
            case "TheCryptoChat":
                apiCoinNameEnabled = true;
                coinNameDescriptionText =
                    "First part of the explorer host name, example: <i><b>honey</b>.thecryptochat.net</i>";
                break;
            case "Altmix":
                apiCoinNameEnabled = true;
                coinNameDescriptionText =
                    "Last section of the explorer URL, example: <i>https://altmix.org/coins/<b>28-Scorecoin</b></i>";
                break;
            default:
                break;
        }

        apiUrlField.prop("disabled", !apiUrlEnabled);
        urlDescription.html(urlDescriptionText);
        apiCoinNameField.prop("disabled", !apiCoinNameEnabled);
        coinNameDescription.html(coinNameDescriptionText);
    });
    $("select#NetworkInfoApiType").change();

    $("select#AddressFormat").change(function() {
        var prefixesField = $("input#AddressPrefixes");
        var addressSampleField = $("#addressSample");

        addressSampleField.empty();
        if (this.selectedIndex < 0)
            return;

        var prefixesEnabled = false;
        var addressSample = null;
        switch (this.options[this.selectedIndex].value) {
            case "Base58Check":
                prefixesEnabled = true;
                addressSample = "BP7Ci6v1S1PDw4i2HFLMv2v9wyBa7mQAgS";
                break;
            case "EthereumHex":
                addressSample = "0x00EB5678228c31e0AC08381d26A00d1eDc9e49b6 (pay attention to letters' case, it serves as a checksum)";
                break;
            case "Special":
                break;
            default:
                break;
        }

        prefixesField.prop("disabled", !prefixesEnabled);
        addressSampleField.text(addressSample);
    });
    $("select#AddressFormat").change();

    // ** Pools Index
    bindDisableButton($("tbody#pools-table"), "pool-name", "pool");
    bindDeleteButton($("tbody#pools-table"), "pool-name", "pool");

    // ** Pools Edit
    $("select#PoolApiProtocol").change(function() {
        var apiUrlField = $("input#ApiUrl");
        var urlDescription = $("#poolApiUrlDescription");
        var apiKeyField = $("input#ApiKey");
        var apiKeyDescription = $("#poolApiKeyDescription");
        var apiPoolNameField = $("input#ApiPoolName");
        var apiPoolNameDescription = $("#poolApiPoolNameDescription");
        var apiUserIdField = $("input#ApiPoolUserId");
        var apiUserIdDescription = $("#poolApiUserIdDescription");

        urlDescription.empty();
        apiKeyDescription.empty();
        apiPoolNameDescription.empty();
        apiUserIdDescription.empty();
        if (this.selectedIndex < 0)
            return;

        var apiUrlEnabled = false, apiKeyEnabled = false, apiPoolNameEnabled = false, apiUserIdEnabled = false;
        var urlDescriptionText = null, apiKeyDescriptionText = null, apiPoolNameDescriptionText = null, apiUserIdDescriptionText = null;
        switch (this.options[this.selectedIndex].value) {
            case "None":
                break;
            case "Qwak":
                apiUrlEnabled = apiKeyEnabled = apiUserIdEnabled = true;
                urlDescriptionText =
                    "Navigate to <i>My Account -> Edit Account</i> and find there a hyperlink with API key. " +
                    "<br />Example: <i>https://oc.suprnova.cc/index.php?page=api&action=getuserstatus&api_key=some-long-number&id=513350</i>" +
                    "<br />This is an API URL (without query string, e.g. only this part: <b>https://oc.suprnova.cc/index.php</b>)";
                apiKeyDescriptionText = "The value of <var>api_key</var> parameter of the API hyperlink URL";
                apiUserIdDescriptionText =
                    "The value of <var>id</var> parameter of the API hyperlink URL. May be empty if there is no such parameter.";
                break;
            case "Tbf":
                apiUrlEnabled = apiKeyEnabled = true;
                urlDescriptionText =
                    "Navigate to <i>My Account -> Account Details</i> and find there a hyperlink with API key. " +
                    "<br />Example: <i>https://orb.theblocksfactory.com/api.php?api_key=some-big-number</i>" +
                    "<br />This is an API URL (without query string, e.g. only this part: <b>https://orb.theblocksfactory.com/api.php</b>)";
                apiKeyDescriptionText = "The value of <var>api_key</var> parameter of the API hyperlink URL";
                break;
            case "OpenEthereum":
                apiUrlEnabled = true;
                urlDescriptionText = ""; //TODO: find how to determine API URL
                break;
            case "Bitfly":
                apiUrlEnabled = true;
                urlDescriptionText = "API URL can be found in the API documentation of the pool";
                break;
            case "NodeOpenMiningPortal":
                apiUrlEnabled = apiPoolNameEnabled = true;
                urlDescriptionText = "URL of the 'API' button or menu item";
                apiPoolNameDescriptionText = "Open 'Tab Stats' page and reference 'Pool Name' column";
                break;
            case "JsonRpcWallet":
                urlDescriptionText = "Local node URL and credentials will be taken from coin options";
                break;
            case "Yiimp":
                apiUrlEnabled = apiPoolNameEnabled = true;
                urlDescriptionText = "Reference the API documentation of the pool (Links section). Example for yiimp.eu pool: <i>http://api.yiimp.eu/api</i>";
                apiPoolNameDescriptionText = "Coin algorithm name. Reference 'Algo' column on the main page. Example: <i>skein</i>";
                break;
            default:
                break;
        }

        apiUrlField.prop("disabled", !apiUrlEnabled);
        urlDescription.html(urlDescriptionText);
        apiKeyField.prop("disabled", !apiKeyEnabled);
        apiKeyDescription.html(apiKeyDescriptionText);
        apiPoolNameField.prop("disabled", !apiPoolNameEnabled);
        apiPoolNameDescription.html(apiPoolNameDescriptionText);
        apiUserIdField.prop("disabled", !apiUserIdEnabled);
        apiUserIdDescription.html(apiUserIdDescriptionText);
    });
    $("select#PoolApiProtocol").change();

    // ** Wallet Index
    bindDisableButton($("tbody#wallets-table"), "wallet-name", "wallet");
    bindDeleteButton($("tbody#wallets-table"), "wallet-name", "wallet");
    //bind 'Set as mining target' button
    bindButtonPostAction($("tbody#wallets-table"),
        "set-as-target-url",
        function(button, data) {
            var row = button.closest("tr");
            new Notification(format("Wallet {0} has been set as mining target for {1}", row.data("wallet-name"), row.data("coin-name")))
                .success();
            $(format("tr[data-coin-id='{0}']", row.data("coin-id")))
                .not(row)
                .remove();
            row.replaceWith(data);
        },
        function(button, error) {
            new Notification(format("Error while setting the new mining target: {0}", error)).danger();
        });
});

function bindDisableButton(table, rowNameKey, entityName) {
    bindButtonPostAction(table,
        "disable-url",
        function(button, data) {
            var row = button.closest("tr");
            new Notification(format("Activity of {0} {1} has been updated", entityName, row.data(rowNameKey)))
                .success();
            row.replaceWith(data);
        },
        function(button, error) {
            new Notification(format("Error while changing {0} status: {1}", entityName, error)).danger();
        });
}

function bindDeleteButton(table, rowNameKey, entityName) {
    var capitalizedEntity = entityName.substr(0, 1).toUpperCase() + entityName.substr(1);
    bindButtonPostAction(table,
        "delete-url",
        function(button) {
            var row = button.closest("tr");
            new Notification(format("{0} {1} has been deleted", capitalizedEntity, row.data(rowNameKey)))
                .success();
            row.remove();
        },
        function(button, error) {
            new Notification(format("Error while deleting {0}: {1}", entityName, error)).danger();
        },
        function(button, callback) {
            var row = button.closest("tr");
            MessageBox.confirm(
                format("Delete {0} {1}?", entityName, row.data(rowNameKey)),
                format("You are going to delete {0} {1}. Are you sure?", entityName, row.data(rowNameKey)),
                function(result) {
                    if (result)
                        callback();
                });
        });
}

// *** End of DOMContentLoaded handler

function bindButtonPostAction(parent, urlAttribute, success, error, preview) {
    parent.on("click",
        format("button[data-{0}]", urlAttribute),
        function(e) {
            var button = $(e.currentTarget);
            var postAction = function() {
                var operationButton = new AjaxOperationControl(button);
                if (!operationButton.disable())
                    return;
                $.ajax({
                        url: button.data(urlAttribute),
                        method: "post",
                        disableThrobber: true
                    })
                    .done(function(data) {
                        success(button, data);
                    })
                    .fail(function(xhr, textStatus, errorThrown) {
                        error(button, errorThrown);
                        operationButton.enable();
                    });
            }
            if (preview === undefined) {
                postAction();
                return;
            }
            preview(button, postAction);
        });
}


function showModal(srcHref, callback) {
    var dialogSection = $("<div>").prependTo("body");
    dialogSection.load(srcHref, null, function() {
        dialogSection.find(".modal").modal("show");
    });
    dialogSection.on("hidden.bs.modal",
        function () {
            if (callback !== undefined)
                callback(dialogSection);
            dialogSection.off();
            dialogSection.remove();
        });
}

function showConfirm(srcHref, callback) {
    showModal(srcHref,
        function(dialog) {
            callback(dialog.find(".modal").data("result") === true);
        });
}

function confirmAndPost(srcHref) {
    showConfirm(srcHref, function (result) {
        if (!result) 
            return;
        $("<form>")
            .appendTo("body")
            .attr({
                action: srcHref,
                method: "post"
            })
            .submit();
    });
}

function format(pattern) {
    arguments.slice = [].slice;
    var formatArgs = arguments.slice(1);
    return pattern.replace(/\{\d+\}/g,
        function(match) {
            var index = parseInt(match.substr(1, match.length - 2));
            return index < formatArgs.length
                ? formatArgs[index].toString()
                : "";
        });
}