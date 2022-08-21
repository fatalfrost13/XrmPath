
var ForgotPasswordApplication = function() {

    var me = {};

    me.Initialize = function(scopeSelector) {

        var app = new Vue({
            el: scopeSelector + ' #forgotPasswordMacro',
            data: {
                Id: $("#contentId").val() / 1,
                ForgotEmail: "",
                EmailSent: false,
                InputParameters: { TemplateId: $(scopeSelector + " #templateId").val() / 1 },
                Message: ""
            },
            methods: {
                sendPassword: function () {
            
                    var self = this;
                    var url = "/api/membership/ForgotPasswordPost/";
                    var data =
                    {
                        Username: self.ForgotEmail,
                        InputParameters: self.InputParameters
                    };

                    self.Message = "";

                    if (!self.validateForm()) {
                        return false;
                    }

                    XrmPathApp.TriggerLoading(scopeSelector + " #forgotPasswordLoading", "", scopeSelector + " #forgotPasswordButton", "");

                    $.post(url, data, function (e) {
                        var authenticationModel = e;
                        self.EmailSent = authenticationModel.EmailStatus.EmailSent;
                        self.Message = authenticationModel.EmailStatus.ErrorMessage;

                        if (XrmPathApp.HasValue(self.EmailSent) && !self.EmailSent && XrmPathApp.HasValue(self.Message)) {
                            self.Message = self.Message;
                            $(scopeSelector + " .message").removeClass("success");
                        } else {
                            self.Message = "Login credentials have been sent, please check your email.";
                            $(scopeSelector + " .message").addClass("success");
                        }
                        XrmPathApp.TriggerLoading("", scopeSelector + " #forgotPasswordLoading", "", scopeSelector + " #forgotPasswordButton");
                    });

                    return false;
                },
                validateForm: function () {
                    var isFormValid = $(scopeSelector + ' #forgotPasswordForm').valid();
                    return isFormValid;
                }
            },
            created: function () {
                var self = this;
                $(scopeSelector + " #forgotPasswordForm").validate({
                    rules: {
                        ForgotEmail: { required: true }
                    },
                    messages: {
                        ForgotEmail: { required: "* Email is required." }
                    }
                });

                var emailParam = XrmPathApp.GetQueryStringParam("Email");
                if (XrmPathApp.HasValue(emailParam)) {
                    self.ForgotEmail = emailParam;
                }
            },
            computed:
            {

            }

        });

        
        return false;
    }

    return me;
}();
