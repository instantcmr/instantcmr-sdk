using Icmr.Integration;

class Settings
{
    public string dpatRoot { get; set; } = "./downloads/";
    public string kid { get; set; } = "gqdfrYj0o9uIIiIS7ZhLuQ";
    public string shs { get; set; } = "vjISm4YH-7AwxeIUd-SmfvZ9h6189brYUW7NKng7DC8";
    public Severity loglevel { get; set; } = Severity.ERROR;
    public string copid { get; set; } = "dev";
    public string iepn { get; set; } = "dub";
    public string url { get; set; } = "http://localhost:8080/v3/igr/";
}