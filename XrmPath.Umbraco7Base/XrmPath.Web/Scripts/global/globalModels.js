//Used to store generic lookup information
var GenericModel = /** @class */ (function () {
    function GenericModel(Id, Value, Text) {
        if (Id === void 0) { Id = 0; }
        if (Value === void 0) { Value = ""; }
        if (Text === void 0) { Text = ""; }
        this.Id = Id;
        this.Value = Value;
        this.Text = Text;
    }
    return GenericModel;
}());
//Used to for attributes that make up a link
var LinkItem = /** @class */ (function () {
    function LinkItem() {
    }
    return LinkItem;
}());
//Holds all attributes stored in a URL Picker
var UrlPicker = /** @class */ (function () {
    function UrlPicker() {
        this.Title = "";
        this.LinkType = 0; //0 = Content, 1 = Media, 2 = External
        this.NodeId = null;
        this.NewWindow = false;
    }
    return UrlPicker;
}());
var DynamicFormModel = /** @class */ (function () {
    function DynamicFormModel() {
        this.FieldList = new Array();
    }
    return DynamicFormModel;
}());
var DynamicField = /** @class */ (function () {
    function DynamicField() {
    }
    return DynamicField;
}());
var ApiPostModel = /** @class */ (function () {
    function ApiPostModel() {
        this.Attributes = new Array();
    }
    return ApiPostModel;
}());
var ApiPostAttributes = /** @class */ (function () {
    function ApiPostAttributes() {
    }
    return ApiPostAttributes;
}());
//# sourceMappingURL=globalModels.js.map