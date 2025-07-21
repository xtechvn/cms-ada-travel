$(document).ready(function () {
    _ratings.init();
    //--scroll event
    $(window).scroll(function () {
        if ($(window).scrollTop() >= $('.main-user-chua-dat table').offset().top + $('.main-user-chua-dat table').outerHeight() - window.innerHeight) {
            var DateFrom = $('#filter_date_daterangepicker_DateFrom').val()
            var DateTo = $('#filter_date_daterangepicker_DateTo').val()
            if (DateFrom == null || DateFrom == undefined || DateFrom == "") {
                var newdate = new Date();
                newdate.setDate(01);
                DateFrom = newdate.toLocaleDateString("en-GB");
            }
            if (DateTo == null || DateTo == undefined || DateTo == "") {
                DateTo = new Date().toLocaleDateString("en-GB");;
            }
            Index++;
            var modelSearch = {
                CreateDateFromStr: DateFrom,
                CreateDateToStr: DateTo,
                Type: 1,
                PageIndex: Index,
                PageSize: 20,
            }
            _ratings.loadSearch(modelSearch);
        }
    });
    var date = new Date();
    var dd = date.getDate();
    var MM = date.getMonth() + 1;
    var yyyy = date.getFullYear();
    $('#filter_date_daterangepicker_DateFrom').daterangepicker({
        "autoApply": true,
        singleDatePicker: true,   
        "startDate": ('01/' + MM + '/' + yyyy),
        locale: {
            format: 'DD/MM/YYYY',
        }
    });
    $('#filter_date_daterangepicker_DateTo').daterangepicker({
        "autoApply": true,
        singleDatePicker: true,   
        "startDate": (dd + '/' + MM + '/' + yyyy),
        locale: {
            format: 'DD/MM/YYYY',
        }
    });
})
let Index = 1;
var _ratings = {
    init: function () {
        var newdate = new Date();
        newdate.setDate(01);
        var DateFrom = newdate.toLocaleDateString("en-GB");
        var DateTo = new Date().toLocaleDateString("en-GB");;
        var modelSearch = {
            CreateDateFromStr: DateFrom,
            CreateDateToStr: DateTo,
            Type: 1,
            PageIndex: 1,
            PageSize: 20,
        }
        _ratings.SearchTop(modelSearch);
        _ratings.Search(modelSearch);
    },
    SearchTop: function (input) {
        $.ajax({
            url: "/ratings/SearchTop",
            type: "Post",
            data: { model: input },
            success: function (result) {
                $('#grid_data_Top').html(result);
            }
        });
    },
    Search: function (input) {
        $.ajax({
            url: "/ratings/Search",
            type: "Post",
            data: { model: input },
            success: function (result) {
                $('#grid_data').html(result);
            }
        });
    },
    loadSearch: function (input) {
        $.ajax({
            url: "/ratings/Search",
            type: "Post",
            data: { model: input },
            success: function (result) {
                $('#grid_data').append(result);
            }
        });
    },
    getListData: function () {

        Index = 1;
        var DateFrom = $('#filter_date_daterangepicker_DateFrom').val()
        var DateTo = $('#filter_date_daterangepicker_DateTo').val()
        if (DateFrom == null || DateFrom == undefined || DateFrom == "") {
            var newdate = new Date();
            newdate.setDate(01);
            DateFrom = newdate.toLocaleDateString("en-GB");
        }
        if (DateTo == null || DateTo == undefined || DateTo == "") {
            DateTo = new Date().toLocaleDateString("en-GB");;
        }
        var modelSearch = {
            CreateDateFromStr: DateFrom,
            CreateDateToStr: DateTo,
            Type: 1,
            PageIndex: 1,
            PageSize: 20,
        }
        _ratings.SearchTop(modelSearch);
        _ratings.Search(modelSearch);
    }
}