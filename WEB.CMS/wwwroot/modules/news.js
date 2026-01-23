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
a        debugger;

        // 🧩 Thu thập dữ liệu từ form
        const data = {
            Id: parseInt($('#Id').val()) || 0,
            CampaignName: ($('#modal-CampaignName').val() || "").trim(),  // keyword chính
            PlatForm: parseInt($('input[name="PlatForm"]:checked', _news.modal_element).val()),
            AiContent: ($('#modal-AiContent').val() || "").trim(),        // ngữ cảnh / keyword phụ / yêu cầu thêm
            AimodelType: 1
        };

        // ⚠️ Bắt buộc phải có nội dung (ngữ cảnh / yêu cầu)
        if (!data.AiContent) {
            alert("Bạn cần nhập nội dung để gửi lên AI.");
            return;
        }

        // ⚠️ Nếu là Web hoặc Web2 thì phải có keyword chính
        if ((data.PlatForm === 0 || data.PlatForm === 2) && !data.CampaignName) {
            alert("Bạn cần nhập Keyword cho nền tảng Website.");
            return;
        }

        // 👉 Lấy tên nền tảng text
        let platformText = "web";
        if (data.PlatForm === 1) platformText = "facebook";
        if (data.PlatForm === 2) platformText = "web2";

        // 🧠 SYSTEM MESSAGE: quy định vai trò + format output + SEO rules
        const system_message = `
Bạn là chuyên gia SEO & Content Marketing chuyên viết blog cho website doanh nghiệp B2C.

Nhiệm vụ:
Viết 1 bài blog chuẩn SEO cho website Adavigo.com dựa trên keyword người dùng cung cấp.

SEO INPUT:
- Từ khóa chính (MAIN KEYWORD): "${data.CampaignName}"
- Search Intent: Informational
- Độ dài toàn bài: 1000–1800 từ
- Đối tượng: khách lẻ, khách đoàn, khách doanh nghiệp
- Giọng văn: thân thiện, truyền cảm hứng, dễ đọc
- Ngôn ngữ: tiếng Việt

YÊU CẦU NỘI DUNG:
- Title: dưới 60 ký tự, BẮT BUỘC chứa keyword chính, hấp dẫn
- Meta description: 140–160 ký tự, có keyword + CTA
- Intro: dẫn dắt vấn đề, chèn keyword chính 1 lần tự nhiên
- Thân bài:
  - Chia tối đa 3–4 H2
  - Mỗi H2 có 2–3 H3
  - Độ dài:
    + H2: ~300–400 từ
    + H3: ~150–200 từ
  - Có bullet points, ví dụ thực tế, gợi ý triển khai
- Kết bài:
  - Tổng kết giá trị chính
  - Nhấn mạnh vai trò & uy tín của Adavigo
  - CTA: “Liên hệ Adavigo để triển khai ${data.CampaignName} hiệu quả!”

FAQ:
- 3–5 câu hỏi
- Trả lời ngắn gọn, tự nhiên, schema-friendly

SEO KỸ THUẬT:
- Từ khóa chính xuất hiện 3–5 lần, phân bố tự nhiên
- Tự sinh 3–5 keyword phụ (LSI) liên quan chặt chẽ đến keyword chính
- Mỗi đoạn không quá 120 từ
- Heading theo chuẩn H1 – H2 – H3
- Tối ưu semantic, không nhồi keyword
- Có yếu tố E-E-A-T (kinh nghiệm thực tế, chuyên môn, lời khuyên đáng tin)

QUY ƯỚC KEYWORD (BẮT BUỘC):
- "${data.CampaignName}" là TỪ KHÓA CHÍNH DUY NHẤT
- Nội dung KHÔNG được xây dựng xoay quanh keyword phụ
- Nếu keyword phụ có chữ "concept" thì chỉ được dùng để hỗ trợ cho hoạt động "${data.CampaignName}"
- Mọi tiêu đề H2 phải liên quan trực tiếp đến "${data.CampaignName}"

OUTPUT FORMAT (BẮT BUỘC):
- CHỈ trả về JSON, KHÔNG thêm bất kỳ text nào bên ngoài
- Content sử dụng HTML
- Chỉ dùng các thẻ: <h2>, <h3>, <p>, <ul>, <li>, <strong>

Cấu trúc JSON:
{
  "title": "string",
  "lead": "string",
  "content": "HTML content",
  "keywords": ["keyword1", "keyword2", "keyword3"],
  "img_lst": ["suggested image keyword 1", "suggested image keyword 2"]
}
`.trim();

        // 🧠 FULL PROMPT: ép rõ keyword chính vs ngữ cảnh (FIX RỜI RẠC)
        const fullPrompt = `
Viết bài với TỪ KHÓA CHÍNH là: "${data.CampaignName}"

Yêu cầu:
- Lấy "${data.CampaignName}" làm trọng tâm xuyên suốt bài viết
- Nội dung dưới đây chỉ là ngữ cảnh / keyword phụ / yêu cầu thêm, KHÔNG được làm trọng tâm
- KHÔNG viết kiểu liệt kê outline sơ sài; viết thành bài blog mạch lạc, có dẫn dắt, có ví dụ thực tế

Ngữ cảnh / yêu cầu thêm:
${data.AiContent}
`.trim();

        // 🔥 Chuẩn bị payload gửi lên n8n
        const payload = {
            chatInput: fullPrompt,
            platform: platformText,
            system_message: system_message
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
                debugger;
                $('#loadingOverlay').hide();

                console.log("✅ Phản hồi từ N8n:", res);

                // ✅ Một số webhook trả về string JSON -> parse để tránh vỡ
                let parsed = res;
                try {
                    if (typeof res === "string") parsed = JSON.parse(res);
                    // trường hợp n8n trả { content: "....json...." }
                    if (parsed && typeof parsed.content === "string") {
                        const trimmed = parsed.content.trim();
                        if ((trimmed.startsWith("{") && trimmed.endsWith("}")) ||
                            (trimmed.startsWith("[") && trimmed.endsWith("]"))) {
                            parsed = JSON.parse(trimmed);
                        }
                    }
                } catch (e) {
                    // ignore parse error, fallback dùng res như cũ
                }

                // ✅ Gán kết quả AI trả về (ưu tiên parsed)
                const ai = parsed || res;

                data.AiResult = ai.content || ai.Content || "";
                data.Title = ai.title || ai.Title || "";
                data.Lead = ai.lead || ai.Lead || "";
                data.Images = (ai.img_lst || ai.Images || []).slice(0, 10);

                // ✅ Fix mapping keywords (tránh res.keyword sai key)
                data.Keywords = ai.keywords || ai.keyword || [];

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
                console.error("❌ Gửi thất bại:", err, xhr?.responseText);
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