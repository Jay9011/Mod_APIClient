namespace APIClient.Models.Interfaces
{
    public enum RequestContentType
    {
        Raw,
        FormData,
        UrlEncoded,
        XML,
        Json,
        Binary,
        Multipart,
        GraphQL,
    }

    public class ContentTypeInfo
    {
        public string MediaType { get; set; }
        public string CharSet { get; set; }

        public string GetContentType()
        {
            return string.IsNullOrEmpty(CharSet) ? MediaType
                : $"{MediaType}; charset={CharSet}";
        }
    }
}