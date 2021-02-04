using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace CEC.Blazor.SPA.Extensions
{
    public static class ControllerBaseExtensions
    {
        //public bool dddd(JsonElement kind, out object value)
        //{
        //    switch (kind.ValueKind)
        //    {
        //        case JsonValueKind.Number:
        //            if (kind.ToString().Contains("."))
        //            {
        //                value = kind.GetDouble();
        //                return true;
        //            }
        //            else
        //            {
        //                kind.t
        //            }
        //    }
        //    return false;
        //}

        //public bool TryGetMember(JsonElement kind, out object result)
        //{

            //// Get the property value
            //var srcData = RealObject.GetProperty(binder.Name);

            //result = null;

            //switch (srcData.ValueKind)
            //{
            //    case JsonValueKind.Null:
            //        result = null;
            //        break;
            //    case JsonValueKind.Number:
            //        result = srcData.GetDouble();
            //        break;
            //    case JsonValueKind.False:
            //        result = false;
            //        break;
            //    case JsonValueKind.True:
            //        result = true;
            //        break;
            //    case JsonValueKind.Undefined:
            //        result = null;
            //        break;
            //    case JsonValueKind.String:
            //        result = srcData.GetString();
            //        break;
            //    case JsonValueKind.Object:
            //        result = new ReflectionDynamicObject
            //        {
            //            RealObject = srcData
            //        };
            //        break;
            //    case JsonValueKind.Array:
            //        result = srcData.EnumerateArray()
            //            .Select(o => new ReflectionDynamicObject { RealObject = o })
            //            .ToArray();
            //        break;
            //}

            //// Always return true; other exceptions may have already been thrown if needed
            //return true;
        //}
    }
}
