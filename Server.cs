using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace winui_db;

public enum QueryTypes
    {
        EXIT,
        LOGIN,
        REGISTER,
        SEARCH,
        GET_CART,
        ADD_TO_CART,
        GET_BY_ID,
        DELETE_FROM_CART,
        SET_CART_ITEM_COUNT
}

public enum SentDataMessages
{
    SUCCESS,
    ERROR
}
class Server
{
    private static Server instance;
    private bool breakpoint = false;
    private TcpListener server;
    private string key;
    public static Server GetInstance()
    {
        if (instance == null)
            instance = new Server();
        return instance;
    }
    public async Task StartAsync()
    {
        try
        {
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, port);
            server.Start();
            Console.WriteLine("Server started. Waiting for connections...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine($"{((IPEndPoint)client.Client.RemoteEndPoint).Address}: connected");

                // Process each client on a separate task
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[102400];
            int bytesRead;
            
            while (!breakpoint && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (breakpoint)
                    break;
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: {0}", data);
                byte[] dataToSend = new byte[1];
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(data);
                int query_code = jsonObject.GetProperty("QueryType").GetInt32();
                var user_name = string.Empty;
                var user_login = string.Empty;
                var user_password = string.Empty;
                var category = string.Empty;
                int count = 0;
                int id = 0;
                switch (query_code)
                {
                    case (int)QueryTypes.EXIT:
                        Exit(client, stream);
                        break;
                    case (int)QueryTypes.LOGIN:
                        await LoginAsync(jsonObject, stream);
                        break;
                    case (int)QueryTypes.REGISTER:
                        await RegisterAsync(jsonObject, stream);
                        break;

                    case (int)QueryTypes.SEARCH:
                        await SearchAsync(jsonObject, stream);
                        break;
                    case (int)QueryTypes.GET_CART:
                        await GetCartAsync(stream);
                        break;
                    case (int)QueryTypes.GET_BY_ID:
                        await GetByIdAsync(jsonObject, stream);
                        break;

                    case (int)QueryTypes.ADD_TO_CART:
                        await AddToCartAsync(jsonObject, stream);
                        break;
                    case (int)QueryTypes.DELETE_FROM_CART:
                        await DeleteFromCartAsync(jsonObject, stream);
                        break;
                    case (int)QueryTypes.SET_CART_ITEM_COUNT:
                        await SetCartItemCountAsync(jsonObject, stream);
                        break;
                }
            }

            
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: {0}", ex);
        }

    }

    private async Task GetCartAsync(NetworkStream stream)
    {
        using (var database = new Database())
        {
            var c = await database.Users.Where(k => k.Email == key).FirstOrDefaultAsync();
            var cart = await database.ShoppingCarts.Include(sc => sc.View).Where(o => o.Id == c.Id).FirstOrDefaultAsync();

            var dataToSend = JsonSerializer.SerializeToUtf8Bytes<List<MedicineShoppingCartView>>(cart.View, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles });

            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
        }
    }

    private async Task LoginAsync(JsonElement jsonObject, NetworkStream stream)
    {
        if (!jsonObject.TryGetProperty("Email", out JsonElement login) || (!jsonObject.TryGetProperty("Password", out JsonElement password)))
            return;

        var user_login = login.GetString();
        var user_password = password.GetString();
        
        var user = await Manager.GetUserFromDatabaseAsync(user_login, user_password);
        
        byte[] dataToSend;
        if (user != null)
        {
            key = user_login;
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<User>(user, new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles });
        }
        else
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
        }
        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
    }

    private async Task RegisterAsync(JsonElement jsonObject, NetworkStream stream)
    {
        if (!jsonObject.TryGetProperty("Email", out JsonElement login2) || (!jsonObject.TryGetProperty("Password", out JsonElement password2)) || (!jsonObject.TryGetProperty("Name", out JsonElement name)))
            return;

        var user_name = name.GetString();
        var user_login = login2.GetString();
        var user_password = password2.GetString();
        
        var result = await Manager.TryAddUserAsync(user_name, user_login, user_password);

        byte[] dataToSend;
        if (result)
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.SUCCESS);
        }
        else
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
        }
        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
    }

    private async Task SearchAsync(JsonElement jsonObject, NetworkStream stream)
    {
        if (!jsonObject.TryGetProperty("Category", out JsonElement cat))
            return;

        var category = cat.GetString();
        
        var list = await Manager.GetByCategoryFromDatabaseAsync(category);
        
        byte[] dataToSend;
        if (list != null)
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<List<Medicine>>(list);
        }
        else
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
        }
        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
    }

    private async Task GetByIdAsync(JsonElement jsonObject, NetworkStream stream)
    {
        if (!jsonObject.TryGetProperty("Id", out JsonElement j_id))
            return;
        
        var id = j_id.GetInt32();
        
        var item = await Manager.GetByIdFromDatabaseAsync(id);
        
        byte[] dataToSend;
        if (item != null)
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<Medicine>(item);
        }
        else
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
        }
        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
    }

    private async Task DeleteFromCartAsync(JsonElement jsonObject, NetworkStream stream)
    {
        using (var database = new Database())
        {
            if (!jsonObject.TryGetProperty("Id", out JsonElement del_id))
                return;
            
            var id = del_id.GetInt32();
            
            var del_result = await Manager.TryDeleteFromCartAsync(id, await database.Users.Where(k => k.Email == key).FirstOrDefaultAsync());
            
            byte[] dataToSend;
            if (del_result)
            {
                dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.SUCCESS);
            }
            else
            {
                dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
            }
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
        }
    }

    private async Task SetCartItemCountAsync(JsonElement jsonObject, NetworkStream stream)
    {
        using (var database = new Database())
        {
            if (!jsonObject.TryGetProperty("Id", out JsonElement s_id) || !jsonObject.TryGetProperty("Count", out JsonElement s_count))
                return;
            
            var id = s_id.GetInt32();
            var count = s_count.GetInt32();
            
            var count_result = await Manager.TryEditCountAsync(id, count, await database.Users.Where(k => k.Email == key).FirstOrDefaultAsync());
            
            byte[] dataToSend;
            if (count_result)
            {
                dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.SUCCESS);
            }
            else
            {
                dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
            }
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
        }
        
    }

    private async Task AddToCartAsync(JsonElement jsonObject, NetworkStream stream)
    {
        if (!jsonObject.TryGetProperty("Id", out JsonElement js_id))
            return;
        
        var id = js_id.GetInt32();
        
        var add_result = await Manager.TryAddToCartAsync(id, key);
        
        byte[] dataToSend;
        if (add_result)
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.SUCCESS);
        }
        else
        {
            dataToSend = JsonSerializer.SerializeToUtf8Bytes<int>((int)SentDataMessages.ERROR);
        }
        await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
    }
    private void Exit(TcpClient client, NetworkStream stream)
    {
        breakpoint = true;
        client.Close();
        stream.Close();
        server.Stop();
    }
}
