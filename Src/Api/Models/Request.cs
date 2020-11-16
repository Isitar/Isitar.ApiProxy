namespace Api.Models
{
    public class Request
    {
        
        public string Url { get; set; }
        public string Method { get; set; }
        public Header[] Headers { get; set; }
        public Property[] Params { get; set; }
    }
}