namespace UpdaterService.Model
{
    public class Response
    {
        public string? Log { get; set; }
        public Guid? ServiceId { get; set; }
        public bool? Complete { get; set; }
    }
}
