using Picanol.Helpers;
using Picanol.Models;
using Picanol.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Controllers
{
    [RedirectingAction]
    public class UserController : BaseController
    {
        private readonly RoleHelper _roleHelper;
        private readonly UserHelper _userHelper;
        public UserController()
        {
            
            _roleHelper = new RoleHelper(this);
            _userHelper = new UserHelper(this);
          

        }

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddUser(int? id)
        {
            UserViewModel uv = new UserViewModel();
            uv.role = _roleHelper.GetAllRole();
            var user = _userHelper.GetAllUsers();

            foreach (var item in user)
            {
                if (item.UserId == id)
                    uv.User = item;
            }

            return View(uv);
        }

        [HttpPost]
        public ActionResult AddUser(UserDto um)
        {
            string response = "";
            if (um.UserId == 0)
            {
                response = _userHelper.InsertUser(um);
                if (response == "success")
                {
                    string ActionName = $"add User - {um.UserName}";
                    string TableName = "TblUser";
                    if (ActionName != null)
                    {
                        var userinfo = (UserSession)Session["UserInfo"];
                        _userHelper.recordUserActivityHistory(userinfo.UserId, ActionName, TableName);
                    }
                }
            }
            else
            {
                response = _userHelper.UpdateUser(um);
                if (response == "success")
                {
                    string ActionName = $"Update User - {um.UserName}";
                    string TableName = "TblUser";
                    if (ActionName != null)
                    {
                        var userinfo = (UserSession)Session["UserInfo"];
                        _userHelper.recordUserActivityHistory(userinfo.UserId, ActionName, TableName);
                    }
                }
            }

            if (response == "success")
            {
                //ModelState.Clear();
                Response.Write("<script>alert('User Added successfully')</script>");
                return RedirectToAction("UserList", "User");
                //return View();
            }
            else
            {
                return View();
            }

        }

        public ActionResult DeleteUser(int id)
        {
            UserViewModel nvm = new UserViewModel();
            var users = _userHelper.GetAllUsers();
            foreach (var item in users)
            {
                if (item.UserId == id)
                    nvm.User = item;

            }
            string response = "";
            if (nvm.User.UserId != 0)
            {
                response = _userHelper.DeleteUser(nvm.User.UserId);
            }
            if (response == "success")
            {
                string ActionName = $"Delete User - {nvm.User.UserId}";
                string TableName = "TblUser";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(nvm.User.UserId, ActionName, TableName);
                }
            }
            Response.Write("<script>alert('User Deleted successfully')</script>");
            return RedirectToAction("UserList", "User");
        }

        public ActionResult UserList()
        {
            List<UserDto> nl = new List<UserDto>();
            UserViewModel vm = new UserViewModel();
            var users = _userHelper.GetAllUsers();
            vm.user = users;

            return View(vm);

        }

        //View User account delete for playstore
        public ActionResult UserDeleteAccount()
        {
            return View();
        }

        public string UserDelete(string MobileNumber)
        {
            string response = _userHelper.DeleteUserAccount(MobileNumber);
            if (response == "Success")
            {
                string ActionName = $"Account Delete User - from play google store";
                string TableName = "TblUser";
                if (ActionName != null)
                {
                    _userHelper.recordUserActivityHistory(0, ActionName, TableName);
                }
            }

            return response;
        }

    }
}