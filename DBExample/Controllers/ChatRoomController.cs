﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using DBExample.Models;
using System.Diagnostics;

namespace DBExample.Controllers
{
    public class ChatRoomController : Controller
    {
        //
        // GET: /ChatRoom/
        private string connectionString = "Data Source=(LocalDb)\\v11.0;Initial Catalog=aspnet-DBExample-20140921191911;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\\aspnet-DBExample-20140921191911.mdf";

        public ActionResult Index()
        {
            //create the table if its not already created
            //not really appropriate but I'm lazy
            //it'll fail quietly if the tables exist anyway
            CreateTable(connectionString);

            //find a list of all the chatrooms
            List<ChatRoomModel> chatRooms = new List<ChatRoomModel>();
            //make the query happen
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();

                command.CommandText = "select * from ChatRooms;";
                SqlDataReader reader = command.ExecuteReader();

                //read a list
                while (reader.Read())
                {
                    ChatRoomModel model = new ChatRoomModel();
                    model.ChatID = Convert.ToInt32(reader["ChatID"]);
                    model.ChatRoomName = reader["ChatRoomName"].ToString();
                    model.Owner = reader["Owner"].ToString();
                    chatRooms.Add(model);
                }
                connection.Close();
            }

            return View(chatRooms);
        }

        public ActionResult CreateChatRoom()
        {
            return View();
        }

        public ActionResult Create(string name)
        {
            //create the chatroom
            ChatRoomModel chatRoom = new ChatRoomModel();
            chatRoom.ChatRoomName = name;
            chatRoom.Owner = User.Identity.Name;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "insert into ChatRooms (ChatRoomName, Owner) values(@ChatRoomName, @Owner)";
                command.Parameters.AddWithValue("@ChatRoomName", chatRoom.ChatRoomName);
                command.Parameters.AddWithValue("@Owner", chatRoom.Owner);
                command.ExecuteNonQuery();
                connection.Close();
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult DeleteConfirm(int ChatID)
        {
            ViewBag.ChatID = ChatID;
            return View();
        }

        [Authorize]
        public ActionResult Delete(int ChatID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                //check if there was a chatroom first
                command.CommandText = "SELECT Owner FROM ChatRooms WHERE ChatID=" + ChatID + ";";
                //if the user owns it then go ahead, else break
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader["Owner"].ToString() != User.Identity.Name)
                        {
                            reader.Close();
                            return RedirectToAction("Index");
                        }
                    }
                    reader.Close();
                }
                else
                {
                    reader.Close();
                    return RedirectToAction("Index");
                }

                //delete the messages from that chatroom
                command.CommandText = @"DELETE FROM Messages
                                        WHERE ChatID=" + ChatID + ";";
                command.ExecuteNonQuery();

                //delete the chat room
                command.CommandText = @"DELETE FROM ChatRooms
                                        WHERE ChatID=" + ChatID + ";";
                command.ExecuteNonQuery();

                connection.Close();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Messages(int ChatID)
        {
            //find a list of all the chatrooms
            List<MessageModel> messages = new List<MessageModel>();
            //make the query happen
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();

                command.CommandText = @"SELECT *
                                        FROM Messages
                                        WHERE ChatID=" + ChatID + ";";
                SqlDataReader reader = command.ExecuteReader();

                //read a list
                while (reader.Read())
                {
                    MessageModel model = new MessageModel();
                    model.MessageID = Convert.ToInt32(reader["MessageID"]);
                    model.Message = reader["Message"].ToString();
                    model.Author = reader["Author"].ToString();
                    model.Timestamp = reader.GetDateTime(3);
                    model.ChatID = Convert.ToInt32(reader["ChatID"]);
                    messages.Add(model);
                }
                connection.Close();
            }
            ViewBag.ChatID = ChatID;
            return View(messages);
        }

        [Authorize]
        public ActionResult AddMessageForm(int ChatID)
        {
            ViewBag.ChatID = ChatID;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddMessage(string Message, int ChatID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "insert into Messages (Message, Author, Timestamp, ChatID ) values(@Message, @Author, @Timestamp, @ChatID)";
                command.Parameters.AddWithValue("@Message", Message);
                command.Parameters.AddWithValue("@Author", User.Identity.Name.ToString());
                command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                command.Parameters.AddWithValue("@ChatID", ChatID);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return RedirectToAction("Messages", new { ChatID = ChatID });
        }

        [Authorize]
        public ActionResult EditMessageForm(int MessageID)
        {
            ViewBag.MessageID = MessageID;
            return View();
        }

        [Authorize]
        public ActionResult EditMessage(int MessageID, string Message)
        {
            int ChatID = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();

                command.CommandText = "SELECT ChatID FROM Messages WHERE MessageID=" + MessageID + ";";
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while(reader.Read()) {
                        ChatID = Convert.ToInt32(reader["ChatID"]);
                    }

                    command.CommandText = @"UPDATE Messages
                                            SET Message='" + Message + "'" +
                                           "WHERE MessageID=" + MessageID + ";";
                    reader.Close();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            return RedirectToAction("Messages", new { ChatID = ChatID });
        }

        //creates a table
        //@param connection string
        private void CreateTable(string conStr)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                try
                {
                    //
                    // Open the SqlConnection.
                    //
                    con.Open();
                    //
                    // The following code uses an SqlCommand based on the SqlConnection.
                    //
                    using (SqlCommand chatrooms = new SqlCommand(@"CREATE TABLE ChatRooms
                                                                 ( ChatID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
                                                                   Owner varchar(100) NOT NULL,
                                                                   ChatRoomName varchar(100));", con))
                        chatrooms.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }

                try
                {
                    using (SqlCommand messages = new SqlCommand(@"CREATE TABLE Messages
                                                                ( MessageID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
                                                                  Message varchar(1000),
                                                                  Author varchar(100),
                                                                  Timestamp DATETIME NOT NULL,
                                                                  ChatID INT NOT NULL,
                                                                  FOREIGN KEY(ChatID) REFERENCES ChatRooms );", con))
                        messages.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }

                con.Close();
            }
        }
    }
}
