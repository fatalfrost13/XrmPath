
//Used to store generic lookup information
class GenericModel{

    Id: number;
    Value: string;
    Text: string;

    constructor(Id: number = 0, Value: string = "", Text: string = ""){
        this.Id = Id;
        this.Value = Value;
        this.Text = Text;
    }
}

//Used to for attributes that make up a link
class LinkItem
{
    NodeId: number;
    Title: string;
    UrlPicker: UrlPicker;
    Description: string;
    LinkType: string;
    InternalUrl: string;
}

//Holds all attributes stored in a URL Picker
class UrlPicker
{
    Title: string = "";
    LinkType: number = 0;  //0 = Content, 1 = Media, 2 = External
    NodeId: number = null;
    Url: string;
    NewWindow: boolean = false;
}

class DynamicFormModel{
    Id: string;
    Name: string;
    FieldList: Array<DynamicField> = new Array<DynamicField>();
}

class DynamicField{
    Name: string;
    Type: string;
    Value: string;
}

class ApiPostModel
    {
        Name: string;
        Id: string;
        Attributes: Array<ApiPostAttributes> = new Array<ApiPostAttributes>();
    }
class ApiPostAttributes
{
    Name: string;
    Value: string;
}