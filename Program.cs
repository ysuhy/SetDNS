using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SetDNS
{
    class Program
    {


        public static List<DNSInfo> DNSs = new List<DNSInfo>();
        static void Main(string[] args)
        {

            var s = "114.114.114.114;114.114.115.115;223.5.5.5;223.6.6.6;180.76.76.76;119.29.29.29;182.254.116.116;1.2.4.8;210.2.4.8;112.124.47.27;114.215.126.16;101.226.4.6;218.30.118.6;123.125.81.6;140.207.198.6;8.8.8.8;8.8.4.4;208.67.222.222;208.67.220.220;199.91.73.222;178.79.131.110";
            s += ";221.11.1.67;221.11.1.68";//陕西联通

            var ds = s.Split(';');
            foreach (var item in ds)
            {
                DNSs.Add(new DNSInfo() { DNS = item });
            }
            for (int index = 0; index < DNSs.Count; index++)
            {
                GetPing(DNSs[index]);
            }
            DNSs.Sort((a, b) => a.Ping.CompareTo(b.Ping));
            var list = DNSs.Where(a => a.Ping < 70).Select(a => a.DNS).ToArray<string>(); 
            SetDNS(list);
        }


        public static void SetDNS(string[] dns)
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            foreach (ManagementObject mo in moc)
            {
                //如果没有启用IP设置的网络设备则跳过
                if (!(bool)mo["IPEnabled"])
                    continue;
                //设置DNS地址
                if (dns != null)
                {
                    inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    inPar["DNSServerSearchOrder"] = dns;
                    outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                }
            }
        }

        public static void GetPing(DNSInfo dns)
        {

            List<long> result = new List<long>();
            for (int i = 0; i <3; i++)
            {
                var p = Ping(dns.DNS);
                result.Add(p);
            }
            result.Sort((a, b) => a.CompareTo(b));
            dns.Ping = result[0];
        }

        public static long Ping(string ip)
        {
            try
            {
                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
                options.DontFragment = true;
                string data = "Test Data!";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 70; // Timeout 时间，单位：毫秒
                System.Net.NetworkInformation.PingReply reply = p.Send(ip, timeout, buffer, options);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return reply.RoundtripTime;
            }
            catch
            {

            }
            return 999999;
        }


    }
}