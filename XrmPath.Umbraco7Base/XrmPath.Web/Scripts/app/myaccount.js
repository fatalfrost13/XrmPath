var MyAccountApplication = function() {

    var me = {};

    me.Initialize = function(scopeSelector) {
        
        var app = new Vue({
            el: scopeSelector,
            data: {
                Id: $("#contentId").val() / 1,
                ActiveTab: "home",
                Email: "",
                Message: ""
            },
            methods: {
                saveChanges: function () {
                    return false;
                },
                tabClick: function(tab) {
                    var self = this;
                    self.ActiveTab = tab;
                    return false;
                },
                validateForm: function () {
                    var isFormValid = $(scopeSelector + ' #myAccountForm').valid();
                    return isFormValid;
                },
                logout: function() {
                    var self = this;
                    var url = "/api/membership/MemberLogoutPost/";
                    var data = {};

                    XrmPathApp.TriggerLoading(scopeSelector + " #logoutLoading", "", scopeSelector + " #logoutButton", "");

                    $.post(url, data, function (e) {
                        var authenticationModel = e;
                        if (XrmPathApp.HasValue(authenticationModel.RedirectUrl)) {
                            location.href = authenticationModel.RedirectUrl;
                        } else {
                            location.href = location.href;
                        }
                        XrmPathApp.TriggerLoading("", scopeSelector + " #logoutLoading", "", scopeSelector + " #logoutButton");
                    });
                }
            },
            created: function () {
                var self = this;
                $(scopeSelector + " #myAccountForm").validate({
                    rules: {
                        Email: { required: true }
                    },
                    messages: {
                        Email: { required: "* Email is required." }
                    }
                });
            },
            computed:
            {

            }

        });
        
        return false;
    }

    return me;
}();


