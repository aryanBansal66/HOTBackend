using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;

namespace HouseOfTalent.APIFolder
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class WebService1 : System.Web.Services.WebService
    {
        string strcon = ConfigurationManager.ConnectionStrings["HouseOfTalentConnectionString"].ConnectionString;
        JavaScriptSerializer js = new JavaScriptSerializer();
        
        private static readonly HttpClient client = new HttpClient();
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/firebase.messaging" };


        [WebMethod]
        public void SendNotification(string deviceToken, string title, string message,string type)
        {
            try
            {
           
                string serviceAccountPath = Server.MapPath("~/APIFolder/service-account.json");
                var accessToken = GetAccessToken(serviceAccountPath);

                SendNotificationSync(deviceToken, title, message, accessToken,type);
            }
            catch (Exception ex)
            {
                Context.Response.Write($"An error occurred: {ex.Message}<br>");
            }
        }

        private string GetAccessToken(string serviceAccountJsonPath)
        {
            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream(serviceAccountJsonPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
                }

                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(Scopes);
                }

                var token = credential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting access token: {ex.Message}");
            }
        }

        private void SendNotificationSync(string deviceToken, string title, string body, string accessToken,string type)
        {
            try
            {
                var requestUri = "https://fcm.googleapis.com/v1/projects/notifications-try-39714/messages:send";

                if (type.Equals("Single"))
                {
                    var message = new
                    {
                        message = new
                        {
                            topic = deviceToken,
                            notification = new
                            {
                                title = title,
                                body = body
                            }
                        }
                    };

                    var jsonMessage = JsonConvert.SerializeObject(message);

                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json")
                    };
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var response = client.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

                    response.EnsureSuccessStatusCode();
                    var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Context.Response.Write("{\"message\":"+js.Serialize("Notification Sended Successfully.")+"}");
                }
                else
                {
                    var message = new
                    {
                        message = new
                        {
                            token = deviceToken,
                            notification = new
                            {
                                title = title,
                                body = body
                            }
                        }
                    };

                    var jsonMessage = JsonConvert.SerializeObject(message);

                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json")
                    };
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

          
                    var response = client.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

                    response.EnsureSuccessStatusCode();
                    //var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Context.Response.Write("{\"message\":" + js.Serialize("Notification Sended Successfully.") + "}");


                }
            }
            catch (HttpRequestException httpEx)
            {
                Context.Response.Write($"HTTP Request error: {httpEx.Message}<br>");
            }
            catch (Exception ex)
            {
                Context.Response.Write($"An error occurred: {ex.Message}<br>");
            }
        }

        [WebMethod]
        public void UpdateFBTokens(string token,string ph,string fbtoken)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdateFBTokens(ph, fbtoken).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void Register(string token, string type, string fname, string lname, string email, string ph, string gender, string DOB, string username, string image, string BIO, string Insta, string YT, string IMEI,string FCM)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                if (type.Equals("Register"))
                {
                    SqlConnection sqlConnection = new SqlConnection(strcon);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("Login", sqlConnection);
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@Phone", ph);
                    int k = (Int32)sqlCommand.ExecuteScalar();
                    if (k == 0)
                    {
                        checkusername(fname,lname,email,ph,gender,DOB,username,image,BIO,Insta,YT,IMEI,FCM);
                    }
                    else
                    {
                        DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                        dc.UpdateIMEI(IMEI, ph).ToString();
                        Context.Response.Write("{\"message\":" + js.Serialize("Already Registered!") + "}");
                    }
                    sqlConnection.Close();
                }

                else
                {
                    SqlConnection sqlConnection1 = new SqlConnection(strcon);
                    sqlConnection1.Open();
                    SqlCommand sqlCommand1 = new SqlCommand("Login", sqlConnection1);
                    sqlCommand1.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand1.Parameters.AddWithValue("@Phone", ph);
                    int k1 = (Int32)sqlCommand1.ExecuteScalar();
                    if (k1 == 0)
                    {
                        Context.Response.Write("{\"message\":" + js.Serialize("Please Register!") + "}");
                    }
                    else
                    {
                        Context.Response.Write("{\"message\":" + js.Serialize("Loggedin!") + "}");
                    }
                    sqlConnection1.Close();
                }
            }
        }

        void checkusername(string fname, string lname, string email, string ph, string gender, string DOB, string username, string image, string BIO, string Insta, string YT, string IMEI,string FCMToken)
        {
            SqlConnection sqlConnection = new SqlConnection(strcon);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand("Select count(*) from Register where username=@Phone", sqlConnection);
            sqlCommand.CommandType = System.Data.CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@Phone", username);
            int k = (Int32)sqlCommand.ExecuteScalar();
            if (k == 0)
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.UserRegister(fname, lname, email, ph, gender, DOB, username, image, BIO, Insta, YT, IMEI,"1", FCMToken,"1").ToString();
                dc.InsertWallet(ph).ToString();
                dc.InsertReadUnRead("Unread", ph).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Registered!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Username Already Taken!") + "}");
            }
        }

        [WebMethod]
        public void CheckBadge(string token,string ph,string vid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list=dc.GetBadge(ph,vid).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertWithdrawRequest(string token, string Phone, string Mode, string Accountnumber, string IFSC, string Branch, string Name, string UPI, string Status, string Amount)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.InsertWithdraw(Phone, Mode, Accountnumber, IFSC, Branch, Name, UPI, Status, Amount).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertComment(string token, string comment, string username,string logo,string videoid,string rplyid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.InsertComment(comment, username,logo,videoid,rplyid,DateTime.Now.ToString("dd-MM-yyyy HH:mm")).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertFeedBack(string token, string comment, string extra, string phone,string image)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {

                byte[] imageBytes1 = Convert.FromBase64String(image);

                string fileExtension1 = ".png";

                string fileName1 = $"{Guid.NewGuid()}{fileExtension1}";

                string folderPath1 = Server.MapPath("~/Images/");

                string filePath1 = Path.Combine(folderPath1, fileName1);

                File.WriteAllBytes(filePath1, imageBytes1);

                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.InsertFeedBack(comment, extra, phone,fileName1,DateTime.Now.ToString("dd-MM-yyyy HH:mm")).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DeleteComment(string token, string commentid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.DeleteComment(commentid).ToString();
                dc.Commentrplydel(commentid).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Deleted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdateLikes(string token, string videoid, string contact)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                SqlConnection sqlConnection = new SqlConnection(strcon);
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("CheckLike", sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@VideoId", videoid);
                sqlCommand.Parameters.AddWithValue("@contact", contact);
                int k = (Int32)sqlCommand.ExecuteScalar();

                if (k > 0)
                {
                    if (!int.TryParse(videoid, out int videoId))
                    {
                        Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID format!") + "}");
                        return;
                    }

                    try
                    {
                        string updateQuery = $"UPDATE Likes SET Likecount = CAST(Likecount AS INT) - 1 WHERE videoid = '" + videoid + "'";

                        using (SqlConnection connection = new SqlConnection(strcon))
                        {
                            SqlCommand command = new SqlCommand(updateQuery, connection);
                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
                                DeleteLike(token,videoid,contact);
                            }
                            else
                            {
                                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID!") + "}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Context.Response.Write("{\"message\":" + js.Serialize("Error updating likes count: " + ex.Message) + "}");
                    }
                }
                else
                {
                    if (!int.TryParse(videoid, out int videoId))
                    {
                        Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID format!") + "}");
                        return;
                    }

                    try
                    {
                        string updateQuery = $"UPDATE Likes SET Likecount = CAST(Likecount AS INT) + 1 WHERE videoid = '" + videoid + "'";

                        using (SqlConnection connection = new SqlConnection(strcon))
                        {
                            SqlCommand command = new SqlCommand(updateQuery, connection);
                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
                                InsertLike(token, videoid, contact);
                            }
                            else
                            {
                                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID!") + "}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Context.Response.Write("{\"message\":" + js.Serialize("Error updating likes count: " + ex.Message) + "}");
                    }
                }

               
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DisplayLikes(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list=dc.DisplayLikes(videoid).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetFollowers(string token, string phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.MyFollowing(phone).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetFollowers2(string token, string phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.MyFollowers(phone).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertLikes(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.InsertLikes(videoid).ToString();
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetComment(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list=dc.GetComments(videoid).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetCommentreply(string token, string videoid,string rplyid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetCommentsreply(videoid,rplyid).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void CommentCount(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.CommentCount(videoid).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdateVideoViews(string token,string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                if (!int.TryParse(videoid, out int videoId))
                {
                    Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID format!") + "}");
                    return;
                }

                try
                {
                    string updateQuery = $"UPDATE VideoViews SET Viewcount = CAST(Viewcount AS INT) + 1 WHERE videoid = '" + videoid + "'";

                    using (SqlConnection connection = new SqlConnection(strcon))
                    {
                        SqlCommand command = new SqlCommand(updateQuery, connection);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
                        }
                        else
                        {
                            Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID!") + "}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Context.Response.Write("{\"message\":" + js.Serialize("Error updating likes count: " + ex.Message) + "}");
                }
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertVideoViews(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertVideoViews(videoid).ToString();
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertNotification(string token, string title,string msg,string phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertNotification(title,msg,phone,DateTime.Now.ToString("dd-MM-yyyy")).ToString();
                if (phone.Equals(""))
                {
                    dc.UpdateReadUnReadAll("Unread","").ToString();
                }
                else
                {
                    dc.UpdateReadUnRead("Unread",phone).ToString();
                }
                Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DeleteNotification(string token, int id)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DeleteNotification(id).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Deleted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void ShowNotificationStatus(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetReadUnread(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void Updatedot(string token,string status,string phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdateReadUnRead(status, phone).ToString();
            
                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void ShowNotifications(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.ShowNotifications(ph).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetProfilebyusername(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetProfilebyusername(ph).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);
                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertNotifications(string token, string title, string msg, string phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertNotification(title, msg, phone,DateTime.Now.ToString("dd-MM-yyyy")).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DisplayVideoViews(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetVideoViews(videoid).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertVideoShare(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertVideoShare(videoid).ToString();
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdateVideoShare(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                if (!int.TryParse(videoid, out int videoId))
                {
                    Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID format!") + "}");
                    return;
                }

                try
                {
                    string updateQuery = $"UPDATE Share SET Sharecount = CAST(Sharecount AS INT) + 1 WHERE videoid = '" + videoid + "'";

                    using (SqlConnection connection = new SqlConnection(strcon))
                    {
                        SqlCommand command = new SqlCommand(updateQuery, connection);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
                        }
                        else
                        {
                            Context.Response.Write("{\"message\":" + js.Serialize("Invalid Video ID!") + "}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Context.Response.Write("{\"message\":" + js.Serialize("Error updating likes count: " + ex.Message) + "}");
                }
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DisplayVideoShare(string token, string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetVideoShare(videoid).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void Displayfollow(string token, string followed)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetFollow(followed).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdateMode(string token, string videoid,string mode)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdateVideoMode(mode,videoid).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void CheckFollow(string token, string Followername, string followed)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                SqlConnection sqlConnection = new SqlConnection(strcon);
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("CheckFollow", sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Followername", Followername);
                sqlCommand.Parameters.AddWithValue("@followed", followed);
                int k = (Int32)sqlCommand.ExecuteScalar();
                if (k == 0)
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    dc.InsertFollow(Followername,followed).ToString();
                    Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
                }
                else
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    dc.DeleteFollow(Followername, followed).ToString();
                    Context.Response.Write("{\"message\":" + js.Serialize("Deleted!") + "}");
                }
                sqlConnection.Close();
            }
        }

        [WebMethod]
        public void CheckFollowwithnumber(string token, string Followername, string followed)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list=dc.CheckFollow(Followername, followed).ToList();
                Context.Response.Write(js.Serialize(list));
            }
        }

        [WebMethod]
        public void InsertLike(string token,string videoid,string contact)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                dc.InsertLike(videoid,contact).ToString();
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DeleteLike(string token, string videoid, string contact)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DeleteLike(videoid, contact).ToString();
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void ShowLike(string token, string videoid, string contact)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.ShowLike(videoid, contact).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void Notifications(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.Notifications.ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetMyVideos(string token, string contact)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetMyVideos(contact).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetCompetitionVideos(string token, string cid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetCompetitionVideos(cid).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void SendMessage(string token, string message, string sendername, string sendermail, string recievername, string recieveremail, string Onlinestatus, string readunreadstatus)
        {
            DateTime currentTime = DateTime.Now;
            string timeWithMilliseconds = currentTime.ToString("HH:mm:ss.fff");
            DateTime currentDate = DateTime.Now.Date;
            string formattedDate = currentDate.ToString("dd-MM-yyyy");
            if (token.Equals("dkkjhhhunnnmmkvvvbbzaiklnqqblloomnnnbzk"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertChats(message, timeWithMilliseconds, formattedDate, sendername, sendermail, recievername, recieveremail, Onlinestatus, readunreadstatus).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("MSG SENT!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdateMessageReadUnReadStatus(string token, string sendermail, string recieveremail)
        {
            if (token.Equals("dkkjhhhunnnmmkvvvbbzaiklnqqblloomnnnbzk"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdateChatNumber(recieveremail, sendermail).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Status Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GETChatmessages(string token, string recievermail, string sendermail)
        {
            if (token.Equals("dkkjhhhunnnmmkvvvbbzaiklnqqblloomnnnbzk"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DisplayChatMessages(recievermail, sendermail).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GETUnreadmessages(string token, string recievermail, string sendermail)
        {
            if (token.Equals("dkkjhhhunnnmmkvvvbbzaiklnqqblloomnnnbzk"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DisplayChatNumber(recievermail, sendermail).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GETChatList(string token, string recievermail)
        {
            if (token.Equals("dkkjhhhunnnmmkvvvbbzaiklnqqblloomnnnbzk"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DisplayChatlist(recievermail).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetStanding(string token, string cid, string username)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetStanding(cid, username).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetChallanges(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.Competitions.ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void Search(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.Search().ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetForyou(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetForYou().ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertJoinCompetition(string token, string cid, string userph, string Competitionname,string Liveon,string Endon,string ResultDate,string CompetitionBasis,string CompetitionCriteria,string DisqualifyCriteria,string WinningAMT,string EntryFee,string Seats,string SeatsLeftPercent,string bgImage,string Status,string videoid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertJoinCompetition(cid, userph, Competitionname, Liveon, Endon, ResultDate, CompetitionBasis, CompetitionCriteria, DisqualifyCriteria, WinningAMT, EntryFee, Seats, SeatsLeftPercent, bgImage, Status,videoid).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Insert!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void CheckJoinCompetition(string token, string cid, string userph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                SqlConnection sqlConnection = new SqlConnection(strcon);
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand("CheckJoinCompetition", sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@cid", cid);
                sqlCommand.Parameters.AddWithValue("@userph", userph);
                int k = (Int32)sqlCommand.ExecuteScalar();
                if (k == 0)
                {
                    Context.Response.Write("{\"message\":" + js.Serialize("AVL!") + "}");
                }
                else
                {
                    Context.Response.Write("{\"message\":" + js.Serialize("NOT AVL!") + "}");
                }
                sqlConnection.Close();
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid token!") + "}");
            }
        }

        [WebMethod]
        public void GetJoinCompetition(string token, string cid, string userph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetJoinCompetition(cid, userph).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);
                Context.Response.Write(json);

            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdatePost(string token, string mode, string D,string Thumbnail,string creator,int id)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdatePost(mode, D,Thumbnail,creator,id).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void AddPost(string token, string video, string mode,string D,string Thumbnail,string creator,string status,string contestid,string creatorph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                byte[] imageBytes = Convert.FromBase64String(video);

                string fileExtension = ".mp4";

                string fileName = $"{Guid.NewGuid()}{fileExtension}";

                string folderPath = Server.MapPath("~/Images/");

                string filePath = Path.Combine(folderPath, fileName);

                File.WriteAllBytes(filePath, imageBytes);


                byte[] imageBytes1 = Convert.FromBase64String(Thumbnail);

                string fileExtension1 = ".png"; 

                string fileName1 = $"{Guid.NewGuid()}{fileExtension1}";

                string folderPath1 = Server.MapPath("~/Images/");

                string filePath1 = Path.Combine(folderPath1, fileName1);

                File.WriteAllBytes(filePath1, imageBytes1);

                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.AddPost(fileName,mode,D,fileName1,creator,status,contestid,creatorph).ToString();

                var insertedPost = dc.Posts.OrderByDescending(p => p.id).FirstOrDefault();
                int postId = insertedPost != null ? insertedPost.id : -1;
                InsertLikes(token,""+postId);
                InsertVideoShare(token, "" + postId);
                InsertVideoViews(token,""+postId);
                Context.Response.Write("{\"message\":" + js.Serialize(postId) + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DeletePost(string token, int id)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DeletePost(id).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Insert!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetPost(string token, string creator)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetPost(creator).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);
                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetCoins(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.Coins.ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetProfile(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetProfile(ph).ToList();

                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }

        }

        [WebMethod]
        public void GetCompetitionCount(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetCCount(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetVideosCount(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetFollowing(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetFollowingcount(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetFollowingReal(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetFollowVideos(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetFollowingVideos(ph).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetFollowerscount(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetFollower(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetWallet(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetWallet(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetCompetitions(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.Competitions.ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);
                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetHistory(string token, string ph)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetHistory(ph).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void getBanner(string token)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.Banners.ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetVideoInfo(string token,string id)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetVideoInfo(id).ToList();
                Context.Response.ContentType = "application/json; charset=utf-8";

                JavaScriptSerializer js = new JavaScriptSerializer
                {
                    MaxJsonLength = Int32.MaxValue,
                    RecursionLimit = 100
                };

                string json = js.Serialize(list);

                Context.Response.Write(json);
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void EditProfile(string token, string fname, string lname, string email, string ph, string gender, string DOB, string username, string image, string BIO, string Insta, string YT)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                byte[] imageBytes = Convert.FromBase64String(image);

                string fileExtension = ".png";

                string fileName = $"{Guid.NewGuid()}{fileExtension}";

                string folderPath = Server.MapPath("~/Images/");

                string filePath = Path.Combine(folderPath, fileName);

                File.WriteAllBytes(filePath, imageBytes);

                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.EditProfile(fname,lname, email, ph, gender, DOB, username,fileName,BIO,Insta,YT).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetResultsbyCompetition(string token,string cid,string basis)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                if (basis.Equals("Likes"))
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    var list = dc.GetLikesbycompetition(cid).ToList();
                    Context.Response.Write(js.Serialize(list));
                }
                if (basis.Equals("Comments"))
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    var list = dc.GetCommentsbycompetition(cid).ToList();
                    Context.Response.Write(js.Serialize(list));
                }
                if (basis.Equals("Shares"))
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    var list = dc.GetSharesbycompetition(cid).ToList();
                    Context.Response.Write(js.Serialize(list));
                }
                if (basis.Equals("Views"))
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    var list = dc.GetViewsbycompetition(cid).ToList();
                    Context.Response.Write(js.Serialize(list));
                }
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetResultsbyphone(string token, string cid, string basis,string ph)
        {  
                if (token.Equals("HMNNOJMIFHAT"))
                {
                    if (basis.Equals("Likes"))
                    {
                        DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                        var list = dc.GetLikesbycompetitionbyph(cid,ph).ToList();
                        Context.Response.Write(js.Serialize(list));
                    }
                    if (basis.Equals("Comments"))
                    {
                        DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                        var list = dc.GetCommentsbycompetitionbyph(cid, ph).ToList();
                        Context.Response.Write(js.Serialize(list));
                    }
                    if (basis.Equals("Shares"))
                    {
                        DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                        var list = dc.GetSharesbycompetitionbyph(cid, ph).ToList();
                        Context.Response.Write(js.Serialize(list));
                    }
                    if (basis.Equals("Views"))
                    {
                        DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                        var list = dc.GetViewsbycompetitionbyph(cid, ph).ToList();
                        Context.Response.Write(js.Serialize(list));
                    }
                }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void UpdateBalance(string token, string Mark, decimal Balance, string ph,string Reason)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                if (Mark.Equals("Add"))
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    dc.UpdateBalanceAdd(Balance, ph).ToString();
                    dc.InsertHistory(Reason, DateTime.Now.ToString(), "" + Balance, ph,"Added");
                    Context.Response.Write("{\"message\":"+js.Serialize("Updated")+"}");
                }
                else
                {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    dc.UpdateBalanceMinus(Balance, ph).ToString();
                    Context.Response.Write("{\"message\":" + js.Serialize("Updated") + "}");
                    dc.InsertHistory(Reason, DateTime.Now.ToString(), "" + Balance, ph,"Withdraw");
                }
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void LeaderBoards(string token,string competitionid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                    DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                    var list=dc.GetLeaderboard(competitionid).ToList();
                    Context.Response.Write(js.Serialize(list));
                
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetEndLeaderBoards(string token, string competitionid)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetLeaderboardAfterEnd(competitionid).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void ActiveUsers(string token,string phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdateActiveCount(phone).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        /*[WebMethod]
        public void CronJob(string token)
        {
            if (token.Equals("CRONJOBSHHKA"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);

                dc.UpdateCompetition(DateTime.Now.ToString(DateTime.Now.ToString("dd-MM-yyyy HH:mm:00.000")), "Live");

                dc.UpdateJoinedCompetition(DateTime.Now.ToString("dd-MM-yyyy HH:mm:00.000"), "Live");

                var competition = dc.Competitions.FirstOrDefault(c => c.Liveon == DateTime.Now.ToString("dd-MM-yyyy HH:mm:00.000"));

                if (competition != null)
                {
                    var postsToUpdate = dc.Posts.Where(p => p.Competitionid == competition.id.ToString() && p.Mode != "Reject");
                    foreach (var post in postsToUpdate)
                    {
                        post.Mode = "Public";
                    }

                    dc.SubmitChanges();

                    Context.Response.Write("{\"message\":" + js.Serialize("Updated") + "}");

                }

                var competition1 = dc.Competitions.FirstOrDefault(c => c.Endon == DateTime.Now.ToString("dd-MM-yyyy HH:mm:00.000"));

                if (competition1 != null)
                {
                    var postsToUpdate = dc.Posts.Where(p => p.Competitionid == competition1.id.ToString() && p.Mode != "Reject");
                    foreach (var post in postsToUpdate)
                    {
                        post.Mode = "Public";
                    }

                    dc.SubmitChanges();

                    dc.UpdateCompetitionEnd(DateTime.Now.ToString(DateTime.Now.ToString("dd-MM-yyyy HH:mm:00.000")), "End");

                    dc.UpdateJoinedCompetitionEnd(DateTime.Now.ToString("dd-MM-yyyy HH:mm:00.000"), "End");

                    AddInLeaderboard(""+competition1.id);

                    UpdateBalancesBasedOnPositions(""+competition1.id);

                    Context.Response.Write("{\"message\":" + js.Serialize("Updated") + "}");

                }
                else
                {
                    Context.Response.Write("{\"error\":" + js.Serialize("Competition not found") + "}");
                }
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }*/

        [WebMethod]
        public void CronJob(string token)
        {
            if (token.Equals("CRONJOBSHHKA"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);

                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:00.000");

                Context.Response.Write("{\"currentTime\":" + js.Serialize(currentTime) + "}");

                try
                {
                    dc.UpdateCompetition(currentTime, "Live");
                    dc.UpdateJoinedCompetition(currentTime, "Live");

                    var competition = dc.Competitions.FirstOrDefault(c => c.Liveon == currentTime);

                    if (competition != null)
                    {
                        var postsToUpdate = dc.Posts.Where(p => p.Competitionid == competition.id.ToString() && p.Mode != "Reject");
                        foreach (var post in postsToUpdate)
                        {
                            post.Mode = "Public";
                        }

                        dc.SubmitChanges();

                        Context.Response.Write("{\"message\":" + js.Serialize("Updated") + "}");
                    }
                    else
                    {
                        Context.Response.Write("{\"error\":" + js.Serialize("No competition found with Liveon time: " + currentTime) + "}");
                    }

                    var competition1 = dc.Competitions.FirstOrDefault(c => c.Endon == currentTime);

                    if (competition1 != null)
                    {
                        var postsToUpdate = dc.Posts.Where(p => p.Competitionid == competition1.id.ToString() && p.Mode != "Reject");
                        foreach (var post in postsToUpdate)
                        {
                            post.Mode = "Public";
                        }

                        dc.SubmitChanges();

                        dc.UpdateCompetitionEnd(currentTime, "End");
                        dc.UpdateJoinedCompetitionEnd(currentTime, "End");

                        AddInLeaderboard(competition1.id.ToString());

                        UpdateBalancesBasedOnPositions(competition1.id.ToString());

                        Context.Response.Write("{\"message\":" + js.Serialize("Updated") + "}");
                    }
                    else
                    {
                        Context.Response.Write("{\"error\":" + js.Serialize("No competition found with Endon time: " + currentTime) + "}");
                    }
                }
                catch (Exception ex)
                {
                    Context.Response.Write("{\"error\":" + js.Serialize(ex.Message + " " + ex.StackTrace) + "}");
                }
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }


        [WebMethod]
        public void CronNotification(string token)
        {
            if (token.Equals("CRONJOBSHHKA"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                string formattedDate = DateTime.Now.ToString("M/d/yyyy h:mm:00 tt", CultureInfo.InvariantCulture);
                var list = dc.UpdateSchedule(formattedDate).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Notification Updated! " + formattedDate) + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void InsertPushNotification(string token, string Image, string Title, string Message, string Phone, string DateTime)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.InsertPushNotification(Image, Title, Message, Phone, DateTime).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Inserted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void DeletePushNotification(string token, int ID)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.DeletePushNotification(ID).ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Deleted!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void GetPushNotificationsByPhone(string token, string Phone)
        {
            if (token.Equals("HMNNOJMIFHAT"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.GetPushNotificationsByPhone(Phone).ToList();
                Context.Response.Write(js.Serialize(list));
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        [WebMethod]
        public void CronDaily(string token)
        {
            if (token.Equals("CRONJOBSHHKA"))
            {
                DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
                var list = dc.UpdateDayssCount().ToString();
                Context.Response.Write("{\"message\":" + js.Serialize("Notification Updated!") + "}");
            }
            else
            {
                Context.Response.Write("{\"message\":" + js.Serialize("Invalid Token!") + "}");
            }
        }

        public void UpdateBalancesBasedOnPositions(string competitionId)
        {
            DataClasses1DataContext dc = new DataClasses1DataContext(strcon);
            using (SqlConnection connection = new SqlConnection(strcon))
            {
                string leaderboardQuery = "SELECT TOP 3 * FROM Leaderboard WHERE Competitionid=@id ORDER BY LikeCount DESC";

                string competitionQuery = "SELECT * FROM Competition WHERE id=@id";

                SqlCommand leaderboardCommand = new SqlCommand(leaderboardQuery, connection);
                leaderboardCommand.CommandType = CommandType.Text;
                leaderboardCommand.Parameters.AddWithValue("@id", competitionId);

                SqlCommand competitionCommand = new SqlCommand(competitionQuery, connection);
                competitionCommand.CommandType = CommandType.Text;
                competitionCommand.Parameters.AddWithValue("@id", competitionId);

                connection.Open();

                string resultDate = "";
                string seats = "";
                string seatsLeftPercent = "";

                using (SqlDataReader compReader = competitionCommand.ExecuteReader())
                {
                    if (compReader.Read())
                    {
                        resultDate = compReader["ResultDate"].ToString();
                        seats = compReader["Seats"].ToString();
                        seatsLeftPercent = compReader["SeatsLeftPercent"].ToString();
                    }
                }

                using (SqlDataReader reader = leaderboardCommand.ExecuteReader())
                {
                    int position = 1;

                    while (reader.Read())
                    {
                        string phone = reader["Phone"].ToString();
                        int amount = 0;

                        switch (position)
                        {
                            case 1:
                                amount = int.Parse(resultDate);
                                dc.InsertNotification("Competition Win!" , "Hey, congratulations! You have won the competition.", phone, DateTime.Now.ToString()).ToString();
                                dc.InsertHistory("Competition Win!", DateTime.Now.ToString(), ""+amount,phone,"Added").ToString();
                                SendNotification(getToken(phone), "Competition Win!", "Hey, congratulations! You have won the competition.", "");
                                break;
                            case 2:
                                amount = int.Parse(seats);
                                dc.InsertNotification("Competition Win!", "Hey, congratulations! You have won the competition.", phone, DateTime.Now.ToString()).ToString();
                                dc.InsertHistory("Competition Win!", DateTime.Now.ToString(), "" + amount, phone, "Added").ToString();
                                SendNotification(getToken(phone), "Competition Win!", "Hey, congratulations! You have won the competition.", "");
                                break;
                            case 3:
                                amount = int.Parse(seatsLeftPercent);
                                dc.InsertNotification("Competition Win!", "Hey, congratulations! You have won the competition.", phone, DateTime.Now.ToString()).ToString();
                                dc.InsertHistory("Competition Win!", DateTime.Now.ToString(), "" + amount, phone, "Added").ToString();
                                SendNotification(getToken(phone), "Competition Win!", "Hey, congratulations! You have won the competition.", "");
                                break;
                            default:
                             
                                dc.InsertNotification("Competition Ended", "Oh no, you lost the competition. Better luck next time!", phone, DateTime.Now.ToString()).ToString();
                                SendNotification(getToken(phone), "Competition Ended", "Oh no, you lost the competition. Better luck next time!", "");
                                break;
                        }

                        
                        dc.UpdateBalanceAdd(amount, phone).ToString();
                        
                        position++;
                    }
                }
            }
        }

        string getToken(string id)
        {
            SqlConnection conn = new SqlConnection(strcon);
            conn.Open();
            SqlCommand cmd = new SqlCommand("select FCMToken from Register where Phone='" + id + "'", conn);
            string k = (string)cmd.ExecuteScalar();
            conn.Close();
            return k;
        }

        [WebMethod]
        public void AddInLeaderboard(string id)
        {
            using (SqlConnection connection = new SqlConnection(strcon))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(@"
            SELECT P.id AS VideoId,
                   COUNT(L.id) AS LikeCount,
                   P.createorcontact AS Phone,
                   R.Firstname,
                   R.Lastname,
                   R.EmailId,
                   R.Gender,
                   R.Birthday,
                   R.Image,
                   R.Bio,
                   R.Insta,
                   R.YT,
                   @CompetitionID AS CompetitionID
            FROM Post AS P
            LEFT JOIN LikeVideo AS L ON P.id = L.Videoid
            LEFT JOIN Register AS R ON P.createorcontact = R.Phone
            WHERE P.Competitionid = @CompetitionID and P.Mode='Public'
            GROUP BY P.id, P.createorcontact, R.Firstname, R.Lastname, R.EmailId, R.Gender, R.Birthday, R.Image, R.Bio, R.Insta, R.YT
            ORDER BY LikeCount DESC;", connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CompetitionID", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable leaderboardData = new DataTable();
                        leaderboardData.Load(reader);

                        foreach (DataRow row in leaderboardData.Rows)
                        {
                            InsertIntoLeaderboard(row, connection, id);
                        }
                    }
                }
            }
        }


        static void InsertIntoLeaderboard(DataRow row, SqlConnection connection,string id)
        {
            using (SqlCommand insertCmd = new SqlCommand("INSERT INTO Leaderboard (VideoId,Competitionid, LikeCount, Phone, FirstName, LastName, EmailId, Gender, Birthday, Image, Bio, Insta, YT) VALUES (@VideoId,@Competitionid, @LikeCount, @Phone, @FirstName, @LastName, @EmailId, @Gender, @Birthday, @Image, @Bio, @Insta, @YT)", connection))
            {
                insertCmd.Parameters.AddWithValue("@VideoId", row["VideoId"]);
                insertCmd.Parameters.AddWithValue("@Competitionid", id);
                insertCmd.Parameters.AddWithValue("@LikeCount", row["LikeCount"]);
                insertCmd.Parameters.AddWithValue("@Phone", row["phone"]);
                insertCmd.Parameters.AddWithValue("@FirstName", row["Firstname"]);
                insertCmd.Parameters.AddWithValue("@LastName", row["Lastname"]);
                insertCmd.Parameters.AddWithValue("@EmailId", row["EmailId"]);
                insertCmd.Parameters.AddWithValue("@Gender", row["Gender"]);
                insertCmd.Parameters.AddWithValue("@Birthday", "");
                insertCmd.Parameters.AddWithValue("@Image", row["Image"]);
                insertCmd.Parameters.AddWithValue("@Bio", row["Bio"]);
                insertCmd.Parameters.AddWithValue("@Insta", row["Insta"]);
                insertCmd.Parameters.AddWithValue("@YT", row["YT"]);

                insertCmd.ExecuteNonQuery();
            }
        }

    }
}
