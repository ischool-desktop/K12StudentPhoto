using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12StudentPhoto
{
    class Permissions
    {
        //國中
        //JHSchool.Student.Ribbon0120 - 匯出照片
        //JHSchool.Student.Ribbon0130 - 匯入照片

        //高中
        //Button0290 - 匯入學生照片
        //Button0290.5 - 匯出學生照片

        public static string 匯出學生照片_高中 { get { return "Button0290.5"; } }
        public static bool 匯出學生照片權限_高中
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出學生照片_高中].Executable;
            }
        }

        public static string 匯入學生照片_高中 { get { return "Button0290"; } }
        public static bool 匯入學生照片權限_高中
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入學生照片_高中].Executable;
            }
        }

        public static string 匯出學生照片_國中 { get { return "JHSchool.Student.Ribbon0120"; } }
        public static bool 匯出學生照片權限_國中
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出學生照片_國中].Executable;
            }
        }

        public static string 匯入學生照片_國中 { get { return "JHSchool.Student.Ribbon0130"; } }
        public static bool 匯入學生照片權限_國中
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入學生照片_國中].Executable;
            }
        }
    }
}
