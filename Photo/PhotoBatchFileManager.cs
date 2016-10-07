using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using K12.Data;
using FISCA.Data;
using System.Data;

namespace K12StudentPhoto
{
    class PhotoBatchFileManager
    {
        private string _FilePath;
        private List<FileInfo> _Files;
        private Dictionary<DirectoryInfo, List<FileInfo>> _DefaultFolderAndFilesInfo;
        private Dictionary<string, List<string>> _DefaultFolederAndFilesName;
        private Dictionary<string, List<int>> _DefaultFolederAndFilesNameForClassNameSeatNo;

        /// <summary>
        /// 取得檔案路徑
        /// </summary>
        /// <returns></returns>
        public string GetFilePath()
        {
            return _FilePath;
        }

        /// <summary>
        /// 設定檔案路徑
        /// </summary>
        /// <param name="pathName"></param>
        public void SetFilePath(string pathName)
        {
            _FilePath = pathName;
        }

        public PhotoBatchFileManager()
        {
            _Files = new List<FileInfo>();
            _DefaultFolderAndFilesInfo = new Dictionary<DirectoryInfo, List<FileInfo>>();
            _DefaultFolederAndFilesName = new Dictionary<string, List<string>>();
            _DefaultFolederAndFilesNameForClassNameSeatNo = new Dictionary<string, List<int>>();
        }

        public void SaveFiles(Dictionary<string, Bitmap> SaveFileDic)
        {
            foreach (KeyValuePair<string, Bitmap> file in SaveFileDic)
            {
                    file.Value.Save(file.Key, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

        }

        /// <summary>
        /// 取得目前資料夾內的完整資料夾與檔案
        /// </summary>
        /// <returns></returns>
        public Dictionary<DirectoryInfo, List<FileInfo>> GetCurrentFullFoldersAndFilesInfo()
        {
            return _DefaultFolderAndFilesInfo;
        }

        /// <summary>
        /// 設定目前資料夾內的完整資料夾與檔案
        /// </summary>
        /// <param name="FolderPath"></param>
        public bool SetCurrentFullFoldersAndFilesInfo(string FolderPath,StudPhotoEntity.PhotoNameRule PhotoNameRule)
        {
            bool checkSetPass = false;
            _DefaultFolderAndFilesInfo.Clear();
            _DefaultFolederAndFilesName.Clear();
            _DefaultFolederAndFilesNameForClassNameSeatNo.Clear();


            #region 整理會考班級名稱 與班級名稱對照表

            List<StudentRecord> Students = Student.SelectAll();

            List<string> Stu_ids = new List<string>();

            Dictionary<string, string> ClassNo_to_ClassName = new Dictionary<string, string>();

            foreach (var Stu in Students)
            {
                Stu_ids.Add(Stu.ID);

            }

            QueryHelper _Q = new QueryHelper();
            DataTable dt = _Q.Select(string.Format(@"
SELECT 
	student.id, 
	student.seat_no,
	class.class_name,
	class.grade_year,
	class.display_order,
	class.class_No,
	CASE class.class_no  is null WHEN  true    THEN '999'  ELSE  class.class_no END || 	lpad( student.seat_no::text,2,'0' ) AS NewStudentPhotoName
FROM 
	student
	LEFT OUTER JOIN (
		SELECT 
			class.id,
			class_name, 
			grade_year, 
			display_order, 
			CASE grade_year WHEN 1 THEN 7 WHEN 2 THEN 8 WHEN 3 THEN 9 ELSE grade_year END::text||lpad(CASE display_order is null WHEN TRUE THEN (rank() over (PARTITION BY grade_year ORDER BY class_name)) ELSE display_order END::text, 2, '0') as class_No
		FROM class
		ORDER BY grade_year, display_order, class_name
	) as class ON class.id = student.ref_class_id
WHERE
    student.id in ({0});", string.Join(",", Stu_ids)));

            foreach (DataRow row in dt.Rows)
            {
                if (!ClassNo_to_ClassName.ContainsKey("" + row["class_no"]))
                {
                    ClassNo_to_ClassName.Add("" + row["class_no"], "" + row["class_name"]);
                }

            }
            #endregion


            if (checkHasFolder(FolderPath))
            {

                DirectoryInfo dirInfo = new DirectoryInfo(FolderPath);
                DirectoryInfo[] dicInfoArray = dirInfo.GetDirectories();
                foreach (DirectoryInfo di in dicInfoArray)
                {
                    List<int> IntFileNameList = new List<int>();
                    List<string> filesName = new List<string>();
                    List<FileInfo> FilesInfo = new List<FileInfo>();
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        int num = 0;
                        filesName.Add(fi.Name);
                        FilesInfo.Add(fi);

                        if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.班級座號) 
                        {
                            int.TryParse(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length), out num);
                        }
                        if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.會考格式)
                        {
                            //int.TryParse(fi.Name.Substring(2,2), out num);
                           //固定抓後兩碼
                            int.TryParse(fi.Name.Substring(fi.Name.Length - fi.Extension.Length - 2, 2), out num);
                        }


                        IntFileNameList.Add(num);
                    }

                    if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.班級座號)
                    {
                        _DefaultFolderAndFilesInfo.Add(di, FilesInfo);
                        _DefaultFolederAndFilesName.Add(di.Name, filesName);
                        _DefaultFolederAndFilesNameForClassNameSeatNo.Add(di.Name, IntFileNameList);
                    }
                    
                    if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.會考格式)
                    {
                        string di_name = "";


                        if (ClassNo_to_ClassName.ContainsKey(di.Name))
                        {
                            di_name = ClassNo_to_ClassName[di.Name];
                        }

                        _DefaultFolderAndFilesInfo.Add(di, FilesInfo);
                        _DefaultFolederAndFilesName.Add(di_name, filesName);
                        _DefaultFolederAndFilesNameForClassNameSeatNo.Add(di_name, IntFileNameList);
                    }

            
                }
                checkSetPass = true;
            }
            return checkSetPass;
        }

        /// <summary>
        /// 檢查資料夾名稱與檔名是否存在目前目錄內
        /// </summary>
        /// <param name="FolderName"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool CheckFolderAndFileInCurrent(string FolderName, string FileName)
        {
            bool returnValue = false, check1 = false, check2 = false;

            if (!string.IsNullOrEmpty(FolderName))
                if (_DefaultFolederAndFilesName.ContainsKey(FolderName))
                {
                    check1 = true;
                    if (!string.IsNullOrEmpty(FileName))
                        if (_DefaultFolederAndFilesName[FolderName].Contains(FileName))
                        {
                            check2 = true;
                        }
                }
            if (check1 == true && check2 == true)
                returnValue = true;

            return returnValue;
        }

        /// <summary>
        /// 檢查資料夾名稱與檔名是否存在目前目錄內(班級座號檢查用)
        /// </summary>
        /// <param name="FolderName"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public bool CheckFolderAndFileInCurrentForClassNameSeatNo(string ClassName, string SeatNo)
        {
            bool returnValue = false, check1 = false, check2 = false;
            if (!string.IsNullOrEmpty(ClassName))
                if (_DefaultFolederAndFilesNameForClassNameSeatNo.ContainsKey(ClassName))
                {
                    check1 = true;
                    int IntSeatNo = 0;
                    int.TryParse(SeatNo, out IntSeatNo);
                    if (!string.IsNullOrEmpty(SeatNo))
                        if (_DefaultFolederAndFilesNameForClassNameSeatNo[ClassName].Contains(IntSeatNo))
                        {
                            check2 = true;
                        }
                }
            if (check1 == true && check2 == true)
                returnValue = true;

            return returnValue;
        }


        /// <summary>
        /// 取得檔案資訊
        /// </summary>
        /// <returns></returns>
        public List<FileInfo> GetFiles()
        {
            _Files.Clear();

            if (string.IsNullOrEmpty(_FilePath.Trim()))
            {
                MessageBox.Show("請選擇存放照片的資料夾。");
                return null;
            }

            DirectoryInfo dirInfo;
            try
            {
                dirInfo = new DirectoryInfo(_FilePath);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

            if (!dirInfo.Exists)
            {
                MessageBox.Show("資料夾不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            FileInfo[] file1 = dirInfo.GetFiles("*.jpg", SearchOption.AllDirectories);
            FileInfo[] file2 = dirInfo.GetFiles("*.jpeg", SearchOption.AllDirectories);

            _Files.AddRange(file1);
            _Files.AddRange(file2);

            return _Files;
        }

        /// <summary>
        /// 檢查目錄是否存在
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public bool checkHasFolder(string folderPath)
        {
            bool check = true;

            DirectoryInfo dirInfo;
            try
            {
                dirInfo = new DirectoryInfo(folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            if (!dirInfo.Exists)
                check = false;
            return check;
        }

        /// <summary>
        /// 建立目錄,目錄已存在不建立
        /// </summary>
        /// <param name="FolderPath"></param>
        public void CreateFolder(string FolderPath)
        {
            // 當目錄不存在
            if (checkHasFolder(FolderPath) == false)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(FolderPath);
                dirInfo.Create();
            }
        }

        /// <summary>
        /// 透過 Dialog 取得所選的檔案路徑
        /// </summary>
        /// <returns></returns>
        public string GetFilefoldrBrowserDialog()
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() != DialogResult.OK)
                _FilePath = string.Empty;
            else
                _FilePath = folderBrowser.SelectedPath;
            return _FilePath;
        }

        /// <summary>
        /// 取得檔案資訊
        /// </summary>
        /// <returns></returns>
        public List<FileInfo> GetFileInfo()
        {
            List<FileInfo> FileInfos = new List<FileInfo>();
            if (_Files.Count > 0)
                FileInfos = _Files;
            else
                FileInfos = GetFiles();

            return FileInfos;
        }

        /// <summary>
        /// 檢查是否有同名
        /// </summary>
        /// <returns></returns>
        public bool checkHasSameName()
        {
            bool check = false;
            List<string> fileName1 = new List<string>();
            foreach (FileInfo fi in GetFileInfo())
            {
                fileName1.Add(fi.Name);
            }


            string fName = "";
            fileName1.Sort();
            foreach (string str in fileName1)
            {
                if (str == fName)
                {
                    check = true;
                    break;
                }
                fName = str;
            }
            return check;
        }
    }
}
