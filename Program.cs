using System.Reflection.Metadata;

namespace winui_db
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            //var db = db;
            //Manager.FillMedicine(db);
            await Server.GetInstance().StartAsync();
            
        }
    }
}
