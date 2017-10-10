function bindAnchorsToModal(action) {
    $(function() {
        $.ajaxSetup({ cache: false });
        $("a[href*='/" + action + "?']").click(function(e) {
            e.preventDefault();
            showModal(this.href);
        });
    });
}

function showModal(srcHref, callback) {
    var dialogSection = $("<div>").prependTo("body");
    dialogSection.load(srcHref, null, function() {
        dialogSection.find(".modal").modal("show");
    });
    dialogSection.on("hidden.bs.modal",
        function () {
            if (callback != undefined)
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

$(function () {
    $(document).ajaxSend(function () {
        $('#throbber').show();
    });

    $(document).ajaxComplete(function () {
        $('#throbber').hide();
    });
})