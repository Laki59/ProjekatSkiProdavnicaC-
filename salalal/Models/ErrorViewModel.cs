namespace salalal.Models
{
    public class ErrorViewModel
    {
        //Ovo svojstvo cuva ID trenutnog zahteva,da se vidi koji je ID greske
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
