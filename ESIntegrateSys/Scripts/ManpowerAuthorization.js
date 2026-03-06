$(function () {
    // 分頁設定
    const pageSize = 10;
    let unitPage = 1, userPage = 1, permissionPage = 1;
    let unitSearch = '', userSearch = '', permissionSearch = '';

    // 報工生產單位
    function loadUnits(page, search) {
        $.getJSON('/ManpowerAuthorization/取得所有報工生產單位', function (data) {
            let filtered = data;
            if (search) filtered = filtered.filter(x => x.單位名稱.indexOf(search) !== -1);
            renderPagedList(filtered, page, 'unitList', 'unitPagination', renderUnitRow, pageSize, loadUnits, search);
        });
    }
    function renderUnitRow(row, idx) {
        return `<tr><td>${row.單位ID}</td><td>${row.單位名稱}</td><td>${row.描述||''}</td><td>${row.啟用?'啟用':'停用'}</td>
        <td><button class='btn btn-sm btn-primary' onclick='editUnit(${row.單位ID})'>編輯</button> <button class='btn btn-sm btn-danger' onclick='deleteUnit(${row.單位ID})'>刪除</button></td></tr>`;
    }
    window.editUnit = function (id) { /* 彈窗編輯 */ };
    window.deleteUnit = function (id) { if(confirm('確定刪除?')) {/* AJAX 刪除 */} };

    // 使用者
    function loadUsers(page, search) {
        $.getJSON('/ManpowerAuthorization/取得所有使用者', function (data) {
            let filtered = data;
            if (search) filtered = filtered.filter(x => x.使用者ID.indexOf(search) !== -1);
            renderPagedList(filtered, page, 'userList', 'userPagination', renderUserRow, pageSize, loadUsers, search);
        });
    }
    function renderUserRow(row, idx) {
        return `<tr><td>${row.使用者ID}</td><td>${row.姓名}</td><td>${row.啟用?'啟用':'停用'}</td>
        <td><button class='btn btn-sm btn-primary' onclick='editUser("${row.使用者ID}")'>編輯</button> <button class='btn btn-sm btn-danger' onclick='deleteUser("${row.使用者ID}")'>刪除</button></td></tr>`;
    }
    window.editUser = function (id) { /* 彈窗編輯 */ };
    window.deleteUser = function (id) { if(confirm('確定刪除?')) {/* AJAX 刪除 */} };

    // 權限
    function loadPermissions(page, search) {
        $.getJSON('/ManpowerAuthorization/取得所有權限', function (data) {
            let filtered = data;
            if (search) filtered = filtered.filter(x => x.使用者ID.indexOf(search) !== -1);
            renderPagedList(filtered, page, 'permissionList', 'permissionPagination', renderPermissionRow, pageSize, loadPermissions, search);
        });
    }
    function renderPermissionRow(row, idx) {
        return `<tr><td>${row.權限ID}</td><td>${row.單位ID}</td><td>${row.使用者ID}</td><td>${row.授權日期?row.授權日期.substr(0,10):''}</td><td>${row.到期日?row.到期日.substr(0,10):''}</td><td>${row.備註||''}</td>
        <td><button class='btn btn-sm btn-primary' onclick='editPermission(${row.權限ID})'>編輯</button> <button class='btn btn-sm btn-danger' onclick='deletePermission(${row.權限ID})'>刪除</button></td></tr>`;
    }
    window.editPermission = function (id) { /* 彈窗編輯 */ };
    window.deletePermission = function (id) { if(confirm('確定刪除?')) {/* AJAX 刪除 */} };

    // 共用分頁與表格渲染
    function renderPagedList(data, page, listId, pageId, rowFunc, pageSize, reloadFunc, search) {
        let start = (page - 1) * pageSize;
        let paged = data.slice(start, start + pageSize);
        let html = `<table class='table table-bordered'><thead><tr>`;
        if (listId === 'unitList') html += '<th>ID</th><th>單位名稱</th><th>描述</th><th>狀態</th><th>操作</th>';
        if (listId === 'userList') html += '<th>工號</th><th>姓名</th><th>狀態</th><th>操作</th>';
        if (listId === 'permissionList') html += '<th>ID</th><th>單位ID</th><th>工號</th><th>授權日期</th><th>到期日</th><th>備註</th><th>操作</th>';
        html += '</tr></thead><tbody>';
        html += paged.map(rowFunc).join('');
        html += '</tbody></table>';
        $('#' + listId).html(html);
        // 分頁
        let totalPages = Math.ceil(data.length / pageSize);
        let pagHtml = '';
        for (let i = 1; i <= totalPages; i++) {
            pagHtml += `<li class='page-item${i === page ? ' active' : ''}'><a class='page-link' href='#' onclick='return false;'>${i}</a></li>`;
        }
        $('#' + pageId).html(pagHtml);
        $('#' + pageId + ' .page-link').click(function () {
            let p = parseInt($(this).text());
            reloadFunc(p, search);
        });
    }

    // 搜尋欄位事件
    $('#searchUnit').on('input', function () {
        unitSearch = $(this).val();
        loadUnits(1, unitSearch);
    });
    $('#searchUser').on('input', function () {
        userSearch = $(this).val();
        loadUsers(1, userSearch);
    });
    $('#searchPermission').on('input', function () {
        permissionSearch = $(this).val();
        loadPermissions(1, permissionSearch);
    });

    // 初始載入
    loadUnits(1, '');
    loadUsers(1, '');
    loadPermissions(1, '');

    // 新增按鈕事件（可補充彈窗與AJAX邏輯）
    $('#addUnitBtn').click(function () { /* 彈窗新增單位 */ });
    $('#addUserBtn').click(function () { /* 彈窗新增使用者 */ });
    $('#addPermissionBtn').click(function () { /* 彈窗新增權限 */ });
});
