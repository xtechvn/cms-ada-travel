_price_policy_detail_html = {
    NewPriceDetail_Lv4: `
    <div class="mb10 level_4 add_new_block add_new_block-all" data-value="-1" data-id="-1">
                CPDV: <nw class="readonly price_detail_readonly price_detail_readonly_profit no_display" data-profit="0" data-unitid="VND">0 VND</nw>
                <div class="tag-input-price price_detail_input">
                    <input type="text" class="form-control currency price_detail_input price_detail_input_profit" value="0">
                    <div class="tag  price_detail_input_price_unit ">
                        <a class="active price_detail_unit_active price_detail_event_changeprice_unit price_detail_unit_vnd" data-unitid="2" href="javascript:;">VND</a>
                        <a class=" price_detail_event_changeprice_unit price_detail_unit_percent" data-unitid="1" href="javascript:;">%</a>
                    </div>
                </div>
                <span class="controler show  ml-2 price_detail_input_button">
                    <a href="javascript:;" class="btn-default update_price_detail">Cập nhật</a>
                    <a href="javascript:;" class="ml-1 blue cancel_edit_price_detail" data-profit-value="0" data-profit-unitid="2">Hủy</a>
                </span>
                <span class="controler price_detail_block price_detail_readonly_button no_display" style="">
                    <a href="javascript:;" class="fa fa-pencil ml-2 green enable_edit_price_detail"></a>
                    <a href="javascript:;" class="fa fa-trash ml-1 red delete_price_detail"></a>
                </span>
            </div>
    `,
    NewPriceDetail_Lv2: `
    <div class="mb10 level_4 add_new_block add-new-policy-all" data-level="{level}">
                Lợi nhuận: 
                <div class="tag-input-price price_detail_input">
                    <input type="text" class="form-control currency add-new-policy-all-input" value="0">
                    <div class="tag  price_detail_input_price_unit ">
                        <a class="active add-new-policy-all-unitid" data-unitid="2" href="javascript:;">VND</a>
                        <a class="add-new-policy-all-unitid" data-unitid="1" href="javascript:;">%</a>
                    </div>
                </div>

                <span class="controler show  ml-2 ">
                    <a href="javascript:;" class="btn-default add-new-policy-all-confirm" style="height: 20px; line-height: 20px; font-size: 12px; padding: 0 8px;">Áp dụng</a>
                    <a href="javascript:;" class="ml-1 blue add-new-policy-all-cancel" data-profit-value="0" data-profit-unitid="2">Hủy</a>
                </span>
            </div>
    `,
    CampaignFromDate: '',
    CampaignToDate: ''
}
//-- Hotel Policy:
var _price_policy_detail = {
    Initialization: function () {
        _global_function.RemoveLoading()
        $('.contract_date_range').hide()
        var campaign_id = $('#campaign_code').attr('data-campaign-id')
        var provider_id = $('#policy-hotel-name').find(':selected').val();
        _price_policy_detail.Edited = false
        if (campaign_id != undefined && !isNaN(parseInt(campaign_id)) && parseInt(campaign_id) > 0) {
            _price_policy_detail.Edited = true
        }
        _price_policy_detail.LoadPolicyRule(campaign_id, provider_id);
        $("#policy-hotel-name").select2({
            theme: 'bootstrap4',
            placeholder: "Tên khách sạn",
            dropdownParent: $("#wrap_input_search"),
            ajax: {
                url: "/Order/HotelSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                tags: true,
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
                                text: item.name,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
        $("#client_type_id").select2({
            minimumResultsForSearch: Infinity

        });
        _order_detail_common.SingleDateTimePicker($('input[name="campaign_date_range"]'))
        if (_pricepolicymanagement.LoadingDetail == undefined || !_pricepolicymanagement.LoadingDetail) {
            _pricepolicymanagement.LoadingDetail = true
            _price_policy_detail.DynamicBind()
        }

    },

    Cancel: function () {
        $('body').removeClass('stop-scrolling');

        $.magnificPopup.close();

    },
    FromDate: null,
    ToDate: null,
    Edited: false,
    LoadPolicyRule: function (campaign_id, hotel_id) {
        var fromdate_value = _global_function.ParseJSDate($('.campaign_from_date').val())
        var todate_value = _global_function.ParseJSDate($('.campaign_to_date').val())
        var from_date = $('.campaign_from_date').data('daterangepicker') == undefined ? _global_function.GetDayText(fromdate_value, true) : _global_function.GetDayText($('.campaign_from_date').data('daterangepicker').startDate._d, true)
        var to_date = $('.campaign_to_date').data('daterangepicker') == undefined ? _global_function.GetDayText(todate_value, true) : _global_function.GetDayText($('.campaign_to_date').data('daterangepicker').startDate._d, true)

        $('#box_list_price_policy').html(`<img src="/images/icons/loading.gif" style=" width: 100px; height: 100px;" id="imgLoading" />`)
        $.ajax({
            url: "/PricePolicy/PolicyRule",
            type: "post",
            data: { campaign_id: campaign_id, hotel_id: hotel_id, from_date: from_date, to_date: to_date },
            success: function (result) {
                $('#box_list_price_policy').html(result);
                _price_policy_detail.PolicyRuleInit()

            }
        });

    },

    Summit: function () {

        //-- Validation:

        var campaign_id = $('#campaign_code').attr('data-campaign-id');
        var campaign_code = $('#campaign_code').val();
        if (campaign_code == undefined || campaign_code.trim() == "") {
            _msgalert.error('Mã chính sách giá không được để trống.');
            return;
        }

        //-- regex campaign code
        campaign_code = campaign_code.trim()
        var regex = new RegExp('^[a-zA-Z0-9\-\_\+]*$')
        if (!regex.test(campaign_code)) {
            _msgalert.error('Mã chính sách giá chỉ chứa kí tự chữ, số, -, _');
            return
        }
        var client_type_id = $('#client_type_id :selected').val();
        if (parseInt(client_type_id) < 1) {
            _msgalert.error('Vui lòng chọn loại khách hàng');
            return;
        }
        var provider_id = $('#policy-hotel-name').find(':selected').val();
        if (provider_id == undefined || provider_id.trim() == '' || isNaN(parseInt(provider_id))) {
            _msgalert.error('Vui lòng lựa chọn khách sạn cho chính sách giá');
            return;
        }

        //---- Date: 
        var campaign_ToDate = $('#campaign_to_date').data('daterangepicker').startDate._d;
        var campaign_fromDate = $('#campaign_from_date').data('daterangepicker').startDate._d;
        if (campaign_ToDate < campaign_fromDate) {
            _msgalert.error('Thời gian bắt đầu chính sách giá không được lớn hơn thời gian kết thúc chính sách giá.');
            return;
        }
       
        //-- Summit Data:
        var campaign_description = $('#campaign_description').val();


        var campaign_status = $('input[name="campaign_status"]:checked').val();

        var list_price_policy = [];
        var campaign_detail = {
            CampaignCode: campaign_code,
            Id: campaign_id,
            ClientTypeId: client_type_id,
            Description: campaign_description,
            Status: campaign_status,
            ContractType: 1,
            FromDate: _price_policy_detail.GetDayText($('#campaign_from_date').data('daterangepicker').startDate._d, true),
            ToDate: _price_policy_detail.GetDayText($('#campaign_to_date').data('daterangepicker').startDate._d, true)
        }

        if ($('.level_4_block').length < 1) {
            _msgalert.error('Khách sạn này không có chương trình, vui lòng bổ sung chương trình hoặc chọn khách sạn khác');
            return;
        }
        var is_failed = false;
        var msg_local = '';
        $('.level_4_block').each(function (i, obj) {
            var element = $(this);

            var product_service = {
                Id: element.find('.level_4_title').find('.room_name').attr('data-productserviceid'),
                ContractNo: element.closest('li').find('.level_1_title').find('.contract_no').attr('data-value'),
                PackageId: element.closest('.level_2_content').find('.level_2_title').find('.package_name').attr('data-value'),
                PackageName: element.closest('.level_2_content').find('.level_2_title').find('.package_name').text(),
                RoomID: element.find('.level_4_title').find('.room_name').attr('data-roomid'),
                ProductServiceID: element.find('.level_4_title').find('.room_name').attr('data-productserviceid'),
                AllotmentsId: element.find('.level_4_title').find('.room_name').attr('data-allotmentid'),
                GroupProviderType: element.find('.level_4_title').find('.room_name').attr('data-group-provider'),
                ProviderId: provider_id
            }
            var price_policy = [];

            element.find('.level_4').each(function (i, obj) {
                var element_policy = $(this);
                if (!$.isNumeric(element_policy.find('.price_detail_input_profit').val().replaceAll(',', '')) || parseFloat(element_policy.find('.price_detail_input_profit').val().replaceAll(',', '')) < 0) {
                    var err_room_name = element.closest('.level_4_block').find('.level_4_title').find('.room_name').text();
                    msg_local = ('Chính sách thứ [' + (price_policy.length + 1) + '] tại phòng ' + err_room_name + ': Lợi nhuận không được nhỏ hơn [0]');
                    is_failed = true;
                    return false;
                }
                var unit_id_text = element_policy.find('.price_detail_unit_active').attr('data-unitid');
                if (unit_id_text == undefined) unit_id_text = '0';
                var policy = {
                    Id: element_policy.attr('data-value'),
                    ProductServiceId: element_policy.find('.level_4_title').find('.room_name').attr('data-productserviceid'),
                    GroupProviderType: element_policy.find('.level_4_title').find('.room_name').attr('data-group-provider'),
                    ServiceType: $('.campaign_detail_value').attr('data-servicetype'),
                    Profit: element_policy.find('.price_detail_input_profit').val().replace(',', ''),
                    UnitID: unit_id_text,
                    FromDate: campaign_fromDate,
                    ToDate: campaign_ToDate,
                    Price: element_policy.find('.detail_price').attr('data-value'),
                    AmountLast: element_policy.find('.price_detail_total_amount').attr('data-value')
                }

                price_policy.push(policy);

            });
            if (is_failed) {
                return false;
            }
            var product_room_service = {
                Detail: product_service,
                PriceDetail: price_policy
            };
            list_price_policy.push(product_room_service);
        });
        if (is_failed) {
            _msgalert.error(msg_local);
            return;
        }
        var model = {
            Detail: campaign_detail,
            PricePolicy: list_price_policy
        }
        var no_policy_exists = true;
        $.each(model.PricePolicy, function (index, value) {
            if (value.PriceDetail != undefined && value.PriceDetail.length > 0) {
                no_policy_exists = false;
                return false;
            }
        });



        if (!no_policy_exists) {
            let title = 'Xác nhận lưu';
            let description = 'Mọi thông tin về chính sách giá hiện tại sẽ được lưu, bạn có chắc chắn không?';
            _msgconfirm.openDialog(title, description, function () {
                $('.img_loading_summit').css("display", "");
                _price_policy_detail.SummitData()
            });
        }
        else {
            _msgalert.error('Vui lòng nhập ít nhất 01 chính sách giá cho ít nhất 01 loại phòng bất kỳ');

            return;
        }

    },
    CollapseAll: function () {
        $('.lvl_collapse').each(function (index, obj) {
            var element = $(this);
            var level = element.attr('data-level');
            switch (level) {
                case '1': {
                    element.closest('li').find('.level_2_content').hide();
                    element.removeClass('fa-minus');
                    element.addClass('fa-plus');

                } break;
                case '2': {
                    element.closest('.level_2_content').find('.level_3_content').hide();
                    element.removeClass('fa-minus');
                    element.addClass('fa-plus');

                } break;
                case '4': {
                    element.closest('.level_4_block').find('.level_4_content').hide();
                    element.removeClass('fa-minus');
                    element.addClass('fa-plus');
                } break;
                case '5': {
                    element.closest('li').find('.level_2_content').each(function () {
                        var element = $(this);
                        element.hide();
                    });
                    element.closest('li').find('.level_3_content').each(function () {
                        var element = $(this);
                        element.hide();
                    });
                    element.closest('li').find('.level_4_content').each(function () {
                        var element = $(this);
                        element.hide();
                    });
                    element.closest('li').find('.lvl_collapse').addClass('fa-plus');
                    element.closest('li').find('.lvl_collapse').removeClass('fa-minus');
                } break;
            }
        });
        $('.expand_all').removeClass('active')
        $('.collapse_all').addClass('active')
    },
    ExpandAll: function () {
        $('.lvl_collapse').each(function (index, obj) {
            var element = $(this);
            var level = element.attr('data-level');
            switch (level) {
                case '1': {
                    element.closest('li').find('.level_2_content').show();
                    element.removeClass('fa-plus');
                    element.addClass('fa-minus');

                } break;
                case '2': {
                    element.closest('.level_2_content').find('.level_3_content').show();
                    element.removeClass('fa-plus');
                    element.addClass('fa-minus');
                } break;
                case '4': {
                    element.closest('.level_4_block').find('.level_4_content').show();
                    element.removeClass('fa-plus');
                    element.addClass('fa-minus');
                } break;
                case '5': {
                    element.closest('li').find('.level_2_content').each(function () {
                        var element_lvl1 = $(this);
                        element_lvl1.show();
                        element_lvl1.find('.level_3_content').each(function () {
                            var element_lv2 = $(this);
                            element_lv2.show();
                            element_lv2.find('.level_4_block').find('.level_4_content').each(function () {
                                var element_lv3 = $(this);
                                element_lv3.show();
                            });
                        });

                    });
                    element.closest('li').find('.lvl_collapse').removeClass('fa-plus');
                    element.closest('li').find('.lvl_collapse').addClass('fa-minus');
                } break;
            }
        });
        $('.expand_all').addClass('active')
        $('.collapse_all').removeClass('active')
    },

    FormatMoney: function (number, currency) {
        if (number == undefined) {
            return 0;
        }
        else if (!$.isNumeric(number)) {
            return number;
        }
        var formatter = new Intl.NumberFormat('en-US');
        switch (currency) {
            case 'VND': {
                number = number.toFixed(0);
                return formatter.format(number);
            }
            case 'USD': {
                number = number.toFixed(2);
                return formatter.format(number);
            }
            case 'percent': {
                return number.toFixed(2);
            }
            default: {
                return number.toFixed(2);
            }
        }
    },


    PolicyRuleInit: function () {
        $('.add_new_policy_rule').hide();

        $('.price_detail_readonly_profit').each(function (i, obj) {
            var element = $(this);
            var unit_id = element.closest('.level_4').find('.price_detail_unit_active').attr('data-unitid');
            if (unit_id != undefined) {
                switch (parseInt(unit_id)) {
                    case 1: {
                        element.html(_price_policy_detail.FormatMoney(parseFloat(element.attr('data-profit')), 'percent') + ' ' + element.attr('data-unitid'));

                    } break;
                    case 2: {
                        element.html(_price_policy_detail.FormatMoney(parseFloat(element.attr('data-profit')), 'VND') + ' ' + element.attr('data-unitid'));

                    } break;
                    default: {
                        element.html(_price_policy_detail.FormatMoney(parseFloat(element.attr('data-profit')), 'others') + ' ' + element.attr('data-unitid'));

                    } break;

                }
            }
        });
        $('.price_detail_input_profit').each(function (i, obj) {
            var element = $(this);
            var unit_id = element.closest('.level_4').find('.price_detail_unit_active').attr('data-unitid');
            if (unit_id != undefined) {
                switch (parseInt(unit_id)) {
                    case 1: {
                        element.val(_price_policy_detail.FormatMoney(parseFloat(element.val()), 'percent')).change();

                    } break;
                    case 2: {
                        element.val(_price_policy_detail.FormatMoney(parseFloat(element.val()), 'VND')).change();

                    } break;
                    default: {
                        element.val(_price_policy_detail.FormatMoney(parseFloat(element.val()), 'others')).change();

                    } break;

                }
            }
        });

        _price_policy_detail.CollapseAll();

        //-- Format Money:
        $('.detail_price').each(function () {
            var element = $(this);
            element.text(_price_policy_detail.FormatMoney(parseFloat(element.attr('data-value')), 'VND') + ' VND');
        });
        $('.price_detail_total_amount').each(function () {
            var element = $(this);
            element.text(_price_policy_detail.FormatMoney(parseFloat(element.attr('data-value')), 'VND') + ' VND');
        });
    },


    RestoreLastSavedInput: function (price_detail_element = undefined) {

        if (price_detail_element != undefined) {
            if (price_detail_element.closest('.level_4').find('.price_detail_input_allotment_price').length > 0) {
                price_detail_element.closest('.level_4').find('.price_detail_input_allotment_price').val(price_detail_element.attr('data-allotmentprice-value'));
            }

            if (price_detail_element.attr('data-profit-value') != undefined) {
                price_detail_element.closest('.level_4').find('.price_detail_input_profit').val(price_detail_element.attr('data-profit-value'));

                price_detail_element.closest('.level_4').find('.price_detail_event_changeprice_unit').each(function (i, obj) {
                    var ele = $(this);
                    ele.removeClass('active');
                    ele.removeClass('price_detail_unit_active');
                    if (ele.attr('data-unitid') == price_detail_element.attr('data-profit-unitid')) {
                        ele.addClass('active');
                        ele.addClass('price_detail_unit_active');
                    }

                });
            }
        }

    },

    ChangeCampaignDateRange: function (FromDate, ToDate) {
        $('input[name="campaign_date_range"]').daterangepicker({
            singleDatePicker: true,
            showDropdowns: true,
            drops: 'down',
            timePicker: true,
            timePicker24Hour: true,
            minDate: FromDate,
            maxDate: ToDate,
            locale: {
                format: 'DD/MM/YYYY HH:mm'
            }
        }, function (start, end, label) {


        });
    },
    GetDayText: function (date, donetdate = false) {
        var text = ("0" + date.getDate()).slice(-2) + '/' + ("0" + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear() + ' ' + ("0" + date.getHours()).slice(-2) + ':' + ("0" + date.getMinutes()).slice(-2);
        if (donetdate) {
            text = ("0" + (date.getMonth() + 1)).slice(-2) + '/' + ("0" + date.getDate()).slice(-2) + '/' + date.getFullYear() + ' ' + ("0" + date.getHours()).slice(-2) + ':' + ("0" + date.getMinutes()).slice(-2);
        }
        return text;
    },
    DynamicBind: function () {

        $(".white-popup").on('scroll', function () {
            if ($('input[name="campaign_date_range"]') != undefined) {
                $('input[name="campaign_date_range"]').each(function (i, obj) {
                    $(this).data('daterangepicker').hide();
                });
            }

            if ($('input[name="price_detail_date"]') != undefined) {
                $('input[name="price_detail_date"]').each(function (i, obj) {
                    $(this).data('daterangepicker').hide();
                });
            }
            if ($('input[name="price_detail_date_new"]') != undefined) {
                $('input[name="price_detail_date_new"]').each(function (i, obj) {
                    $(this).data('daterangepicker').hide();
                });
            }
            if ($('input[name="price_detail_date_add_new"]') != undefined) {
                $('input[name="price_detail_date_add_new"]').each(function (i, obj) {
                    $(this).data('daterangepicker').hide();
                });
            }
        });
        //---- Expand All click
        $('body').on('click', '.expand_all', function (event) {
            var element = $(this);
            if (!element.hasClass('active')) {
                _price_policy_detail.ExpandAll();
                element.addClass('active');
                $('.collapse_all').removeClass('active');
            }
        });
        //---- CollapseAll Click
        $('body').on('click', '.collapse_all', function (event) {
            var element = $(this);
            if (!element.hasClass('active')) {
                _price_policy_detail.CollapseAll();
                element.addClass('active');
                $('.expand_all').removeClass('active');
            }
        });
       
        $('body').on('select2:select', '.policy-hotel-name', function (event) {
            var element = $(this)
            var campaign_id = $('#campaign_code').attr('data-campaign-id');
            var provider_id = element.find(':selected').val();
            var contract_type = 1



            if ($('#box_list_price_policy').find('ul').length <= 0) {
                $('#box_list_price_policy').val('').change();
                _price_policy_detail.LoadPolicyRule('-1', provider_id);
            }
            else {
                let title = 'Xác nhận đổi nhà cung cấp';
                let description = 'Mọi thông tin về chính sách giá hiện tại sẽ được xóa, bạn có chắc chắn không?';
                _msgconfirm.openDialog(title, description, function () {
                    _price_policy_detail.LoadPolicyRule('-1', provider_id);
                    $('#campaign_description').val('').change();
                });
            }


        });
        $('body').on('mouseenter', '.level_4', function (event) {
            if (!$(this).find('.price_detail_input_button').hasClass('no_display')) {
                $(this).closest('.level_4').find('.price_detail_readonly_button').addClass('no_display')

            }
            else {
                $(this).closest('.level_4').find('.price_detail_readonly_button').removeClass('no_display')

                $(this).find('.price_detail_readonly_button').show();
            }

        });
        $('body').on('mouseleave', '.level_4', function (event) {
            if (!$(this).find('.price_detail_input_button').hasClass('no_display')) {
                $(this).closest('.level_4').find('.price_detail_readonly_button').addClass('no_display')

            }
            else {
                $(this).closest('.level_4').find('.price_detail_readonly_button').addClass('no_display')
                $(this).find('.price_detail_readonly_button').hide();
            }

        });

        //$('body').on('mouseenter', '.level_4_title', function (event) {
        //    $(this).find('.add_new_policy_rule').show();


        //});
        //$('body').on('mouseleave', '.level_4_title', function (event) {
        //    $(this).find('.add_new_policy_rule').hide();

        //});
        $('body').on('mouseenter', '.level_2_title', function (event) {
            $(this).find('.add_new_policy_rule').show();


        });
        $('body').on('mouseleave', '.level_2_title', function (event) {
            $(this).find('.add_new_policy_rule').hide();

        });
        $('body').on('mouseenter', '.level_1_title', function (event) {
            $(this).find('.add_new_policy_rule').show();


        });
        $('body').on('mouseleave', '.level_1_title', function (event) {
            $(this).find('.add_new_policy_rule').hide();

        });
        $('body').on('click', '.lvl_collapse', function (event) {

            var level = $(this).attr('data-level');
            switch (level) {
                case '1': {
                    if ($(this).hasClass('fa-minus')) {
                        $(this).closest('li').find('.level_2_content').hide();
                        $(this).removeClass('fa-minus');
                        $(this).addClass('fa-plus');
                    }
                    else {
                        $(this).closest('li').find('.level_2_content').show();
                        $(this).removeClass('fa-plus');
                        $(this).addClass('fa-minus');
                    }

                } break;
                case '2': {
                    if ($(this).hasClass('fa-minus')) {
                        $(this).closest('.level_2_content').find('.level_3_content').hide();
                        $(this).removeClass('fa-minus');
                        $(this).addClass('fa-plus');
                        $(this).closest('.level_2_content').find('.level_4_title').find('.lvl_collapse').each(function (i, obj) {
                            var element = $(this);
                            if (element.hasClass('fa-minus')) element.trigger('click');
                        });
                    }
                    else {
                        $(this).closest('.level_2_content').find('.level_3_content').show();
                        $(this).removeClass('fa-plus');
                        $(this).addClass('fa-minus');
                        $(this).closest('.level_2_content').find('.level_4_title').find('.lvl_collapse').each(function (i, obj) {
                            var element = $(this);
                            if (element.hasClass('fa-plus')) element.trigger('click');
                        });
                    }
                } break;
                case '4': {
                    if ($(this).hasClass('fa-minus')) {
                        $(this).closest('.level_4_block').find('.level_4_content').hide();
                        $(this).removeClass('fa-minus');
                        $(this).addClass('fa-plus');
                    }
                    else {
                        $(this).closest('.level_4_block').find('.level_4_content').show();
                        $(this).removeClass('fa-plus');
                        $(this).addClass('fa-minus');
                    }
                } break;
                case '5': {

                    if ($(this).hasClass('fa-minus')) {
                        $(this).closest('li').find('.level_2_content').hide();
                        $(this).closest('li').find('.level_2_title').find('em').addClass('fa-plus');
                        $(this).closest('li').find('.level_3_content').hide();
                        $(this).removeClass('fa-minus');
                        $(this).addClass('fa-plus');
                        $(this).closest('li').find('.level_4_title').find('.lvl_collapse').each(function (i, obj) {
                            var element = $(this);
                            if (element.hasClass('fa-plus')) element.trigger('click');
                        });
                    }
                    else {
                        $(this).closest('li').find('.level_2_content').show();
                        $(this).closest('li').find('.level_2_title').find('em').addClass('fa-minus');
                        $(this).closest('li').find('.level_3_content').show();
                        $(this).removeClass('fa-plus');
                        $(this).addClass('fa-minus');
                        $(this).closest('li').find('.level_4_title').find('.lvl_collapse').each(function (i, obj) {
                            var element = $(this);
                            if (element.hasClass('fa-plus')) element.trigger('click');
                        });
                    }
                } break;
            }

        });
        $('body').on('click', '.enable_edit_price_detail', function (event) {
            $(this).closest('.level_4').find('.price_detail_input_button').removeClass('no_display')
            $(this).closest('.level_4').find('.price_detail_readonly_button').addClass('no_display')
            $(this).closest('.level_4').find('.price_detail_readonly').addClass('no_display')
            $(this).closest('.level_4').find('.price_detail_input').removeClass('no_display')
        });

        $('body').on('click', '.cancel_edit_price_detail', function (event) {
            $(this).closest('.level_4').find('.price_detail_input_button').addClass('no_display')
            $(this).closest('.level_4').find('.price_detail_readonly_button').removeClass('no_display')
            $(this).closest('.level_4').find('.price_detail_readonly').removeClass('no_display')
            $(this).closest('.level_4').find('.price_detail_input').addClass('no_display')

            _price_policy_detail.UpdateReadOnlyDateField($(this));
            _price_policy_detail.RestoreLastSavedInput($(this));
        });
        $('body').on('click', '.price_detail_event_changeprice_unit', function (event) {
            var element = $(this);
            if (!element.hasClass('active')) {
                element.closest('.price_detail_input_price_unit').find('.price_detail_event_changeprice_unit').removeClass('active');
                element.closest('.price_detail_input_price_unit').find('.price_detail_event_changeprice_unit').removeClass('price_detail_unit_active');
                element.addClass('active');
                element.addClass('price_detail_unit_active');
                if (element.attr('data-unitid') == '2') {
                    $(this).closest('.price_detail_input').find('.price_detail_input_profit').val('0');
                }
                else if (element.attr('data-unitid') == '1') {
                    element.closest('.price_detail_input').find('.price_detail_input_profit').val('0');
                }
            }


        });
        $('body').on('click', '.delete_price_detail', function () {
            var element = $(this);
            let title = 'Thông báo xác nhận';
            let description = "Thông tin về chính sách giá này sẽ bị xóa. Bạn có chắc chắn không?";
            _msgconfirm.openDialog(title, description, function () {
                var id = element.closest('.level_4').attr('data-id');
                if (id != undefined && parseInt(id) > 0) {
                    $.ajax({
                        url: '/PricePolicy/RemovePriceDetail',
                        type: "post",
                        data: { id: id },
                        success: function (result) {
                            if (result.status == 0) {
                                element.closest('.level_4').remove();
                                _msgalert.success(result.message);
                            } else {
                                _msgalert.error(result.message);
                            }
                            //-- Update Contract Label
                            var label_contract = element.closest('li').find('.contract_date_range');
                            _price_policy_detail.AddOrUpdateContractLabel(label_contract);
                        }
                    });
                } else {
                    element.closest('.level_4').remove();
                    _msgalert.success("Xóa chính sách giá thành công.");
                }
            });

        });
        // dynamic bind: nhấn nút cập nhật chính sách giá
        $('body').on('click', '.update_price_detail', function () {
            //-- Get Element
            var price_detail_element = $(this);
            //--Get data
            var campaign_id = $('#campaign_code').attr('data-campaign-id');
            var profit = price_detail_element.closest('.level_4').find('.price_detail_input_profit').val() == undefined ? '-1' : price_detail_element.closest('.level_4').find('.price_detail_input_profit').val().replaceAll(',', '');
            if (profit.trim() == '') {
                price_detail_element.closest('.level_4').find('.price_detail_input_profit').val('0').change();
                profit = '0';
                _msgalert.error('Lợi nhuận phải là số lớn hơn hoặc bằng 0');
                show_success = false;
            }

            if (profit == undefined || !$.isNumeric(profit.replaceAll(',', '')) || parseFloat(profit.replaceAll(',', '')) < 0) {
                //_msgalert.error('Lợi nhuận phải là số lớn hơn hoặc bằng 0');
                _msgalert.error('Lợi nhuận phải là số lớn hơn hoặc bằng 0');
                return;
            }

            //---- Date: 
            var campaign_ToDate = $('#campaign_to_date').data('daterangepicker').startDate._d
            var campaign_fromDate = $('#campaign_from_date').data('daterangepicker').startDate._d
            if (campaign_ToDate < campaign_fromDate) {
                _msgalert.error('Không thể lưu chính sách giá khi thời gian chính sách giá đang sai');
                return;
            }
            var unit_id = price_detail_element.closest('.level_4').find('.price_detail_unit_active').attr('data-unitid') == undefined ? '0' : price_detail_element.closest('.level_4').find('.price_detail_unit_active').attr('data-unitid');

            //-- If new campaign, skip upload:
            if (isNaN(parseInt(campaign_id)) || parseInt(campaign_id) <= 0) {
                price_detail_element.closest('.level_4').find('.price_detail_readonly_profit').attr('data-profit', profit);
                price_detail_element.closest('.level_4').find('.price_detail_readonly_profit').attr('data-unitid', unit_id == '2' ? "VND" : "%");
                price_detail_element.closest('.level_4').find('.price_detail_readonly_profit').html(price_detail_element.closest('.level_4').find('.price_detail_input_profit').val() + ' ' + (unit_id == '2' ? "VND" : "%"));
                price_detail_element.closest('.level_4').attr('data-value', '0');
            }
            else {

                _price_policy_detail.SummitPriceDetail(price_detail_element.closest('.level_4'))
            }
            _price_policy_detail.UpdateReadOnlyDateField(price_detail_element);
            _price_policy_detail.Edited=true
        });

        $('body').on('click', '.price_detail_input_profit', function () {
            var price_detail_element = $(this);
            $(this).closest('.level_4').find('.cancel_edit_price_detail').attr('data-profit-value', price_detail_element.val().replaceAll(',', ''));
            $(this).closest('.level_4').find('.cancel_edit_price_detail').attr('data-profit-unitid', price_detail_element.closest('.level_4').find('.price_detail_unit_active').attr('data-unitid'));

        });
        $('body').on('click', '.price_detail_input_allotment_price', function () {
            var allotment_price_element = $(this);
            var profit = $(this).val().replaceAll(',', '');
            allotment_price_element.val(profit);
            $(this).closest('.level_4').find('.cancel_edit_price_detail').attr('data-allotmentprice-value', allotment_price_element.val().replaceAll(',', ''));
        });
        $('body').on('focusout', '.price_detail_input_allotment_price', function () {

            var allotment_price_element = $(this);
            var price = allotment_price_element.val().replace(',', '');

            if (!$.isNumeric(price) || parseFloat(price) < 0) {
                _msgalert.error('Giá Allotment phải lớn hơn hoặc bằng 0');
                allotment_price_element.val('0').change();
                return;
            }
            allotment_price_element.val(_price_policy_detail.FormatMoney(parseFloat(price), 'VND'));
            allotment_price_element.closest('.level_4').find('.detail_price').attr('data-value', parseFloat(price))
            debugger
            allotment_price_element.closest('.level_4').find('.detail_price').html(_price_policy_detail.FormatMoney(parseFloat(price), 'VND') + ' VND');
            //-- calucate amount_last:
            var profit = allotment_price_element.closest('.level_4').find('.price_detail_readonly_profit').attr('data-profit');
            var unit_id = allotment_price_element.closest('.level_4').find('.price_detail_unit_active').attr('data-unitid');
            switch (parseInt(unit_id)) {
                case 1: {
                    allotment_price_element.closest('.level_4').find('.price_detail_total_amount').html(_price_policy_detail.FormatMoney(parseFloat(price) * (100 + parseFloat(profit)) / 100, 'VND') + ' VND');
                    allotment_price_element.closest('.level_4').find('.price_detail_total_amount').attr('data-value', (parseFloat(price) * (100 + parseFloat(profit))));

                } break;
                case 2: {
                    allotment_price_element.closest('.level_4').find('.price_detail_total_amount').html(_price_policy_detail.FormatMoney(parseFloat(price) + parseFloat(profit), 'VND') + ' VND');
                    allotment_price_element.closest('.level_4').find('.price_detail_total_amount').attr('data-value', parseFloat(price) + parseFloat(profit));
                } break;
                default: {
                    allotment_price_element.closest('.level_4').find('.price_detail_total_amount').html(_price_policy_detail.FormatMoney(parseFloat(price), 'VND') + ' VND');
                    allotment_price_element.closest('.level_4').find('.price_detail_total_amount').attr('data-value', parseFloat(price));
                } break;
            }

        });
        // dynamic bind: thêm box tạo mới để thêm chính sách giá:
        $('body').on('click', '.add_new_policy_rule', function () {
            var button_element = $(this)
            var lvl = $(this).attr('data-newlevel');
            var html = undefined;
            switch (lvl) {

                case '1': {
                    html = _price_policy_detail_html.NewPriceDetail_Lv2.replaceAll('{level}', lvl)
                    var parent = button_element.closest('li')
                    var title_element = parent.find('.level_1_title')
                    title_element.after(html)

                  
                } break;

                case '2': {
                    html = _price_policy_detail_html.NewPriceDetail_Lv2.replaceAll('{level}', lvl)
                    var parent = button_element.closest('.level_2_content')
                    var title_element = parent.find('.level_2_title')
                    title_element.after(html)

                  
                } break;
                case '4': {
                    //var element_to_append = button_element.closest('.level_4_room').find('.level_4_content');
                    //html = _price_policy_detail_html.NewPriceDetail_Lv4
                    //if (element_to_append.find('.level_4').length >0) {
                    //    element_to_append.find('.level_4').first().before(html)
                    //} else {
                    //    element_to_append.append(html)
                    //}
                  
                    //$('.add_new_block-all').removeClass('add_new_block-all')
                } break;
                default: {
                    return;
                }
            }


        });
        $('body').on('click', '.add-new-policy-all-cancel', function () {
            $(this).closest('.add-new-policy-all').remove()

        })
        $('body').on('click', '.add-new-policy-all-confirm', function () {
            var element = $(this)
            var parent_element = element.closest('.add-new-policy-all')
            var lvl = element.closest('.add-new-policy-all').attr('data-level')
            var title = 'Xác nhận áp dụng chính sách giá chung';
            var description = 'Áp dụng sẽ xóa tất cả các chính sách giá đã có và thay thế bằng chính sách giá chung, bạn có chắc chắn không?';
           
            _msgconfirm.openDialog(title, description, function () {
                var parent = element.closest('.level_4')
                var html = _price_policy_detail_html.NewPriceDetail_Lv4
                switch (lvl) {
                    case '1': {
                         parent = element.closest('li')
                    } break;
                    case '2': {
                         parent = element.closest('.level_2_content')
                    } break;
                    default: {
                        return;
                    }
                }
                //parent.find('.level_4').remove()
                //parent.find('.level_4_content').each(function () {
                //    var level_4_content = $(this)
                //    level_4_content.append(html)
                //    var pricedetail_element = $('.add_new_block-all')

                //    pricedetail_element.find('.price_detail_readonly_profit').attr('data-profit', profit)
                //    pricedetail_element.find('.price_detail_input_profit').val(profit)
                //    switch (unit_id) {
                //        case '1': {
                //            pricedetail_element.find('.price_detail_readonly_profit').attr('data-unitid', 'percent')
                //            pricedetail_element.find('.price_detail_unit_vnd').removeClass('active')
                //            pricedetail_element.find('.price_detail_unit_percent').addClass('active')
                //            pricedetail_element.find('.price_detail_readonly_profit').html(profit + ' %')

                //        } break
                //        case '2': {
                //            pricedetail_element.find('.price_detail_readonly_profit').attr('data-unitid', 'VND')
                //            pricedetail_element.find('.price_detail_unit_vnd').addClass('active')
                //            pricedetail_element.find('.price_detail_unit_percent').removeClass('active')
                //            pricedetail_element.find('.price_detail_readonly_profit').html(profit + ' VND')

                //        } break
                //    }

                //    //-- Change To only-read mode:
                //    pricedetail_element.find('.price_detail_input_button').addClass('no_display')
                //    pricedetail_element.find('.price_detail_readonly_button').removeClass('no_display')
                //    pricedetail_element.find('.price_detail_readonly').removeClass('no_display')
                //    pricedetail_element.find('.price_detail_input').addClass('no_display')
                //    pricedetail_element.find('.update_price_detail').html('Cập nhật')
                //    pricedetail_element.find('.update_price_detail').prop('disabled', false)
                //    pricedetail_element.removeClass('add_new_block-all')
                //});
                var profit = parent_element.find('.add-new-policy-all-input').val()
                var unit_id = '2'
                $('.add-new-policy-all-unitid').each(function () {
                    var unit_element = $(this)
                    if (unit_element.hasClass('active')) {
                        unit_id = unit_element.attr('data-unitid')
                        return false
                    }
                })

                _price_policy_detail.CampaignGlobalRuleApply(parent, profit, unit_id)
                parent_element.remove()
            });

        })
        $('body').on('apply.daterangepicker', '.campaign_from_date', function (ev, picker) {

            var campaign_element = $(this);
            var campaign_fromDate = campaign_element.data('daterangepicker').startDate._d;

            if (campaign_fromDate > $('#campaign_to_date').data('daterangepicker').startDate._d) {
                _msgalert.error('Thời gian bắt đầu chính sách giá không được lớn hơn thời gian kết thúc chính sách giá.');
                campaign_element.val(_price_policy_detail_html.CampaignFromDate).change();
                return;
            }
          
            $('.contract_date_range').each(function () {
                var element = $(this);
                _price_policy_detail.AddOrUpdateContractLabel(element);
            });
            _global_function.AddLoading()
            var from_date = _price_policy_detail.GetDayText(campaign_fromDate);
            _price_policy_detail_html.CampaignFromDate = from_date;
            _global_function.RemoveLoading()

        });
        $('body').on('apply.daterangepicker', '.campaign_to_date', function (ev, picker) {

            var campaign_element = $(this);

            var campaign_ToDate = campaign_element.data('daterangepicker').startDate._d;

            if (campaign_ToDate < $('#campaign_from_date').data('daterangepicker').startDate._d) {
                _msgalert.error('Thời gian kết thúc chính sách giá không được nhỏ hơn thời gian bắt đầu chính sách giá.');
                campaign_element.val(_price_policy_detail_html.CampaignToDate).change();

                return;
            }

          
            //-- Update Contract Label
            $('.contract_date_range').each(function () {
                var element = $(this);
                _price_policy_detail.AddOrUpdateContractLabel(element);
            });
            _global_function.AddLoading()

            _price_policy_detail_html.CampaignToDate = _price_policy_detail.GetDayText(campaign_ToDate)
            _global_function.RemoveLoading()

        });
        $('body').on('click', '.add-new-policy-all-unitid', function (ev, picker) {
            var element = $(this)
            if (!element.hasClass('active')) {
                element.addClass('add-new-policy-all-unitid-confirmed-active')
                $('.add-new-policy-all-unitid').removeClass('active')
                $('.add-new-policy-all-unitid-confirmed-active').addClass('active')
                element.removeClass('add-new-policy-all-unitid-confirmed-active')
            }
        });
        $('body').on('click', '.campaign-global-rule-unit', function (event) {
            var element = $(this);
            $('.campaign-global-rule-unit').removeClass('btn-default')
            element.addClass('btn-default')
            $('#campaign-global-rule-value').attr('data-unit', element.attr('data-unit'))
        });
        $('body').on('click', '#campaign-global-rule-apply', function (event) {
            let title = 'Xác nhận áp dụng mức chính sách giá chung';
            let description = 'Tất cả các chính sách bên dưới sẽ được ghi đè bằng chính sách chung này, bạn có chắc chắn không?';
            
            _msgconfirm.openDialog(title, description, function () {
                _global_function.AddLoading()
                setTimeout(function () {
                    _price_policy_detail.CampaignGlobalRuleApply()

                }, 500)
            })

        });
    },
    CampaignGlobalRuleApply: function (global_element = $('body'), profit = undefined, unit_id = undefined) {
        if (profit == undefined) profit = $('#campaign-global-rule-value').val()
        if (unit_id == undefined) unit_id = $('#campaign-global-rule-value').attr('data-unit')
        //var profit = $('#campaign-global-rule-value').val()
        //var unit_id = $('#campaign-global-rule-value').attr('data-unit')
        global_element.find('.level_4_content').each(function (index, item) {
            var parent_element = $(this)
            if (parent_element.find('.level_4').length < 1) {
                /* parent_element.closest('.level_4_room').find('.level_4_title').find('.add_new_policy_rule').trigger('click')*/
                parent_element.append(_price_policy_detail_html.NewPriceDetail_Lv4)

            } else if (parent_element.find('.level_4').length > 1) {
                var first = false;
                parent_element.find('.level_4').each(function (index, item) {
                    var level_4_item = $(this)
                    if (!first) {
                        first=true
                        return true
                    } else {
                        level_4_item.remove()
                    }
                })
            }
        })
        global_element.find('.level_4').each(function (index, item) {
            var element = $(this)
            element.find('.price_detail_input_profit').val(_global_function.Comma(profit))
            element.find('.price_detail_readonly_profit').attr('data-profit', profit);
            switch (unit_id) {
                case '1': {
                    element.find('.price_detail_readonly_profit').attr('data-unitid', "%");
                    element.find('.price_detail_readonly_profit').html(element.find('.price_detail_input_profit').val() + " %");
                    element.find('.price_detail_unit_percent').addClass('active')
                    element.find('.price_detail_unit_percent').addClass('price_detail_unit_active')
                    element.find('.price_detail_unit_vnd').removeClass('active')
                    element.find('.price_detail_unit_vnd').removeClass('price_detail_unit_active')
                } break
                case '2': {
                    element.find('.price_detail_readonly_profit').attr('data-unitid', "VND");
                    element.find('.price_detail_readonly_profit').html(element.find('.price_detail_input_profit').val() + " VND");
                    element.find('.price_detail_unit_percent').removeClass('active')
                    element.find('.price_detail_unit_percent').removeClass('price_detail_unit_active')
                    element.find('.price_detail_unit_vnd').addClass('active')
                    element.find('.price_detail_unit_vnd').addClass('price_detail_unit_active')

                } break
            }
            element.attr('data-value', '0');
            _price_policy_detail.UpdateReadOnlyDateField(element.find('.price_detail_input_profit'))
            _global_function.RemoveLoading()

        })
    },
    SummitData: function () {

        var summit_model = {
            Detail: {
                Id: $('#campaign_code').attr('data-campaign-id'),
                CampaignCode: $('#campaign_code').val(),
                FromDate: _global_function.GetDayText($('#campaign_from_date').data('daterangepicker').startDate._d, true),
                ToDate: _global_function.GetDayText($('#campaign_to_date').data('daterangepicker').startDate._d, true),
                ClientTypeId: $('#client_type_id').find(':selected').val(),
                Description: $('#campaign_description').val(),
                Status: $('input[name="campaign_status"]:checked').val(),
            },
            PricePolicy: []
        }
        var hotel_id = $('#policy-hotel-name').find(':selected').val()
        $('#box_list_price_policy').find('li').each(function () {
            var li_element = $(this);
            var program_id = li_element.find('.contract_no').first().attr('data-value')
            li_element.find('.level_2_content').each(function () {
                var lv2_element = $(this);
                var package_id = lv2_element.find('.package_name').first().attr('data-value')
                var package_code = lv2_element.find('.package_name').first().html()
                lv2_element.find('.level_4_room').each(function () {
                    var lv4_element = $(this);
                    var product_service_id = lv4_element.find('.room_name').attr('data-productserviceid')
                    var room_id = lv4_element.find('.room_name').attr('data-roomid')
                    var allotment_id = lv4_element.find('.room_name').attr('data-allotmentid')
                    lv4_element.find('.level_4').each(function () {
                        var pricedetail_element = $(this);
                        var unit_id = 2
                        if (pricedetail_element.find('.price_detail_unit_percent').hasClass('active')) {
                            unit_id = 1
                        }
                        summit_model.PricePolicy.push({
                            PriceDetailId: pricedetail_element.attr('data-id'),
                            ProductServiceId: product_service_id,
                            ProgramName: '',
                            HotelId: hotel_id,
                            ProgramId: program_id,
                            PackageCode: package_code,
                            PackageName: '',
                            PackageId: package_id,
                            RoomId: room_id,
                            AllotmentsId: allotment_id,
                            FromDate: summit_model.Detail.FromDate,
                            ToDate: summit_model.Detail.ToDate,
                            Profit: _global_function.GetAmountFromCurrencyInput(pricedetail_element.find('.price_detail_input_profit')),
                            UnitId: unit_id
                        })

                    });
                });
            });
        });

        
        $.ajax({
            url: '/PricePolicy/SummitHotelPolicy',
            type: "post",
            data: { model: JSON.stringify(summit_model) },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    $.magnificPopup.close();
                    _pricepolicymanagement.AdvanceSearch();
                } else {
                    _msgalert.error(result.msg);
                }
                $('body').removeClass('stop-scrolling');
                $('.img_loading_summit').hide();

            }
        });
    },
    SummitPriceDetail: function (pricedetail_element) {
        //-- Validate:
        var show_success = false

        var hotel_id = $('#policy-hotel-name').find(':selected').val()
        var unit_id = 2
        if (pricedetail_element.find('.price_detail_unit_percent').hasClass('active')) {
            unit_id = 1
        }
        var summit_model = {
            Detail: {
                Id: $('#campaign_code').attr('data-campaign-id'),
                CampaignCode: $('#campaign_code').val(),
                FromDate: _global_function.GetDayText($('#campaign_from_date').data('daterangepicker').startDate._d, true),
                ToDate: _global_function.GetDayText($('#campaign_to_date').data('daterangepicker').startDate._d, true),
                ClientTypeId: $('#client_type_id').find(':selected').val(),
                Description: $('#campaign_description').val(),
                Status: $('input[name="campaign_status"] :checked').val(),
            },
            PricePolicy: [
                {
                    PriceDetailId: pricedetail_element.attr('data-id'),
                    ProductServiceId: pricedetail_element.closest('.level_4_room').find('.room_name').attr('data-productserviceid'),
                    ProgramName: '',
                    HotelId: hotel_id,
                    ProgramId: pricedetail_element.closest('li').find('.contract_no').first().attr('data-value'),
                    PackageCode: pricedetail_element.closest('level_2_content').find('.package_name').first().html(),
                    PackageName: '',
                    PackageId: pricedetail_element.closest('level_2_content').find('.package_name').first().attr('data-value'),
                    RoomId: pricedetail_element.closest('level_4_room').find('.room_name').attr('data-roomid'),
                    AllotmentsId: pricedetail_element.closest('level_4_room').find('.room_name').attr('data-allotmentid'),
                    FromDate: _global_function.GetDayText($('#campaign_from_date').data('daterangepicker').startDate._d, true),
                    ToDate: _global_function.GetDayText($('#campaign_to_date').data('daterangepicker').startDate._d, true),
                    Profit: _global_function.GetAmountFromCurrencyInput(pricedetail_element.find('.price_detail_input_profit')),
                    UnitId: unit_id
                }
            ]
        }
        if (summit_model.Detail.Id != undefined && !isNaN(parseInt(summit_model.Detail.Id))) show_success = true
        pricedetail_element.find('.update_price_detail').html('Đang cập nhật ...')
        pricedetail_element.find('.update_price_detail').prop('disabled', true)
        $.ajax({
            url: '/PricePolicy/UpdateHotelPriceDetail',
            type: "post",
            data: { summit_model: summit_model },
            success: function (result) {

                if (result.status == 0) {
                    if (show_success) _msgalert.success(result.msg);
                } else {
                    _msgalert.error(result.msg);
                    _price_policy_detail.RestoreLastSavedInput(pricedetail_element.find('.price_detail_readonly_profit'));

                }
                pricedetail_element.find('.price_detail_readonly_profit').attr('data-profit', summit_model.PricePolicy[0].Profit);
                pricedetail_element.find('.price_detail_readonly_profit').attr('data-unitid', summit_model.PricePolicy[0].UnitId == '2' ? "VND" : "%");
                pricedetail_element.find('.price_detail_readonly_profit').html(_global_function.Comma(summit_model.PricePolicy[0].Profit) + ' ' + (summit_model.PricePolicy[0].UnitId == '2' ? "VND" : "%"));
                pricedetail_element.attr('data-value', summit_model.PricePolicy[0].Profit);
                _price_policy_detail.UpdateReadOnlyDateField(pricedetail_element.find('.price_detail_readonly_profit'));
                //--Update price and amount_last
                pricedetail_element.find('.cancel_edit_price_detail').attr('data-profit-value', summit_model.PricePolicy[0].Profit);
                pricedetail_element.find('.cancel_edit_price_detail').attr('data-profit-unitid', summit_model.PricePolicy[0].UnitId);

                //-- Change To only-read mode:
                pricedetail_element.find('.price_detail_input_button').addClass('no_display')
                pricedetail_element.find('.price_detail_readonly_button').removeClass('no_display')
                pricedetail_element.find('.price_detail_readonly').removeClass('no_display')
                pricedetail_element.find('.price_detail_input').addClass('no_display')
                pricedetail_element.find('.update_price_detail').html('Cập nhật')
                pricedetail_element.find('.update_price_detail').prop('disabled', false)
            }
        });
    },
    UpdateReadOnlyDateField: function (price_detail_element) {
     
        //-- Change To only-read mode:
        price_detail_element.closest('.level_4').find('.price_detail_input_button').addClass('no_display')
        price_detail_element.closest('.level_4').find('.price_detail_readonly_button').removeClass('no_display')
        price_detail_element.closest('.level_4').find('.price_detail_readonly').removeClass('no_display')
        price_detail_element.closest('.level_4').find('.price_detail_input').addClass('no_display')
        price_detail_element.closest('.level_4').find('.update_price_detail').html('Cập nhật')
        price_detail_element.closest('.level_4').find('.update_price_detail').prop('disabled', false)

    },
}

/*
$('.update_price_detail').each(function () {
    $(this).trigger('click')
});
$('.price_detail_input_fromdate').data('daterangepicker').setStartDate('27/11/2024 00:00');
$('.update_price_detail').each(function () {
    $(this).trigger('click')
});


*/

