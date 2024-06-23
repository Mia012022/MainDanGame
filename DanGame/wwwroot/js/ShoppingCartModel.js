$(function () {

    $.get("/shoppingcart/User/ShoppingCart", function (data) {
        let appids = data.map((d) => d.appId)

        $.ajax({
            type: "POST",
            url: "/shoppingcart/App/Detail",
            data: JSON.stringify(appids),
            contentType: "application/json",
            dataType: "json",
            success: function (data) {
                datasource = data;
                console.log(datasource);
                /console.log(datasource.price);/
                rendermyOffcanvasGameItems(datasource)
            }
        });
    });
    //購物車圖標model功能開始
    let myOffcanvas = $(`
    <div class="offcanvas offcanvas-bottom mikecanvas" tabindex="-1" id="offcanvasBottom" aria-labelledby="offcanvasBottomLabel">
        <div class="offcanvas-header mikecanvas_header">
            <h5 class="offcanvas-title mikecanvas_title" id="offcanvasBottomLabel">快速檢視購物車</h5>
            <button type="button" class="btn-close text-reset" data-bs-dismiss="offcanvas" aria-label="Close"></button>
        </div>
        <div class="offcanvas-body small mikecanvas_body">
        </div>
        <div class="canvasgotocheck">
            <a href="/shoppingcart/index"><i class="fa-solid fa-cart-shopping"></i> <b></b></a>
        </div>
    </div>
  `)
        .appendTo("body")
        .css({
            "border-radius": "30px 30px 30px 30px",
            margin: "20px 100px",
            height: "45vh",
            width: "90vw",
        });

    function rendermyOffcanvasGameItems(apps) {
        console.log("拿資料囉");
        console.log(apps); // 正确的变量名是 apps，而不是 aaps

        $.each(apps, function (index, app) {
            const myOffcanvasGameItem = $(`
                <div class="canvasgameitem" data-appid="${app.appId}">
                    <p class="canvasitem-name">${app.appName}</p>
                    <div class="itempic">
                        <a href=""><img src="https://steamcdn-a.akamaihd.net/steam/apps/${app.appId}/library_600x900.jpg" alt=""></a>
                    </div>
                    <div class="itemprice">
                        <p class="canvasGameItemrp">NT:${app.price}</p>
                    </div>
                    <i class="fa-solid fa-circle-minus deleticon"></i>
                </div>
            `);

            // 将创建的 myOffcanvasGameItem 添加到 .mikecanvas_body 中
            $(".mikecanvas_body").append(myOffcanvasGameItem);

            canvascalculate();

            myOffcanvasGameItem.find(".deleticon").on("click", function () {
                $(this).parent(".canvasgameitem").remove();
                console.log($(this).closest(".canvasgameitem").data("appid"));
                var canvanappid = $(this).closest(".canvasgameitem").data("appid");
                $.ajax({
                    method: "DELETE",
                    url: `/shoppingcart/delete/ShoppingCart/${canvanappid}`,
                    success: function (data) {
                        console.log("成功刪除");
                    },
                    error: function (xhr, status, error) {
                        console.log("刪除失敗", status, error);
                    },
                });

                canvascalculate();
            });
        });
    }

    //canvas的計算功能開始
    function canvascalculate() {
        let canvastotal = 0;
        myOffcanvas.find(".canvasGameItemrp").each(function () {
            let canvaspriceText = $(this).text().replace("NT:", "");
            let canvasprice = parseFloat(canvaspriceText);
            if (!isNaN(canvasprice)) {
                canvastotal += canvasprice;
            }
        });
        myOffcanvas.find(".canvasgotocheck b").text(canvastotal);
        if (canvastotal == 0) {
            myOffcanvasGameItem.find(".canvasgotocheck a").attr("href", "#");
            bsOffcanvas.hide();
        }
    }
    //建立model
    var bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas);

    //點擊跳窗
    $(".bi-cart-plus").on("click", (event) => {
        event.preventDefault();
        // if (canvastotal == 0) {
        //     bsOffcanvas.block();
        // }
        bsOffcanvas.toggle();
        console.log("嗨");
    });

    //購物車圖標model功能結束
});
