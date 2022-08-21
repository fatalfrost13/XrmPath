using System.Collections.Generic;

namespace XrmPath.Helpers.Model
{
    public class GenericModel
    {
        public int Id { get; set; }                 //user for any Id integer
        public string Value { get; set; }           //any string value
        public string Text { get; set; }            //any string text
        public string Element { get; set; }         //store element id
        public List<int> Ids { get; set; }          //store list of integers
        public List<string> Values { get; set; }    //store list of strings
        public List<object> Objects { get; set; }   //store list of generic objects
        public string Key { get; set; }             //stores a generic key value
        public int ValueInt { get; set; }           //any integer value
        public double ValueNumeric { get; set; }    //any numeric value
        public decimal ValueDecimal { get; set; }    //any numeric value
    }

    public class MediaItem
    {
        public int Id { get; set; } 
        public string Name { get; set; }           
        public string Url { get; set; }   
    }
}