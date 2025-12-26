var _apartment_payment = {

    Initialization: function () {
        this.ActionSearch(1);

        // Gắn format tiền ngay khi popup open
        this.BindAmountFormat();
    },

    GetParam: function () {
        return {
            keyword: $("#txt_shareholder").val(),
            currentPage: 1,
            pageSize: 20
        };
    },

    ActionSearch: function (page) {
        var param = this.GetParam();
        param.currentPage = page;

        $("#loading").show();

        $.post("/ApartmentPayment/Search", param, function (html) {
            $("#loading").hide();
            $("#grid_data").html(html);
        });
    },

    OnPaging: function (page) {
        this.ActionSearch(page);
    },

    OpenAddPopup: function () {
        _ajax_caller.get("/ApartmentPayment/Add", {}, function (html) {
            $("#global_modal_popup .modal-body").html(html);
            $("#global_modal_popup").modal("show");

            // Rebind format tiền vì popup vừa load HTML mới
            _apartment_payment.BindAmountFormat();
        });
    },

    // ============================
    // 🔥 FORMAT TIỀN + CHẶN CHỮ
    // ============================
    BindAmountFormat: function () {
        $("#amount").off("input").on("input", function () {
            let val = $(this).val();

            // CHỈ GIỮ LẠI SỐ
            val = val.replace(/\D/g, "");

            // FORMAT TIỀN VNĐ
            if (val.length > 0) {
                val = parseInt(val).toLocaleString("vi-VN");
            }

            $(this).val(val);
        });
    },

    // ============================
    // 🔥 SAVE PHIẾU CHI
    // ============================
    Save: function () {

        // Lấy số tiền đã format → trả về số raw
        let rawAmount = $("#amount").val().replace(/\./g, "");
        $("#amount").val(rawAmount);

        var form = $("#form_apartment_payment").serialize();

        _ajax_caller.post("/ApartmentPayment/Add", form, function (res) {
            if (res.isSuccess) {
                _msgalert.success(res.message);
                $("#global_modal_popup").modal("hide");
                _apartment_payment.ActionSearch(1);
            } else {
                _msgalert.error(res.message);
            }
        });
    },

    // ============================
    // 🔥 DELETE PHIẾU CHI
    // ============================
    Delete: function (id) {
        if (!confirm("Bạn có chắc muốn xóa phiếu chi này không?")) return;

        _ajax_caller.post("/ApartmentPayment/Delete", { id: id }, function (res) {
            if (res.isSuccess) {
                _msgalert.success(res.message);
                _apartment_payment.ActionSearch(1);
            } else {
                _msgalert.error(res.message);
            }
        });
    }
};

$(document).ready(function () {
    _apartment_payment.Initialization();
});
