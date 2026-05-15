var _flight_warehouse_hold_ticket_index = {
    Init: function () {
        $("#CreateBy").select2({
            theme: 'bootstrap4',
            placeholder: "Người tạo",
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
        $("#search-departure").select2({
            theme: 'bootstrap4',
            placeholder: "Điểm đi",
            maximumSelectionLength: 1,
            ajax: {
                url: "/FlightWarehouse/AirPortCodeSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.code + '(' + item.districtVi + ')',
                                id: item.code,
                            }
                        })
                    };
                },
            }
        });
        $("#search-arrival").select2({
            theme: 'bootstrap4',
            placeholder: "Điểm đến",

            ajax: {
                url: "/FlightWarehouse/AirPortCodeSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.code + '(' + item.districtVi + ')',
                                id: item.code,
                            }
                        })
                    };
                },
            }
        });
        this.Search();
    },
    Search: function () {
        var obj = {
            CreatedBy: $('#CreateBy').val(),
            DeparturePoint: $('#search-departure').val(),
            ArrivalPoint: $('#search-arrival').val(),
            Status: $('#search-status').val(),
            GroupObject: $('#search-group-object').val(),
            Date: $('#Date').val(),
            pageIndex: 1,
            pageSize: $("#selectPaggingOptions").find(':selected').val() == undefined ? 20 : $("#selectPaggingOptions").find(':selected').val()
        };
        $.ajax({
            url: "/FlightWarehouseHoldTicket/Search",
            type: "POST",
            data: obj,
            success: function (result) {
                $('#grid-data').html(result);
                $('#selectPaggingOptions').val(obj.pageSize).attr("selected", "selected");
            }
        });
    },
    OnPaging: function (pageIndex) {
        var obj = {
            CreatedBy: $('#CreateBy').val(),
            DeparturePoint: $('#search-departure').val(),
            ArrivalPoint: $('#search-arrival').val(),
            Status: $('#search-status').val(),
            GroupObject: $('#search-group-object').val(),
            Date: $('#Date').val(),
            pageIndex: pageIndex,
            pageSize: $("#selectPaggingOptions").find(':selected').val() == undefined ? 20 : $("#selectPaggingOptions").find(':selected').val()
        };
        $.ajax({
            url: "/FlightWarehouseHoldTicket/Search",
            type: "POST",
            data: obj,
            success: function (result) {
                $('#grid-data').html(result);
                $('#selectPaggingOptions').val(obj.pageSize).attr("selected", "selected");
            }
        });
    },
    onSelectPageSize: function () {
        this.Search();
    },
    QuickOrderManual: function (id, bookingId) {
        // Mở popup tạo đơn hàng nhanh dựa theo AddFlyBookingService
        let title = 'Tạo đơn hàng nhanh (Vé máy bay)';
        let url = '/FlightWarehouseHoldTicket/QuickOrderManual';
        let param = {
            id: id,
            bookingId: bookingId
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    InitQuickOrderSelect2: function () {
        var parent = $('#quick-order-fly-modal');

        $("#quick-order-client").select2({
            theme: 'bootstrap4',
            placeholder: "Tìm kiếm khách hàng",
            minimumInputLength: 1,
            dropdownParent: parent,
            ajax: {
                url: "/Order/ClientSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { txt_search: params.term };
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.clientname + ' - ' + item.email + ' - ' + item.phone,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });

        $(".main-staff-select, #quick-service-operator").select2({
            theme: 'bootstrap4',
            placeholder: "Chọn nhân viên",
            dropdownParent: parent,
            ajax: {
                url: "/Order/UserSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { txt_search: params.term };
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
        $("#quick-order-branch").select2({
            theme: 'bootstrap4',
            dropdownParent: parent
        });
    },
    InitQuickOrderManual: function () {
        _flight_warehouse_hold_ticket_index.InitQuickOrderSelect2();

        // Setup price calculations
        $('.quick-order-price-row input').on('input', function () {
            _flight_warehouse_hold_ticket_index.CalculateQuickOrder();
        });

        // Initial calc
        _flight_warehouse_hold_ticket_index.CalculateQuickOrder();
    },
    CalculateQuickOrder: function () {
        var totalAmount = 0;
        var totalProfit = 0;

        $('.quick-order-price-row').each(function () {
            var row = $(this);
            var qty = parseInt(row.find('.qty-input').val()) || 0;
            var net = parseFloat(row.find('.price-net').val().replace(/,/g, '')) || 0;
            var sell = parseFloat(row.find('.price-sell').val().replace(/,/g, '')) || 0;

            var rowAmount = qty * sell;
            var rowProfit = qty * (sell - net);

            row.find('.row-total').text(rowAmount.toLocaleString('en-US'));

            totalAmount += rowAmount;
            totalProfit += rowProfit;
        });

        $('#quick-order-total-amount').text(totalAmount.toLocaleString('en-US') + ' VND');
        $('#quick-order-total-profit').text(totalProfit.toLocaleString('en-US') + ' VND');
    },
    SubmitQuickOrder: function () {
        var orderData = {
            HoldTicketId: $('#quick-order-hold-ticket-id').val(),
            BookingId: $('#quick-order-booking-id').val(),
            ClientId: $('#quick-order-client').val(),
            SalerId: $('#quick-order-main-staff').val(),
            OperatorId: $('#quick-service-operator').val(),
            BranchCode: $('#quick-order-branch').val(),
            Label: $('#quick-order-label').val(),
            Note: $('#quick-order-note').val(),
            Amount: parseFloat($('#quick-order-total-amount').text().replace(/,/g, '')) || 0,
            Profit: parseFloat($('#quick-order-total-profit').text().replace(/,/g, '')) || 0,
            OthersAmount: $('#quick-order-other-amount').val() ? parseFloat($('#quick-order-other-amount').val().replace(/,/g, '')) : 0,
            Commission: $('#quick-order-commission').val() ? parseFloat($('#quick-order-commission').val().replace(/,/g, '')) : 0,

            AdultQuantity: parseInt($('#quick-order-adt-qty').val()) || 0,
            AdultPrice: parseFloat($('#quick-order-adt-price').val().replace(/,/g, '')) || 0,
            AdultAmount: parseFloat($('#quick-order-adt-amount').val().replace(/,/g, '')) || 0,

            ChildQuantity: parseInt($('#quick-order-chd-qty').val()) || 0,
            ChildtPrice: parseFloat($('#quick-order-chd-price').val().replace(/,/g, '')) || 0,
            ChildAmount: parseFloat($('#quick-order-chd-amount').val().replace(/,/g, '')) || 0,

            InfantQuantity: parseInt($('#quick-order-inf-qty').val()) || 0,
            InfantPrice: parseFloat($('#quick-order-inf-price').val().replace(/,/g, '')) || 0,
            InfantAmount: parseFloat($('#quick-order-inf-amount').val().replace(/,/g, '')) || 0
        };

        if (!orderData.ClientId) { _msgalert.error("Vui lòng chọn khách hàng"); return; }

        if (!orderData.OperatorId) { _msgalert.error("Vui lòng chọn điều hành viên"); return; }
        if (!orderData.Label) { _msgalert.error("Vui lòng nhập nhãn đơn"); return; }

        var btn = $('#btn-submit-quick-order-fly');
        btn.prop('disabled', true).addClass('disabled').append(' <i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            url: '/FlightWarehouseHoldTicket/SubmitQuickOrder',
            type: 'post',
            data: orderData,
            success: function (response) {
                if (response.status === 0) {
                    _msgalert.success(response.msg);
                    $.magnificPopup.close();
                    _flight_warehouse_hold_ticket_index.Search();
                } else {
                    _msgalert.error(response.msg);
                    btn.prop('disabled', false).removeClass('disabled').find('i.fa-spinner').remove();
                }
            },
            error: function () {
                _msgalert.error("Lỗi hệ thống khi tạo đơn hàng.");
                btn.prop('disabled', false).removeClass('disabled').find('i.fa-spinner').remove();
            }
        });
    }
};
