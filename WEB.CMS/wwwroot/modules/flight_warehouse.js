var _flight_warehouse = {
    Init: function () {
        this.Search();
    },
    Search: function () {
        var obj = {
            BookingCode: $('#search-booking-code').val(),
            DeparturePoint: $('#search-departure').val(),
            ArrivalPoint: $('#search-arrival').val(),
            Airline: $('#search-airline').val()
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
    Upsert: function (id) {
        $.ajax({
            url: "/FlightWarehouse/Upsert",
            type: "GET",
            data: { id: id },
            success: function (result) {
                $('#flight-warehouse-upsert-modal').remove();
                $('body').append(result);
                _flight_warehouse.InitUpsert();
            }
        });
    },
    InitUpsert: function () {
        $('.datepicker').daterangepicker({
            singleDatePicker: true,
            showDropdowns: true,
            locale: {
                format: 'DD/MM/YYYY'
            }
        });
        $('.currency').on('keyup', function () {
            var val = $(this).val().replace(/,/g, '');
            if (!isNaN(val) && val !== '') {
                $(this).val(parseFloat(val).toLocaleString('en-US'));
            }
        });
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
        if (!form[0].checkValidity()) {
            form[0].reportValidity();
            return;
        }

        var booking = {
            Id: form.find('input[name="Id"]').val(),
            BookingCode: form.find('input[name="BookingCode"]').val(),
            TripType: form.find('select[name="TripType"]').val(),
            DeparturePoint: form.find('input[name="DeparturePoint"]').val(),
            ArrivalPoint: form.find('input[name="ArrivalPoint"]').val(),
            TotalTicket: form.find('input[name="TotalTicket"]').val(),
            IsRefundable: form.find('select[name="IsRefundable"]').val(),
            CarryOnBaggage: form.find('input[name="CarryOnBaggage"]').val(),
            CheckedBaggage: form.find('input[name="CheckedBaggage"]').val(),
            Note: form.find('textarea[name="Note"]').val()
        };

        var segments = [];
        $('.segment-row').each(function () {
            var row = $(this);
            segments.push({
                Id: row.attr('data-id'),
                SegmentType: row.find('.segment-type').val(),
                FlightDate: _flight_warehouse.ParseDate(row.find('.segment-date').val()),
                Airline: row.find('.segment-airline').val(),
                FlightCode: row.find('.segment-flight-code').val(),
                Pnrcode: row.find('.segment-pnr').val()
            });
        });

        var prices = [];
        $('.price-row').each(function () {
            var row = $(this);
            prices.push({
                Id: row.attr('data-id'),
                PriceType: row.find('.price-type').val(),
                ApplyAgencyIds: row.find('.apply-agencies').val(),
                AdtPrice: row.find('.adt-price').val().replace(/,/g, ''),
                AdtAmount: row.find('.adt-amount').val().replace(/,/g, ''),
                ChdPrice: row.find('.chd-price').val().replace(/,/g, ''),
                ChdAmount: row.find('.chd-amount').val().replace(/,/g, ''),
                InfPrice: row.find('.inf-price').val().replace(/,/g, ''),
                InfAmount: row.find('.inf-amount').val().replace(/,/g, '')
            });
        });

        var data = {
            Booking: booking,
            Segments: segments,
            Prices: prices
        };

        $.ajax({
            url: "/FlightWarehouse/UpsertBooking",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    _flight_warehouse.CloseModal();
                    _flight_warehouse.Search();
                } else {
                    _msgalert.error(result.msg);
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
    }
};
