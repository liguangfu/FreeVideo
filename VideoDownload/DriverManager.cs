using CocoaAniCore.Downloaders.Enums;
using VideoDownload.Drivers;
using VideoDownload.Drivers.Base;

namespace VideoDownload
{
    public class DriverManager
    {
        private static readonly Dictionary<string, IHttpFileDownloadDriver> DriverDictionary = GetDriverDictionary();

        public static IHttpFileDownloadDriver? GetDriver(string driverName)
        {
            return DriverDictionary.ContainsKey(driverName) ? DriverDictionary[driverName] : null;
        }

        public static IHttpFileDownloadDriver? GetDriver(DownloadFileType fileType)
        {
            return DriverDictionary.ContainsKey(fileType.ToString()) ? DriverDictionary[fileType.ToString()] : null;
        }

        public static bool RegisterDriver(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return false;
        }

        private static Dictionary<string, IHttpFileDownloadDriver> GetDriverDictionary()
        {
            var dictionary = new Dictionary<string, IHttpFileDownloadDriver>();
            //var assembly = typeof(IHttpFileDownloadDriver).Assembly;
            //var drivers = assembly?.GetTypes();
            //if (drivers == null) return dictionary;
            //foreach (var type in drivers)
            //{
            //    if (type.GetInterfaces().Length <= 0 || type.IsAbstract) continue;
            //    //type.GetInterfaces()[0]获取到直接实现的接口判断是否所需要的类型
            //    //当实现多个接口时需要根据具体的情况获取
            //    if (type.GetInterface(nameof(IHttpFileDownloadDriver)) == null) continue;
            //    //Activator创建实例并强制类型转换赋值
            //    var drive = (IHttpFileDownloadDriver)Activator.CreateInstance(type)!;
            //    dictionary.Add(drive., drive);
            //}
            dictionary["Http"]=HttpDownloadDriver.Instance;
            dictionary["M3U8"] = M3U8DownloadDriver.Instance;
            return dictionary;
        }

    }
}