 

var ChangePasswordApplication = function() {

    var me = {};

    me.Initialize = function(scopeSelector) {

        var app = new Vue({
            el: scopeSelector + " #changePasswordMacro",
            data: {
                Id: $("#contentId").val() / 1,
                Email: $(scopeSelector + " #email").val(),
                OldPassword: "",
                NewPassword: "",
                ConfirmPassword: "",
                Message: ""
            },
            methods: {
                changePassword: function () {

                    var self = this;
                    var url = "/api/membership/ChangePasswordPost/";
                    var data =
                    {
                        Username: self.Email,
                        Password: self.NewPassword,
                        InputParameters: { OldPassword: self.OldPassword }
                    };

                    self.Message = "";

                    if (!self.validateForm()) {
                        return false;
                    }

                    XrmPathApp.TriggerLoading(scopeSelector + " #changePasswordLoading", "", scopeSelector + " #changePasswordButton", "");

                    $.post(url, data, function (e) {
                        var authenticationModel = e;
                        self.Message = authenticationModel.Message;

                        if (XrmPathApp.HasValue(self.Message)) {
                            self.Message = self.Message;
                            $(scopeSelector + " .message").removeClass("success");
                        } else {
                            self.Message = "Your password has be successfully updated!";
                            $(scopeSelector + " .message").addClass("success");
                        }
                        XrmPathApp.TriggerLoading("", scopeSelector + " #changePasswordLoading", "", scopeSelector + " #changePasswordButton");
                    });

                    return false;
                },
                validateForm: function () {
                    var isFormValid = $(scopeSelector + " #changePasswordForm").valid();
                    return isFormValid;
                }
            },
            created: function () {
                var self = this;
                $(scopeSelector + " #changePasswordForm").validate({
                    rules: {
                        OldPassword: { required: true },
                        NewPassword: { required: true },
                        ConfirmPassword: { required: true, equalTo: "#NewPassword" }
                    },
                    messages: {
                        OldPassword: { required: "* Old Password is required." },
                        NewPassword: { required: "* New Password is required." },
                        ConfirmPassword: {
                            required: "* Confirm Password is required.",
                            equalTo: "* New Password and Confirm Password must match."
                        }
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
