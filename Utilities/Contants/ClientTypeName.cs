using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Contants
{
    public enum ClientTypekh
    {
        ALL = -1,
        AGENT = 1, // Đại lý
        TIER_1_AGENT = 2, // Đối tác chiến lược
        TIER_2_AGENT = 3, // Đại lý cấp 2
        TIER_3_AGENT = 4,// Đại lý cấp 3
        CUSTOMER = 5, //Khách lẻ
        SALE = 6, // nv kinh doanh
        ENTERPRISE = 7, // Doanh nghiệp
        COLLABORATORS = 8,// Cộng tác viên
        STAFF = 9,//Nhân viên trong công ty
        DTDB = 10//Nhân viên trong công ty

    }
    public class ClientTypeName
    {
        public static readonly Dictionary<Int16, string> service = new Dictionary<Int16, string>
        {
            {Convert.ToInt16(ClientTypekh.AGENT), "DL" },
            {Convert.ToInt16(ClientTypekh.TIER_1_AGENT), "CL" },
            {Convert.ToInt16(ClientTypekh.TIER_2_AGENT), "DL" },
            {Convert.ToInt16(ClientTypekh.TIER_3_AGENT), "DL" },
            {Convert.ToInt16(ClientTypekh.CUSTOMER), "KL" },
            {Convert.ToInt16(ClientTypekh.SALE), "SL" },
            {Convert.ToInt16(ClientTypekh.ENTERPRISE), "DN" },
            {Convert.ToInt16(ClientTypekh.COLLABORATORS), "CT" },
            {Convert.ToInt16(ClientTypekh.STAFF), "NV" },
            {Convert.ToInt16(ClientTypekh.DTDB), "CLDB" }
        };
    }
}
