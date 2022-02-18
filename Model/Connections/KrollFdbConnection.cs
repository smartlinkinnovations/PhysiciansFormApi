using PhysiciansFormApi.Model.Proto;

namespace IncidentReportsApi.Model.Connections
{
    public class KrollFdbConnection : IDbConnection
    {
        public string ConnectionString()
        {
            return "Data Source=10.33.2.56,1433;Initial Catalog=FDB;" +
                   "User Id=Kroll.Read;Password=E8U$8JkCdp&8;" +
                   "MultipleActiveResultSets=True;" +
                   "Persist Security Info=True;";
        }
    }
}
