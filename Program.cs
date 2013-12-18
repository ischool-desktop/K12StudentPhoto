using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA;
using FISCA.Presentation;
using FISCA.Permission;

namespace K12StudentPhoto
{
    public class Program
    {
        [MainMethod()]
        static public void Main()
        {
            RibbonBarButton rbItemExport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            rbItemExport.SupposeHasChildern = true;
            rbItemExport.Image = Properties.Resources.Export_Image;

            rbItemExport["學籍相關匯出"]["匯出學生照片"].Enable = Permissions.匯出學生照片權限_高中 || Permissions.匯出學生照片權限_國中;
            rbItemExport["學籍相關匯出"]["匯出學生照片"].Click += delegate
            {
                new PhotosBatchExportForm().ShowDialog();
            };

            RibbonBarButton rbItemImport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            rbItemImport.SupposeHasChildern = true;
            rbItemImport.Image = Properties.Resources.Import_Image;

            rbItemImport["學籍相關匯入"]["匯入學生照片"].Enable = Permissions.匯入學生照片權限_高中 || Permissions.匯入學生照片權限_國中;
            rbItemImport["學籍相關匯入"]["匯入學生照片"].Click += delegate
            {
                new PhotosBatchImportForm().ShowDialog();
            };


            Catalog detail1;
            detail1 = RoleAclSource.Instance["學生"]["功能按鈕"];
            detail1.Add(new RibbonFeature(Permissions.匯出學生照片_國中, "匯出學生照片"));
            detail1.Add(new RibbonFeature(Permissions.匯入學生照片_國中, "匯入學生照片"));
        }
    }
}
