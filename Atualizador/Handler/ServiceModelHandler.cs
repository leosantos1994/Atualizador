namespace UpdaterService.Handler
{
    public static class ServiceModelHandler
    {
        public static MidModel.ServiceModel _service;
        public static string BackupZipFile;
        public static string SiteFolderPath;
        public static string ServiceFolderPath;

        public static void Updater(MidModel.ServiceModel service) => _service = service;

        public static MidModel.ServiceModel Get() => _service;
    }
}
