using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WORKFLOW.Models;
using System.Configuration;


namespace WORKFLOW.Controllers
{
    public class AccountController : Controller
    {
        private readonly WorkFlowEntities2 dbContext;

        public AccountController()
        {
            dbContext = new WorkFlowEntities2();
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var user = Session["User"] as User;
            if (user != null)
            {
                // Retrieve leave requests for the user
                var leaveRequests = dbContext.LeaveRequests.Where(r => r.fk_user_id == user.user_id).ToList();

                ViewBag.User = user;
                ViewBag.LeaveRequests = leaveRequests;

                return View(user);
            }
            else
            {
                // User is not logged in, redirect to the Login action
                return RedirectToAction("Login");
            }


        }

        public new ActionResult Request()
        {
            return View();
        }
        // Assuming you have a database context called 'DbContext' and a 'LeaveRequest' table/entity
        public ActionResult Admin()
        {
            using (var dbContext = new WorkFlowEntities2())
            {
                var leaveRequests = dbContext.LeaveRequests.ToList();
                var model = new List<Tuple<WORKFLOW.User, WORKFLOW.LeaveRequest>>();

                foreach (var request in leaveRequests)
                {
                    // Assuming you have a user object associated with each leave request
                    var user = dbContext.Users.FirstOrDefault(u => u.user_id == request.fk_user_id);
                    var tuple = new Tuple<WORKFLOW.User, WORKFLOW.LeaveRequest>(user, request);
                    model.Add(tuple);
                }

                return View(model);
            }
        }
        [HttpPost]
        public ActionResult UpdateStatus(int leaveRequestId, string status)
        {
            using (var dbContext = new WorkFlowEntities2())
            {
                var leaveRequest = dbContext.LeaveRequests.FirstOrDefault(r => r.leave_request_Id == leaveRequestId);

                if (leaveRequest != null)
                {
                    // Update the leave request status
                    leaveRequest.leave_status = status;
                    dbContext.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Leave request not found." });
                }
            }
        }









        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Loginmodel model)
        {
            // Perform authentication logic using the DbContext
            var user = dbContext.Users.FirstOrDefault(u => u.email == model.Username && u.password == model.Password);

            if (user != null)
            {
                // Successful login
                // Store the user in Session for later use
                Session["User"] = user;

                // Redirect to Account/Index after successful login
                return RedirectToAction("Index", "Account");
            }
            else
            {
                // Failed login
                ModelState.AddModelError("", "Invalid username or password");
            }

            return View(model);
        }

        private List<LeaveRequest> GetLeaveRequestsFromAPI()
        {
            try
            {
                var apiUrl = "<https://localhost:44398/api/LeaveRequest>"; // Replace with the actual API endpoint URL
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(apiUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var leaveRequestsJson = response.Content.ReadAsStringAsync().Result;
                        var leaveRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LeaveRequest>>(leaveRequestsJson);
                        return leaveRequests;
                    }
                    else
                    {
                        // Handle the API error response
                        // You can log the error or handle it as per your requirements
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the API request
                // You can log the exception or handle it as per your requirements
            }

            return new List<LeaveRequest>(); // Return an empty list if there was an error or no leave requests were retrieved
        }

        /* [HttpPost]
         public ActionResult SendEmail(EmailModel model)
         {
             if (!ModelState.IsValid)
             {
                 // If model validation fails, return the view with validation errors
                 return View(model);
             }
             var user = Session["User"] as User;
             if (user != null)
             {
                 try
                 {
                     // Send email using the user and other relevant data
                     MailMessage mailMessage = new MailMessage();
                     mailMessage.From = new MailAddress("dpatidar1221@gmail.com");
                     mailMessage.To.Add(model.EmailTo);
                     mailMessage.Subject = model.Subject;
                     mailMessage.Body = model.Message;
                     SmtpClient smtpClient = new SmtpClient("sandbox.smtp.mailtrap.io", 2525);
                     smtpClient.Credentials = new System.Net.NetworkCredential("049f5af11a0bc0", "8888f78319a790");
                     smtpClient.EnableSsl = true;
                     smtpClient.Send(mailMessage);
                     // Email sent successfully
                     TempData["SuccessMessage"] = "Email sent successfully";
                     // You can perform any additional logic here
                     // Redirect to Account/Index
                     return RedirectToAction("Index", "Account");
                 }
                 catch (Exception ex)
                 {
                     // Handle email sending error
                     ModelState.AddModelError("", "Failed to send email: " + ex.Message);
                 }
             }
             // Redirect to Account/Index if there is no user in the session or in case of error
             return RedirectToAction("Index", "Account");
         }*/
        /*[HttpPost]
        public ActionResult SendEmail(EmailModel model)
        {
            if (!ModelState.IsValid)
            {
                // If model validation fails, return the view with validation errors
                return View(model);
            }

            var user = Session["User"] as User;

            if (user != null)
            {
                try
                {
                    // Send email using the user and other relevant data
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("dpatidar1221@gmail.com");
                    mailMessage.To.Add(model.EmailTo);
                    mailMessage.Subject = model.Subject;
                    mailMessage.Body = model.Message;

                    SmtpClient smtpClient = new SmtpClient("sandbox.smtp.mailtrap.io", 2525);
                    smtpClient.Credentials = new System.Net.NetworkCredential("049f5af11a0bc0", "8888f78319a790");
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(mailMessage);

                    // Email sent successfully
                    TempData["SuccessMessage"] = "Email sent successfully";

                    // Save the data in the LeaveRequest table

                   

        LeaveRequest leaveRequest = new LeaveRequest
                  {     leave_request_Id=7,
                      
                      start_date = model.FromDate,
                      end_date = model.ToDate,
                      leave_status="Pending",
                      Description = model.Message
                  };


                    using (var db = new WorkFlowEntities2())
                    {
                        db.LeaveRequests.Add(leaveRequest);
                        db.SaveChanges();
                    }

                    // You can perform any additional logic here

                    // Redirect to Account/Index
                    return RedirectToAction("Index", "Account");
                }
                catch (Exception ex)
                {
                    // Handle email sending error
                    ModelState.AddModelError("", "Failed to send email: " + ex.Message);
                }
            }

            // Redirect to Account/Index if there is no user in the session or in case of error
            return RedirectToAction("Index", "Account");
        }*/

        [HttpPost]
        public async Task<ActionResult> SendEmail(EmailModel model)
        {
            if (!ModelState.IsValid)
            {
                // If model validation fails, return the view with validation errors
                return View(model);
            }

            try
            {
                // Send email using the user and other relevant data
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("dpatidar1221@gmail.com");
                mailMessage.To.Add(model.EmailTo);
                mailMessage.Subject = model.Subject;
                mailMessage.Body = model.Message;

                SmtpClient smtpClient = new SmtpClient("sandbox.smtp.mailtrap.io", 2525);
                smtpClient.Credentials = new System.Net.NetworkCredential("5992f670b73e58", "37da1dd4699842");
                smtpClient.EnableSsl = true;
                smtpClient.Send(mailMessage);

                // Email sent successfully
                 TempData["SuccessMessage"] = "Email sent successfully";

                // Save the data in the LeaveRequest table
                /*  string connectionString = "Data Source=10.0.0.43,1433;initial Catalog=WorkFlow;User ID=deep;Password=deep123;Encrypt=True;TrustServerCertificate=True";*/

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnectionString"]?.ConnectionString))
                {
                    string insertQuery = "INSERT INTO LeaveRequest (fk_user_id, start_date, end_date, leave_status, Description) " +
                                         "VALUES (@UserId, @StartDate, @EndDate, @LeaveStatus, @Description)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Retrieve the fk_user_id for the logged-in user
                        var loggedInUser = Session["User"] as User;
                        int staticUserId = loggedInUser?.user_id ?? 0; // Replace 'user_id' with the actual property name

                        command.Parameters.AddWithValue("@UserId", staticUserId);
                        command.Parameters.AddWithValue("@StartDate", model.FromDate);
                        command.Parameters.AddWithValue("@EndDate", model.ToDate);
                        command.Parameters.AddWithValue("@LeaveStatus", "Pending");
                        command.Parameters.AddWithValue("@Description", model.Subject);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }





                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                // Handle email sending error
                ModelState.AddModelError("", "Failed to send email: " + ex.Message);
            }

            // Redirect to Account/Index in case of error
            return RedirectToAction("Index", "Account");
        }




    }
}
