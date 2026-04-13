var userReserveHotelRoomFund = {
    isSaving: false,

    Init: function () {
        this.Search();
    },

    Search: function () {
        this.ClientOnPaging(1);
    },

    GetGrid: function (obj) {
        _ajax_caller.post('/UserReserveHotelRoomFund/Search', obj, function (result) {
            $('#grid-user-reserve-hotel-room-fund').html(result);
        });
    },

    ClientOnPaging: function (page) {
        var obj = {
            HotelName: $('#search-hotel-name').val(),
            Status: $('#search-status').val(),
            HotelId: $('#detail-hotel-id').val() || 0,
            SupplierId: $('#detail-supplier-id').val() || 0,
            PageIndex: page,
            PageSize: $('#selectPaggingOptions').val() || 20
        };
        this.GetGrid(obj);
    },

    onSelectPageSize: function () {
        this.Search();
    },

    AddOrUpdate: function (id, hotelId = 0, supplierId = 0, hotelRoomId = 0) {
        var title = id > 0 ? "Chỉnh sửa yêu cầu giữ phòng" : "Thêm mới yêu cầu giữ phòng";
        var url = '/UserReserveHotelRoomFund/AddOrUpdate';
        var param = { id: id, hotelId: hotelId, supplierId: supplierId, hotelRoomId: hotelRoomId };
        _magnific.OpenSmallPopup(title, url, param, function () {
            // Callback after load
        });
    },

    InitUpsert: function () {
        var self = this;
        this.isSaving = false;

        // Init Select2 for Hotel
        $('#reserve-hotel-id').select2({
            placeholder: "-- Chọn khách sạn --",
            allowClear: true,
            ajax: {
                url: '/UserReserveHotelRoomFund/GetHotelWithFund',
                type: 'POST',
                data: function (params) {
                    return { name: params.term };
                },
                processResults: function (response) {
                    if (response.status === 0) {
                        return { results: $.map(response.data, function (item) { return { text: item.name, id: item.id }; }) };
                    }
                    return { results: [] };
                }
            }
        }).on('select2:select', function (e) {
            self.LoadSuppliers(e.params.data.id);
        }).on('select2:unselect', function () {
            $('#reserve-supplier-id').val('').html('<option value="">-- Chọn khách sạn trước --</option>').prop('disabled', true);
            $('#div-room-type-table, #div-quick-apply').hide();
            $('#table-reserve-room-types tbody').empty();
        });

        $('#reserve-supplier-id').select2({ placeholder: "-- Chọn nhà cung cấp --" }).on('change', function () {
            var val = $(this).val();
            if (val) {
                $('#div-room-type-table, #div-quick-apply').show();
                $('#table-reserve-room-types tbody').empty();
                self.LoadQuickRoomTypes();
            } else {
                $('#div-room-type-table, #div-quick-apply').hide();
            }
        });

        // Quick Apply Date Range
        $('#quick-date-range').daterangepicker({
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' },
            minDate: moment()
        });

        // If pre-filled, trigger change
        if ($('#reserve-supplier-id').val()) {
            $('#reserve-supplier-id').trigger('change');
        }
    },

    LoadSuppliers: function (hotelId) {
        _ajax_caller.post('/UserReserveHotelRoomFund/GetSupplierByHotel', { hotelId: hotelId }, function (response) {
            if (response.status === 0) {
                var html = '<option value="">-- Chọn nhà cung cấp --</option>';
                $.each(response.data, function (i, item) {
                    html += '<option value="' + item.id + '">' + item.name + '</option>';
                });
                $('#reserve-supplier-id').html(html).prop('disabled', false).select2();
            }
        });
    },

    LoadQuickRoomTypes: function () {
        var hotelId = $('#reserve-hotel-id').val();
        var supplierId = $('#reserve-supplier-id').val();
        _ajax_caller.post('/UserReserveHotelRoomFund/GetRoomTypeList', { hotelId: hotelId, supplierId: supplierId }, function (response) {
            if (response.status === 0) {
                var roomOptions = '<option value="">-- Chọn hạng phòng --</option>';
                $.each(response.data, function (i, item) {
                    roomOptions += '<option value="' + item.id + '">' + item.name + '</option>';
                });
                $('#quick-room-type').html(roomOptions).select2({ placeholder: "-- Chọn hạng phòng --" });
                
                // Pre-select if needed
                var preRoomId = $('#pre-hotel-room-id').val();
                if (preRoomId && preRoomId > 0) {
                    $('#quick-room-type').val(preRoomId).trigger('change');
                }
            }
        });
    },

    QuickAdd: function () {
        var roomId = $('#quick-room-type').val();
        var roomName = $('#quick-room-type option:selected').text();
        var dateRangeVal = $('#quick-date-range').val();
        if (!dateRangeVal) { _msgalert.error("Vui lòng chọn dải ngày"); return; }

        var dateRange = dateRangeVal.split(' - ');
        var startDate = moment(dateRange[0], 'DD/MM/YYYY');
        var endDate = moment(dateRange[1], 'DD/MM/YYYY');
        var count = parseInt($('#quick-room-count').val());
        
        if (!roomId) { _msgalert.error("Vui lòng chọn hạng phòng"); return; }
        if (isNaN(count) || count <= 0) { _msgalert.error("Số lượng phòng không hợp lệ"); return; }

        var isDup = false;
        $('#table-reserve-room-types tbody tr.room-row').each(function() {
             var rId = $(this).find('.room-select').val();
             var rsStr = $(this).find('.start-date').val();
             var reStr = $(this).find('.end-date').val();
             if (rId == roomId && rsStr == dateRange[0].trim() && reStr == dateRange[1].trim()) {
                 isDup = true;
                 return false;
             }
        });

        if (!isDup) {
            this.AddRoomTypeRowWithData(roomId, roomName, dateRange[0].trim(), dateRange[1].trim(), count);
        }
    },

    AddRoomTypeRow: function () {
        var hotelId = $('#reserve-hotel-id').val();
        var supplierId = $('#reserve-supplier-id').val();
        var self = this;

        _ajax_caller.post('/UserReserveHotelRoomFund/GetRoomTypeList', { hotelId: hotelId, supplierId: supplierId }, function (response) {
            if (response.status === 0) {
                var roomOptions = '<option value="">-- Hạng phòng --</option>';
                $.each(response.data, function (i, item) {
                    roomOptions += '<option value="' + item.id + '">' + item.name + '</option>';
                });

                var html = '<tr class="room-row">';
                html += '<td><select class="form-control select2 room-select" style="width:100%">' + roomOptions + '</select></td>';
                html += '<td><input type="text" class="form-control datepicker-reserve start-date" value="' + moment().format('DD/MM/YYYY') + '" /></td>';
                html += '<td><input type="text" class="form-control datepicker-reserve end-date" value="' + moment().add(1, 'days').format('DD/MM/YYYY') + '" /></td>';
                html += '<td class="text-center avail-count">0</td>';
                html += '<td><input type="number" class="form-control room-count" min="1" value="1" /></td>';
                html += '<td class="msg-note text-danger" style="font-weight:bold;"></td>';
                html += '<td><button type="button" class="btn btn-danger btn-sm" onclick="$(this).closest(\'tr\').remove()"><i class="fa fa-times"></i></button></td>';
                html += '</tr>';

                $('#table-reserve-room-types tbody').append(html);
                var newRow = $('#table-reserve-room-types tbody tr:last');
                
                newRow.find('.select2').select2({ placeholder: "-- Hạng phòng --" });
                newRow.find('.datepicker-reserve').daterangepicker({ singleDatePicker: true, autoApply: true, locale: { format: 'DD/MM/YYYY' } });

                newRow.find('.room-select, .start-date, .end-date, .room-count').on('change', function () {
                    self.CheckAvailability(newRow);
                });
                
                self.CheckAvailability(newRow);
            }
        });
    },

    AddRoomTypeRowWithData: function (roomId, roomName, startDate, endDate, count) {
        var self = this;
        var html = '<tr class="room-row">';
        html += '<td><input type="hidden" class="room-select" value="' + roomId + '" />' + roomName + '</td>';
        html += '<td><input type="text" class="form-control datepicker-reserve start-date" value="' + startDate + '" /></td>';
        html += '<td><input type="text" class="form-control datepicker-reserve end-date" value="' + endDate + '" /></td>';
        html += '<td class="text-center avail-count">0</td>';
        html += '<td><input type="number" class="form-control room-count" min="1" value="' + count + '" /></td>';
        html += '<td class="msg-note text-danger" style="font-weight:bold;"></td>';
        html += '<td><button type="button" class="btn btn-danger btn-sm" onclick="$(this).closest(\'tr\').remove()"><i class="fa fa-times"></i></button></td>';
        html += '</tr>';

        $('#table-reserve-room-types tbody').append(html);
        var newRow = $('#table-reserve-room-types tbody tr:last');
        
        newRow.find('.datepicker-reserve').daterangepicker({ singleDatePicker: true, autoApply: true, locale: { format: 'DD/MM/YYYY' } });

        newRow.find('.start-date, .end-date, .room-count').on('change', function () {
            self.CheckAvailability(newRow);
        });
        
        this.CheckAvailability(newRow);
    },

    CheckAvailability: function (row) {
        var hotelId = $('#reserve-hotel-id').val();
        var supplierId = $('#reserve-supplier-id').val();
        var roomId = row.find('.room-select').val();
        var start = row.find('.start-date').val();
        var end = row.find('.end-date').val();
        var requestedCount = parseInt(row.find('.room-count').val());

        if (hotelId && supplierId && roomId && start && end) {
            _ajax_caller.post('/UserReserveHotelRoomFund/GetRoomAvailability', {
                hotelId: hotelId,
                supplierId: supplierId,
                hotelRoomId: roomId,
                startDate: start,
                endDate: end
            }, function (response) {
                if (response.status === 0) {
                    var avail = response.totalAvailable;
                    row.find('.avail-count').text(avail);
                    var msgNote = row.find('.msg-note');
                    
                    if (requestedCount > avail || avail === 0) {
                        row.css('color', 'red').addClass('has-error');
                        row.find('input').css('color', 'red');
                        msgNote.text('Hết phòng');
                    } else {
                        row.css('color', '').removeClass('has-error');
                        row.find('input').css('color', '');
                        msgNote.text('');
                    }
                }
            });
        }
    },

    Save: function () {
        if (this.isSaving) return;

        var hotelId = $('#reserve-hotel-id').val();
        var supplierId = $('#reserve-supplier-id').val();

        if (!hotelId || !supplierId) {
            _msgalert.error("Vui lòng chọn khách sạn và nhà cung cấp");
            return;
        }

        var rows = $('#table-reserve-room-types tbody tr.room-row');
        if (rows.length === 0) {
            _msgalert.error("Vui lòng chọn ít nhất một hạng phòng");
            return;
        }

        var requests = [];
        var hasError = false;
        var uniqueKeys = {};

        rows.each(function () {
            var row = $(this);
            var roomId = row.find('.room-select').val();
            var count = parseInt(row.find('.room-count').val());
            var avail = parseInt(row.find('.avail-count').text());
            var start = row.find('.start-date').val();
            var end = row.find('.end-date').val();

            if (!roomId) {
                _msgalert.error("Vui lòng chọn hạng phòng");
                hasError = true; return false;
            }
            
            // Duplicate check
            var key = roomId + "_" + start;
            if (uniqueKeys[key]) {
                 _msgalert.error("Hạng phòng và ngày " + start + " bị trùng trong danh sách");
                 row.css('background', '#ffebeb');
                 hasError = true; return false;
            }
            uniqueKeys[key] = true;

            if (isNaN(count) || count <= 0) {
                _msgalert.error("Số lượng phòng tại ngày " + start + " không hợp lệ");
                hasError = true; return false;
            }
            if (count > avail) {
                _msgalert.error("Số lượng phòng tại ngày " + start + " không được vượt quá số lượng trống (" + avail + ")");
                hasError = true; return false;
            }

            requests.push({
                HotelId: hotelId,
                SupplierId: supplierId,
                HotelRoomId: roomId,
                NumberOfRooms: count,
                StartDate: start,
                EndDate: end
            });
        });

        if (hasError) return;

        this.isSaving = true;
        var btnLuu = $('.btn-success[onclick*="Save"]');
        btnLuu.prop('disabled', true).addClass('disabled').html('<i class="fa fa-spinner fa-spin"></i> Đang lưu...');

        var self = this;
        var successCount = 0;
        
        // Use a small delay to ensure UI updates before synchronous AJAX if needed, 
        // though async: false is used here.
        setTimeout(function() {
            $.each(requests, function (i, model) {
                $.ajax({
                    url: '/UserReserveHotelRoomFund/Save',
                    type: 'POST',
                    data: model,
                    async: false,
                    success: function (result) {
                        if (result.status === 0) successCount++;
                        else _msgalert.error(result.msg);
                    },
                    error: function() {
                        _msgalert.error("Lỗi hệ thống khi lưu yêu cầu tại ngày " + model.StartDate);
                    }
                });
            });

            if (successCount === requests.length) {
                _msgalert.success("Giữ phòng thành công");
                $.magnificPopup.close();
                userReserveHotelRoomFund.Search();
            } else {
                self.isSaving = false;
                btnLuu.prop('disabled', false).removeClass('disabled').html('Lưu');
            }
        }, 100);
    }
};

$(document).ready(function () {
    userReserveHotelRoomFund.Init();
});
