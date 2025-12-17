namespace TrackifyApis
{
    public class ConnectionString
    {
        private static string cs = "Server=(localdb)\\MSSQLLocalDB;Database=Trackify;Trusted_Connection=True;MultipleActiveResultSets=true";
        public static string dbcs { get => cs; }
    }
}
