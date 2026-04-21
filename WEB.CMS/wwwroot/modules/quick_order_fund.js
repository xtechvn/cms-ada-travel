var _quick_order_fund = {
    Init: function () {
        this.InitSelect2();
        this.InitEvents();
    },

    InitSelect2: function () {
        var parent = $('.quick-order-fund');
        // Client Suggestions
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

        // Staff Suggestions
        $(".main-staff-select, #quick-order-sub-staff, #quick-service-operator").select2({
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
    },

    InitEvents: function () {
        var self = this;

        $('body').off('click', '#btn-add-quick-guest');
        $('body').on('click', '#btn-add-quick-guest', function () {
            self.AddGuestRow();
        });

        $('body').off('click', '.btn-delete-guest');
        $('body').on('click', '.btn-delete-guest', function () {
            $(this).closest('tr').remove();
            self.UpdateGuestOrder();
        });

        $('body').off('click', '#btn-submit-quick-order');
        $('body').on('click', '#btn-submit-quick-order', function () {
            self.Submit();
        });

        $('body').off('click', '.btn-add-package');
        $('body').on('click', '.btn-add-package', function () {
            var row = $(this).closest('tr');
            var packageDiv = row.find('.quick-room-package-div:last');
            var clone = packageDiv.clone();
            clone.find('input').val('');
            clone.attr('data-fund-id', '0');
            clone.find('.btn-delete-package').show();
            row.find('.servicemanual-hotel-room-td-rates-code').append(clone);
            
            // Show the delete button for the first package if it was hidden
            row.find('.btn-delete-package').show();

            // Add corresponding divs in other columns using exact standard classes
            row.find('.servicemanual-hotel-room-td-rates-daterange').append(row.find('.quick-room-package-daterange-div:first').clone());
            row.find('.servicemanual-hotel-room-td-rates-operator-price').append(row.find('.quick-room-package-import-price-div:first').clone().find('input').val('').end());
            row.find('.servicemanual-hotel-room-td-rates-sale-price').append(row.find('.quick-room-package-export-price-div:first').clone().find('input').val('').end());
            row.find('.servicemanual-hotel-room-td-rates-nights').append(row.find('.quick-room-package-nights-div:first').clone());
            
            // Financial columns
            row.find('.servicemanual-hotel-room-td-rates-operator-amount').append(row.find('.quick-room-package-total-import-div:first').clone().find('input').val('0').end());
            row.find('.servicemanual-hotel-room-td-rates-total-amount').append(row.find('.quick-room-package-total-export-div:first').clone().find('input').val('0').end());
            row.find('.servicemanual-hotel-room-td-rates-profit').append(row.find('.quick-room-package-profit-div:first').clone().find('input').val('0').end());

            self.CalculateTotal();
        });

        $('body').off('click', '.btn-delete-package');
        $('body').on('click', '.btn-delete-package', function () {
            var div = $(this).closest('.quick-room-package-div');
            var index = div.index();
            var row = div.closest('tr');
            div.remove();
            row.find('.servicemanual-hotel-room-td-rates-daterange .quick-room-package-daterange-div').eq(index).remove();
            row.find('.servicemanual-hotel-room-td-rates-operator-price .quick-room-package-import-price-div').eq(index).remove();
            row.find('.servicemanual-hotel-room-td-rates-sale-price .quick-room-package-export-price-div').eq(index).remove();
            row.find('.servicemanual-hotel-room-td-rates-nights .quick-room-package-nights-div').eq(index).remove();
            
            row.find('.servicemanual-hotel-room-td-rates-operator-amount .quick-room-package-total-import-div').eq(index).remove();
            row.find('.servicemanual-hotel-room-td-rates-total-amount .quick-room-package-total-export-div').eq(index).remove();
            row.find('.servicemanual-hotel-room-td-rates-profit .quick-room-package-profit-div').eq(index).remove();

            self.CalculateTotal();
        });

        $('body').off('click', '.btn-delete-room');
        $('body').on('click', '.btn-delete-room', function () {
            $(this).closest('tr').remove();
            self.UpdateRoomOrder();
            self.CalculateTotal();
        });

        // Guest Import
        $('body').off('change', '.import_data_guest');
        $('body').on('change', '.import_data_guest', function () {
            var element = $(this);
            if (element[0].files && element[0].files.length > 0) {
                var formData = new FormData();
                formData.append("file", element[0].files[0]);
                formData.append("service_type", "1");

                $.ajax({
                    url: "/Order/GetGuestFromFile",
                    type: "post",
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.status == 0 && result.data && result.data.length > 0) {
                            $('#table-quick-order-guests tbody').empty();
                            $(result.data).each(function (index, item) {
                                self.AddGuestRow(item);
                            });
                            _msgalert.success(result.msg);
                        } else {
                            _msgalert.error(result.msg || "Lỗi khi nhập danh sách khách");
                        }
                    }
                });
            }
            element.val('');
        });

        // Currency formatting and calculation
        $('body').off('blur', '.currency');
        $('body').on('blur', '.currency', function () {
            var val = $(this).val().replace(/,/g, '');
            if (!isNaN(val) && val !== '') {
                $(this).val(parseFloat(val).toLocaleString('en-US'));
            }
            self.CalculateTotal();
        });

        $('body').off('keyup', '.quick-room-import-price, .quick-room-export-price');
        $('body').on('keyup', '.quick-room-import-price, .quick-room-export-price', function () {
            self.CalculateTotal();
        });

        $('body').off('keyup', '#quick-service-other-amount, #quick-service-commission');
        $('body').on('keyup', '#quick-service-other-amount, #quick-service-commission', function () {
            self.CalculateTotal();
        });

        $('.datepicker-input').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' },
            showDropdowns: true
        });
    },

    CalculateTotal: function () {
        var totalImport = 0;
        var totalExport = 0;
        var totalProfit = 0;

        $('#table-quick-order-rooms tbody tr').each(function () {
            var row = $(this);
            var roomCount = parseInt(row.find('.quick-room-count').val()) || 0;
            var rowImport = 0;
            var rowExport = 0;

            row.find('.quick-room-package-div').each(function (index) {
                var nights = parseInt(row.find('.quick-room-package-nights').eq(index).val()) || 0;
                var importPrice = parseFloat(row.find('.quick-room-import-price').eq(index).val().replace(/,/g, '')) || 0;
                var exportPrice = parseFloat(row.find('.quick-room-export-price').eq(index).val().replace(/,/g, '')) || 0;

                rowImport += importPrice * roomCount * nights;
                rowExport += exportPrice * roomCount * nights;
            });

            var rowProfit = rowExport - rowImport;

            row.find('.quick-room-total-import').val(rowImport.toLocaleString('en-US'));
            row.find('.quick-room-total-export').val(rowExport.toLocaleString('en-US'));
            row.find('.quick-room-profit').val(rowProfit.toLocaleString('en-US'));

            totalImport += rowImport;
            totalExport += rowExport;
            totalProfit += rowProfit;
        });

        $('.quick-room-total-import-final').text(totalImport.toLocaleString('en-US'));
        $('.quick-room-total-export-final').text(totalExport.toLocaleString('en-US'));
        $('.quick-room-total-profit-final').text(totalProfit.toLocaleString('en-US'));

        var otherAmount = parseFloat($('#quick-service-other-amount').val().replace(/,/g, '')) || 0;
        var commission = parseFloat($('#quick-service-commission').val().replace(/,/g, '')) || 0;

        var finalAmount = totalExport + otherAmount;
        var finalProfit = totalProfit + otherAmount - commission;

        $('.quick-total-amount').text(finalAmount.toLocaleString('en-US'));
        $('.quick-total-profit').text(finalProfit.toLocaleString('en-US'));
    },

    AddGuestRow: function (data) {
        var rowCount = $('#table-quick-order-guests tbody tr').length;
        var firstRow = $('#table-quick-order-guests tbody tr:first');
        var roomOptions = firstRow.find('.quick-guest-room').html();
        
        var name = data ? data.name : '';
        var birthday = data ? data.birthday : '';
        var type = data ? data.type : 1;
        var note = data ? data.note : '';

        var html = '<tr class="quick-guest-row">';
        html += '<td class="quick-guest-order">' + (rowCount + 1) + '</td>';
        html += '<td><input type="text" class="form-control quick-guest-name" placeholder="Tên khách" value="' + name + '"></td>';
        html += '<td><select class="form-control quick-guest-type">';
        html += '<option value="1" ' + (type == 1 ? 'selected' : '') + '>Người lớn</option>';
        html += '<option value="2" ' + (type == 2 ? 'selected' : '') + '>Trẻ em</option>';
        html += '<option value="3" ' + (type == 3 ? 'selected' : '') + '>Em bé</option>';
        html += '</select></td>';
        html += '<td><input type="text" class="form-control datepicker-input quick-guest-birthday" placeholder="dd/mm/yyyy" value="' + birthday + '"></td>';
        html += '<td><select class="form-control quick-guest-room">' + roomOptions + '</select></td>';
        html += '<td><input type="text" class="form-control quick-guest-note" value="' + note + '"></td>';
        html += '<td><a href="javascript:;" class="red btn-delete-guest"><i class="fa fa-times"></i></a></td>';
        html += '</tr>';

        $('#table-quick-order-guests tbody').append(html);
        
        $('.datepicker-input').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' },
            showDropdowns: true
        });
    },

    UpdateGuestOrder: function () {
        $('#table-quick-order-guests tbody tr').each(function (index) {
            $(this).find('.quick-guest-order').text(index + 1);
        });
    },

    UpdateRoomOrder: function () {
        $('#table-quick-order-rooms tbody tr').each(function (index) {
            $(this).find('.quick-room-order').text(index + 1);
        });
    },

    Submit: function () {
        var self = this;
        var orderData = {
            ClientId: $('#quick-order-client').val(),
            SalerId: $('#quick-order-main-staff').val(),
            OperatorId: $('#quick-service-operator').val(),
            Label: $('#quick-order-label').val(),
            Note: $('#quick-order-note').val(),
            HotelId: $('#quick-service-hotel-id').val(),

            Adult: $('#quick-service-people').val(),
            Child: 0,
            Infant: 0,
            OtherAmount: $('#quick-service-other-amount').val().replace(/,/g, ''),
            Commission: $('#quick-service-commission').val().replace(/,/g, ''),

            Rooms: [],
            Guests: []
        };

        if (!orderData.ClientId) { _msgalert.error("Vui lòng chọn khách hàng"); return; }
        if (!orderData.SalerId) { _msgalert.error("Vui lòng chọn nhân viên phụ trách"); return; }
        if (!orderData.OperatorId) { _msgalert.error("Vui lòng chọn điều hành viên"); return; }
        if (!orderData.Label) { _msgalert.error("Vui lòng nhập nhãn đơn"); return; }

        var hasPriceError = false;
        $('#table-quick-order-rooms tbody tr').each(function () {
            var row = $(this);
            var roomId = row.data('room-id');
            var roomName = row.find('.quick-room-name').val();
            var roomCount = parseInt(row.find('.quick-room-count').val()) || 0;

            var roomObj = {
                RoomId: roomId,
                RoomName: roomName,
                NumberOfRooms: roomCount,
                Packages: []
            };

            row.find('.quick-room-package-div').each(function (index) {
                var pkgDiv = $(this);
                var fundId = pkgDiv.data('fund-id') || 0;
                var packageName = pkgDiv.find('.quick-room-package-name').val();
                
                var daterange = row.find('.quick-room-package-daterange').eq(index).val();
                var start = "";
                var end = "";
                if (daterange && daterange.includes('-')) {
                    var parts = daterange.split('-');
                    start = parts[0].trim();
                    end = parts[1].trim();
                }

                var importPrice = row.find('.quick-room-import-price').eq(index).val().replace(/,/g, '');
                var exportPrice = row.find('.quick-room-export-price').eq(index).val().replace(/,/g, '');

                if (!importPrice || isNaN(importPrice.replace(/,/g, ''))) {
                    _msgalert.error("Vui lòng nhập giá nhập cho tất cả hạng phòng");
                    hasPriceError = true; return false;
                }
                if (!exportPrice || isNaN(exportPrice.replace(/,/g, ''))) {
                    _msgalert.error("Vui lòng nhập giá bán cho tất cả hạng phòng");
                    hasPriceError = true; return false;
                }

                roomObj.Packages.push({
                    FundId: fundId,
                    PackageName: packageName,
                    ImportPrice: parseFloat(importPrice),
                    ExportPrice: parseFloat(exportPrice),
                    StartDate: start,
                    EndDate: end
                });
            });

            if (hasPriceError) return false;
            orderData.Rooms.push(roomObj);
        });

        if (hasPriceError) return;

        $('#table-quick-order-guests tbody tr').each(function () {
            var row = $(this);
            var name = row.find('.quick-guest-name').val();
            if (name) {
                var birthdayVal = row.find('.quick-guest-birthday').val();
                orderData.Guests.push({
                    Name: name,
                    Type: row.find('.quick-guest-type').val(),
                    Birthday: birthdayVal,
                    RoomId: row.find('.quick-guest-room').val(),
                    Note: row.find('.quick-guest-note').val()
                });
            }
        });

        var btn = $('#btn-submit-quick-order');
        btn.prop('disabled', true).addClass('disabled').append(' <i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            url: '/UserReserveHotelRoomFund/SubmitQuickOrder',
            type: 'post',
            data: { data: orderData, fundIds: $('#quick-order-fund-ids').val() },
            success: function (response) {
                if (response.status === 0) {
                    _msgalert.success(response.msg);
                    $.magnificPopup.close();
                    if (typeof userReserveHotelRoomFund !== 'undefined') {
                        userReserveHotelRoomFund.Search();
                    }
                } else {
                    _msgalert.error(response.msg);
                    btn.prop('disabled', false).removeClass('disabled').find('i').remove();
                }
            },
            error: function () {
                _msgalert.error("Lỗi hệ thống khi tạo đơn hàng.");
                btn.prop('disabled', false).removeClass('disabled').find('i').remove();
            }
        });
    }
};
