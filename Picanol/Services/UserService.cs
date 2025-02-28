using Picanol.DataModel;
using Picanol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Picanol.Services
{
    public class UserService : BaseService<PicannolEntities, tblUser>
    {
        #region Constructors
        internal UserService(PicannolEntities context, iValidation validationDictionary) :
            base(context, validationDictionary)
        { }

        #endregion

        #region BaseMethods
        public void SaveStudent(tblUser cl)
        {
            AddCustomer(cl);
        }
        public void AddCustomer(tblUser cls)
        {
            if (cls == null)
                throw new ArgumentNullException("student", "Null Parameter");
            Add(cls);
        }
        public void AddUser(tblUser user)
        {
            if (user == null)
                throw new ArgumentNullException("role", "Null Parameter");
            Add(user);
        }

        public void UpdateUser(tblUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user", "Null Parameter");
            Update(user);
        }

        public void DeleteUser(tblUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user", "Null Parameter");
            Delete(user);

        }

        public List<UserDto> GetAllUsers()
        {
            List<UserDto> user = new List<UserDto>();
            user = (from r in Context.tblUsers
                    where r.DelInd == false
                    select new UserDto
                    {
                        UserId = r.UserId,
                        UserName = r.UserName,
                        Email = r.Email,
                        Password = r.Password,
                        RoleId = r.RoleId,
                        MobileNo = r.MobileNo,

                    }).ToList();
            return user;
        }
       
        public List<UserDto> GetUsersList()
        {
            var st = (from a in Context.tblUsers
                      where a.RoleId == 1 || a.RoleId == 7 && a.DelInd==false
                      select new UserDto
                      {
                          UserId = a.UserId,
                          UserName = a.UserName
                      }).ToList();
            return st;
        }

        public List<UserDto> GetAssignedUsersList()
        {
            var st = (from a in Context.tblOrders
                      join b in Context.tblUsers on a.AssignedUserId equals b.UserId
                      where b.RoleId == 1 && a.DelInd==false
                      select new UserDto
                      {
                          UserId = b.UserId,
                          UserName = b.UserName
                      }).ToList();
            return st;
        }
        #endregion

        #region Overrides
        public override void Add(tblUser student)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                base.Add(student);
            }
        }

        public override void Update(tblUser user)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                tblUser updateuser = Context.tblUsers.Where(p => p.UserId == user.UserId).FirstOrDefault();
                updateuser.UserName = user.UserName;
                updateuser.Password = user.Password;
                updateuser.RoleId = user.RoleId;
                updateuser.Email = user.Email;
                updateuser.MobileNo = user.MobileNo;


                base.Update(updateuser);
            }
        }

        public override void Delete(tblUser user)
        {
            bool validationStatus = ValidationDictionary.isValid;
            if (validationStatus)
            {
                var itemToRemove = Context.tblUsers.SingleOrDefault(x => x.UserId == user.UserId); //returns a single item.

                if (itemToRemove != null)
                {
                    Context.tblUsers.Remove(itemToRemove);
                    Context.SaveChanges();
                }

            }
        }


        #endregion
    }
}