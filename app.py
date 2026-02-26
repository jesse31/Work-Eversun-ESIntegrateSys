import os
import secrets
from flask import Flask, render_template, redirect, url_for, flash, request
from flask_login import LoginManager, UserMixin, login_user, logout_user, login_required, current_user
from werkzeug.security import generate_password_hash, check_password_hash

app = Flask(__name__)
app.secret_key = os.environ.get('SECRET_KEY') or secrets.token_hex(32)

login_manager = LoginManager(app)
login_manager.login_view = 'login'
login_manager.login_message = '請先登入以存取此頁面。'


# Simple in-memory user store (replace with a database in production)
_admin_password = os.environ.get('ADMIN_PASSWORD', 'admin123')
USERS = {
    'admin': {
        'id': '1',
        'username': 'admin',
        'password_hash': generate_password_hash(_admin_password),
        'display_name': '管理員',
    }
}


class User(UserMixin):
    def __init__(self, user_id, username, display_name):
        self.id = user_id
        self.username = username
        self.display_name = display_name


@login_manager.user_loader
def load_user(user_id):
    for u in USERS.values():
        if u['id'] == user_id:
            return User(u['id'], u['username'], u['display_name'])
    return None


@app.route('/')
@login_required
def index():
    modules = [
        {'name': '訂單管理', 'icon': '📋', 'url': '#', 'desc': '管理客戶訂單與出貨作業'},
        {'name': '庫存管理', 'icon': '📦', 'url': '#', 'desc': '追蹤商品庫存與補貨需求'},
        {'name': '財務報表', 'icon': '💰', 'url': '#', 'desc': '查看財務數據與報表分析'},
        {'name': '人事管理', 'icon': '👥', 'url': '#', 'desc': '員工資料與排班管理'},
        {'name': '生產排程', 'icon': '🏭', 'url': '#', 'desc': '生產計劃與進度追蹤'},
        {'name': '系統設定', 'icon': '⚙️', 'url': '#', 'desc': '系統參數與權限設定'},
    ]
    return render_template('index.html', modules=modules)


@app.route('/login', methods=['GET', 'POST'])
def login():
    if current_user.is_authenticated:
        return redirect(url_for('index'))
    if request.method == 'POST':
        username = request.form.get('username', '').strip()
        password = request.form.get('password', '')
        user_data = USERS.get(username)
        if user_data and check_password_hash(user_data['password_hash'], password):
            user = User(user_data['id'], user_data['username'], user_data['display_name'])
            login_user(user)
            next_page = request.args.get('next')
            return redirect(next_page or url_for('index'))
        flash('帳號或密碼錯誤，請重新輸入。', 'danger')
    return render_template('login.html')


@app.route('/logout')
@login_required
def logout():
    logout_user()
    flash('您已成功登出。', 'success')
    return redirect(url_for('login'))


if __name__ == '__main__':
    app.run(debug=True)
