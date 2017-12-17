﻿// --- MessageBox ----

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

    $("#confirmDialog").modal("show");
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
});

// *** End of DOMContentLoaded handler

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