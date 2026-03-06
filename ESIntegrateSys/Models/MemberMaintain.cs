using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ESIntegrateSys.ViewModels;

namespace ESIntegrateSys.Models
{
    public class MemberMaintain
    {
        private readonly ESIntegrateSysEntities db;
        public MemberMaintain(ESIntegrateSysEntities dbContext)
        {
            db = dbContext;
        }
        public string dEpt;
        public class MemberResult
        {
            public bool ErrMsg { get; set; }
            public string fUserId { get; set; }            
            public MemberCreateViewModels Member { get; set; }
        }
        public List<MemberViewModels> ESI_Member()
        {
            using (var context = new ESIntegrateSysEntities())
            {

                var query = from a in context.ES_Member
                            join b in context.ES_MemberRole on a.fUserId equals b.USER_ID into roleJoin
                            from c in roleJoin.DefaultIfEmpty()
                            join d in context.ES_RoleClassification on c.ROLE_ID equals d.ROLE_ID into roleDescJoin
                            from e in roleDescJoin.DefaultIfEmpty()
                            where dEpt == "IT" || 
                            (dEpt == "QE" && (a.Dept_No =="S" || a.Dept_No == "QE")) ||
                            (dEpt == "IE" && (a.Dept_No == "SD" || a.Dept_No == "IE"))
                            orderby a.fUserId
                            select new MemberViewModels
                            {
                                fId = a.fId,
                                fUserId = a.fUserId,
                                fName = a.fName,
                                ROLE_DESC = e != null ? e.ROLE_DESC : null,
                                ROLE_ID = c != null ? c.ROLE_ID : null,
                                fStatus = (bool)a.fStatus ? "啟用" : "停用"
                            };
                List<MemberViewModels> models = query.ToList();
                return models;
            }
        }

        //編輯畫面
        public List<MemberViewModels> Edit(int fId)
        {
            using (var context = new ESIntegrateSysEntities())
            {
                var query = from a in context.ES_Member
                            join b in context.ES_MemberRole on a.fUserId equals b.USER_ID
                            join c in context.ES_RoleClassification on b.ROLE_ID equals c.ROLE_ID
                            join e in context.ES_MemberFunction on a.fId equals e.UserNo_sno
                            join d in context.ES_FunctionItem on e.FunctionNo equals d.FunctionNo
                            where a.fId == fId
                            orderby a.fUserId
                            select new MemberViewModels
                            {
                                fId = a.fId,
                                fUserId = a.fUserId,
                                fName = a.fName,
                                ROLE_DESC = c != null ? c.ROLE_DESC : null,
                                ROLE_ID = b != null ? b.ROLE_ID : null,
                                Func=d.FunctionNo
                            };
                List<MemberViewModels> models = query.ToList();
                return models;
            }
        }
        //進入編輯程序
        public void Edit(string fUserId, string fName, string ROLE_ID, List<string> fItem)
        {
            //改名字
            var member = db.ES_Member.Where(m => m.fUserId == fUserId).FirstOrDefault();
            //member.fUserId = fUserId;
            member.fName = fName;
            db.SaveChanges();

            //改人員權限
            var member_Role = db.ES_MemberRole.Where(r => r.USER_ID == fUserId).FirstOrDefault();
            member_Role.USER_ID = fUserId;
            member_Role.ROLE_ID = ROLE_ID;
            member_Role.EXPIRED_DATE = DateTime.Now;
            db.SaveChanges();

            //改人員權限
            int m_sno = db.ES_Member
               .Where(n => n.fUserId == fUserId).Select(n => n.fId).SingleOrDefault();            
            if (m_sno != 0) // 檢查是否有找到符合的 fUserId
            {

                var _entity = db.ES_MemberFunction.Where(f => f.UserNo_sno == m_sno).ToList();
                db.ES_MemberFunction.RemoveRange(_entity);

                List<ES_MemberFunction> memberFuncList = new List<ES_MemberFunction>();

                foreach (var item in fItem)
                {
                    var memberFunc = new ES_MemberFunction
                    {
                        UserNo_sno = m_sno,
                        FunctionNo = item
                    };
                    memberFuncList.Add(memberFunc);
                }

                db.ES_MemberFunction.AddRange(memberFuncList); // 批量新增
                db.SaveChanges();
            }
            else
            {
                // 處理找不到 fUserId 的情況
                throw new Exception("找不到對應的 fUserId");
            }
        }

        //變更密碼畫面
        public List<MemberViewModels> Password(int fId)
        {
            using (var context = new ESIntegrateSysEntities())
            {
                var query = from a in context.ES_Member
                            join b in context.ES_MemberRole on a.fUserId equals b.USER_ID into roleJoin
                            from c in roleJoin.DefaultIfEmpty()
                            join d in context.ES_RoleClassification on c.ROLE_ID equals d.ROLE_ID into roleDescJoin
                            from e in roleDescJoin.DefaultIfEmpty()
                            where a.fId == fId
                            orderby a.fUserId
                            select new MemberViewModels
                            {
                                fPwd = a.fPwd,
                                fId = a.fId,
                                fUserId = a.fUserId,
                                fName = a.fName,
                            };
                List<MemberViewModels> models = query.ToList();
                return models;
            }
        }
        //進入密碼變更程序
        public bool Password(int fId, string fPwd, string fNPwd)
        {
            var result = string.CompareOrdinal(fPwd, fNPwd);
            if (result != 0)
            {
                return false;
            }
            else
            {
                var member = db.ES_Member.Where(m => m.fId == fId).FirstOrDefault();
                //member.fUserId = fUserId;
                member.fPwd = fNPwd;
                db.SaveChanges();
                return true;
            }
        }

        //人員新增畫面沒有, 直接進入新增人員程序
        public MemberResult Create(string fUserId, string fName, string ROLE_ID,string email, List<string> funcItem)
        {
            //檢查是否重複
            var repeat = db.ES_Member.Where(u => u.fUserId == fUserId).FirstOrDefault();
            MemberCreateViewModels result = new MemberCreateViewModels();
            if (repeat != null)
            {
                return new MemberResult { ErrMsg = false };
            }
            else
            {
                //人員名單
                ES_Member member = new ES_Member();
                member.fUserId = fUserId;
                member.fPwd = fUserId;
                member.fName = fName;
                member.fStatus = true;
                member.email = email;
                member.classRoom = "D";
                db.ES_Member.Add(member);
                db.SaveChanges();

                //人員權限名單
                ES_MemberRole member_Role = new ES_MemberRole();
                member_Role.USER_ID = fUserId;
                member_Role.ROLE_ID = ROLE_ID;
                member_Role.EXPIRED_DATE = DateTime.Now;
                db.ES_MemberRole.Add(member_Role);
                db.SaveChanges();

                int m_sno = db.ES_Member
               .Where(n => n.fUserId == fUserId).Select(n => n.fId).SingleOrDefault();
                if (m_sno != 0) // 檢查是否有找到符合的 fUserId
                {
                    List<ES_MemberFunction> memberFuncList = new List<ES_MemberFunction>();

                    foreach (var item in funcItem)
                    {
                        var memberFunc = new ES_MemberFunction
                        {
                            UserNo_sno = m_sno,
                            FunctionNo = item
                        };
                        memberFuncList.Add(memberFunc);
                    }

                    db.ES_MemberFunction.AddRange(memberFuncList); // 批量新增
                    db.SaveChanges();
                }
                else
                {
                    // 處理找不到 fUserId 的情況
                    throw new Exception("找不到對應的 fUserId");
                }

                //member_Func.FunctionNo = ES_FunctionItem;

                return new MemberResult { ErrMsg = true };
            }
        }

        //人員刪除
        public void Del(int fId)
        {
            var member = db.ES_Member.Where(m => m.fId == fId).FirstOrDefault();

            member.fStatus = (member.fStatus == true) ? false : true;

            db.SaveChanges();
        }
    }
}