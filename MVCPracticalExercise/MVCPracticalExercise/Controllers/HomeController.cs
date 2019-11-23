using MVCPracticalExercise.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCPracticalExercise.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=QuoteData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(string firstName, string lastName, string eMail, DateTime dateOfBirth, 
                                    int carYear, string carMake, string carModel, string DUI, int tickets,
                                    string coverage) 
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(eMail) || dateOfBirth == null 
                || carYear <= 0 || string.IsNullOrEmpty(carMake) || string.IsNullOrEmpty(carModel) || string.IsNullOrEmpty(DUI) 
                || tickets < 0 || string.IsNullOrEmpty(coverage))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            else
            {
                //logic for finding quote value
                decimal myQuote = 50; 
                DateTime dob = dateOfBirth;
                DateTime today = DateTime.Now;
                int age = today.Year - dob.Year;

                if (age < 18) { myQuote = myQuote + 100; }
                else if (age < 25) { myQuote = myQuote + 25; }
                
                if (carYear < 2000) { myQuote = myQuote + 25; }
                if (carYear > 2015) { myQuote = myQuote + 25; }
                if (carMake == "Porsche") { myQuote = myQuote + 25; }
                if (carMake == "Porsche" && carModel == "911 Carrera") { myQuote = myQuote + 25; }

                myQuote = myQuote + (tickets * 10);

                if (DUI == "yes") { myQuote = myQuote + (myQuote / 4); }
                if (coverage == "full") { myQuote = myQuote + (myQuote / 2); }


                //SQL interaction

                string queryString = @"INSERT INTO QuoteData (FirstName, LastName, Email, Quote, Time) VALUES 
                                        (@FirstName, @LastName, @Email, @Quote, @Time)";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.Add("@FirstName", SqlDbType.VarChar);
                    command.Parameters.Add("@LastName", SqlDbType.VarChar);
                    command.Parameters.Add("@Email", SqlDbType.VarChar);
                    command.Parameters.Add("@Quote", SqlDbType.Money);
                    command.Parameters.Add("@Time", SqlDbType.DateTime);

                    command.Parameters["@FirstName"].Value = firstName;
                    command.Parameters["@LasttName"].Value = lastName;
                    command.Parameters["@Email"].Value = eMail;
                    command.Parameters["@Quote"].Value = myQuote;
                    command.Parameters["@Time"].Value = DateTime.Now;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }


                return View("Success");
            }
        }

        public ActionResult Admin()
        {
            string queryString = @"SELECT ID, FirstName, LastName, Email, Quote, Time from Quotes";
            List<GetQuote> getQuotes = new List<GetQuote>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);


                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var getQuote = new GetQuote();

                    getQuote.Id = Convert.ToInt32(reader["Id"]);
                    getQuote.FirstName = reader["FirstName"].ToString();
                    getQuote.LastName = reader["LastName"].ToString();
                    getQuote.Email = reader["Email"].ToString();
                    getQuote.Quote = Convert.ToDecimal(reader["Quote"]);
                    getQuote.Time = Convert.ToDateTime(reader["Time"]);

                    getQuotes.Add(getQuote);
                }
            }


            return View(getQuotes);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}