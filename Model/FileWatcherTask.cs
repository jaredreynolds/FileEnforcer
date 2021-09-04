namespace FileEnforcer.Model
{
    public class FileWatcherTask
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public FileWatcherAction Action { get; set; }
    }
}
