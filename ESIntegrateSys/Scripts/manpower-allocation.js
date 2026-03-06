/**
 * 人力配置模組 JavaScript 功能
 * 提供記錄鎖定狀態檢查、顯示當前編輯者信息等功能
 */

// 鎖定狀態檢查間隔（毫秒）
const LOCK_CHECK_INTERVAL = 30000; // 30秒
// 鎖定超時時間（分鐘）
const LOCK_TIMEOUT_MINUTES = 30;
// 當前檢查的記錄ID
let currentRecordId = null;
// 定時器ID
let lockCheckTimerId = null;
// 最後一次檢查時間戳
let lastTimestamp = null;

/**
 * 初始化鎖定狀態檢查
 * @param {string} recordId 記錄ID
 */
function initLockStatusCheck(recordId) {
    // 清除之前的定時器
    if (lockCheckTimerId) {
        clearInterval(lockCheckTimerId);
        lockCheckTimerId = null;
    }
    
    currentRecordId = recordId;
    
    // 立即檢查一次
    checkLockStatus();
    
    // 設置定時檢查
    lockCheckTimerId = setInterval(checkLockStatus, LOCK_CHECK_INTERVAL);
    
    // 頁面卸載時清除定時器
    $(window).on('beforeunload', function() {
        if (lockCheckTimerId) {
            clearInterval(lockCheckTimerId);
        }
    });
}

/**
 * 檢查記錄鎖定狀態
 */
function checkLockStatus() {
    if (!currentRecordId) return;
    
    $.ajax({
        url: '/ManpowerAllocation/GetRecordLockStatus',
        type: 'GET',
        data: { id: currentRecordId },
        dataType: 'json',
        success: function(response) {
            if (response.success) {
                // 檢查時間戳是否變化
                if (lastTimestamp && lastTimestamp !== response.timestamp) {
                    // 記錄已被修改，顯示警告
                    showRecordModifiedWarning();
                }
                
                lastTimestamp = response.timestamp;
                
                if (response.isLocked) {
                    // 記錄被鎖定，顯示鎖定信息
                    showLockInfo(response.editorName, response.lockTime);
                    
                    // 如果不是當前用戶鎖定的，禁用編輯功能
                    if (!isCurrentUserEditor(response.editorName)) {
                        disableEditing();
                    }
                } else {
                    // 記錄未被鎖定，隱藏鎖定信息
                    hideLockInfo();
                    
                    // 啟用編輯功能
                    enableEditing();
                }
            } else {
                console.error('檢查鎖定狀態失敗：', response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('檢查鎖定狀態請求失敗：', error);
        }
    });
}

/**
 * 顯示記錄鎖定信息
 * @param {string} editorName 編輯者名稱
 * @param {string} lockTime 鎖定時間
 */
function showLockInfo(editorName, lockTime) {
    // 格式化鎖定時間
    const lockDate = new Date(parseInt(lockTime.substr(6)));
    const formattedTime = lockDate.toLocaleString();
    
    // 計算剩餘鎖定時間
    const now = new Date();
    const elapsedMinutes = Math.floor((now - lockDate) / (1000 * 60));
    const remainingMinutes = Math.max(0, LOCK_TIMEOUT_MINUTES - elapsedMinutes);
    
    // 創建或更新鎖定信息元素
    let $lockInfo = $('#recordLockInfo');
    if ($lockInfo.length === 0) {
        $lockInfo = $('<div id="recordLockInfo" class="alert alert-warning"></div>');
        $('#formContainer').prepend($lockInfo);
    }
    
    // 更新鎖定信息內容
    $lockInfo.html(`
        <strong>注意！</strong> 此記錄正被 <strong>${editorName}</strong> 編輯中。
        <br>鎖定開始時間：${formattedTime}
        <br>鎖定將在 <strong>${remainingMinutes}</strong> 分鐘後自動釋放。
    `);
    
    // 如果是當前用戶鎖定的，添加提示
    if (isCurrentUserEditor(editorName)) {
        $lockInfo.append('<br><small>您已鎖定此記錄進行編輯。</small>');
        $lockInfo.removeClass('alert-warning').addClass('alert-info');
    } else {
        $lockInfo.removeClass('alert-info').addClass('alert-warning');
    }
}

/**
 * 隱藏記錄鎖定信息
 */
function hideLockInfo() {
    $('#recordLockInfo').remove();
}

/**
 * 顯示記錄已被修改的警告
 */
function showRecordModifiedWarning() {
    Swal.fire({
        title: '記錄已被修改',
        text: '此記錄已被其他用戶修改。請刷新頁面獲取最新數據。',
        icon: 'warning',
        confirmButtonText: '刷新頁面',
        showCancelButton: true,
        cancelButtonText: '稍後刷新'
    }).then((result) => {
        if (result.isConfirmed) {
            location.reload();
        }
    });
}

/**
 * 檢查是否為當前用戶
 * @param {string} editorName 編輯者名稱
 * @returns {boolean} 是否為當前用戶
 */
function isCurrentUserEditor(editorName) {
    // 獲取當前用戶名（假設存儲在頁面中的隱藏字段）
    const currentUser = $('#CurrentUserId').val();
    return currentUser === editorName;
}

/**
 * 禁用編輯功能
 */
function disableEditing() {
    // 禁用所有輸入字段和按鈕
    $('#formContainer input, #formContainer select, #formContainer textarea').prop('disabled', true);
    $('#saveButton, #deleteButton').prop('disabled', true).addClass('disabled');
    
    // 添加提示
    $('#editDisabledMessage').remove();
    $('#formContainer').prepend(`
        <div id="editDisabledMessage" class="alert alert-danger">
            <strong>編輯已禁用！</strong> 此記錄正被其他用戶編輯，您無法進行修改。
        </div>
    `);
}

/**
 * 啟用編輯功能
 */
function enableEditing() {
    // 啟用所有輸入字段和按鈕
    $('#formContainer input, #formContainer select, #formContainer textarea').prop('disabled', false);
    $('#saveButton, #deleteButton').prop('disabled', false).removeClass('disabled');
    
    // 移除提示
    $('#editDisabledMessage').remove();
}

/**
 * 頁面載入時初始化
 */
$(document).ready(function() {
    // 檢查是否在編輯頁面
    const recordId = $('#RecordId').val();
    if (recordId) {
        initLockStatusCheck(recordId);
    }
    
    // 表單提交前檢查鎖定狀態
    $('#editForm').on('submit', function(e) {
        // 再次檢查鎖定狀態，確保沒有被其他用戶鎖定
        $.ajax({
            url: '/ManpowerAllocation/GetRecordLockStatus',
            type: 'GET',
            data: { id: currentRecordId },
            dataType: 'json',
            async: false, // 同步請求
            success: function(response) {
                if (response.success && response.isLocked) {
                    // 如果不是當前用戶鎖定的，阻止提交
                    if (!isCurrentUserEditor(response.editorName)) {
                        e.preventDefault();
                        Swal.fire({
                            title: '無法保存',
                            text: '此記錄已被其他用戶鎖定，您無法保存修改。',
                            icon: 'error',
                            confirmButtonText: '確定'
                        });
                    }
                }
            },
            error: function() {
                // 出錯時阻止提交
                e.preventDefault();
                Swal.fire({
                    title: '無法保存',
                    text: '檢查鎖定狀態時發生錯誤，請稍後再試。',
                    icon: 'error',
                    confirmButtonText: '確定'
                });
            }
        });
    });
});
