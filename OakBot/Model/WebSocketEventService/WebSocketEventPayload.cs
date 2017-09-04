using Newtonsoft.Json;

namespace OakBot.Model
{
    internal class WebSocketEventPayload
    {
        [JsonProperty("event")]
        public string Event { get; private set; }

        [JsonProperty("data")]
        public object Data { get; private set; }

        /// <summary>
        /// Create event payload package.
        /// </summary>
        /// <param name="wsevent">Event name.</param>
        /// <param name="data">
        /// If given data is type of string it will try to deserialize to a C# object else raw string is used.
        /// Any other object needs to be serilizable to a json string else null will be set as data.
        /// </param>
        public WebSocketEventPayload(string wsevent, object data)
        {
            // Set event
            Event = wsevent;
            Data = data;

            // Verify Data if exists
            if (data != null)
            {
                // Data is typeof string, try to deserialize to C# object
                if (data?.GetType() == typeof(string))
                {
                    try
                    {
                        Data = JsonConvert.DeserializeObject((string)data);
                    }
                    catch (JsonReaderException)
                    {
                        // Do nothing, raw string is used as data
                    }
                }
                // If other than string test if it is serializable else set null
                else if (data != null)
                {
                    try
                    {
                        // Test if serializable to C# object
                        var test = JsonConvert.SerializeObject(data);
                    }
                    catch
                    {
                        // Set null on failure
                        Data = null;
                    }
                }
            }
            

        }
    }
}
