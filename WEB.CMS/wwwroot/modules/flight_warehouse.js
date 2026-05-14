var _flight_warehouse = {
    Init: function () {
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
                                text: item.code+'('+ item.districtVi+')',
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
            BookingCode: $('#search-booking-code').val(),
            DeparturePoint: $('#search-departure').val(),
            ArrivalPoint: $('#search-arrival').val(),
            Airline: $('#search-airline').val(),
            Date: $('#Date').val()
        };
        $.ajax({
            url: "/FlightWarehouse/Search",
            type: "POST",
            data: obj,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },
    OnPaging: function (pageIndex) {
        var obj = {
            BookingCode: $('#search-booking-code').val(),
            DeparturePoint: $('#search-departure').val(),
            ArrivalPoint: $('#search-arrival').val(),
            Airline: $('#search-airline').val(),
            Date: $('#Date').val(),
            pageIndex: pageIndex
        };
        $.ajax({
            url: "/FlightWarehouse/Search",
            type: "POST",
            data: obj,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },
    Upsert: function (id, order_id, group_fly) {
        $.ajax({
            url: "/FlightWarehouse/Upsert",
            type: "GET",
            data: { id: id, order_id: order_id, group_fly: group_fly },
            success: function (result) {
                $('#flight-warehouse-upsert-modal').remove();
                $('body').append(result);
                _flight_warehouse.InitUpsert();
            }
        });
    },
    Detail: function (id) {
        window.location.href = "/FlightWarehouse/Detail?id=" + id;
    },
    InitUpsert: function () {
        $('.datepicker').each(function () {
            var element = $(this);
            var row = element.closest('.segment-row');
            var timeValue = row.find('.segment-time').val();
            var dateValue = element.val();
            var fullValue = dateValue;
            if (timeValue && timeValue.length === 5) fullValue += ' ' + timeValue;

            element.daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                timePicker: true,
                timePicker24Hour: true,
                startDate: (fullValue && fullValue.length >= 10) ? moment(fullValue, 'DD/MM/YYYY') : moment(),
                locale: {
                    format: 'DD/MM/YYYY'
                }
            });
        });

        $('.datepicker').on('apply.daterangepicker', function (ev, picker) {
            var date = picker.startDate.format('DD/MM/YYYY');
            var time = picker.startDate.format('HH:mm');

            $(this).val(date);
            $(this).closest('.segment-row').find('.segment-time').val(time);
        });

        $(document).on('change', '.segment-time', function () {
            var row = $(this).closest('.segment-row');
            var dateInput = row.find('.segment-date');
            var timeInput = row.find('.segment-time');
            var dateValue = dateInput.val();
            var timeValue = $(this).val();
            if (dateValue && timeValue && timeValue.length === 5) {
                var fullValue = dateValue + ' ' + timeValue;
                if (fullValue.length >= 10 && dateInput.data('daterangepicker')) {
                    dateInput.data('daterangepicker').setStartDate(moment(fullValue, 'DD/MM/YYYY'));
                    timeInput.data('daterangepicker').setStartDate(moment(fullValue, 'HH:mm'));
                }
            }
        });

        $(document).off('input', '.currency').on('input', '.currency', function () {
            var val = $(this).val().replace(/,/g, '');
            if (!isNaN(val) && val !== '') {
                $(this).val(parseFloat(val).toLocaleString('en-US'));
            }
            _flight_warehouse.CalculateProfit($(this).closest('tr'));
        });

        $('#upsert-trip-type').on('change', function () {
            if ($(this).val() == '1') {
                $('#back-segment-container').hide();
            } else {
                $('#back-segment-container').show();
            }
        });
        $('#upsert-trip-type').trigger('change');

        // Initial profit calculation for all rows
        $('.price-data-row, .price-row').each(function () {
            _flight_warehouse.CalculateProfit($(this));
        });

        this.AirPortCodeSuggestion();
        this.AirlineCodeSuggesstion();
    },
    CalculateProfit: function (row) {
        var netVal = row.find('.price-net').val() || "0";
        var sellVal = row.find('.price-sell').val() || "0";

        var net = parseFloat(netVal.toString().replace(/,/g, '')) || 0;
        var sell = parseFloat(sellVal.toString().replace(/,/g, '')) || 0;

        var profit = sell - net;
        var profitCell = row.find('.price-profit');

        if (profitCell.length > 0) {
            profitCell.text(profit.toLocaleString('en-US'));
            if (profit < 0) {
                profitCell.removeClass('green').addClass('red');
            } else {
                profitCell.removeClass('red').addClass('green');
            }
        }
    },
    CloseModal: function () {
        $('#flight-warehouse-upsert-modal').remove();
    },
    AddSegmentRow: function () {
        var html = `
            <tr class="segment-row" data-id="0">
                <td>
                    <select class="form-control segment-type">
                        <option value="0">Đi</option>
                        <option value="1">Về</option>
                    </select>
                </td>
                <td><input type="text" class="form-control datepicker segment-date" value=""></td>
                <td><input type="text" class="form-control segment-airline" value=""></td>
                <td><input type="text" class="form-control segment-flight-code" value=""></td>
                <td><input type="text" class="form-control segment-pnr" value=""></td>
                <td><button type="button" class="btn btn-danger btn-sm" onclick="$(this).closest('tr').remove()"><i class="fa fa-times"></i></button></td>
            </tr>`;
        $('#segment-table tbody').append(html);
        _flight_warehouse.InitUpsert();
    },
    AddPriceRow: function () {
        var html = `
            <tr class="price-row" data-id="0">
                <td>
                    <select class="form-control price-type">
                        <option value="1">Giá nét</option>
                        <option value="2">Giá bán</option>
                    </select>
                </td>
                <td><input type="text" class="form-control apply-agencies" value=""></td>
                <td><input type="text" class="form-control currency adt-price" value="0"></td>
                <td><input type="text" class="form-control currency adt-amount" value="0"></td>
                <td><input type="text" class="form-control currency chd-price" value="0"></td>
                <td><input type="text" class="form-control currency chd-amount" value="0"></td>
                <td><input type="text" class="form-control currency inf-price" value="0"></td>
                <td><input type="text" class="form-control currency inf-amount" value="0"></td>
                <td><button type="button" class="btn btn-danger btn-sm" onclick="$(this).closest('tr').remove()"><i class="fa fa-times"></i></button></td>
            </tr>`;
        $('#price-table tbody').append(html);
        _flight_warehouse.InitUpsert();
    },
    Submit: function () {
        var form = $('#flight-warehouse-form');
        var obj = {
            Booking: {
                Id: parseInt(form.find('input[name="Id"]').val()) || 0,
                TripType: parseInt(form.find('select[name="TripType"]').val()) || 0,
                DeparturePoint: form.find('select[name="DeparturePoint"]').val() || "",
                ArrivalPoint: form.find('select[name="ArrivalPoint"]').val() || "",
                TotalTicket: parseInt(form.find('input[name="TotalTicket"]').val()) || 0,
                IsRefundable: parseInt(form.find('select[name="IsRefundable"]').val()) || 0,
                CarryOnBaggage: form.find('input[name="CarryOnBaggage"]').val() || "",
                CheckedBaggage: form.find('input[name="CheckedBaggage"]').val() || ""
            },
            Segments: [],
            Prices: []
        };

        $('.segment-row:visible').each(function () {
            var row = $(this);
            var dateStr = row.find('.segment-date').val();
            var timeStr = row.find('.segment-time').val();
            var flightDate = dateStr;
            if (dateStr) {
                var parts = dateStr.split('/');
                flightDate += ' ' + timeStr + ':00';
                //if (parts.length === 3) {
                //    flightDate = parts[2] + '-' + parts[1] + '-' + parts[0];
                //    flightDate += ' ' + timeStr + ':00';
                //}
            }
            obj.Segments.push({
                Id: parseInt(row.attr('data-id')) || 0,
                BookingId: obj.Booking.Id,
                SegmentType: parseInt(row.find('.segment-type').val()) || 0,
                FlightDateStr: flightDate,
                Airline: row.find('.segment-airline').val() || "",
                FlightCode: row.find('.segment-flight-code').val() || "",
                Pnrcode: row.find('.segment-pnr').val() || ""
            });
        });

        var netPriceId = parseInt($('#net-price-id').val()) || 0;
        var sellPriceId = parseInt($('#sell-price-id').val()) || 0;

        // Helper to get decimal value safely
        var getDecimal = function (container, selector) {
            var val = container.find(selector).val();
            if (!val) return 0;
            return parseFloat(val.replace(/,/g, '')) || 0;
        };

        // 1: Set Đại lý (Agent)
        var agentRows = $('.price-policy-box table').eq(0).find('.price-data-row');
        if (agentRows.length > 0) {
            var adtRow = agentRows.filter('[data-type="adt"]');
            var chdRow = agentRows.filter('[data-type="chd"]');
            var infRow = agentRows.filter('[data-type="inf"]');

            obj.Prices.push({
                Id: netPriceId,
                BookingId: obj.Booking.Id,
                PriceType: 1,
                AdtAmount: getDecimal(adtRow, '.price-sell'),
                AdtPrice: getDecimal(adtRow, '.price-net'),
                ChdAmount: getDecimal(chdRow, '.price-sell'),
                ChdPrice: getDecimal(chdRow, '.price-net'),
                InfAmount: getDecimal(infRow, '.price-sell'),
                InfPrice: getDecimal(infRow, '.price-net')
            });
        }

        // 2: Set Khách lẻ (Retail)
        var retailRows = $('.price-policy-box table').eq(1).find('.price-data-row');
        if (retailRows.length > 0) {
            var adtRow = retailRows.filter('[data-type="adt"]');
            var chdRow = retailRows.filter('[data-type="chd"]');
            var infRow = retailRows.filter('[data-type="inf"]');

            obj.Prices.push({
                Id: sellPriceId,
                BookingId: obj.Booking.Id,
                PriceType: 2,
                AdtAmount: getDecimal(adtRow, '.price-sell'),
                AdtPrice: getDecimal(adtRow, '.price-net'),
                ChdAmount: getDecimal(chdRow, '.price-sell'),
                ChdPrice: getDecimal(chdRow, '.price-net'),
                InfAmount: getDecimal(infRow, '.price-sell'),
                InfPrice: getDecimal(infRow, '.price-net')
            });
        }

        $.ajax({
            url: "/FlightWarehouse/UpsertBooking",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(obj),
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    _msgalert.error(result.msg);
                }
            }
        });
    },

    AirPortCodeSuggestion: function () {
        var element_from = $('.service-fly-select-route-from');
        var element_to = $('.service-fly-select-route-to');
        var val_from = element_from.val();
        var val_to = element_to.val();

        element_from.select2();
        element_to.select2();

        $.ajax({
            url: "/FlightWarehouse/AirPortCodeSuggestion",
            type: "post",
            data: { txt_search: "" },
            success: function (result) {
                if (result != undefined && result.data != undefined && result.data.length > 0) {
                    result.data.forEach(function (item) {
                        if (element_from.find('option[value="' + item.code + '"]').length == 0) {
                            element_from.append(_order_detail_html.html_airport_code_option.replaceAll('{airport_code}', item.code).replaceAll('{airport_name}', item.districtVi).replaceAll('{if_selected}', ''));
                        }
                        if (element_to.find('option[value="' + item.code + '"]').length == 0) {
                            element_to.append(_order_detail_html.html_airport_code_option.replaceAll('{airport_code}', item.code).replaceAll('{airport_name}', item.districtVi).replaceAll('{if_selected}', ''));
                        }
                    });

                    element_from.val(val_from).trigger('change');
                    element_to.val(val_to).trigger('change');
                }
            }
        });
    },
    ParseDate: function (dateStr) {
        if (!dateStr) return null;
        var parts = dateStr.split('/');
        if (parts.length === 3) {
            return parts[2] + '-' + parts[1] + '-' + parts[0];
        }
        return null;
    },
    AirlineCodeSuggesstion: function () {
        var element = $('.segment-airline')
        var value = element.val()
        element.select2();

        $.ajax({
            url: "/FlightWarehouse/AirlinesSuggestion",
            type: "post",
            data: { txt_search: "" },
            success: function (result) {
                if (result != undefined && result.data != undefined && result.data.length > 0) {
                    result.data.forEach(function (item) {
                        element.append(_order_detail_html.html_airline_option.replaceAll('{airline_code}', item.code).replace('{airline_name}', item.nameVi).replace('{if_selected}', ''))

                    });
                    element.trigger('change');
                }
                else {
                    element.trigger('change');

                }
                element.val(value).trigger('change');

            }
        });
    },
    onSelectPageSize: function () {
        this.Search();
    }
};
