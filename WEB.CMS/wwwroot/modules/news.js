$(document).ready(function () {
    _news.Init();
    // Ẩn mặc định phần Tên Chủ Đề
    //$('#campaignNameGroup').hide();

    //// Bắt sự kiện thay đổi radio
    //$(document).on('change', 'input[name="PlatForm"]', function () {
    //    debugger
    //    var value = $('input[name="PlatForm"]:checked').val();

    //    if (value == "1") {
    //        $('#campaignNameGroup').show(); // Facebook
    //    } else {
    //        $('#campaignNameGroup').hide(); // Website
    //    }
    //});

    //// Khi mở form (edit) có sẵn giá trị
    //var currentValue = $('input[name="PlatForm"]:checked').val();
    //if (currentValue == "1") {
    //    $('#campaignNameGroup').show();
    //}

});



$('.btn-toggle-cate').click(function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});

$('.ckb-news-cate').click(function () {
    var seft = $(this);
    var ul_lever2 = seft.parent().siblings('ul.lever2');
    if (ul_lever2.length > 0) {
        var btn_toggle = seft.parent().siblings('a.btn-toggle-cate');
        if (seft.is(":checked")) {
            ul_lever2.find('.ckb-news-cate').prop('checked', true);
            if (btn_toggle.hasClass('plus')) {
                btn_toggle.trigger('click');
            }
        } else {
            if (btn_toggle.hasClass('minus')) {
                btn_toggle.trigger('click');
            }
            ul_lever2.find('.ckb-news-cate').prop('checked', false);
        }
    }
});

var _news = {
    Init: function () {
        let _searchModel = {
            Title: null,
            ArticleId: -1,
            FromDate: null,
            ToDate: null,
            AuthorId: -1,
            ArticleStatus: -1,
            ArrCategoryId: null,
            SearchType: 0
        };

        let objSearch = {
            searchModel: _searchModel,
            currentPage: 1,
            pageSize: 20
        };
        this.modal_element = $('#global_modal_popup');

        this.SearchParam = objSearch;
        this.Search(objSearch);
    },
    ShowAddOrUpdate: function (id, parent_id = 0) {
        debugger
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} Bài Viết AI`;
        let url = '/news/AddOrUpdate';
        _news.modal_element.find('.modal-title').html(title);
        _news.modal_element.find('.modal-dialog').css('max-width', '680px');
        _ajax_caller.get(url, { id: id, parent_id: parent_id }, function (result) {
            _news.modal_element.find('.modal-body').html(result);
            _news.modal_element.modal('show');
        });
    },

    OnSave: function () {

        // 🧩 Thu thập dữ liệu từ form
        const data = {
            Id: parseInt($('#Id').val()) || 0, // vẫn lấy Id từ view chính
            CampaignName: $('#modal-CampaignName').val(), // giờ là Keyword
            PlatForm: parseInt($('input[name="PlatForm"]:checked', _news.modal_element).val()),
            AiContent: $('#modal-AiContent').val(),
            AimodelType: 1
        };

        // ⚠️ Bắt buộc phải có nội dung
        if (!data.AiContent) {
            alert("Bạn cần nhập nội dung để gửi lên AI.");
            return;
        }

        // ⚠️ Nếu là Web hoặc Web2 thì phải có keyword
        if ((data.PlatForm === 0 || data.PlatForm === 2) && !data.CampaignName) {
            alert("Bạn cần nhập Keyword cho nền tảng Website.");
            return;
        }

        // 👉 Lấy tên nền tảng text
        let platformText = "web";
        if (data.PlatForm === 1) platformText = "facebook";
        if (data.PlatForm === 2) platformText = "web2";
        // 🧠 Tạo system_message (gộp keyword hoặc cấu hình riêng của hệ thống)
        // Ví dụ: "Viết bài cho lĩnh vực du lịch, keyword: tour Hà Nội"
        const system_message = `
        Bạn là chuyên gia SEO và Content Marketing trong lĩnh vực du lịch.
        Hãy viết một bài blog chuẩn SEO cho website Adavigo.com với các thông tin sau:
        - Từ khóa chính: ${data.CampaignName}
        - Từ khóa phụ (LSI): [liệt kê 3–5 keyword phụ liên quan]
        - Độ dài: khoảng 1000–1800 từ
        - Mục tiêu SEO: Informational
        - Đối tượng khách hàng: khách du lịch tự túc, nhóm bạn, gia đình
        - Giọng văn: thân thiện, truyền cảm hứng
        - Ngôn ngữ: tiếng Việt

        Cấu trúc bài viết:
        - Title (chuẩn SEO, dưới 60 ký tự, có từ khóa chính)
        - Meta description (140–160 ký tự, có keyword, có CTA)
        - Mở bài (intro)
        - Thân bài (gồm H2 – H3 logic, chia đoạn, có bullet)
        - Kết bài (CTA, tổng kết, nhấn mạnh thương hiệu Adavigo)
        - FAQ (3–5 câu hỏi, trả lời ngắn gọn, thân thiện, schema-friendly)

        Yêu cầu SEO kỹ thuật:
        - Chèn keyword chính 3–5 lần tự nhiên.
        - Không đoạn nào dài quá 120 từ.
        - Heading theo chuẩn H1 – H2 – H3.
        - Có yếu tố “E-E-A-T” (kinh nghiệm, chuyên môn, độ tin cậy).

       Bạn luôn tuân thủ format output là JSON với cấu trúc:
        {
          "title": "string",
          
          "lead": "string",
          "content": "HTML content hoặc markdown",
          "keywords": ["keyword1", "keyword2"],
          "img_lst": ["url1", "url2"]
        }
        `;

        // 🧠 Ghép nội dung yêu cầu + keyword thành prompt hoàn chỉnh
        const fullPrompt = `${data.AiContent.trim()}. Keyword trong bài: ${data.CampaignName}`;


        // 🔥 Chuẩn bị payload gửi lên n8n
        const payload = {
            chatInput: fullPrompt, // nội dung chính Câu lệnh Ai
            platform: platformText,    // web / facebook / web2
            system_message: system_message // keyword người nhập (phẩy cách nhau)
        };

        console.log("🚀 Payload gửi lên N8n:", payload);

        // ✨ Hiện loading
        $('#loadingOverlay').show();

        // 🚀 Gửi yêu cầu sang n8n webhook
        $.ajax({
            url: "https://n8n.adavigo.com/webhook/send-message",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
               
                $('#loadingOverlay').hide();

                console.log("✅ Phản hồi từ N8n:", res);

                // ✅ Gán kết quả AI trả về
                data.AiResult = res.content;
                data.Title = res.title || "";
                data.Lead = res.lead || "";
                data.Images = (res.img_lst || []).slice(0, 10);
                data.Keywords = res.keyword || [];

                // ✅ Lưu localStorage
                let aiArticles = JSON.parse(localStorage.getItem('aiArticles') || '[]');
                aiArticles = aiArticles.filter(item => item.Id !== data.Id);
                aiArticles.push(data);
                localStorage.setItem('aiArticles', JSON.stringify(aiArticles));

                // ✅ Điều hướng
                window.location.href = `/news/detail/0?fromAI=true&platform=${data.PlatForm}&AimodelType=1`;
            },
            error: function (xhr, status, err) {
                $('#loadingOverlay').hide();
                console.error("❌ Gửi thất bại:", err);
                alert("❌ Lỗi khi gửi lên AI. Kiểm tra console để xem chi tiết.");
            }
        });
    },

    GetFormData: function ($form) {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};
        $.map(unindexed_array, function (n, i) {
            indexed_array[n['name']] = n['value'];
        });
        return indexed_array;
    },

    Search: function (input) {
        $.ajax({
            url: "/news/search",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
                var total = $('#data-total-record').val();
                $('#total-article-filter').text(total);
            }
        });
    },

    OnPaging: function (value) {
        var objSearch = this.SearchParam;
        objSearch.currentPage = value;
        this.Search(objSearch);
    },

    BasicSearch: function () {
        var objSearch = this.SearchParam;
        objSearch.searchModel.Title = $('#BasicTitle').val().trim();
        objSearch.searchModel.AuthorId = -1;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    AdvanceSearch: function () {
        var objSearch = this.SearchParam;

        var _arrCateGory = [];
        $('.ckb-news-cate').each(function () {
            if ($(this).is(":checked")) {
                _arrCateGory.push(parseInt($(this).val()));
            }
        });

        objSearch.searchModel.Title = $('#AdvanceTitle').val().trim();
        objSearch.searchModel.ArticleId = $('#ArticleId').val() <= 0 ? -1 : $('#ArticleId').val();
        objSearch.searchModel.FromDate = $('#FromDate').val();
        objSearch.searchModel.ToDate = $('#ToDate').val();
        objSearch.searchModel.AuthorId = $('#AuthorId').val();
        objSearch.searchModel.ArticleStatus = $('#ArticleStatus').val();
        objSearch.searchModel.ArrCategoryId = _arrCateGory;
        objSearch.searchModel.SearchType = 1;

        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnOpenAdvanceSearch: function () {
        $('.fillter-search').slideDown();
        $('.dynamic-dom').hide();
    },

    OnCloseAdvanceSearch: function () {

        this.SearchParam.searchModel.Title = null;
        this.SearchParam.searchModel.ArticleId = -1;
        this.SearchParam.searchModel.FromDate = null;
        this.SearchParam.searchModel.ToDate = null;
        this.SearchParam.searchModel.AuthorId = -1;
        this.SearchParam.searchModel.ArticleStatus = -1;
        this.SearchParam.searchModel.ArrCategoryId = null;
        this.SearchParam.searchModel.SearchType = 0;

        $('.fillter-search').slideUp();
        $('#form-advance-search').trigger("reset");
        $('.dynamic-dom').show();
    },

    OpenCategoryPanel: function () {
        $('#panel-category-filter ul.lever2').show();
        $('#panel-category-filter .btn-toggle-cate').removeClass('plus').addClass('minus');
    },

    CloseCategoryPanel: function () {
        $('#panel-category-filter ul.lever2').hide();
        $('#panel-category-filter .btn-toggle-cate').removeClass('minus').addClass('plus');
    },
    LoadPageView: function (list_id) {
        if (list_id != undefined && list_id.length>0) {
            $.ajax({
                url: '/News/GetPageViewByList',
                type: 'POST',
                data: {
                    article_id: list_id,
                },
                success: function (result) {
                    if (result != undefined && result.length > 0) {
                        
                        result.forEach(function (item) {
                            $('#pv_' + item.articleID).html(item.pageview);
                        });
                    }
                },
                error: function (jqXHR) {
                },
            });
        }
       
    }
};