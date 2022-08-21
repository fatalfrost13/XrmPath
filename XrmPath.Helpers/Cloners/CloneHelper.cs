using System;
using Newtonsoft.Json;
using Nuclex.Cloning;

namespace XrmPath.Helpers.Cloners
{
    public static class CloneHelper
    {
        public static T Clone<T>(T source)
        {
            try
            {
                var serialized = JsonConvert.SerializeObject(source);
                return JsonConvert.DeserializeObject<T>(serialized);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning($"XrmPath.Helpers caught error on CloneHelper.Clone(): {ex.ToString()}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on CloneHelper.Clone(): {ex.ToString()}");
                return default(T);
            }
        }

        public static T DeepClone<T>(T source)
        {
            try
            {
                var clone = ExpressionTreeCloner.DeepFieldClone(source);
                return clone;
            }
            catch(Exception ex)
            {
                Serilog.Log.Warning($"XrmPath.Helpers caught error on CloneHelper.DeepClone(): {ex.ToString()}");
                //LogHelper.Warn<T>($"XrmPath.Helpers caught error on CloneHelper.DeepClone(): {ex.ToString()}");
                return default(T);
            }
        }
    }
}