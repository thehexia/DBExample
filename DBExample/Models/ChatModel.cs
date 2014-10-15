using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBExample.Models
{
    public class ChatRoomModel 
    {
        [Key]
        public int ChatID { get; set; }

        [Required]
        public string Owner { get; set; }

        [Required]
        public string ChatRoomName { get; set; }
    }

    public class MessageModel
    {
        [Key]
        public int MessageID { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public int ChatID { get; set; }

        [ForeignKey("ChatID")]
        public virtual ChatRoomModel ChatRoom { get; set; }
    }
}