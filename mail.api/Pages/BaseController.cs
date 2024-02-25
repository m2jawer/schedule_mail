using mail.api.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace mail.api.Pages
{
    public class BaseController : ControllerBase
    {
        internal static ScheduleDb Db = new ScheduleDb();

        protected virtual string GetJsonValue(JsonElement input, string propName, string defaultValue = "")
        {
            JsonElement propValue;

            if (input.TryGetProperty(propName, out propValue))
            {
                return propValue.ToString();
            }
            else
            {
                return defaultValue;
            }
        }

        protected virtual int GetJsonValue(JsonElement input, string propName, int defaultValue = 0)
        {
            JsonElement propValue;

            if (input.TryGetProperty(propName, out propValue))
            {
                return propValue.GetInt32();
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
