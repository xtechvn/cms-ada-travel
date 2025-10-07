$(document).ready(function () {
    _yc_recruitment.Init();

});
let isPickerApproveRecruitment = false;
var _yc_recruitment = {
    Init: function () {
        var input = {
            location: null,
            area: null,
            FromDateStr: null,
            ToDateStr: null
        };
        _yc_recruitment.GetList(input);
    },
    GetList: function (input) {
        $.ajax({
            url: "/Recruitment/GetListRecruitment",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
            }
        });
    },
    Search: function () {
        var FromDateStr = null;
        var ToDateStr = null;
        if ($('#date_time').data('daterangepicker') !== undefined &&
            $('#date_time').data('daterangepicker') != null && isPickerApprove) {
            FromDateStr = $('#date_time').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDateStr = $('#date_time').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            FromDateStr = null
            ToDateStr = null
        }
        var input = {
            location: null,
            area: null,
            FromDateStr: FromDateStr,
            ToDateStr: ToDateStr
        };
        _yc_recruitment.GetList(input)
    },

}