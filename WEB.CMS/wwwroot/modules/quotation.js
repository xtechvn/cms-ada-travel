var _quotation = {

    // =====================================================================
    // INDEX PAGE
    // =====================================================================
    InitIndex: function () {
        // Datepicker cho filter
        $('.datepicker').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            locale: { format: 'DD/MM/YYYY' }
        });

        // Select2 cho saler
        $('#sl_search_saler').select2({
            placeholder: 'Tất cả người phụ trách',
            ajax: {
                url: '/User/Suggest',
                type: 'post',
                dataType: 'json',
                delay: 250,
                data: function (params) { return { text: params.term, size: 10 }; },
                processResults: function (data) {
                    return {
                        results: $.map(data, function (item) {
                            return { text: item.name, id: item.id };
                        })
                    };
                },
                cache: true
            }
        });

        // Card thống kê click lọc
        $(document).on('click', '#quotation_stats_cards .card-stats', function () {
            $('#quotation_stats_cards .card-stats').removeClass('active');
            $(this).addClass('active');
            var status = $(this).data('status');
            $('#sl_search_status').val(status);
            _quotation.Search(1);
        });

        // Nút tìm kiếm
        $('#btn_search_quotation').on('click', function () { _quotation.Search(1); });
        $('#ip_search_query').on('keypress', function (e) { if (e.which === 13) _quotation.Search(1); });

        // Xóa lọc
        $('#btn_clear_filters').on('click', function () {
            $('#ip_search_query').val('');
            $('#sl_search_status').val('-1');
            $('#sl_search_saler').val('0').trigger('change');
            $('#ip_search_from_date').val('');
            $('#ip_search_to_date').val('');
            $('#quotation_stats_cards .card-stats').removeClass('active');
            $('#quotation_stats_cards .card-stats[data-status="-1"]').addClass('active');
            _quotation.Search(1);
        });

        _quotation.Search(1);
    },

    Search: function (pageIndex) {
        var model = {
            Keyword: $('#ip_search_query').val(),
            Status: parseInt($('#sl_search_status').val()),
            SalerId: $('#sl_search_saler').val() || 0,
            FromDateSTR: $('#ip_search_from_date').val(),
            ToDateSTR: $('#ip_search_to_date').val(),
            PageIndex: pageIndex || 1,
            PageSize: parseInt($('#selectPaggingOptions').val()) || 10
        };
        _ajax_caller.post('/Quotation/Search', model, function (result) {
            $('#grid_data').html(result);
        });
    },

    OnPaging: function (pageIndex) { _quotation.Search(pageIndex); },

    OnSelectPageSize: function (pageSize) {
        $('#selectPaggingOptions').val(pageSize);
        _quotation.Search(1);
    },

    ChangeStatus: function (id, status) {
        var msg = status === 1 ? 'Xác nhận gửi báo giá này cho khách?' : 'Xác nhận khách đã chốt báo giá này?';
        _msgconfirm.openDialog('Xác nhận', msg, function () {
            _ajax_caller.post('/Quotation/ChangeStatus', { id: id, status: status }, function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    _quotation.Search(1);
                } else {
                    _msgalert.error(result.msg);
                }
            });
        });
    },

    Delete: function (id) {
        _msgconfirm.openDialog('Xác nhận xóa', 'Bạn có chắc muốn xóa báo giá này?', function () {
            _ajax_caller.post('/Quotation/Delete', { id: id }, function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    _quotation.Search(1);
                } else {
                    _msgalert.error(result.msg);
                }
            });
        });
    },

    // =====================================================================
    // ADD OR UPDATE PAGE
    // =====================================================================
    InitAddOrUpdate: function (model) {
        // Khởi tạo datepicker cho các field tĩnh (nếu có)
        _quotation._initDatepickers($('body'));

        // Currency input formatting — lắng nghe cả .currency-input (JS rows) và .currency (template rows)
        $(document).on('input', '.currency-input, .currency', function () {
            _quotation._formatCurrencyInput(this);
        });

        // Tính lại tổng khi thay đổi chi phí sidebar
        $(document).on('input', '.financial-calc', function () {
            _quotation._formatCurrencyInput(this);
            _quotation.RecalcTotals();
        });

        // Đếm ký tự ghi chú
        $('#txt_client_note').on('input', function () {
            $('#lbl_char_count').text($(this).val().length + '/500');
        }).trigger('input');

        // Nút lưu
        $('#btn_save_draft').on('click', function () { _quotation.Save(0); });
        $('#btn_save_publish').on('click', function () { _quotation.Save(1); });

        // Nạp dữ liệu cũ nếu đang edit
        if (model && model._id) {
            _quotation._loadExistingModel(model);
        }

        // Tính tổng ban đầu
        _quotation.RecalcTotals();
    },

    // Nạp lại toàn bộ dữ liệu từ model C# khi edit
    _loadExistingModel: function (model) {
        // Hotels
        if (model.Hotels) {
            $.each(model.Hotels, function (i, svc) {
                var block = _quotation._cloneBlock(1);
                block.find('.hotel-name').val(svc.HotelName || '');
                block.find('.hotel-checkin').val(_quotation._fmtDate(svc.ArriveDate));
                block.find('.hotel-checkout').val(_quotation._fmtDate(svc.DepartureDate));
                block.find('.hotel-rooms-count').val(svc.NumberOfRooms || 0);
                _quotation._calcHotelNights(block);

                var tbody = block.find('.hotel-rooms-body');
                tbody.empty();
                if (svc.Rooms && svc.Rooms.length) {
                    $.each(svc.Rooms, function (j, room) {
                        var rate = (room.Package && room.Package.length) ? room.Package[0] : {};
                        var row = _quotation._newHotelRoomRow();
                        row.find('.room-type-name').val(room.RoomTypeName || '');
                        row.find('.room-count').val(room.NumberOfRooms || 0);
                        row.find('.room-guests-per').val(room.NumberOfAdult || 0);
                        row.find('.import-price').val(_quotation._fmtNum(rate.OperatorPrice || 0));
                        row.find('.export-price').val(_quotation._fmtNum(rate.SalePrice || 0));
                        tbody.append(row);
                        _quotation._calcRow(row);
                    });
                } else {
                    tbody.append(_quotation._newHotelRoomRow());
                }
                _quotation._initDatepickers(block);
                $('#services_container').append(block);
            });
        }

        // Flights
        if (model.Flights) {
            $.each(model.Flights, function (i, svc) {
                var block = _quotation._cloneBlock(2);
                block.find('.flight-start-point').val(svc.StartPoint || '');
                block.find('.flight-end-point').val(svc.EndPoint || '');
                block.find('.flight-airline').val((svc.Go && svc.Go.Airline) || '');
                block.find('.flight-code').val((svc.Go && svc.Go.FlyCode) || '');
                block.find('.flight-departure-date').val(_quotation._fmtDate(svc.StartDate));
                block.find('.flight-return-date').val(_quotation._fmtDate(svc.EndDate));

                var tbody = block.find('.flight-tickets-body');
                tbody.empty();
                if (svc.ExtraPackages && svc.ExtraPackages.length) {
                    $.each(svc.ExtraPackages, function (j, pkg) {
                        var row = _quotation._newFlightTicketRow();
                        row.find('.ticket-class').val(pkg.PackageCode || '');
                        row.find('.ticket-qty').val(pkg.Quantity || 0);
                        row.find('.import-price').val(_quotation._fmtNum(pkg.BasePrice || 0));
                        row.find('.export-price').val(_quotation._fmtNum(pkg.Amount || 0));
                        tbody.append(row);
                        _quotation._calcRow(row);
                    });
                } else {
                    tbody.append(_quotation._newFlightTicketRow());
                }
                _quotation._initDatepickers(block);
                $('#services_container').append(block);
            });
        }

        // Tours
        if (model.Tours) {
            $.each(model.Tours, function (i, svc) {
                var block = _quotation._cloneBlock(3);
                block.find('.tour-name').val(svc.TourProductName || '');
                block.find('.tour-type').val(svc.TourType || 0);
                block.find('.tour-end-point').val(svc.EndPoint || '');
                block.find('.tour-start-date').val(_quotation._fmtDate(svc.StartDate));
                block.find('.tour-end-date').val(_quotation._fmtDate(svc.EndDate));
                block.find('.tour-guests-count').val((svc.Guest && svc.Guest.length) || 0);

                var tbody = block.find('.tour-packages-body');
                tbody.empty();
                if (svc.ExtraPackages && svc.ExtraPackages.length) {
                    $.each(svc.ExtraPackages, function (j, pkg) {
                        var row = _quotation._newTourPackageRow();
                        row.find('.tour-include').val(pkg.PackageCode || '');
                        row.find('.tour-qty').val(pkg.Quantity || 0);
                        row.find('.import-price').val(_quotation._fmtNum(pkg.BasePrice || 0));
                        row.find('.export-price').val(_quotation._fmtNum(pkg.Amount || 0));
                        tbody.append(row);
                        _quotation._calcRow(row);
                    });
                } else {
                    tbody.append(_quotation._newTourPackageRow());
                }
                _quotation._initDatepickers(block);
                $('#services_container').append(block);
            });
        }

        // Others
        if (model.Others) {
            $.each(model.Others, function (i, svc) {
                var block = _quotation._cloneBlock(4);
                var tbody = block.find('.other-services-body');
                tbody.empty();
                if (svc.Packages && svc.Packages.length) {
                    $.each(svc.Packages, function (j, pkg) {
                        var row = _quotation._newOtherServiceRow();
                        row.find('.other-service-name').val(pkg.PackageName || '');
                        row.find('.other-qty').val(pkg.Quantity || 0);
                        row.find('.import-price').val(_quotation._fmtNum(pkg.BasePrice || 0));
                        row.find('.export-price').val(_quotation._fmtNum(pkg.SalePrice || 0));
                        tbody.append(row);
                        _quotation._calcRow(row);
                    });
                } else {
                    tbody.append(_quotation._newOtherServiceRow());
                }
                $('#services_container').append(block);
            });
        }

        // VinWonders
        if (model.VinWonders) {
            $.each(model.VinWonders, function (i, svc) {
                var block = _quotation._cloneBlock(5);
                block.find('.vinwonder-location').val(svc.LocationName || '');
                var tbody = block.find('.vinwonder-packages-body');
                tbody.empty();
                if (svc.Packages && svc.Packages.length) {
                    $.each(svc.Packages, function (j, pkg) {
                        var row = _quotation._newVinWonderPackageRow();
                        row.find('.vinwonder-ticket-name').val(pkg.PackageName || '');
                        row.find('.vinwonder-date-used').val(_quotation._fmtDate(pkg.DateUsed));
                        row.find('.vinwonder-qty').val(pkg.Quantity || 0);
                        row.find('.import-price').val(_quotation._fmtNum(pkg.BasePrice || 0));
                        row.find('.export-price').val(_quotation._fmtNum(pkg.Amount || 0));
                        tbody.append(row);
                        _quotation._calcRow(row);
                    });
                } else {
                    tbody.append(_quotation._newVinWonderPackageRow());
                }
                _quotation._initDatepickers(block);
                $('#services_container').append(block);
            });
        }

        // Sidebar tài chính
        $('#ip_other_fees').val(_quotation._fmtNum(model.OtherFees || 0));
        $('#ip_collaborator_comm').val(_quotation._fmtNum(model.CollaboratorComm || 0));
        $('#ip_customer_care_fund').val(_quotation._fmtNum(model.CustomerCareFund || 0));
        $('#txt_client_note').val(model.Note || '').trigger('input');

        _quotation.RecalcTotals();
    },

    // =====================================================================
    // THÊM BLOCK DỊCH VỤ
    // =====================================================================
    ToggleServiceMenu: function (e) {
        e.stopPropagation();
        var menu = $('#dd_service_menu');
        menu.toggle();
        // Đóng khi click ra ngoài
        if (menu.is(':visible')) {
            $(document).one('click', function () { menu.hide(); });
        }
    },

    AddServiceBlock: function (type) {
        // Đóng menu sau khi chọn
        $('#dd_service_menu').hide();
        var block = _quotation._cloneBlock(type);
       
        $('#services_container').append(block);
        _quotation.RecalcTotals();
        // Scroll đến block mới
        $('html, body').animate({ scrollTop: block.offset().top - 80 }, 300);
        _quotation._initDatepickers(block);
        $('.servicemanual-hotel-room-rates-daterange').each(function (index, item) {
            var element = $(this)
            _quotation.OnApplyPackageDateDateRange()(element, $('#servicemanual_hotel_checkin'), $('#servicemanual_hotel_checkout'));
            //_order_detail_hotel.OnApplyDayUsesToRoomNight(element)

        });

    },

    _cloneBlock: function (type) {
        var tplMap = { 1: '#tpl-hotel', 2: '#tpl-flight', 3: '#tpl-tour', 4: '#tpl-other', 5: '#tpl-vinwonder' };
        var tpl = $(tplMap[type]);
        if (!tpl.length) return $('<div></div>');
        var block = tpl.find('.card-service').clone(false);

        if (type === 1) {
            // Hotel: select2 ajax theo mẫu order_detail_hotel.js / HotelSuggesstion
            block.find('.hotel-name').select2({
                theme: 'bootstrap4',
                placeholder: 'Tên khách sạn',
                minimumInputLength: 1,
                ajax: {
                    url: '/Order/HotelSuggestion',
                    type: 'post',
                    dataType: 'json',
                    delay: 250,
                    tags: true,
                    data: function (params) { return { txt_search: params.term }; },
                    processResults: function (response) {
                        return {
                            results: $.map(response.data, function (item) {
                                return { text: item.name, id: item.hotelid };
                            })
                        };
                    },
                    cache: true
                }
            });

        } 
        else if (type === 2) {
            // Flight: load toàn bộ airport codes rồi append vào select2 theo mẫu set_service_fly.js
            var fromEl = block.find('.flight-start-point');
            var toEl = block.find('.flight-end-point');
            fromEl.select2({ placeholder: 'Điểm đi', allowClear: true });
            toEl.select2({ placeholder: 'Điểm đến', allowClear: true });

            $.ajax({
                url: '/Order/AirPortCodeSuggestion',
                type: 'post',
                data: { txt_search: '' },
                success: function (result) {
                    if (result && result.data && result.data.length) {
                        result.data.forEach(function (item) {
                            var opt = '<option value="' + item.code + '">' + item.code + ' (' + item.districtVi + ')</option>';
                            fromEl.append(opt);
                            toEl.append(opt);
                        });
                        fromEl.trigger('change');
                        toEl.trigger('change');
                    }
                }
            });

        } 
        else if (type === 3) {
            // Tour: select2 ajax province theo mẫu order_detail_tour.js
            block.find('.tour-start-point').select2({
                placeholder: 'Điểm đi',
                allowClear: true,
                ajax: {
                    url: '/Order/ProvinceSuggestion',
                    type: 'post',
                    dataType: 'json',
                    delay: 250,
                    data: function (params) { return { txt_search: params.term }; },
                    processResults: function (response) {
                        return {
                            results: $.map(response.data, function (item) {
                                return { text: item.name, id: item.id };
                            })
                        };
                    },
                    cache: true
                }
            });
            block.find('.tour-end-point').select2({
                placeholder: 'Điểm đến',
                allowClear: true,
                ajax: {
                    url: '/Order/ProvinceSuggestion',
                    type: 'post',
                    dataType: 'json',
                    delay: 250,
                    data: function (params) { return { txt_search: params.term }; },
                    processResults: function (response) {
                        return {
                            results: $.map(response.data, function (item) {
                                return { text: item.name, id: item.id };
                            })
                        };
                    },
                    cache: true
                }
            });
            block.find('.tour-name').select2({
                placeholder: 'Tên tour',
                allowClear: true,
                ajax: {
                    url: "/Order/ExistsTourSuggesstion",
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
                                    text: item.tourName + (item.organizingTypeName != undefined || item.tourTypeName == undefined ? ' (' : '') + (item.organizingTypeName == undefined ? '' : item.organizingTypeName) + (item.tourTypeName == undefined ? '' : (item.organizingTypeName != undefined ? ' - ' : '') + item.tourTypeName) + (item.organizingTypeName != undefined || item.tourTypeName == undefined ? ')' : ''),
                                    id: item.id,
                                }
                            })
                        };
                    },
                    cache: true
                }
            });
            block.find('.tour-name').trigger('change');

        } 
        else if (type === 4){
            block.find('.other-service-type').select2({
                placeholder: 'Tên dịch vụ',
                allowClear: true,
                ajax: {
                    url: "/Quotation/OtherServiceSuggestion",
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
                                    text: item.description,
                                    id: item.codeValue,
                                }
                            })
                        };
                    },
                    cache: true
                }
            });
        }
        else if (type === 5) {
            // VinWonder: load toàn bộ locations rồi append vào select2 theo mẫu order_detail_vinwonder.js
            var locEl = block.find('.vinwonder-location');
            locEl.select2({ placeholder: 'Địa điểm VinWonder', allowClear: true });

            $.ajax({
                url: '/Order/VinWonderLocationSuggestion',
                type: 'post',
                data: { txt_search: '' },
                success: function (result) {
                    if (result && result.data && result.data.length) {
                        result.data.forEach(function (item) {
                            locEl.append('<option value="' + item.code + '">' + item.name + '</option>');
                        });
                        locEl.trigger('change');
                    }
                }
            });
        }

        // Bind calc-trigger
        block.on('input', '.calc-trigger', function () {
            _quotation._formatCurrencyInput(this);
            _quotation._calcRow($(this).closest('tr'));
            _quotation.RecalcTotals();
        });
        // Hotel: tính số đêm khi đổi checkin/checkout
        block.on('apply.daterangepicker', '.hotel-checkin, .hotel-checkout', function () {
            _quotation._calcHotelNights($(this).closest('.card-service'));
        });
        return block;
    },

    RemoveServiceBlock: function (btn) {
        _msgconfirm.openDialog('Xác nhận', 'Bạn có chắc muốn xóa dịch vụ này?', function () {
            $(btn).closest('.card-service').remove();
            _quotation.RecalcTotals();
        });
    },

    // =====================================================================
    // THÊM DÒNG CHO TỪNG LOẠI DỊCH VỤ
    // =====================================================================
    AddHotelRoomRow: function (btn) {
        var block = $(btn).closest('.card-service');
        _quotation._addHotelRoom(block);
    },

    // Thêm một room row mới vào block hotel
    _addHotelRoom: function (block) {
        var tbody = block.find('.hotel-rooms-body');
        var idx = tbody.find('.hotel-room-row').length + 1;
        var row = _quotation._newHotelRoomRow(idx);
        tbody.append(row);
        _quotation._initHotelRoomEvents(block);
        _quotation._reindexHotelRooms(block);
        _quotation._initDatepickers(row);
    },

    AddFlightTicketRow: function (btn) {
        var tbody = $(btn).closest('.card-service-body').find('.flight-tickets-body');
        var row = _quotation._newFlightTicketRow();
        tbody.append(row);
        _quotation._bindRowCalc(row);
    },

    AddTourPackageRow: function (btn) {
        var tbody = $(btn).closest('.card-service-body').find('.tour-packages-body');
        var row = _quotation._newTourPackageRow();
        tbody.append(row);
        _quotation._bindRowCalc(row);
    },

    AddOtherServiceRow: function (btn) {
        var tbody = $(btn).closest('.card-service-body').find('.other-services-body');
        var row = _quotation._newOtherServiceRow();
        tbody.append(row);
        _quotation._bindRowCalc(row);
    },

    AddVinWonderPackageRow: function (btn) {
        var tbody = $(btn).closest('.card-service-body').find('.vinwonder-packages-body');
        var row = _quotation._newVinWonderPackageRow();
        tbody.append(row);
        _quotation._bindRowCalc(row);
        _quotation._initDatepickers(row);
    },

    RemoveRow: function (btn) {
        $(btn).closest('tr').remove();
        _quotation.RecalcTotals();
    },

    // =====================================================================
    // ROW TEMPLATES
    // =====================================================================
    _newHotelRoomRow: function (idx, roomData) {
        idx = idx || 1;
        roomData = roomData || {};
        // Build package rows
        var packagesHtml = '';
        var packages = roomData.Packages || [{}];
        packages.forEach(function (pkg, pkgIdx) {
            packagesHtml += `
        <div class="hotel-room-package-row d-flex align-items-center gap-1 mb-1">
            <a href="javascript:;" class="text-danger mr-1 hotel-pkg-delete" title="Xóa gói"><i class="fa fa-trash-o"></i></a>
            <a href="javascript:;" class="text-success mr-1 hotel-pkg-add" title="Thêm gói"><i class="fa fa-plus-circle"></i></a>
            <input type="text" class="form-control form-control-sm hotel-pkg-name" placeholder="Nhập tên gói" value="${pkg.PackageName || ''}" />
            <input type="text" class="form-control form-control-sm hotel-pkg-daterange" placeholder="dd/mm/yyyy - dd/mm/yyyy" value="${pkg.DateRange || ''}" />
            <input type="text" class="form-control form-control-sm currency import-price calc-trigger" placeholder="0" value="${_quotation._fmtNum(pkg.OperatorPrice || 0)}" />
            <input type="text" class="form-control form-control-sm currency export-price calc-trigger" placeholder="0" value="${_quotation._fmtNum(pkg.SalePrice || 0)}" />
        </div>`;
        });

        return $(`<tr class="hotel-room-row" data-room-index="${idx}">
        <td class="hotel-room-stt align-middle" rowspan="1">${idx}</td>
        <td class="align-middle">
            <input type="text" class="form-control form-control-sm room-type-name" placeholder="Nhập hạng phòng" value="${roomData.RoomTypeName || ''}" />
        </td>
        <td class="hotel-room-packages-td">${packagesHtml}</td>
        <td class="hotel-room-daterange-td"></td>
        <td class="hotel-room-import-td"></td>
        <td class="hotel-room-export-td"></td>
        <td class="hotel-room-nights-td align-middle">
            <input type="number" class="form-control form-control-sm hotel-nights-row" value="${roomData.Nights || 0}" readonly style="background:#f5f5f5;" />
        </td>
        <td class="align-middle">
            <input type="number" class="form-control form-control-sm room-count calc-trigger" placeholder="1" min="0" value="${roomData.NumberOfRooms || 1}" />
        </td>
        <td class="align-middle"><span class="total-import font-weight-bold text-muted">0</span></td>
        <td class="align-middle"><span class="total-export font-weight-bold text-primary">0</span></td>
        <td class="align-middle"><span class="total-profit font-weight-bold text-success">0</span></td>
        <td class="align-middle text-center">
            <a href="javascript:;" class="text-info mr-1 hotel-room-clone" title="Nhân bản"><i class="fa fa-files-o"></i></a>
            <a href="javascript:;" class="text-danger hotel-room-delete" title="Xóa phòng"><i class="fa fa-trash-o"></i></a>
        </td>
    </tr>`);
    },

    _newFlightTicketRow: function () {
        return $(`<tr class="flight-ticket-row">
            <td><input type="text" class="form-control form-control-sm ticket-class" placeholder="Phổ thông" /></td>
            <td><input type="number" class="form-control form-control-sm ticket-qty calc-trigger" placeholder="0" min="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input import-price calc-trigger" placeholder="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input export-price calc-trigger" placeholder="0" /></td>
            <td><span class="total-import font-weight-bold text-muted">0</span></td>
            <td><span class="total-export font-weight-bold text-primary">0</span></td>
            <td><span class="total-profit font-weight-bold text-success">0</span></td>
            <td><button type="button" class="btn btn-sm btn-danger" onclick="_quotation.RemoveRow(this)"><i class="fa fa-times"></i></button></td>
        </tr>`);
    },

    _newTourPackageRow: function () {
        return $(`<tr class="tour-package-row">
            <td><input type="text" class="form-control form-control-sm tour-include" placeholder="Tour trọn gói" /></td>
            <td><input type="number" class="form-control form-control-sm tour-qty calc-trigger" placeholder="0" min="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input import-price calc-trigger" placeholder="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input export-price calc-trigger" placeholder="0" /></td>
            <td><span class="total-import font-weight-bold text-muted">0</span></td>
            <td><span class="total-export font-weight-bold text-primary">0</span></td>
            <td><span class="total-profit font-weight-bold text-success">0</span></td>
            <td><button type="button" class="btn btn-sm btn-danger" onclick="_quotation.RemoveRow(this)"><i class="fa fa-times"></i></button></td>
        </tr>`);
    },

    _newOtherServiceRow: function () {
        return $(`<tr class="other-service-row">
            <td><input type="text" class="form-control form-control-sm other-service-name" placeholder="Xe đưa đón sân bay" /></td>
            <td><input type="text" class="form-control form-control-sm other-content" placeholder="Xe 7 chỗ" /></td>
            <td><input type="number" class="form-control form-control-sm other-qty calc-trigger" placeholder="0" min="0" /></td>
            <td><input type="text" class="form-control form-control-sm other-unit" placeholder="Xe" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input import-price calc-trigger" placeholder="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input export-price calc-trigger" placeholder="0" /></td>
            <td><span class="total-import font-weight-bold text-muted">0</span></td>
            <td><span class="total-export font-weight-bold text-primary">0</span></td>
            <td><span class="total-profit font-weight-bold text-success">0</span></td>
            <td><button type="button" class="btn btn-sm btn-danger" onclick="_quotation.RemoveRow(this)"><i class="fa fa-times"></i></button></td>
        </tr>`);
    },

    _newVinWonderPackageRow: function () {
        return $(`<tr class="vinwonder-package-row">
            <td><input type="text" class="form-control form-control-sm vinwonder-ticket-name" placeholder="Vé người lớn" /></td>
            <td><input type="text" class="form-control form-control-sm datepicker vinwonder-date-used" placeholder="dd/mm/yyyy" /></td>
            <td><input type="number" class="form-control form-control-sm vinwonder-qty calc-trigger" placeholder="0" min="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input import-price calc-trigger" placeholder="0" /></td>
            <td><input type="text" class="form-control form-control-sm currency-input export-price calc-trigger" placeholder="0" /></td>
            <td><span class="total-import font-weight-bold text-muted">0</span></td>
            <td><span class="total-export font-weight-bold text-primary">0</span></td>
            <td><span class="total-profit font-weight-bold text-success">0</span></td>
            <td><button type="button" class="btn btn-sm btn-danger" onclick="_quotation.RemoveRow(this)"><i class="fa fa-times"></i></button></td>
        </tr>`);
    },

    // =====================================================================
    // TÍNH TOÁN
    // =====================================================================
    _bindRowCalc: function (row) {
        row.on('input', '.calc-trigger', function () {
            _quotation._formatCurrencyInput(this);
            _quotation._calcRow(row);
            _quotation.RecalcTotals();
        });
    },

    _calcRow: function (row) {
        var qtyInput = row.find('.room-count, .ticket-qty, .tour-qty, .other-qty, .vinwonder-qty').first();
        var qty = parseFloat(qtyInput.val()) || 0;
        var importPrice = _quotation._parseCurrency(row.find('.import-price').val());
        var exportPrice = _quotation._parseCurrency(row.find('.export-price').val());

        // Với hotel: nhân thêm số đêm từ block cha và cập nhật cột số đêm của row
        var block = row.closest('.card-service');
        var nights = 1;
        if (block.data('type') == 1) {
            nights = parseInt(block.find('.hotel-nights-count').val()) || 1;
            row.find('.hotel-nights-row').val(nights);
        }

        var totalImport = qty * importPrice * nights;
        var totalExport = qty * exportPrice * nights;
        var profit = totalExport - totalImport;

        row.find('.total-import').text(_quotation._fmtNum(totalImport));
        row.find('.total-export').text(_quotation._fmtNum(totalExport));
        var profitEl = row.find('.total-profit');
        profitEl.text(_quotation._fmtNum(profit));
        profitEl.removeClass('text-success text-danger').addClass(profit >= 0 ? 'text-success' : 'text-danger');
    },

    _calcHotelNights: function (block) {
        var checkin = block.find('.hotel-checkin').val();
        var checkout = block.find('.hotel-checkout').val();
        if (checkin && checkout) {
            var d1 = moment(checkin, 'DD/MM/YYYY');
            var d2 = moment(checkout, 'DD/MM/YYYY');
            var nights = d2.diff(d1, 'days');
            block.find('.hotel-nights-count').val(nights > 0 ? nights : 0);
        }
        // Tính lại tất cả row trong block
        block.find('tr').each(function () { _quotation._calcRow($(this)); });
        _quotation.RecalcTotals();
    },

    RecalcTotals: function () {
        var totalImport = 0;
        var totalExport = 0;

        // Cộng tất cả .total-import và .total-export trong các row
        $('#services_container .total-import').each(function () {
            totalImport += _quotation._parseCurrency($(this).text());
        });
        $('#services_container .total-export').each(function () {
            totalExport += _quotation._parseCurrency($(this).text());
        });

        var otherFees = _quotation._parseCurrency($('#ip_other_fees').val());
        var collComm = _quotation._parseCurrency($('#ip_collaborator_comm').val());
        var csFund = _quotation._parseCurrency($('#ip_customer_care_fund').val());

        var profit = totalExport - totalImport - otherFees - collComm - csFund;

        $('#lbl_total_import').text(_quotation._fmtNum(totalImport) + ' đ');
        $('#lbl_total_export').text(_quotation._fmtNum(totalExport) + ' đ');

        var profitEl = $('#lbl_total_profit');
        profitEl.text(_quotation._fmtNum(profit) + ' đ');
        profitEl.removeClass('profit-positive profit-negative').addClass(profit >= 0 ? 'profit-positive' : 'profit-negative');
    },

    // =====================================================================
    // LƯU
    // =====================================================================
    Save: function (statusOverride) {
        var id = $('#hd_quotation_id').val();
        var currentStatus = parseInt($('#hd_quotation_status').val()) || 0;
        var status = (statusOverride !== undefined) ? statusOverride : currentStatus;

        // Nếu bấm "Gửi báo giá" thì status = 1
        if (statusOverride === 1 && currentStatus < 1) status = 1;

        var model = {
            _id: id || null,
            Status: status,
            Note: $('#txt_client_note').val(),
            OtherFees: _quotation._parseCurrency($('#ip_other_fees').val()),
            CollaboratorComm: _quotation._parseCurrency($('#ip_collaborator_comm').val()),
            CustomerCareFund: _quotation._parseCurrency($('#ip_customer_care_fund').val()),
            Hotels: [],
            Flights: [],
            Tours: [],
            Others: [],
            VinWonders: []
        };

        $('#services_container .service-block').each(function () {
            var block = $(this);
            var type = parseInt(block.data('type'));

           if (type === 1) {
    var svc = {
        HotelName: block.find('.hotel-name').val(),
        ArriveDate: _quotation._parseDate(block.find('.hotel-checkin').val()),
        DepartureDate: _quotation._parseDate(block.find('.hotel-checkout').val()),
        NumberOfRooms: parseInt(block.find('.hotel-rooms-count').val()) || 0,
        Rooms: []
    };
    block.find('.hotel-room-row').each(function () {
        var row = $(this);
        var room = {
            RoomTypeName: row.find('.room-type-name').val(),
            NumberOfRooms: parseInt(row.find('.room-count').val()) || 0,
            NumberOfAdult: parseInt(row.find('.room-guests-per').val()) || 0,
            Packages: []
        };
        // Lấy tất cả các gói trong phòng này
        row.find('.hotel-room-package-row').each(function () {
            room.Packages.push({
                PackageName: $(this).find('.hotel-pkg-name').val(),
                DateRange: $(this).find('.hotel-pkg-daterange').val(),
                OperatorPrice: _quotation._parseCurrency($(this).find('.import-price').val()),
                SalePrice: _quotation._parseCurrency($(this).find('.export-price').val())
            });
        });
        svc.Rooms.push(room);
    });
    model.Hotels.push(svc);
} else if (type === 2) {
                var svc = {
                    StartPoint: block.find('.flight-start-point').val(),
                    EndPoint: block.find('.flight-end-point').val(),
                    StartDate: _quotation._parseDate(block.find('.flight-departure-date').val()),
                    EndDate: _quotation._parseDate(block.find('.flight-return-date').val()),
                    Go: {
                        Airline: block.find('.flight-airline').val(),
                        FlyCode: block.find('.flight-code').val()
                    },
                    ExtraPackages: []
                };
                block.find('.flight-ticket-row').each(function () {
                    var row = $(this);
                    svc.ExtraPackages.push({
                        PackageCode: row.find('.ticket-class').val(),
                        Quantity: parseInt(row.find('.ticket-qty').val()) || 0,
                        BasePrice: _quotation._parseCurrency(row.find('.import-price').val()),
                        Amount: _quotation._parseCurrency(row.find('.export-price').val())
                    });
                });
                model.Flights.push(svc);

            } else if (type === 3) {
                var svc = {
                    TourProductName: block.find('.tour-name').val(),
                    TourType: parseInt(block.find('.tour-type').val()) || 0,
                    EndPoint: block.find('.tour-end-point').val(),
                    StartDate: _quotation._parseDate(block.find('.tour-start-date').val()),
                    EndDate: _quotation._parseDate(block.find('.tour-end-date').val()),
                    ExtraPackages: []
                };
                block.find('.tour-package-row').each(function () {
                    var row = $(this);
                    svc.ExtraPackages.push({
                        PackageCode: row.find('.tour-include').val(),
                        Quantity: parseInt(row.find('.tour-qty').val()) || 0,
                        BasePrice: _quotation._parseCurrency(row.find('.import-price').val()),
                        Amount: _quotation._parseCurrency(row.find('.export-price').val())
                    });
                });
                model.Tours.push(svc);

            } else if (type === 4) {
                var svc = { Packages: [] };
                block.find('.other-service-row').each(function () {
                    var row = $(this);
                    svc.Packages.push({
                        PackageName: row.find('.other-service-name').val(),
                        Quantity: parseInt(row.find('.other-qty').val()) || 0,
                        BasePrice: _quotation._parseCurrency(row.find('.import-price').val()),
                        SalePrice: _quotation._parseCurrency(row.find('.export-price').val()),
                        Amount: _quotation._parseCurrency(row.find('.export-price').val())
                    });
                });
                model.Others.push(svc);

            } else if (type === 5) {
                var svc = {
                    LocationName: block.find('.vinwonder-location').val(),
                    Packages: []
                };
                block.find('.vinwonder-package-row').each(function () {
                    var row = $(this);
                    svc.Packages.push({
                        PackageName: row.find('.vinwonder-ticket-name').val(),
                        DateUsed: _quotation._parseDate(row.find('.vinwonder-date-used').val()),
                        Quantity: parseInt(row.find('.vinwonder-qty').val()) || 0,
                        BasePrice: _quotation._parseCurrency(row.find('.import-price').val()),
                        Amount: _quotation._parseCurrency(row.find('.export-price').val())
                    });
                });
                model.VinWonders.push(svc);
            }
        });

        var btn = status === 1 ? $('#btn_save_publish') : $('#btn_save_draft');
        btn.attr('disabled', true);

        _ajax_caller.post('/Quotation/Save', { model: model }, function (result) {
            if (result.status === 0) {
                _msgalert.success(result.msg || 'Lưu thành công');
                if (result.id && !id) {
                    // Redirect sang edit nếu vừa tạo mới
                    setTimeout(function () {
                        window.location.href = '/Quotation/Index';
                    }, 600);
                } else {
                    $('#hd_quotation_status').val(status);
                    _quotation._updateStatusBadge(status);
                    btn.removeAttr('disabled');
                }
            } else {
                _msgalert.error(result.msg || 'Lưu thất bại');
                btn.removeAttr('disabled');
            }
        });
    },

    _updateStatusBadge: function (status) {
        var badge = $('#badge_status');
        var desc = $('#lbl_status_desc');
        badge.removeClass('status-draft status-sent status-won');
        if (status === 0) {
            badge.addClass('status-draft').text('Nháp');
            desc.text('Báo giá đang soạn thảo chưa gửi khách');
        } else if (status === 1) {
            badge.addClass('status-sent').text('Đã gửi báo giá');
            desc.text('Đã gửi thông tin báo giá cho khách');
        } else if (status === 2) {
            badge.addClass('status-won').text('Khách chốt');
            desc.text('Khách hàng đồng ý chốt giá');
        }
    },

    // =====================================================================
    // HELPERS
    // =====================================================================
    _initDatepickers: function (container) {
        container.find('.datepicker').each(function () {
            $(this).addClass('dp-init').daterangepicker({
                singleDatePicker: true,
                autoApply: true,
                locale: { format: 'DD/MM/YYYY' }
            });
        });
        container.find('.datepicker-datetime').each(function () {
            $(this).addClass('dp-init').daterangepicker({
                singleDatePicker: true,
                autoApply: true,
                timePicker: true,
                timePicker24Hour: true,
                locale: { format: 'DD/MM/YYYY HH:mm' }
            });
        });
    },

    _formatCurrencyInput: function (el) {
        var raw = $(el).val().replace(/[^0-9]/g, '');
        var num = parseInt(raw) || 0;
        $(el).val(num > 0 ? _quotation._fmtNum(num) : '');
    },

    _parseCurrency: function (val) {
        if (!val) return 0;
        // _fmtNum dùng , làm phân cách nghìn → loại bỏ , trước khi parse
        return parseFloat(String(val).replace(/,/g, '')) || 0;
    },

    _fmtNum: function (number) {
        number = ('' + number).replace(/[^0-9.,]+/g, '');
        number += '';
        number = number.replaceAll(',', '');
        x = number.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1))
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        return x1 + x2;
    },

    _fmtDate: function (dateVal) {
        if (!dateVal) return '';
        var m = moment(dateVal);
        if (!m.isValid()) return '';
        return m.format('DD/MM/YYYY');
    },

    _parseDate: function (str) {
        if (!str) return null;
        var m = moment(str, 'DD/MM/YYYY');
        return m.isValid() ? m.toISOString() : null;
    },
    // Add event handlers for add/remove package
    _initHotelRoomEvents: function (block) {
        block.off('click', '.hotel-pkg-add').on('click', '.hotel-pkg-add', function () {
            var pkgRow = $(this).closest('.hotel-room-package-row');
            var clone = pkgRow.clone();
            clone.find('input').val('');
            pkgRow.after(clone);
        });
        block.off('click', '.hotel-pkg-delete').on('click', '.hotel-pkg-delete', function () {
            var pkgRows = $(this).closest('.hotel-room-packages-td').find('.hotel-room-package-row');
            if (pkgRows.length > 1) $(this).closest('.hotel-room-package-row').remove();
        });
    },
    OnApplyPackageDateDateRange: function (apply_element, start_date_element, end_date_element) {

        //end_date_element.data('daterangepicker').minDate = start_date_element.data('daterangepicker').startDate._d


        var min_date = _global_function.GetDateFromVNDateTimeSlash(start_date_element.val())
        var min_time = _global_function.GetDayText(min_date)
        var max_date = _global_function.GetDateFromVNDateTimeSlash(end_date_element.val())
        var max_time = _global_function.GetDayText(max_date)
        apply_element.daterangepicker({
            showDropdowns: true,
            drops: 'down',
            autoApply: true,
            minDate: min_time,
            maxDate: max_time,
            locale: {
                format: 'DD/MM/YYYY'
            }
        }, function (start, end, label) {


        });
    },
};
