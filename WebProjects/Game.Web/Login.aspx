<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Game.Web.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link href="styles/main.css" rel="stylesheet" type="text/css" />
    <title>林工定制游戏后台管理系统 微信:15118164480 邮箱:lin_hong_quan_msn@hotmail.com - 登陆</title>
</head>
<script src="scripts/vue.js"></script>
<script src="scripts/axios.min.js"></script>
<script src="scripts/bg.js"></script>
<body>
        <div id="app">
        <div class="login-container">
            <h1>用户登录</h1>
            
            <form @submit.prevent="handleLogin">
                <div class="form-group">
                    <label for="username">用户名：</label>
                    <div class="input-container">
                        <input 
                            type="text" 
                            id="username" 
                            class="form-control" 
                            v-model="form.username" 
                            :class="{'error': errors.username}"
                            placeholder="请输入用户名"
                        >
                    </div>
                    <span class="error-message">{{ errors.username }}</span>
                </div>
                
                <div class="form-group">
                    <label for="password">密码：</label>
                    <div class="input-container">
                        <input 
                            type="password" 
                            id="password" 
                            class="form-control" 
                            v-model="form.password" 
                            :class="{'error': errors.password}"
                            placeholder="请输入密码"
                        >
                    </div>
                    <span class="error-message">{{ errors.password }}</span>
                </div>
                
                <div class="form-group">
                    <label for="captcha">验证码：</label>
                    <div class="captcha-section">
                        <div class="captcha-input">
                            <input 
                                type="text" 
                                id="captcha" 
                                class="form-control" 
                                v-model="form.captcha" 
                                :class="{'error': errors.captcha}"
                                placeholder="请输入验证码"
                            >
                        </div>
                        <img 
                            :src="captchaImage" 
                            class="captcha-image" 
                            width="120" 
                            height="40" 
                            alt="验证码" 
                            @click="refreshCaptcha"
                            title="点击更换验证码"
                        >
                    </div>
                    <a href="javascript:void(0)" class="captcha-refresh" @click="refreshCaptcha">更换验证码</a>
                    <span class="error-message">{{ errors.captcha }}</span>
                </div>
                
                <button 
                    type="submit" 
                    class="btn-login" 
                    :disabled="isSubmitting"
                >
                    {{ isSubmitting ? '登录中...' : '登录' }}
                </button>
            </form>
            
            <div class="additional-links">
                <p><a href="#">忘记密码？</a> | <a href="#">注册新账户</a></p>
            </div>
        </div>
        <!-- 版本与联系信息 -->
        <div style="text-align:center; color:#888; font-size:14px; margin-top:20px;">
            版本：v1.0.0　|　联系邮箱：lin_hong_quan_msn@hotmail.com
        </div>
    </div>
    <script>
        new Vue({
            el: '#app',
            data: {
                form: {
                    username: '',
                    password: '',
                    captcha: ''
                },
                errors: {
                    username: '',
                    password: '',
                    captcha: ''
                },
                message: {
                    text: '',
                    type: ''
                },
                isSubmitting: false,
                captchaImage: '/Tools/VerifyImagePage.aspx?t=' + new Date().getTime()
            },
            methods: {
                validateForm() {
                    // 重置错误信息
                    this.errors = {
                        username: '',
                        password: '',
                        captcha: ''
                    };

                    let isValid = true;

                    // 验证用户名
                    if (!this.form.username.trim()) {
                        this.errors.username = '用户名不能为空';
                        isValid = false;
                    }

                    // 验证密码
                    if (!this.form.password) {
                        this.errors.password = '密码不能为空';
                        isValid = false;
                    } else if (this.form.password.length < 6) {
                        this.errors.password = '密码长度不能少于6位';
                        isValid = false;
                    }

                    // 验证验证码
                    if (!this.form.captcha.trim()) {
                        this.errors.captcha = '验证码不能为空';
                        isValid = false;
                    }

                    return isValid;
                },

                showMessage(text, type = 'error') {
                    this.message.text = text;
                    this.message.type = type;
                    setTimeout(() => {
                        this.message.text = '';
                    }, 3000);
                },

                // 登录函数
                async handleLogin() {
                    if (!this.validateForm()) {
                        return;
                    }

                    this.isSubmitting = true;

                    try {
                        const response = await axios.post('/api.asmx/Login', {
                            username: this.form.username,
                            password: this.form.password,
                            captcha: this.form.captcha
                        }, {
                            headers: {
                                'Content-Type': 'application/x-www-form-urlencoded' //请求会返回xml格式
                                //'Content-Type': 'application/json'  //请求会返回json格式
                            }
                        });

                        if (response.status == 200 && response.statusText == "OK") {
                            //处理xml '<?xml version="1.0" encoding="utf-8"?>\r\n<int xmlns="http://tempuri.org/">0</int>'
                            let ret = response.data.replace('<?xml version="1.0" encoding="utf-8"?>\r\n<int xmlns="http://tempuri.org/">', "");
                            ret = ret.replace("</int>", "");
                            console.log(ret);
                            if (parseInt(ret) == 0) {
                                this.showMessage('登录成功！正在跳转...', 'success');
                                window.location.href = '/home.html';
                            } else {
                                //this.showMessage(response.data.message || '登录失败，请检查用户名、密码或验证码','error');
                                this.refreshCaptcha();
                            }
                        }
                    } catch (error) {
                        console.error('登录错误:', error);
                        if (error.response) {
                            // 服务器返回了错误状态码
                            this.showMessage(`登录失败: ${error.response.status} ${error.response.statusText}`,'error');
                        } else if (error.request) {
                            // 请求已发出但没有收到响应
                            this.showMessage('网络错误，请检查网络连接','error');
                        } else {
                            // 其他错误
                            this.showMessage('登录过程中发生错误','error');
                        }
                        this.refreshCaptcha();
                    } finally {
                        this.isSubmitting = false;
                    }
                },

                refreshCaptcha() {
                    this.captchaImage = '/Tools/VerifyImagePage.aspx?t=' + new Date().getTime();
                    this.form.captcha = '';
                },

                mounted() {
                    this.refreshCaptcha();
                }
            }
        });
    </script>
</body>
</html>
