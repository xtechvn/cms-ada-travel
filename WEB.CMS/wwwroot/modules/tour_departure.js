var _tour_departure = {
    Init: function () {
        this.OnSearch();
        $('#ip_search_tour_product').keypress(function (e) {
            if (e.which == 13) {
                _tour_departure.OnSearch();
            }
        });
        $('.datepicker-input').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            timePicker: true,
            timePicker24Hour: true,
            locale: {
                cancelLabel: 'Clear',
                format: 'DD/MM/YYYY HH:mm'
            }
        });
        $('.datepicker-input').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('DD/MM/YYYY HH:mm'));
        });
    
        $('.datepicker-input').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
        });
  
    },
    InitAddOrUpdate: function () {
    
        $('.datepicker-input').each(function () {
            let value = $(this).attr('value');

            $(this).daterangepicker({
                singleDatePicker: true,
                timePicker: true,
                timePicker24Hour: true,
                startDate: value ? moment(value, 'DD/MM/YYYY HH:mm') : moment(),
                locale: {
                    format: 'DD/MM/YYYY HH:mm'
                }
            });
        });
        // Setup select2 for Tour Product searching
        var selected = $("#TourProductId").find(':selected').val()
        $("#TourProductId").select2({
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
        $("#TourProductId").val(selected).trigger('change');


        // Setup select2 for route from/to
        $('.select-route-from').each(function (index, item) {
            var element_from = $(this);
            var val_from = element_from.val();
            element_from.select2();
 
            $.ajax({
                url: "/FlightWarehouse/AirPortCodeSuggestion",
                type: "post",
                data: { txt_search: "" },
                success: function (result) {
                    if (result != undefined && result.data != undefined && result.data.length > 0) {
                        result.data.forEach(function (item) {
                            var html_airport_code_option = '<option value="{airport_code}" {if_selected}>{airport_code} ({airport_name})</option>';
                            if (element_from.find('option[value="' + item.code + '"]').length == 0) {
                                element_from.append(html_airport_code_option.replaceAll('{airport_code}', item.code).replaceAll('{airport_name}', item.districtVi).replaceAll('{if_selected}', ''));
                            }
                        });

                        element_from.val(val_from).trigger('change');
                    }
                }
            });
        })
        $('.select-route-to').each(function (index, item) {
            var element_to = $(this);
            var val_to = element_to.val();
            element_to.select2();
            $.ajax({
                url: "/FlightWarehouse/AirPortCodeSuggestion",
                type: "post",
                data: { txt_search: "" },
                success: function (result) {
                    if (result != undefined && result.data != undefined && result.data.length > 0) {
                        result.data.forEach(function (item) {
                            var html_airport_code_option = '<option value="{airport_code}" {if_selected}>{airport_code} ({airport_name})</option>';
                           
                            if (element_to.find('option[value="' + item.code + '"]').length == 0) {
                                element_to.append(html_airport_code_option.replaceAll('{airport_code}', item.code).replaceAll('{airport_name}', item.districtVi).replaceAll('{if_selected}', ''));
                            }
                        });
                        element_to.val(val_to).trigger('change');
                    }
                }
            });
        })


        // Transport button toggle
        $(document).on('click', '.btn-transport', function () {
            var block = $(this).closest('.itinerary-block');
            block.find('.btn-transport').removeClass('active').removeAttr('style');
            $(this).addClass('active');
            var routeType = parseInt(block.data('type'));
            if (routeType === 1) {
                $(this).css({ 'background': '#e8f5e9', 'color': '#4caf50', 'border-color': '#c8e6c9' });
            } else {
                $(this).css({ 'background': '#fff3e0', 'color': '#ff9800', 'border-color': '#ffe0b2' });
            }
            block.find('.ip-transport-type').val($(this).data('val'));
        });

        // Auto calculate profit
        $(document).on('input', '.currency', function () {
            _tour_departure.CalcProfit();
        });
        this.CalcProfit();
    },
    CalcProfit: function () {
        // Người lớn (ADT)
        var adtLeNhap = this.ParseCurrency($('.price-adt.le-nhap').val());
        var adtLeBan = this.ParseCurrency($('.price-adt.le-ban').val());
        var adtDlNhap = this.ParseCurrency($('.price-adt.dl-nhap').val());
        var adtDlBan = this.ParseCurrency($('.price-adt.dl-ban').val());
        $('.price-adt.le-loi').text(this.FormatNumber(adtLeBan - adtLeNhap));
        $('.price-adt.dl-loi').text(this.FormatNumber(adtDlBan - adtDlNhap));

        // Trẻ em (CHD)
        var chdLeNhap = this.ParseCurrency($('.price-chd.le-nhap').val());
        var chdLeBan = this.ParseCurrency($('.price-chd.le-ban').val());
        var chdDlNhap = this.ParseCurrency($('.price-chd.dl-nhap').val());
        var chdDlBan = this.ParseCurrency($('.price-chd.dl-ban').val());
        $('.price-chd.le-loi').text(this.FormatNumber(chdLeBan - chdLeNhap));
        $('.price-chd.dl-loi').text(this.FormatNumber(chdDlBan - chdDlNhap));

        // Em bé (INF)
        var infLeNhap = this.ParseCurrency($('.price-inf.le-nhap').val());
        var infLeBan = this.ParseCurrency($('.price-inf.le-ban').val());
        var infDlNhap = this.ParseCurrency($('.price-inf.dl-nhap').val());
        var infDlBan = this.ParseCurrency($('.price-inf.dl-ban').val());
        $('.price-inf.le-loi').text(this.FormatNumber(infLeBan - infLeNhap));
        $('.price-inf.dl-loi').text(this.FormatNumber(infDlBan - infDlNhap));
    },
    FormatNumber: function (val) {
        if (val === undefined || val === null) return "0";
        return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    },
    ParseCurrency: function (val) {
        if (!val) return 0;
        return parseFloat(val.toString().replace(/[^0-9.-]/g, '')) || 0;
    },
    GatherItineraries: function () {
        var list = [];
        $('.itinerary-block').each(function () {
            var block = $(this);
            var item = {
                Id: parseInt(block.find('.ip-id').val()) || 0,
                TourDepartureId: 0,
                RouteType: parseInt(block.data('type')),
                TransportType: parseInt(block.find('.ip-transport-type').val()) || 0,
                StartPoint: block.find('.select-route-from').val() || '',
                EndPoint: block.find('.select-route-to').val() || '',
                TransportProvider: block.find('.ip-provider').val() || '',
                DepartureDate: _tour_departure.FormatDateToIso(block.find('.ip-date').val()),
                TransportCode: block.find('.ip-code').val() || '',
                Note: block.find('.ip-note').val() || ''
            };
            list.push(item);
        });
        return list;
    },
    GatherPrices: function () {
        var list = [];
        // PriceType = 1: Khách lẻ
        list.push({
            Id: parseInt($('.price-retail-id').val()) || 0,
            TourDepartureId: 0,
            PriceType: 1,
            AdtPrice: this.ParseCurrency($('.price-adt.le-nhap').val()),
            AdtAmount: this.ParseCurrency($('.price-adt.le-ban').val()),
            AdtProfit: this.ParseCurrency($('.price-adt.le-loi').text()),
            ChdPrice: this.ParseCurrency($('.price-chd.le-nhap').val()),
            ChdAmount: this.ParseCurrency($('.price-chd.le-ban').val()),
            ChdProfit: this.ParseCurrency($('.price-chd.le-loi').text()),
            InfPrice: this.ParseCurrency($('.price-inf.le-nhap').val()),
            InfAmount: this.ParseCurrency($('.price-inf.le-ban').val()),
            InfProfit: this.ParseCurrency($('.price-inf.le-loi').text())
        });
        // PriceType = 2: Đại lý
        list.push({
            Id: parseInt($('.price-agent-id').val()) || 0,
            TourDepartureId: 0,
            PriceType: 2,
            AdtPrice: this.ParseCurrency($('.price-adt.dl-nhap').val()),
            AdtAmount: this.ParseCurrency($('.price-adt.dl-ban').val()),
            AdtProfit: this.ParseCurrency($('.price-adt.dl-loi').text()),
            ChdPrice: this.ParseCurrency($('.price-chd.dl-nhap').val()),
            ChdAmount: this.ParseCurrency($('.price-chd.dl-ban').val()),
            ChdProfit: this.ParseCurrency($('.price-chd.dl-loi').text()),
            InfPrice: this.ParseCurrency($('.price-inf.dl-nhap').val()),
            InfAmount: this.ParseCurrency($('.price-inf.dl-ban').val()),
            InfProfit: this.ParseCurrency($('.price-inf.dl-loi').text())
        });
        return list;
    },
    OnSearch: function () {
        var fromDate = '';
        var toDate = '';
        var dateStr = $('#ip_search_date').val();
        if (dateStr) {
            var dateArr = dateStr.split(' - ');
            if (dateArr.length == 2) {
                fromDate = dateArr[0];
                toDate = dateArr[1];
            }
        }

        var searchModel = {
            TourProductId: null, // Depending on if you want to search by exact ID or name, here we'll let controller handle or not map
            Status: $('#sl_search_status').val(),
            FromDateStr: fromDate,
            ToDateStr: toDate,
            PageIndex: 1,
            PageSize: 20
        };

        this.SearchParam(searchModel);
    },
    OnPaging: function (value) {
        var fromDate = '';
        var toDate = '';
        var dateStr = $('#ip_search_date').val();
        if (dateStr) {
            var dateArr = dateStr.split(' - ');
            if (dateArr.length == 2) {
                fromDate = dateArr[0];
                toDate = dateArr[1];
            }
        }
        var searchModel = {
            TourProductId: null,
            Status: $('#sl_search_status').val(),
            FromDateStr: fromDate,
            ToDateStr: toDate,
            PageIndex: value,
            PageSize: 20
        };
        this.SearchParam(searchModel);
    },
    SearchParam: function (searchModel) {
        $('#grid_data').html('<div class="col-xl-12 text-center" style="margin-top:200px"><img src="/images/icons/loading.gif" /></div>');
        $.ajax({
            url: "/TourDeparture/Search",
            type: "Post",
            data: searchModel,
            success: function (result) {
                $('#grid_data').html(result);
            }
        });
    },
    Save: function () {
        // Validate required fields
        if ($('#TourProductId').val() == null || $('#TourProductId').val() == '') {
            alert('Vui lòng chọn sản phẩm tour');
            return false;
        }
        if ($('#Total').val() == '' || parseInt($('#Total').val()) < 0) {
            alert('Vui lòng nhập số chỗ hợp lệ');
            return false;
        }
        if ($('#StartDate').val() == '') {
            alert('Vui lòng chọn ngày khởi hành');
            return false;
        }
        if ($('#EndDate').val() == '') {
            alert('Vui lòng chọn ngày về');
            return false;
        }

        var model = {
            Id: $('#Id').val(),
            TourProductId: $('#TourProductId').val(),
            StartDate: this.FormatDateToIso($('#StartDate').val()),
            EndDate: this.FormatDateToIso($('#EndDate').val()),
            BookingDeadline: this.FormatDateToIso($('#BookingDeadline').val()),
            Total: $('#Total').val(),
            Status: $('#Status').val(),
            Note: $('#Note').val(),
            IsShowWebsite: $('#IsShowWebsite').is(':checked'),
            IsShowWaitingList: $('#IsShowWaitingList').is(':checked'),
            IsAllowReserveOnline: $('#IsAllowReserveOnline').is(':checked'),
            IsAllowDepositOnline: $('#IsAllowDepositOnline').is(':checked'),
            IsFeatured: $('#IsFeatured').is(':checked'),
            TourItineraries: this.GatherItineraries(),
            TourPrices: this.GatherPrices()
        };

        $.ajax({
            url: "/TourDeparture/Save",
            type: "Post",
            contentType: "application/json",
            data: JSON.stringify(model),
            success: function (result) {
                if (result.status === 1) {
                    _msgalert.success(result.msg);
                    setTimeout(function () { window.location.href = "/TourDeparture"; },500)
                    
                } else {
                    _msgalert.error(result.msg);
                }
            }
        });
    },
    FormatDateToIso: function (dateStr) {
        if (!dateStr) return null;
        // input format: DD/MM/YYYY HH:mm
        var parts = dateStr.split(' ');
        var dateParts = parts[0].split('/');
        var timeStr = parts.length > 1 ? parts[1] : "00:00:00";
        // format ISO: YYYY-MM-DDTHH:mm:ss
        return dateParts[2] + '-' + dateParts[1] + '-' + dateParts[0] + 'T' + timeStr + ':00';
    }
};

$(document).ready(function () {
    if ($('#ip_search_date').length) {
        _tour_departure.Init();
    }
});
