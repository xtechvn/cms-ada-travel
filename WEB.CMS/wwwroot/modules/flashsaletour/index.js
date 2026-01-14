var _flashsale_tour = {
    Init: function () {
        this.modal = $('#add-flashsale');
        this.flashSaleId = parseInt(this.modal.attr('data-id') || '0');
        this.SearchParam = {
            TourName: "",
            TourType: "",
            OrganizingType: "",
            SupplierIds: "",
            Star: "",
            IsDisplayWeb: -1,
            StartPoint: "",
            Endpoint: "",
            IsSelfDesign: false,
            PageIndex: 1,
            PageSize: 10
        };

        this.BindEvents();
    },

    BindEvents: function () {
        var self = this;

        // Click "Tìm"
        $(document).on('click', '#add-flashsale-search-confirm', function () {
            self.OnSearch();
        });

        // Enter trong ô keyword -> search
        $(document).on('keyup', '#add-flashsale-search-name', function (e) {
            if (e.which === 13) self.OnSearch();
        });

        // Nhập lại
        $(document).on('click', '#add-flashsale-search-clear', function () {
            $('#add-flashsale-search-name').val('');
            $('#add-flashsale-search-display').val('-1').trigger('change');

            self.SearchParam.PageIndex = 1;
            self.SearchParam.TourName = '';
            self.SearchParam.IsDisplayWeb = -1;

            $('#flashsale-tour-tbody').html('');
            $('#check-all-tour').prop('checked', false);
        });

        // Check all
        $(document).on('change', '#check-all-tour', function () {
            $('#flashsale-tour-tbody .check-tour').prop('checked', this.checked);
        });

        // Nếu user bỏ tick 1 item thì bỏ tick "check all"
        $(document).on('change', '#flashsale-tour-tbody .check-tour', function () {
            if (!this.checked) $('#check-all-tour').prop('checked', false);
        });

        // Hủy
        $(document).on('click', '#add-flashsale-btn-cancel', function () {
            self.modal.modal('hide');
        });

        // Xác nhận
        $(document).on('click', '#add-flashsale-btn-confirm', function () {
            self.OnConfirm();
        });
    },

    GetSearchParamFromModal: function () {
        var keyword = $('#add-flashsale-search-name').val() || '';
        var display = $('#add-flashsale-search-display').val();
        display = (display === null || display === undefined || display === "") ? -1 : parseInt(display);

        var obj = this.SearchParam;
        obj.TourName = keyword;
        obj.IsDisplayWeb = display;
        return obj;
    },

    Search: function (input) {
        debugger
        // Endpoint là action bạn đã đổi: ProductFlashSaleSearch(TourProductSearchModel)
        // Thay controller path cho đúng hệ thống của bạn:
        var url = "/FlashSale/ProductFlashSaleSearch";

        _ajax_caller.post(url, input, function (result) {
            debugger
            // result là HTML partial rows
            $('#flashsale-tour-tbody').html(result);

            // reset check all sau khi load list mới
            $('#check-all-tour').prop('checked', false);
        });
    },

    OnSearch: function () {
        var obj = this.GetSearchParamFromModal();
        obj.PageIndex = 1;
        this.SearchParam = obj;
        this.Search(obj);
    },

    OnPaging: function (page) {
        var obj = this.SearchParam;
        obj.PageIndex = page;
        this.SearchParam = obj;
        this.Search(obj);
    },

    GetSelectedTours: function () {
        var selected = [];

        $('#flashsale-tour-tbody .check-tour:checked').each(function () {
            var $tr = $(this).closest('tr');
            selected.push({
                id: $tr.data('id'),
                code: $tr.data('code'),
                name: $tr.data('name'),
                price: $tr.data('price')
            });
        });

        return selected;
    },

    OnConfirm: function () {
        var selected = this.GetSelectedTours();

        if (!selected || selected.length === 0) {
            _msgalert.error("Bạn chưa chọn tour nào.");
            return;
        }

        // 1) Nếu bạn muốn lưu vào hidden input để submit form:
        // $('#SelectedToursJson').val(JSON.stringify(selected));

        // 2) Nếu bạn muốn gọi API add tour vào flashsale (khuyến nghị):
        // Thay URL backend đúng của bạn (ví dụ /FlashSale/AddTours)
        var url = "/FlashSale/AddTourToFlashSale";

        _ajax_caller.post(url, {
            flashSaleId: this.flashSaleId,
            tours: selected
        }, function (res) {
            if (res && res.isSuccess) {
                _msgalert.success(res.message || "Đã thêm tour vào FlashSale");
                $('#add-flashsale').modal('hide');

                // nếu có grid ngoài cần reload:
                // _flashsale.Reload();
            } else {
                _msgalert.error((res && res.message) ? res.message : "Thao tác thất bại");
            }
        });
    }
};

// gọi init sau khi document ready
$(document).ready(function () {
    // nếu modal này có mặt trong DOM
    if ($('#add-flashsale').length) {
        _flashsale_tour.Init();
    }

    // optional: select2 cho display
    if ($('#add-flashsale-search-display').length) {
        $('#add-flashsale-search-display').select2({
            placeholder: "Hiển thị",
            minimumResultsForSearch: Infinity
        });
    }
});
