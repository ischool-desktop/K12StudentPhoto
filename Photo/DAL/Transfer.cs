using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using K12.Data;
using K12.Data.Utility;
using FISCA.Data;
using System.Data;

namespace K12StudentPhoto
{
    public class Transfer
    {
        /// <summary>
        /// 匯入照片
        /// </summary>
        public static void ImportPhotos(List<StudPhotoEntity> StudPhotoEntityList)
        {
            if (StudPhotoEntityList.Count > 0)
            {
                BackgroundWorker bgImportWorker = new BackgroundWorker();
                bgImportWorker.DoWork += new DoWorkEventHandler(bgImportWorker_DoWork);
                bgImportWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgImportWorker_RunWorkerCompleted);
                bgImportWorker.RunWorkerAsync(StudPhotoEntityList);
            }
            //EditStudent.UpdatePhoto();
        }



        static void bgImportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("匯入完成");
        }

        static void bgImportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<StudPhotoEntity> StudPhotoEntityList = (List<StudPhotoEntity>)e.Argument;
            DSXmlHelper xmlHelper = new DSXmlHelper("Request");
            foreach (StudPhotoEntity spe in StudPhotoEntityList)
            {
                string b64 = string.Empty;
                try
                {
                    Bitmap pic = Photo.Resize(spe.PhotoFileInfo);
                    b64 = Photo.GetBase64Encoding(pic);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("B64!" + spe.GetPhotoFullName() + "轉換失敗!");
                    return;
                }
                xmlHelper.AddElement("Student");
                if (spe._PhotoKind == StudPhotoEntity.PhotoKind.入學)
                    xmlHelper.AddElement("Student", "FreshmanPhoto", b64);
                if (spe._PhotoKind == StudPhotoEntity.PhotoKind.畢業)
                    xmlHelper.AddElement("Student", "GraduatePhoto", b64);

                if (spe._PhotoNameRule == StudPhotoEntity.PhotoNameRule.身分證號)
                    xmlHelper.AddElement("Student", "IDNumber", spe.GetPhotoName());

                if (spe._PhotoNameRule == StudPhotoEntity.PhotoNameRule.學號)
                    xmlHelper.AddElement("Student", "StudentNumber", spe.GetPhotoName());

                // 班級座號轉成身分證方
                if (spe._PhotoNameRule == StudPhotoEntity.PhotoNameRule.班級座號)
                {
                    // 先用身分證
                    if (!string.IsNullOrEmpty(spe.StudentIDNumber))
                        xmlHelper.AddElement("Student", "IDNumber", spe.StudentIDNumber);
                    else
                    {
                        // 用學號
                        if (!string.IsNullOrEmpty(spe.StudentNumber))
                            xmlHelper.AddElement("Student", "StudentNumber", spe.StudentNumber);

                    }
                }

                // 會考格式轉成身分證方
                if (spe._PhotoNameRule == StudPhotoEntity.PhotoNameRule.會考格式)
                {
                    // 先用身分證
                    if (!string.IsNullOrEmpty(spe.StudentIDNumber))
                        xmlHelper.AddElement("Student", "IDNumber", spe.StudentIDNumber);
                    else
                    {
                        // 用學號
                        if (!string.IsNullOrEmpty(spe.StudentNumber))
                            xmlHelper.AddElement("Student", "StudentNumber", spe.StudentNumber);

                    }
                }
            }

            try
            {
                DSAServices.CallService("SmartSchool.Student.UpdatePhoto", new DSRequest(xmlHelper.BaseElement));

                //EditStudent.UpdatePhoto(new DSRequest(xmlHelper.BaseElement));
            }
            catch (Exception ex)
            {
                MessageBox.Show("上傳照片發生錯誤!");
                // 待寫
                return;
            }



        }


        /// <summary>
        /// 匯出照片
        /// </summary>
        public static void ExportPhotos()
        {

        }



        /// <summary>
        /// 設定學生基本資訊
        /// </summary>
        /// <param name="StudPhtoEntityList"></param>
        /// <param name="_PhotoNameRule"></param>
        public static List<StudPhotoEntity> SetStudBaseInfo(StudPhotoEntity.PhotoNameRule PhotoNameRule, List<StudPhotoEntity> StudPhtoEntityList)
        {
            //Student.Instance.SyncAllBackground();
            Dictionary<string, StudentRecord> StudIdx = new Dictionary<string, StudentRecord>();

            List<StudentRecord> Students = Student.SelectAll();

            //學號
            if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.學號)
            {
                foreach (StudentRecord studRec in Students)
                    if (!StudIdx.ContainsKey(studRec.StudentNumber))
                        StudIdx.Add(studRec.StudentNumber, studRec);
            }

            // 身分證
            if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.身分證號)
            {
                foreach (StudentRecord studRec in Students)
                    if (!StudIdx.ContainsKey(studRec.IDNumber))
                        StudIdx.Add(studRec.IDNumber, studRec);
            }

            // 班座
            if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.班級座號)
            {
                foreach (StudentRecord studRec in Students)
                {
                    //2018/2/9，穎驊註記， 高雄反映，有學校照片匯入會有錯誤訊息的狀況，
                    //經檢查過後，發現其會抓到已刪除的舊學生資料，導致能作為上傳照片key值的 學號、身分字號可能對不上
                    //導致上傳失敗，在此增加一條件，僅有一般生才會加入對照表
                    if (studRec.Class != null && studRec.Status == StudentRecord.StudentStatus.一般)
                    {
                        string key = studRec.Class.Name.Trim() +"_"+ K12.Data.Int.GetString(studRec.SeatNo);
                        if (!StudIdx.ContainsKey(key))
                            StudIdx.Add(key, studRec);
                    }
                }
            }

            // 會考格式
            if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.會考格式)
            {
                foreach (StudentRecord studRec in Students)
                {
                    //2018/2/9，穎驊註記， 高雄反映，有學校照片匯入會有錯誤訊息的狀況，
                    //經檢查過後，發現其會抓到已刪除的舊學生資料，導致能作為上傳照片key值的 學號、身分字號可能對不上
                    //導致上傳失敗，在此增加一條件，僅有一般生才會加入對照表
                    if (studRec.Class != null && studRec.Status == StudentRecord.StudentStatus.一般)
                    {
                        string key = studRec.Class.Name.Trim() + "_"+K12.Data.Int.GetString(studRec.SeatNo);
                        if (!StudIdx.ContainsKey(key))
                            StudIdx.Add(key, studRec);
                    }
                }
            }


            if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.身分證號 || PhotoNameRule == StudPhotoEntity.PhotoNameRule.班級座號 || PhotoNameRule == StudPhotoEntity.PhotoNameRule.學號)
            {
                // 有符合以上3類填入值
                foreach (StudPhotoEntity spe in StudPhtoEntityList)
                {
                    string PhotoName = spe.GetPhotoName();
                    if (StudIdx.ContainsKey(PhotoName))
                    {
                        if (StudIdx[PhotoName].Class != null)
                            spe.ClassName = StudIdx[PhotoName].Class.Name;
                        spe.SeatNo = K12.Data.Int.GetString(StudIdx[PhotoName].SeatNo);
                        spe.StudentID = StudIdx[PhotoName].ID;
                        spe.StudentIDNumber = StudIdx[PhotoName].IDNumber;
                        spe.StudentName = StudIdx[PhotoName].Name;
                        spe.StudentNumber = StudIdx[PhotoName].StudentNumber;
                    }
                }
            }

            //2016/10/5 穎驊筆記，新增會考格式照片匯入，其與上面三個方法最大的不同是PhotoName 不使用 spe.GetPhotoName，而是去抓SQL 對照
            if (PhotoNameRule == StudPhotoEntity.PhotoNameRule.會考格式)
            {

                #region 整理會考班級名稱 與班級名稱對照表
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

                foreach (StudPhotoEntity spe in StudPhtoEntityList)
                {
                    string PhotoName = "";

                    string FolderclassName = "";

                    if (spe.PhotoFileInfo != null)
                    {

                        if (ClassNo_to_ClassName.ContainsKey(spe.PhotoFileInfo.Directory.Name.Substring(0, 3)))
                        {
                            FolderclassName = ClassNo_to_ClassName[spe.PhotoFileInfo.Directory.Name.Substring(0, 3)];
                        }

                        if (spe.PhotoFileInfo.Name.Substring(spe.PhotoFileInfo.Name.Length - spe.PhotoFileInfo.Extension.Length - 2, 1) == "0")
                        {
                            PhotoName = FolderclassName + "_"+spe.PhotoFileInfo.Name.Substring(spe.PhotoFileInfo.Name.Length - spe.PhotoFileInfo.Extension.Length-1, 1);
                        }
                        else
                        {
                            PhotoName = FolderclassName + "_" + spe.PhotoFileInfo.Name.Substring(spe.PhotoFileInfo.Name.Length - spe.PhotoFileInfo.Extension.Length - 2, 2);
                        }
                    }

                    if (StudIdx.ContainsKey(PhotoName))
                    {
                        if (StudIdx[PhotoName].Class != null)
                            spe.ClassName = StudIdx[PhotoName].Class.Name;
                        spe.SeatNo = K12.Data.Int.GetString(StudIdx[PhotoName].SeatNo);
                        spe.StudentID = StudIdx[PhotoName].ID;
                        spe.StudentIDNumber = StudIdx[PhotoName].IDNumber;
                        spe.StudentName = StudIdx[PhotoName].Name;
                        spe.StudentNumber = StudIdx[PhotoName].StudentNumber;
                    }
                }

            }

            return StudPhtoEntityList;
        }

        /// <summary>
        /// 取得詳細資料列表
        /// </summary>
        /// <param name="id">學生編號</param>
        /// <returns></returns>
        [FISCA.Authentication.AutoRetryOnWebException()]
        private static DSResponse GetDetailList(IEnumerable<string> fields, params string[] list)
        {
            DSRequest dsreq = new DSRequest();
            DSXmlHelper helper = new DSXmlHelper("GetStudentListRequest");
            helper.AddElement("Field");
            bool hasfield = false;
            foreach (string field in fields)
            {
                helper.AddElement("Field", field);
                hasfield = true;
            }
            if (!hasfield)
                throw new Exception("必須傳入Field");
            helper.AddElement("Condition");
            foreach (string id in list)
            {
                helper.AddElement("Condition", "ID", id);
            }
            dsreq.SetContent(helper);
            return K12.Data.Utility.DSAServices.CallService("SmartSchool.Student.GetDetailList", dsreq);
        }

        public static List<StudPhotoEntity> GetStudentPhotoBitmap(List<StudPhotoEntity> StudPhotoEntityList)
        {
            List<string> StudentIDList = new List<string>();
            foreach (StudPhotoEntity spe in StudPhotoEntityList)
                if (!string.IsNullOrEmpty(spe.StudentID))
                    StudentIDList.Add(spe.StudentID);

            DSXmlHelper xmlHelper = new DSXmlHelper("Request");

            DSResponse DSRsp = GetDetailList(new string[] { "ID", "FreshmanPhoto", "GraduatePhoto" }, StudentIDList.ToArray());

            Dictionary<string, string> FreshmanPhotoStr = new Dictionary<string, string>();
            Dictionary<string, string> GraduatePhotoStr = new Dictionary<string, string>();

            if (DSRsp != null)
                foreach (XmlElement elm in DSRsp.GetContent().BaseElement.SelectNodes("Student"))
                {
                    if (!FreshmanPhotoStr.ContainsKey(elm.GetAttribute("ID")))
                    {
                        if (!string.IsNullOrEmpty(elm.SelectSingleNode("FreshmanPhoto").InnerText))
                            FreshmanPhotoStr.Add(elm.GetAttribute("ID"), elm.SelectSingleNode("FreshmanPhoto").InnerText);
                    }
                    if (!GraduatePhotoStr.ContainsKey(elm.GetAttribute("ID")))
                    {
                        if (!string.IsNullOrEmpty(elm.SelectSingleNode("GraduatePhoto").InnerText))
                            GraduatePhotoStr.Add(elm.GetAttribute("ID"), elm.SelectSingleNode("GraduatePhoto").InnerText);
                    }

                }

            foreach (StudPhotoEntity spe in StudPhotoEntityList)
            {
                if (spe._PhotoKind == StudPhotoEntity.PhotoKind.入學)
                {
                    if (FreshmanPhotoStr.ContainsKey(spe.StudentID))
                    {
                        spe.FreshmanPhotoBitmap = Photo.ConvertFromBase64Encoding(FreshmanPhotoStr[spe.StudentID], true);
                    }
                }

                if (spe._PhotoKind == StudPhotoEntity.PhotoKind.畢業)
                {
                    if (GraduatePhotoStr.ContainsKey(spe.StudentID))
                    {
                        spe.GraduatePhotoBitmap = Photo.ConvertFromBase64Encoding(GraduatePhotoStr[spe.StudentID], true);
                    }

                }
            }

            return StudPhotoEntityList;
        }
    }
}
