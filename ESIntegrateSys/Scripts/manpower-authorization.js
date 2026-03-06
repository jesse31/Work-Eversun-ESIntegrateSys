/**
 * 人力配置授權模組共用 JavaScript 功能
 */

// 初始化抽屜式選單功能
function initDrawerMenu() {
    // 抽屜式選單開關點擊事件
    $('.drawer-toggle').off('click').on('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('抽屜式選單開關被點擊');
        $('.drawer-menu').toggleClass('open');
        $('.drawer-backdrop').toggleClass('open');

        // 確保選單項目點擊事件已綁定
        bindDrawerMenuItems();
    });

    // 點擊頁面其他區域關閉選單
    $(document).off('click.drawerOutside').on('click.drawerOutside', function(e) {
        if (!$(e.target).closest('.drawer-menu').length && 
            !$(e.target).closest('.drawer-toggle').length && 
            $('.drawer-menu').hasClass('open')) {
            $('.drawer-menu').removeClass('open');
            $('.drawer-backdrop').removeClass('open');
        }
    });

    // 點擊遮罩關閉側邊欄
    $('.drawer-backdrop').off('click').on('click', function () {
        $('.drawer-menu').removeClass('open');
        $('.drawer-backdrop').removeClass('open');
    });

    // 立即綁定選單項目點擊事件
    bindDrawerMenuItems();
}

// 綁定抽屜式選單項目點擊事件
function bindDrawerMenuItems() {
    console.log('綁定抽屜式選單項目點擊事件');
    $('.drawer-menu-item').off('click').on('click', function(e) {
        // 不阻止默認行為，允許頁面跳轉
        var url = $(this).attr('href');
        var menuText = $(this).text().trim();

        console.log('選單項目被點擊:', menuText);
        console.log('跳轉到頁面:', url);

        // 關閉抽屜式選單
        $('.drawer-menu').removeClass('open');
        $('.drawer-backdrop').removeClass('open');

        // 直接跳轉到對應的頁面
        window.location.href = url;
    });
}

// 初始化 Select2 下拉選單
function initSelect2() {
    if ($.fn.select2) {
        $('.select2').select2({
            width: '100%',
            placeholder: '請選擇',
            allowClear: true
        });
    }
}

// 文檔就緒時初始化所有功能
$(document).ready(function() {
    // 初始化抽屜式選單
    initDrawerMenu();
    
    // 初始化 Select2 下拉選單
    initSelect2();
});
