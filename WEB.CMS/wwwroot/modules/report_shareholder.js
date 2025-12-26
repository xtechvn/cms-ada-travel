$(document).ready(function () {

    // ✅ ENTER để tìm
    $("#txt-shareholder-search").on("keypress", function (e) {
        if (e.which === 13) {
            _report_shareholder.ActionSearch();
        }
    });

    _report_shareholder.Initialization();
});

var _report_shareholder = {
    Initialization: function () {
        var param = this.GetParam();
        this.Search(param);
    },
    OpenDetail: function (shareHolderId, fullName) {
        debugger
        _ajax_caller.get(
            "/ApartmentReport/Detail",
            { shareHolderId: shareHolderId, fullName: fullName },
            function (html) {
                $("#global_modal_popup .modal-body").html(html);
                $("#global_modal_popup").modal("show");
            }
        );
    },
    GetParam: function () {
        return {
            ShareHolderName: $("#txt-shareholder-search").val(), // ✅ TÊN CỔ ĐÔNG
            currentPage: 1,
            pageSize: 20
        };
    },

    OnPaging: function (value) {
        var obj = this.GetParam();
        obj.currentPage = value;
        this.Search(obj);
    },

    ActionSearch: function () {
        this.OnPaging(1);
    },

    Search: function (input) {
        $("#grid-data").hide();
        $("#imgLoading").show();

        $.ajax({
            url: "/ApartmentReport/Search",
            type: "post",
            data: input,
            success: function (result) {
                $("#imgLoading").hide();
                $("#grid-data").html(result).show();
            }
        });
    }
};
