using System.Reflection.Metadata;

namespace winui_db
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            using var db = new Database();
            Manager.FillMedicine(db);
            await Server.GetInstance().StartAsync();
            
        }
    }
}
