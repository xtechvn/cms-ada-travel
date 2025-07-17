$(document).ready(function () {
    _detail_source_client.Init()
});
$('#import-file-ws').change(function () {
    $('#file_import_hotel_error_message').addClass('mfp-hide');
    _source_client.UploadFileExcel();
});
var _detail_source_client = {
    Init: function () {

        _detail_source_client.SearchClient();
    },
    SearchClient: function () {
        var input = {
            MaKH: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            AgencyType: null,
            ClientType: null,
            PermissionType: null,
            PageIndex: 1,
            PageSize: 10,
            MinAmount: null,
            MaxAmount: null,
            UtmSource: $('#UtmSource').val(),
        }
            ;
        $.ajax({
            url: "/CustomerManagerManual/ListClientBySource",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_client').hide();
                $('#grid_data_client').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    }
                    else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");

            }
        });

    },
}