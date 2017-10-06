function bindAnchorsToDialog(action, dialogSectionId) {
    $(function() {
        $.ajaxSetup({ cache: false });
        $("a[href*='/" + action + "?']").click(function(e) {
            e.preventDefault();
            $.get(this.href,
                function(data) {
                    $('#' + dialogSectionId).html(data);
                    $('#' + dialogSectionId).modal('show');
                });
        });
    });
}