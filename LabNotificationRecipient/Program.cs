using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabNotificationRecipient
{
    class Program
    {
        static void Main(string[] args)
        {
            LabNotificationRecipient labRecipient = new LabNotificationRecipient();
            labRecipient.SendMail();

        }
    }
}
