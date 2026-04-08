var hotelRoomFund = {
    InitIndex: function () {
        $("#token-input-supplier").select2({
            placeholder: "Chọn nhà cung cấp",
            ajax: {
                url: "/Supplier/Suggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        text: params.term,
                        size: 10
                    };
                },
                processResults: function (data) {
                    return {
                        results: $.map(data, function (item) {
                            return {
                                text: item.name,
                                id: item.id
                            }
                        })
                    };
                },
                cache: true
            }
        });

        $("#token-input-hotel").select2({
            placeholder: "Chọn khách sạn",
            ajax: {
                url: "/Hotel/GetSuggestionHotel",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        name: params.term,
                        size: 10
                    };
                },
                processResults: function (data) {
                    return {
                        results: $.map(data, function (item) {
                            return {
                                text: item.name,
                                id: item.hotelid
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    Search: function () {
        var searchModel = {
            SupplierId: $('#token-input-supplier').val(),
            HotelId: $('#token-input-hotel').val(),
            PageIndex: 1,
            PageSize: 10
        };
        _ajax_caller.post('/HotelRoomFund/Search', searchModel, function (result) {
            $('#grid-data').html(result);
        });
    },
    AddOrUpdate: function (id) {
        let title = id > 0 ? 'Cập nhật Quỹ phòng khách sạn' : 'Thêm mới Quỹ phòng khách sạn';
        let url = '/HotelRoomFund/AddOrUpdate';
        let param = { id: id };
        _magnific.OpenSmallPopup(title, url, param, function () {
            hotelRoomFund.InitUpsert();
        });
    },
    InitUpsert: function () {
        var elPopup = $('#magnific-popup-small');
        
        $("#upsert-supplier-id").select2({
            placeholder: "Chọn nhà cung cấp",
            dropdownParent: elPopup,
            ajax: {
                url: "/Supplier/Suggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        text: params.term,
                        size: 10
                    };
                },
                processResults: function (data) {
                    return {
                        results: $.map(data, function (item) {
                            return {
                                text: item.name,
                                id: item.id
                            }
                        })
                    };
                },
                cache: true
            }
        }).on('change', function () {
            hotelRoomFund.LoadHotelBySupplier($(this).val());
        });

        $("#upsert-hotel-id").select2({
            placeholder: "Chọn khách sạn"
        }).on('change', function () {
            var hotelId = $(this).val();
            var supplierId = $("#upsert-supplier-id").val();
            var isEdit = $('#form-hotel-room-fund input[name="Id"]').val() > 0;

            if (hotelId && supplierId && !isEdit) {
                _ajax_caller.post('/HotelRoomFund/CheckExistence', { hotelId: hotelId, supplierId: supplierId }, function (result) {
                    if (result.status == 1) {
                        _msgalert.error(result.msg);
                        $("#upsert-hotel-id").val('').trigger('change');
                    } else {
                        hotelRoomFund.LoadRoomTypesByHotel(hotelId);
                    }
                });
            } else {
                hotelRoomFund.LoadRoomTypesByHotel(hotelId);
            }
        });

        $("#select-room-type-dropdown").select2({
            placeholder: "Chọn hạng phòng",
            dropdownParent: elPopup
        }).on('change', function () {
            var val = $(this).val();
            var text = $("#select-room-type-dropdown option:selected").text();
            if (val) {
                hotelRoomFund.renderRoomTypeGroup(val, text);
                if (window.hotelRoomFundExisting && window.hotelRoomFundExisting.length) {
                    var existed = window.hotelRoomFundExisting.filter(function (x) { return x.HotelRoomId == parseInt(val); });
                    if (existed && existed.length > 0) {
                        hotelRoomFund.PopulateExistingDetails(val, existed);
                    }
                }
                $(this).find('option[value="' + val + '"]').remove();
                $(this).val('').trigger('change');
            }
        });

        if ($("#upsert-hotel-id").val() > 0) {
            hotelRoomFund.LoadRoomTypesByHotel($("#upsert-hotel-id").val());
        }

        $('.datepicker').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: {
                format: 'DD/MM/YYYY'
            }
        });
    },
    LoadHotelBySupplier: function (supplierId) {
        if (!supplierId || supplierId == 0) {
            $('#div-upsert-hotel').hide();
            $('#upsert-hotel-id').html('<option value="">Chọn khách sạn</option>').trigger('change');
            return;
        }
        $('#div-upsert-hotel').show();
        _ajax_caller.post('/HotelRoomFund/GetHotelBySupplier', { supplierId: supplierId }, function (result) {
            if (result.status == 0) {
                var options = '<option value="">Chọn khách sạn</option>';
                $.each(result.data, function (i, item) {
                    options += '<option value="' + item.id + '">' + item.name + '</option>';
                });
                $('#upsert-hotel-id').html(options).trigger('change');
            }
        });
    },
    LoadRoomTypesByHotel: function (hotelId) {
        $('#room-fund-details-container').empty();
        $('#select-room-type-dropdown').html('<option value="">-- Chọn hạng phòng --</option>').trigger('change');
        if (!hotelId) {
            $('#div-select-room-type').hide();
            return;
        }

        _ajax_caller.post('/HotelRoomFund/GetRoomTypeByHotel', { hotelId: hotelId }, function (result) {
            if (result.status == 0) {
                $('#div-select-room-type').show();
                var options = '<option value="">-- Chọn hạng phòng --</option>';
                $.each(result.data, function (i, roomType) {
                    options += '<option value="' + roomType.id + '">' + roomType.name + '</option>';
                });
                $('#select-room-type-dropdown').html(options).trigger('change');
            }
        });
    },
    renderRoomTypeGroup: function (roomTypeId, roomTypeName) {
        var groupHtml = `
            <div class="room-type-group" data-room-type-id="${roomTypeId}">
                <h5 class="room-type-name">
                    <span>${roomTypeName}</span>
                    <button type="button" class="btn btn-xs btn-danger" onclick="hotelRoomFund.RemoveRoomTypeGroup(this, '${roomTypeId}', '${roomTypeName}')"><i class="fa fa-times"></i> Xóa nhóm</button>
                </h5>
                <div class="quick-add-section">
                    <span class="title"><i class="fa fa-bolt"></i> Thêm nhanh (Tự động chia ngày)</span>
                    <div class="row">
                        <div class="col-md-3">
                            <input type="number" class="form-control qa-amount" placeholder="Số lượng phòng" />
                        </div>
                        <div class="col-md-6">
                            <input type="text" class="form-control qa-date-range" placeholder="Thời gian chung" />
                        </div>
                        <div class="col-md-3">
                            <button type="button" class="btn btn-sm btn-info" onclick="hotelRoomFund.QuickAdd(this)">Áp dụng</button>
                        </div>
                    </div>
                    <div class="special-dates-container mt10">
                        <label style="font-size: 12px; color: #666;">Ngày đặc biệt (Giá khác)</label>
                        <div class="special-dates-list"></div>
                        <button type="button" class="btn btn-xs btn-default mt5" onclick="hotelRoomFund.AddSpecialDateRow(this)"><i class="fa fa-plus"></i> Thêm ngày đặc biệt</button>
                    </div>
                </div>
                <div class="room-fund-details-list">
                    <!-- Dòng chi tiết sẽ được sinh ra ở đây -->
                </div>
                <div class="mt10 text-right">
                    <button type="button" class="btn btn-xs btn-success" onclick="hotelRoomFund.AddDetailRow(this)"><i class="fa fa-plus"></i> Thêm dòng thủ công</button>
                </div>
            </div>
        `;
        $('#room-fund-details-container').append(groupHtml);
        
        var lastGroup = $('#room-fund-details-container .room-type-group:last');
        lastGroup.find('.qa-date-range').daterangepicker({
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' }
        });
    },
    AddSpecialDateRow: function (btn) {
        var list = $(btn).closest('.special-dates-container').find('.special-dates-list');
        var rowHtml = `
            <div class="special-date-item">
                <input type="number" class="form-control sd-amount" style="width: 100px;" placeholder="Số lượng phòng" />
                <input type="text" class="form-control sd-date-range" style="width: 220px;" placeholder="Thời gian" />
                <i class="fa fa-trash text-danger" style="cursor:pointer" onclick="$(this).parent().remove()"></i>
            </div>
        `;
        list.append(rowHtml);
        list.find('.special-date-item:last .sd-date-range').daterangepicker({
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' }
        });
    },
    RemoveRoomTypeGroup: function (btn, id, name) {
        _msgconfirm.openDialog("Xác nhận?", "Bạn có chắc muốn xóa hạng phòng này?" , function () {
             $(btn).closest('.room-type-group').remove();
            var option = new Option(name, id, false, false);
            $('#select-room-type-dropdown').append(option).trigger('change');
         });
    },
    QuickAdd: function (btn) {
        var group = $(btn).closest('.room-type-group');
        var roomTypeId = group.data('room-type-id');
        var amount = group.find('.qa-amount').val();
        var dateRange = group.find('.qa-date-range').val();

        if (!amount || !dateRange) {
            _msgalert.error("Vui lòng nhập số tiền và thời gian chung");
            return;
        }

        var startGeneral = moment(dateRange.split(' - ')[0], 'DD/MM/YYYY');
        var endGeneral = moment(dateRange.split(' - ')[1], 'DD/MM/YYYY');

        // Kiểm tra xem khoảng thời gian chung có bị chồng lấn với các dòng hiện có không
        var isExistingOverlap = false;
        group.find('.room-fund-details-list .detail-row').each(function () {
            var sStr = $(this).find('input[name="StartDate"]').val();
            var eStr = $(this).find('input[name="EndDate"]').val();
            if (sStr && eStr) {
                var s = moment(sStr, 'DD/MM/YYYY');
                var e = moment(eStr, 'DD/MM/YYYY');
                if (startGeneral.isSameOrBefore(e) && endGeneral.isSameOrAfter(s)) {
                    isExistingOverlap = true;
                    return false;
                }
            }
        });
        if (isExistingOverlap) {
            _msgalert.error("Khoảng thời gian Thêm nhanh bị chồng lấn với dữ liệu hiện có của hạng phòng này.");
            return;
        }

        var specialDates = [];
        var isSpecialOverlap = false;
        group.find('.special-date-item').each(function () {
            var sdAmount = $(this).find('.sd-amount').val();
            var sdRange = $(this).find('.sd-date-range').val();
            if (sdAmount && sdRange) {
                var sdStart = moment(sdRange.split(' - ')[0], 'DD/MM/YYYY');
                var sdEnd = moment(sdRange.split(' - ')[1], 'DD/MM/YYYY');
                
                // Kiểm tra xem Ngày đặc biệt có nằm ngoài khoảng thời gian chung không
                if (sdStart.isBefore(startGeneral) || sdEnd.isAfter(endGeneral)) {
                    _msgalert.error("Ngày đặc biệt phải nằm trong khoảng thời gian chung.");
                    isSpecialOverlap = true;
                    return false;
                }

                // Kiểm tra chồng lấn giữa các Ngày đặc biệt
                for (var sd of specialDates) {
                    if (sdStart.isSameOrBefore(sd.end) && sdEnd.isSameOrAfter(sd.start)) {
                        _msgalert.error("Các Ngày đặc biệt không được chồng lấn thời gian với nhau.");
                        isSpecialOverlap = true;
                        return false;
                    }
                }

                specialDates.push({
                    amount: sdAmount,
                    start: sdStart,
                    end: sdEnd
                });
            }
        });

        if (isSpecialOverlap) return;

        // Sắp xếp ngày đặc biệt theo thời gian bắt đầu
        specialDates.sort((a, b) => a.start - b.start);

        var finalRanges = [];
        var currentStart = startGeneral;

        for (var sd of specialDates) {
            // Nếu ngày đặc biệt bắt đầu sau mốc hiện tại, thêm khoảng trống với giá chung
            if (sd.start.isAfter(currentStart)) {
                finalRanges.push({
                    amount: amount,
                    start: currentStart.clone(),
                    end: sd.start.clone().subtract(1, 'days')
                });
            }
            // Thêm ngày đặc biệt
            finalRanges.push({
                amount: sd.amount,
                start: sd.start.clone(),
                end: sd.end.clone()
            });
            currentStart = sd.end.clone().add(1, 'days');
        }

        // Nếu còn khoảng trống đến cuối mốc chung
        if (currentStart.isSameOrBefore(endGeneral)) {
            finalRanges.push({
                amount: amount,
                start: currentStart.clone(),
                end: endGeneral.clone()
            });
        }

        // Render ra các dòng detail-row
        var detailsList = group.find('.room-fund-details-list');
        // detailsList.empty(); // Bỏ dòng này để QuickAdd luôn thêm dòng mới, không xóa dòng cũ

        for (var range of finalRanges) {
            var rowHtml = `
                <div class="detail-row">
                    <input type="hidden" name="HotelRoomId" value="${roomTypeId}" />
                    <input type="number" class="form-control" name="Amount" value="${range.amount}" />
                    <input type="text" class="form-control datepicker" name="StartDate" value="${range.start.format('DD/MM/YYYY')}" />
                    <input type="text" class="form-control datepicker" name="EndDate" value="${range.end.format('DD/MM/YYYY')}" />
                    <button type="button" class="btn btn-xs btn-danger remove-detail-btn" onclick="hotelRoomFund.RemoveDetailRow(this)"><i class="fa fa-trash"></i></button>
                </div>
            `;
            detailsList.append(rowHtml);
        }

        detailsList.find('.datepicker').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' }
        }).on('apply.daterangepicker', function () {
            if (hotelRoomFund.CheckOverlap(roomTypeId)) {
                _msgalert.error("Thời gian bạn vừa chọn bị chồng lấn với các khoảng thời gian khác của hạng phòng này.");
            }
        });

        _msgalert.success("Đã áp dụng và chia ngày tự động thành công");
    },
    PopulateExistingDetails: function (roomTypeId, details) {
        var group = $('.room-type-group[data-room-type-id="' + roomTypeId + '"]');
        var detailsList = group.find('.room-fund-details-list');
        for (var i = 0; i < details.length; i++) {
            var d = details[i];
            var rowHtml = '<div class="detail-row">' +
                '<input type="hidden" name="HotelRoomId" value="' + roomTypeId + '" />' +
                '<input type="number" class="form-control" name="Amount" value="' + d.Amount + '" />' +
                '<input type="text" class="form-control datepicker" name="StartDate" value="' + d.StartDate + '" />' +
                '<input type="text" class="form-control datepicker" name="EndDate" value="' + d.EndDate + '" />' +
                '<button type="button" class="btn btn-xs btn-danger remove-detail-btn" onclick="hotelRoomFund.RemoveDetailRow(this)"><i class="fa fa-trash"></i></button>' +
                '</div>';
            detailsList.append(rowHtml);
        }
        detailsList.find('.datepicker').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' }
        }).on('apply.daterangepicker', function () {
            if (hotelRoomFund.CheckOverlap(roomTypeId)) {
                _msgalert.error("Thời gian bạn vừa chọn bị chồng lấn với các khoảng thời gian khác của hạng phòng này.");
            }
        });
    },
    ClearDetails: function () {
        $('#room-fund-details-container').empty();
    },
    AddDetailRow: function (btn) {
        var roomTypeGroup = $(btn).closest('.room-type-group');
        var roomTypeId = roomTypeGroup.data('room-type-id');
        var detailsList = roomTypeGroup.find('.room-fund-details-list');
        
        var newRowHtml = `
            <div class="detail-row">
                <input type="hidden" name="HotelRoomId" value="${roomTypeId}" />
                <input type="number" class="form-control" name="Amount" value="0" />
                <input type="text" class="form-control datepicker" name="StartDate" />
                <input type="text" class="form-control datepicker" name="EndDate" />
                <button type="button" class="btn btn-xs btn-danger remove-detail-btn" onclick="hotelRoomFund.RemoveDetailRow(this)"><i class="fa fa-trash"></i></button>
            </div>
        `;
        
        detailsList.append(newRowHtml);
        var newRow = detailsList.find('.detail-row:last');

        newRow.find('.datepicker').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' }
        }).on('apply.daterangepicker', function () {
            if (hotelRoomFund.CheckOverlap(roomTypeId)) {
                _msgalert.error("Thời gian bạn vừa chọn bị chồng lấn với các khoảng thời gian khác của hạng phòng này.");
            }
        });
    },
    RemoveDetailRow: function (btn) {
        $(btn).closest('.detail-row').remove();
    },
    CheckOverlap: function (roomTypeId) {
        var ranges = [];
        var isOverlap = false;
        var group = $('.room-type-group[data-room-type-id="' + roomTypeId + '"]');
        
        group.find('.detail-row').each(function () {
            var startStr = $(this).find('input[name="StartDate"]').val();
            var endStr = $(this).find('input[name="EndDate"]').val();
            
            if (startStr && endStr) {
                var start = moment(startStr, 'DD/MM/YYYY');
                var end = moment(endStr, 'DD/MM/YYYY');
                
                for (var r of ranges) {
                    if (start.isSameOrBefore(r.end) && end.isSameOrAfter(r.start)) {
                        isOverlap = true;
                        return false;
                    }
                }
                ranges.push({ start: start, end: end });
            }
        });
        
        return isOverlap;
    },
    Save: function () {
        var form = $('#form-hotel-room-fund');
        if (!form.valid()) return;

        var details = [];
        var hasOverlap = false;
        
        $('#room-fund-details-container .room-type-group').each(function () {
            var roomTypeGroup = $(this);
            var roomTypeId = roomTypeGroup.data('room-type-id');
            var roomTypeName = roomTypeGroup.find('.room-type-name span').text();
            
            if (hotelRoomFund.CheckOverlap(roomTypeId)) {
                _msgalert.error("Hạng phòng '" + roomTypeName + "' có khoảng thời gian bị chồng lấn. Vui lòng kiểm tra lại.");
                hasOverlap = true;
                return false;
            }

            roomTypeGroup.find('.detail-row').each(function () {
                var row = $(this);
                details.push({
                    HotelRoomId: row.find('input[name="HotelRoomId"]').val(),
                    Amount: row.find('input[name="Amount"]').val(),
                    StartDate: row.find('input[name="StartDate"]').val(),
                    EndDate: row.find('input[name="EndDate"]').val()
                });
            });
        });

        if (hasOverlap) return;

        if (details.length === 0) {
            _msgalert.error("Vui lòng thêm ít nhất một dòng chi tiết quỹ phòng.");
            return;
        }

        var model = {
            Id: form.find('input[name="Id"]').val(),
            SupplierId: form.find('input[name="SupplierId"]').length ? form.find('input[name="SupplierId"]').val() : form.find('select[name="SupplierId"]').val(),
            HotelId: form.find('input[name="HotelId"]').length ? form.find('input[name="HotelId"]').val() : form.find('select[name="HotelId"]').val(),
            Details: details
        };

        _ajax_caller.post('/HotelRoomFund/Save', { model: model, details: details }, function (result) {
            if (result.status == 0) {
                _msgalert.success(result.msg);
                $.magnificPopup.close();
                hotelRoomFund.Search();
            } else {
                _msgalert.error(result.msg);
            }
        });
    }
};

$(document).ready(function () {
    hotelRoomFund.InitIndex();
    hotelRoomFund.Search();
});
