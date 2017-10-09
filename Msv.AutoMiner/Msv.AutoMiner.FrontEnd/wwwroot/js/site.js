function bindAnchorsToDialog(action, dialogSectionId) {
    $(function() {
        $.ajaxSetup({ cache: false });
        $("a[href*='/" + action + "?']").click(function(e) {
            e.preventDefault();
            var dialogSection = $('#' + dialogSectionId);
            dialogSection.load(this.href, null, function() { dialogSection.modal('show') });
        });
    });
}