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


    // Page-specific event handlers

    // ** Coins Index
    bindButtonPostAction($("tbody#coins-table"),
        "disable-url",
        function(button, data) {
            var row = button.closest("tr");
            new Notification(format("Activity of coin {0} has been updated", row.data("coin-name")))
                .success();
            row.replaceWith(data);
        },
        function(button, error) {
            new Notification("Error while changing coin status: " + error).danger();
        });

    bindButtonPostAction($("tbody#coins-table"),
        "delete-url",
        function(button) {
            var row = button.closest("tr");
            new Notification(format("Coin {0} has been deleted", row.data("coin-name")))
                .success();
            row.remove();
        },
        function(button, error) {
            new Notification("Error while changing coin status: " + error).danger();
        },
        function(button, callback) {
            var row = button.closest("tr");
            MessageBox.confirm(
                format("Delete coin {0}?", row.data("coin-name")),
                format("You are going to delete coin {0}. Are you sure?", row.data("coin-name")),
                function(result) {
                    if (result)
                        callback();
                });
        });

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

});

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