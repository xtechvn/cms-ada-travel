var staffFundManagement = {
    Init: function () {
        var self = this;
        var hotelId = $('#mgmt-hotel-id').val();
        var supplierId = $('#mgmt-supplier-id').val();

        // Init DateRangePicker
        $('#mgmt-date-range').daterangepicker({
            autoApply: true,
            locale: { format: 'DD/MM/YYYY', separator: ' - ' },
            startDate: moment().startOf('month'),
            endDate: moment().endOf('month')
        }, function (start, end) {
            self.Search();
        });

        // Load room types if pre-filtered
        if (hotelId > 0 && supplierId > 0) {
            _ajax_caller.post('/UserReserveHotelRoomFund/GetRoomTypeList', { hotelId: hotelId, supplierId: supplierId }, function (response) {
                if (response.status === 0) {
                    var html = '<option value="0">Tất cả hạng phòng</option>';
                    $.each(response.data, function (i, item) {
                        html += '<option value="' + item.id + '">' + item.name + '</option>';
                    });
                    $('#mgmt-room-id').html(html).trigger('change');
                }
            });
        }

        $('.select2').select2();

        $("#mgmt-user-id").select2({
            theme: 'bootstrap4',
            placeholder: "Nhân viên",
            maximumSelectionLength: 1,
            ajax: {
                url: "/Order/UserSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                    }

                    // Query parameters will be ?search=[term]&type=public
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.fullname + ' - ' + item.email,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
        $('#mgmt-user-id, #mgmt-room-id').on('change', function () {
            self.Search();
        });

        // Status filters click
        $('#mgmt-status-filters .status-filter-btn').on('click', function () {
            $('#mgmt-status-filters .status-filter-btn').removeClass('active');
            $(this).addClass('active');
            $('#mgmt-filter-status').val($(this).data('status'));
            self.Search();
        });

        this.Search();
    },

    Search: function () {
        this.OnPaging(1);
    },

    OnPaging: function (page) {
        var self = this;
        var dateRange = $('#mgmt-date-range').val();
        var fromDate = '';
        var toDate = '';
        if (dateRange) {
            var parts = dateRange.split(' - ');
            fromDate = parts[0];
            toDate = parts[1];
        }
        var obj = {
            HotelId: $('#mgmt-hotel-id').val() || 0,
            SupplierId: $('#mgmt-supplier-id').val() || 0,
            HotelRoomId: $('#mgmt-room-id').val() || 0,
            UserId: $('#mgmt-user-id').val() || 0,
            Status: $('#mgmt-filter-status').val() || -1,
            FromDateStr: fromDate,
            ToDateStr: toDate,
            PageIndex: page,
            PageSize: $('#selectPaggingOptions').val() || 10
        };

        _ajax_caller.post('/UserReserveHotelRoomFund/StaffFundManagementSearch', obj, function (result) {
            $('#grid-staff-fund-management').html(result);
        });
    },

    OnSelectPageSize: function () {
        this.Search();
    },

    Edit: function (id) {
        if (typeof userReserveHotelRoomFund !== 'undefined') {
            userReserveHotelRoomFund.AddOrUpdate(id);
        }
    }
};
