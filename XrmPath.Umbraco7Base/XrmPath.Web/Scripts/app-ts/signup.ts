declare var Vue: any;
declare var $: any;
declare var moment: any;
declare var Promise: any;

class SignUpApplication{

    scopeSelector: string;

    constructor (scopeSelector: string) {
        this.scopeSelector = scopeSelector;
    }

    Initialize() {

        let scopeSelector: string = this.scopeSelector;

        var accountModalApp = new Vue({
        el: scopeSelector,
        data: {
            Id: $("#contentId").val() / 1,
            FirstName: "",
            LastName: "",
            CompanyName: "",
            SignUpEmail: "",
            ConfirmEmail: "",
            Password: "",
            ConfirmPassword: "",
            EmailSent: false,
            EventId: null,
            IsContact: false,
            InputParameters: { TemplateId: $(scopeSelector + " #templateId").val() / 1, RedirectId: $(scopeSelector + " #redirectNodeId").val()/1, IsContact: false, EventId: '' },
            Message: ""
        },
        methods: {
            signUp: function () {

                let self = this;
               
                var data =
                {
                    FirstName: self.FirstName,
                    LastName: self.LastName,
                    CompanyName: self.CompanyName,
                    Email: self.SignUpEmail,
                    Username: self.SignUpEmail,
                    Password: self.Password,
                    InputParameters: self.InputParameters
                };

                if (XrmPathApp.HasValue(self.EventId)){
                    data.InputParameters.EventId = self.EventId;
                    data.InputParameters.IsContact = true;
                }

                self.Message = "";

                if (!self.validateForm()) {
                    return false;
                }

                var url = "/api/membership/PostSignUp/";
                var autoProcessRegistration = XrmPathApp.GetQueryStringParam("autoProcessRegistration");
                if (autoProcessRegistration == "1" && XrmPathApp.HasValue(self.EventId)){
                    url = "/api/membership/PostSignUpAndRegisterCheckout/";
                }

                XrmPathApp.TriggerLoading(scopeSelector + " #signupLoading", "", scopeSelector + " #signupButton", "");

                $.post(url, data, function (e) {
                    var authenticationModel = e;
                    self.EmailSent = authenticationModel.EmailStatus.EmailSent;
                    self.Message = authenticationModel.EmailStatus.ErrorMessage;
                    self.RedirectUrl = authenticationModel.RedirectUrl;

                    if (XrmPathApp.HasValue(self.RedirectUrl)) {
                        window.location.href = self.RedirectUrl;
                        return false;
                    }

                    if (XrmPathApp.HasValue(self.EmailSent) && !self.EmailSent && XrmPathApp.HasValue(self.Message)) {
                        self.Message = self.Message;
                        $(scopeSelector + " .message").removeClass("success");
                    }
                    else {
                        self.Message = authenticationModel.Message;
                        if (XrmPathApp.HasValue(self.Message)) {
                            self.Message = self.Message;
                            $(scopeSelector + " .message").removeClass("success");
                        } else {
                            self.Message = "You have successfully signed up.<br />Please check your email to validate your account.";
                            $(scopeSelector + " .message").addClass("success");
                        }
                    }

                    XrmPathApp.TriggerLoading("", scopeSelector + " #signupLoading", "", scopeSelector + " #signupButton");
                });

                return false;
            },
            validateForm: function () {
                var isFormValid = $(scopeSelector + ' #signupForm').valid();
                return isFormValid;
            }
        },
        created:function()
        {
            let self = this;
            var emailParam = XrmPathApp.GetQueryStringParam("email");
            if (XrmPathApp.HasValue(emailParam)) {
                self.SignUpEmail = emailParam;
                self.ConfirmEmail = emailParam;
            }
            var eventIdParam = XrmPathApp.GetQueryStringParam("eventId");
            if (XrmPathApp.HasValue(eventIdParam)) {
                self.EventId = eventIdParam;
                self.IsContact = true;
            }
        },
        mounted: function () {
            let self = this;
            $(scopeSelector + " #signupForm").validate({
                rules: {
                    FirstName: { required: true },
                    LastName: { required: true },
                    CompanyName: { required: self.IsContact },
                    SignUpEmail: { required: true, email: true },
                    ConfirmEmail: { required: true, equalTo: "#SignUpEmail" },
                    Password: { required: true, minlength: 8 },
                    ConfirmPassword: { required: true, equalTo: "#Password" }
                },
                messages: {
                    FirstName: { required: "* First Name is required." },
                    LastName: { required: "* Last Name is required." },
                    CompanyName: { required: "* Company Name is required." },
                    SignUpEmail: { required: "* Email is required.", email: "* Email must be valid" },
                    ConfirmEmail: { required: "* Confirm Email is required", equalTo: "Email and Confirm Email must match" },
                    Password: { required: "* Password is required.", minlength: "Password must be a minimum of 8 characters." },
                    ConfirmPassword: {
                        required: "* Confirm Password is required.",
                        equalTo: "* Password and Confirm Password must match."
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
}


