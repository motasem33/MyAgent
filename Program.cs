using System.Net;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;

// 1. إعدادات البرنامج - استبدل 192.168.1.XX بـ IP جهازك (السيرفر) الذي جلبته من ipconfig
// واستخدم http وبورت 5000 كما اتفقنا لتجنب مشاكل شهادات الأمان SSL
string serverUrl = "http:// 192.168.100.54:5000/api/Devices/update-status";

string deviceName = Environment.MachineName;
string macAddress = GetMacAddress();
string localIp = GetLocalIPAddress(); // جلب الـ IP الحقيقي للجهاز

Console.WriteLine($"--- PulseNet Agent Started ---");
Console.WriteLine($"Device: {deviceName}");
Console.WriteLine($"IP: {localIp}");
Console.WriteLine($"MAC: {macAddress}");
Console.WriteLine("-------------------------------");

// 2. إرسال إشارة "تسجيل دخول" عند تشغيل البرنامج
await SendUpdate(true);

// 3. حلقة تكرارية لإرسال "نبضات قلب" (Heartbeat) كل دقيقة
while (true)
{
    await Task.Delay(60000); // انتظر 60 ثانية
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending heartbeat...");
    await SendUpdate(true);
}

// دالة إرسال البيانات للسيرفر
async Task SendUpdate(bool isOnline)
{
    using var client = new HttpClient();
    var deviceData = new
    {
        Name = deviceName,
        MacAddress = macAddress,
        IPAddress = localIp, // إرسال الـ IP الحقيقي
        Department = "IT Section",
        IsOnline = isOnline
    };

    try
    {
        var response = await client.PostAsJsonAsync(serverUrl, deviceData);
        if (response.IsSuccessStatusCode)
            Console.WriteLine("Successfully reported to server.");
        else
            Console.WriteLine($"Server responded with: {response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not reach server. Make sure the API is running and IP is correct.");
    }
}

// دالة لجلب الماك أدرس
string GetMacAddress()
{
    return NetworkInterface.GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault() ?? "000000000000";
}

// دالة لجلب الـ IP الحقيقي للجهاز الحالي تلقائياً
string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    return "127.0.0.1";
}