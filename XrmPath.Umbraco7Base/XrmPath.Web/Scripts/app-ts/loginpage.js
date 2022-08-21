var LoginPageApplication = /** @class */ (function () {
    function LoginPageApplication(scopeSelector) {
        this.scopeSelector = scopeSelector;
    }
    LoginPageApplication.prototype.Initialize = function () {
        var scopeSelector = this.scopeSelector;
        var app = new Vue({
            el: scopeSelector,
            data: {
                Id: $("#contentId").val() / 1,
                Username: "",
                Password: "",
                RememberMe: false,
                Message: ""
            },
            methods: {
                login: function () {
                    var self = this;
                    var url = "/api/membership/MemberLoginPost/";
                    var data = {
                        Username: self.Username,
                        Password: self.Password,
                        RememberMe: self.RememberMe,
                        InputParameters: { RedirectUrl: $(scopeSelector + " #redirecturl").val() }
                    };
                    self.Message = "";
                    if (!self.validateForm()) {
                        return false;
                    }
                    XrmPathApp.TriggerLoading(scopeSelector + " #loginLoading", "", scopeSelector + " #loginButton", "");
                    XrmPathApp.PostApiRequest(url, data).then(function (e) {
                        var authenticationModel = e;
                        if (XrmPathApp.HasValue(authenticationModel.Message)) {
                            self.Message = authenticationModel.Message;
                        }
                        if (authenticationModel.UserValidated) {
                            if (XrmPathApp.HasValue(authenticationModel.RedirectUrl)) {
                                location.href = authenticationModel.RedirectUrl;
                            }
                            else {
                                location.href = location.href;
                            }
                        }
                        else {
                            XrmPathApp.TriggerLoading("", scopeSelector + " #loginLoading", "", scopeSelector + " #loginButton");
                        }
                    }).then(function (e) {
                        //XrmPathApp.OAuthCreateToken(data.Username, data.Password);
                    });
                    return false;
                },
                validateForm: function () {
                    var isFormValid = $(scopeSelector + ' #loginForm').valid();
                    return isFormValid;
                },
                logout: function () {
                    var self = this;
                    var url = "/api/membership/MemberLogoutPost/";
                    var data = {};
                    XrmPathApp.TriggerLoading(scopeSelector + " #logoutLoading", "", scopeSelector + " #logoutButton", "");
                    XrmPathApp.PostApiRequest(url, data).then(function (e) {
                        var authenticationModel = e;
                        if (XrmPathApp.HasValue(authenticationModel.RedirectUrl)) {
                            location.href = authenticationModel.RedirectUrl;
                        }
                        else {
                            location.href = location.href;
                        }
                        XrmPathApp.TriggerLoading("", scopeSelector + " #logoutLoading", "", scopeSelector + " #logoutButton");
                    });
                }
            },
            created: function () {
                //let self = this;
                //$(scopeSelector + " #loginForm").validate({
                //    rules: {
                //        Username: { required: true },
                //        Password: { required: true }
                //    },
                //    messages: {
                //        Username: { required: "* Email is required." },
                //        Password: { required: "* Password is required." }
                //    }
                //});
            },
            mounted: function () {
                var self = this;
                $(scopeSelector + " #loginForm").validate({
                    rules: {
                        Username: { required: true },
                        Password: { required: true }
                    },
                    messages: {
                        Username: { required: "* Email is required." },
                        Password: { required: "* Password is required." }
                    }
                });
            },
            computed: {}
        });
    };
    return LoginPageApplication;
}());
//# sourceMappingURL=loginpage.js.map