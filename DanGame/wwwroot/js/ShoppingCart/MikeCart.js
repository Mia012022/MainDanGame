
$(function () {

  

    $(".gotocheckbtn").on("click",function (event) {
    console.log("嗨");
    console.log($(".gameitem").length);
    if ($(".gameitem").length == 0) {
        alert("目前購物車是空的");
    } else {
        window.location.href = "/ShoppingCart/CreditCardInfo";
    }
});



// var temp = "[1298, 1998, 920]"//前面先拿到localstorage
// var jsonArray = JSON.parse(temp);

    var itembox = $(".itembox");
    var datasource;
$.get("User/ShoppingCart", function (data) {
    let appids = data.map((d) => d.appId)

    $.ajax({
        type: "POST",
        url: "App/Detail",
        data: JSON.stringify(appids),
        contentType: "application/json",
        dataType: "json",
        success: function (data) {
            datasource = data;
            console.log(datasource);
            /*console.log(datasource.price);*/
            updataprice();
            render(datasource);
            rendermyOffcanvasGameItems(datasource)
        }
    });

}).fail(function (jqXHR, textStatus, errorThrown) {
    console.error("Failed to load JSON: ", textStatus, errorThrown);
});;

function updataprice() {
    totalPrice = 0;
     console.log("嗨");
    //這邊的datasource是沒東西的，因為上面get回來是異步，所以是要等到datasource要給到render，
    //所以我現在先把fumction updataprice先寫在外面，然後在render裡面執行這個updataprice
    datasource.forEach((app) => (totalPrice += app.price));
    $(".totalprice").text("總金額NT:" + totalPrice);
}

// function createDeleteHandler(gameitem, index) {

//         gameitem.remove(); //删除游戏项目
//         checkAndShowEmptyMessage();
//         datasource = datasource.filter(
//             (_, target_index) => target_index !== index
//         );
//         updataprice();

// }
    function render(apps) {
      
    $.each(apps, function (index, app) {
        const platformIconMap = { windows: `<i class="bi bi-windows"></i>`, mac: `<i class="bi bi-apple"></i>`, linux: `<i class="bi bi-ubuntu"></i>`} 
        const gameitem = $(` <div class="gameitem" data-appid="${app.appId}">
                              <div class="itempic">
                                  <a href=""
                                      ><img
                                          src="${app.headerImage}"
                                          alt=""
                                  /></a>
                              </div>
                              <div class="itemtext">
                                  <p class="item-name">${app.appName}</p>
                                  <p class="item-version">
                                      平台:  ${app.platform.split(",").map((p) => platformIconMap[p]).join(" ")}
                                  </p>
                                  <p class="relese-date">上架日期:${app.releaseDate}</p>
                                  <p class="developer">發行商:${app.devloperName}</p>
                                  <div class="price">
                                      <div>
                                          <p class="rp">NT:${app.price}</p>
                                      </div>
                                  </div>
                              </div>
                              <div class="delete">
                                  <a href="#" class="deletebutton"
                                      ><svg
                                          xmlns="http://www.w3.org/2000/svg"
                                          width="16"
                                          height="16"
                                          fill="currentColor"
                                          class="bi bi-trash"
                                          viewBox="0 0 16 16"
                                      >
                                          <path
                                              d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0z"
                                          />
                                          <path
                                              d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4zM2.5 3h11V2h-11z"
                                          /></svg
                                  ></a>
                              </div>
                          </div>`);
        gameitem.find(".deletebutton").on("click", function (e) {
            e.preventDefault(); // 防止點擊鏈接時跳轉
            mymodalremoveone.show();
            
            //$(this)：在點擊事件處理函數中，$(this) 指向當前觸發點擊事件的 DOM 元素，也就是 .deletebutton。
            //.closest('.gameitem')：使用 jQuery 的 closest 方法尋找最接近的包含 gameitem class 的祖先元素，這樣可以確保獲取到正確的遊戲項目元素。
            //我這邊就是先在gameitem裡面設定一個id是，確保點到都是
        /*    console.log($(this).closest('.gameitem').data('appid'));*/
            const deleteapp = $(this).closest('.gameitem').data('appid')
            const itembox = $(this).closest('.gameitem').closest('.itembox')
            const gameItems = itembox.find('.gameitem');
            console.log(deleteapp)
            console.log(itembox)
            console.log(gameItems)
            var allappids = [];
            console.log(allappids)
            gameItems.each(function () {
                const appId = $(this).data('appid');
                console.log(appId)
                console.log($(this))
                allappids.push(appId);
            });
            checkAndShowEmptyMessage();


        
            // $(".yesno").off("click");我這邊已經取消off了
            modalremoveone.find(".yesremoveone").on("click", function () {
                // createDeleteHandler(gameitem, index);
                gameitem.remove(); //這邊可以刪掉
                console.log(deleteapp)
                $.ajax({
                    method: "DELETE",
                    url: `delete/ShoppingCart/${deleteapp}`,
                    success: function (data) {
                        console.log("成功刪除");
                    },
                    error: function (xhr, status, error) {
                        console.log("刪除失敗", status, error);
                    }
                });
            
                checkAndShowEmptyMessage();
                /*console.log(datasource[1].appId);*/
                console.log(datasource);
                 console.log(datasource.length);
                datasource = datasource.filter(
                    (target_app) => target_app.appId !== app.appId
                ); //filter有三個參數，第一個參數，datasource是陣列的元素，第二參數是元素的索引，第三個是陣列本身
                //我上面只寫元素，去過濾，目標的id不是app的id，簡單來說就是刪除了就沒有了
                //只留下不是現在目前刪除之外的app
                console.log(datasource.length);
                console.log(datasource);

                updataprice();
            });
            modalremoveone.find(".noremoveone").on("click", function () {
                // console.log("c");
                mymodalremoveone.hide();
            });
        });
        itembox.append(gameitem);

        //結帳金額的部分
    });
}

    //刪除項目api，我想要把只要是刪除的全部都寫在一起，
    //function deleteapi() {
    //    $.ajax(){
    //        type: "Delete"
    //        url:''
    //    }
    //}



//moda給單獨取消用的
var modalremoveone =
    $(`<div class="modal fade" id="staticBackdrop" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header";>
                        <h5 class="modal-title" id="staticBackdropLabel" ></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body" style="text-align: center;font-size:30px">
                    是否確定刪除
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary yesremoveone" data-bs-dismiss="modal">確定</button>
                    <button type="button" class="btn btn-primary noremoveone">取消</button>
                    </div>
                    </div>
                    </div>
                    </div>`);
var mymodalremoveone = new bootstrap.Modal(modalremoveone);

//這邊是頁首的特效
$(document).on("scroll", function () {
    let value = $(document).scrollTop() > 20;

    $(".my-header").css({
        "background-color": value
            ? `rgb(172, 172, 172, 0.76)`
            : `rgb(52, 58, 64, 0)`,
        height: value ? "60px" : "80px",
        "backdrop-filter": value ? "blur(5px)" : "blur(0px)",
    });
});

    //moda給全部取消用的
 var appIdsarray = [];
var modalremoveall =
    $(`<div class="modal fade" id="staticBackdrop" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header";>
                        <h5 class="modal-title" id="staticBackdropLabel" ></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body" style="text-align: center;font-size:30px">
                    清除購物車
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary yesremoveall" data-bs-dismiss="modal">確定</button>
                    <button type="button" class="btn btn-primary noremoveall">取消</button>
                    </div>
                    </div>
                    </div>
                    </div>`);
var myModal = new bootstrap.Modal(modalremoveall);
modalremoveall.find(".yesremoveall").on("click", function () {
    console.log("c憨");
    $(".gameitem").remove();
    myModal.hide();

    $.ajax({
        url: "delete/allShoppingCart",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(appIdsarray),
        success: function (data) {
            console.log("成功刪除", data);
        },
       error: function (xhr, status, error) {
            console.error('刪除失敗', status, error);
            // 处理错误响应，例如显示错误信息
        }
    })
    console.log(appIdsarray)


    checkAndShowEmptyMessage();
    datasource = [];
    updataprice();
    $(".removebt a").text("已清空").addClass("disabled").css({
        color: "gary" /* 設置文字顏色為灰色 */,
    });
});
    modalremoveall.find(".noremoveall").on("click", function () {
        console.log(appIdsarray)
    myModal.hide();
});

function checkAndShowEmptyalert() {
    if ($(".gameitem").length != 0) {
        myModal.show();
    } else {
        console.log("l");
    }
}

//這是itembox如果空了就補的畫面
function checkAndShowEmptyMessage() {
    if ($(".gameitem").length == 0 && $(".empty-message").length == 0) {
        $(".itembox").append(
            '<div class="empty-message"><img src="/image/ShoppingCart/螢幕擷取畫面 2024-05-23 141622.png" alt="Empty Image"><p class="empty-message-text">目前購物車已清空!</p></div>'
        );
    }
    }

    //這是全部清除
    $(".removebt").on("click", function () {
        
        $(this).closest(".itembox").find(".gameitem").each(function () {
           appIdsarray.push($(this).data("appid"))
        })
        console.log(appIdsarray)
    if ($(".gameitem").length == 0) {
        checkAndShowEmptyMessage();
    } else {
        checkAndShowEmptyalert();
    }
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
                    <a href=""><i class="fa-solid fa-cart-shopping"></i> <b></b></a>
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
            canvascalculate()
            myOffcanvasGameItem.find(".deleticon").on("click", function () {
                $(this).parent(".canvasgameitem").remove();
                console.log($(this).closest(".canvasgameitem").data("appid"))
                var canvanappid = $(this).closest(".canvasgameitem").data("appid")
                $.ajax({
                    method: "DELETE",
                    url: `delete/ShoppingCart/${canvanappid}`,
                    success: function (data) {
                        console.log("成功刪除");
                    },
                    error: function (xhr, status, error) {
                        console.log("刪除失敗", status, error);
                    }
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
    event.preventDefault()
    // if (canvastotal == 0) {
    //     bsOffcanvas.block();
    // }
    bsOffcanvas.toggle();
    console.log("嗨");
});

//購物車圖標model功能結束





//跑馬燈
function runhorse() {
    var list = document.querySelector(".swiperlist");
    var box = document.querySelector(".swiperbox");
    let left = list.style.left;
    let timer;
    function move() {
        clearInterval(timer);
        timer = setInterval(() => {
            left = left - 2;

            if (left <= -650) {
                left = 0;
            }
            list.style.left = left + "px";
        }, 30);
    }

    move();
    box.onmouseover = function () {
        clearInterval(timer);
    };

    box.onmouseleave = function () {
        move();
    };
    
    }
    runhorse()
})
