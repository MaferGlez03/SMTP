using System.Net.Sockets;
using System.Net;

namespace program
{
    public class SmtpServer
    {
        private TcpListener _listener;  
        private bool _isRunning;
        public List<Email> emails = new List<Email>();
        public List<Client> clients = new List<Client>();

        // Variable para almacenar el correo electrónico
        private Email LastEmail { get; set; } = new Email { };

        public SmtpServer()
        {
            _listener = new TcpListener(IPAddress.Any, 25);
            _isRunning = false;
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;

            while (_isRunning)
            {
                //Esto evita aceptar una nueva conexión después de que el TcpListener haya sido detenido.
                if (!_listener.Pending())
                {
                    Thread.Sleep(1500); // Espera segundo y medio antes de comprobar de nuevo
                    continue;
                }
                var client = _listener.AcceptTcpClient();
                var stream = client.GetStream();
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream);

                writer.WriteLine("220 smtp.example.com ESMTP Postfix");
                writer.Flush();

                string from = "", to = "", subject = "";

                while (true)
                {
                    string line = "";
                    line += reader.ReadLine();

                    if (line.StartsWith("RETR"))
                    {
                        string username = line.Substring(5);
                        
                        
                        Console.WriteLine("Mensaje recibido por server, listo para buscar");
                        Console.WriteLine(username);

                        if (emails.Count > 0)
                        {
                            foreach (Email email in emails)
                            {
                                Console.WriteLine();
                                Console.WriteLine(email.To);

                                if (email.To == username)
                                {
                                    writer.WriteLine(email.ToString());
                                    writer.Flush();
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    else if (line.StartsWith("HELO"))
                    {
                        writer.WriteLine("250 Ok");
                    }
                    else if (line.StartsWith("MAIL FROM:"))
                    {
                        from = line.Substring(10);
                        writer.WriteLine("250 Ok");
                    }
                    else if (line.StartsWith("RCPT TO:"))
                    {
                        to = line.Substring(8);
                        writer.WriteLine("250 Ok");
                    }
                    else if (line.StartsWith("RSET"))
                    {
                        // Restablece la transacción actual
                        from = "";
                        to = "";
                        subject = "";
                        writer.WriteLine("250 OK");
                    }
                    else if (line.StartsWith("VRFY"))
                    {
                        // Extrae la dirección de correo electrónico del comando
                        string email = line.Substring(5);

                        bool emailExists = VerifyEmail(email);

                        // Responde al cliente
                        if (emailExists)
                        {
                            writer.WriteLine("250 " + email);
                        }
                        else
                        {
                            writer.WriteLine("550 No such user");
                        }
                    }
                    else if (line.StartsWith("EXPN"))
                    {
                        // Extrae el alias de correo del comando
                        string alias = line.Substring(5);

                        // Busca en la lista 'emails' para ver si el alias de correo existe
                        List<string> emailList = ExpandAlias(alias);

                        // Responde al cliente
                        if (emailList.Count > 0)
                        {
                            foreach (string email in emailList)
                            {
                                writer.WriteLine("250 " + email);
                            }
                        }
                        else
                        {
                            writer.WriteLine("550 No such user");
                        }
                    }
                    else if (line.StartsWith("NOOP"))
                    {
                        // No realiza ninguna operación y responde al cliente
                        writer.WriteLine("250 OK");
                    }
                    else if (line.StartsWith("DATA"))
                    {
                        writer.WriteLine("354 End data with <CR><LF>.<CR><LF>");

                        while (!line.EndsWith("\r\n.\r\n"))
                        {
                            line += reader.ReadLine() + "\r\n";
                            if (line.StartsWith("Subject: "))
                            {
                                subject = line.Substring(9);
                            }
                        }

                        // Almacenar el correo electrónico en la variable LastEmail
                        LastEmail = new Email
                        {
                            From = from,
                            To = to,
                            Subject = subject,
                            Content = line,
                            Received = DateTime.Now,
                            Unread = true,
                            HasAttachment = false,
                            IsImportant = false
                        };

                        emails.Add(LastEmail);

                        writer.WriteLine("250 OK");
                    }
                    else if (line.StartsWith("QUIT"))
                    {
                        writer.WriteLine("221 Bye");
                        break;
                    }

                    writer.Flush();
                }

                client.Close();
            }
        }
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        private bool VerifyEmail(string email)
        {
            // Busca en la lista 'emails' para ver si la dirección de correo electrónico existe.
            foreach (Email e in emails)
            {
                if (e.From == email || e.To == email)
                {
                    // Si se encuentra la dirección de correo electrónico, devuelve true.
                    return true;
                }
            }

            // Si no se encuentra la dirección de correo electrónico, devuelve false.
            return false;
        }

        private List<string> ExpandAlias(string alias)
        {
            List<string> emailList = new List<string>();

            // Busca en la lista 'emails' para ver si el alias de correo existe
            foreach (Email e in emails)
            {
                if (e.From.Contains(alias) || e.To.Contains(alias))
                {
                    emailList.Add(e.From);
                    emailList.Add(e.To);
                }
            }

            return emailList;
        }
    }
}





































































// public class SmtpServer
//     {
//         private TcpListener _listener;
//         private bool _isRunning;
//         public List<Email> emails = new List<Email>();
//         public List<Client> clients = new List<Client>();
//         private Email LastEmail { get; set; } = new Email { };

//         public SmtpServer()
//         {
//             _listener = new TcpListener(IPAddress.Any, 25);
//             _isRunning = false;
//         }

//         public void Start()
//         {
//             _listener.Start();
//             _isRunning = true;

//             while (_isRunning)
//             {
//                 if (!_listener.Pending())
//                 {
//                     Thread.Sleep(500);
//                     continue;
//                 }
//                 var client = _listener.AcceptTcpClient();
//                 var stream = client.GetStream();
//                 var reader = new StreamReader(stream);
//                 var writer = new StreamWriter(stream);

//                 writer.WriteLine("220 smtp.example.com ESMTP Postfix");
//                 writer.Flush();

//                 string from = "", to = "", subject = "";

//                 while (true)
//                 {
//                     string line = "";
//                     line += reader.ReadLine();

//                     if (line.StartsWith("RETR"))
//                     {
//                         int emailNumber = int.Parse(line.Substring(5));
//                         if (emails.Count > 0)
//                         {
//                             Email email = emails[emailNumber];

//                             writer.WriteLine(email.ToString());
//                             writer.Flush();
//                             break;
//                         }
//                         return;
//                     }
//                     else if (line.StartsWith("HELO"))
//                     {
//                         writer.WriteLine("250 Ok");
//                     }
//                     else if (line.StartsWith("MAIL FROM:"))
//                     {
//                         from = line.Substring(10);
//                         writer.WriteLine("250 Ok");
//                     }
//                     else if (line.StartsWith("RCPT TO:"))
//                     {
//                         to = line.Substring(8);
//                         writer.WriteLine("250 Ok");
//                     }
//                     else if (line.StartsWith("DATA"))
//                     {
//                         writer.WriteLine("354 End data with <CR><LF>.<CR><LF>");

//                         while (!line.EndsWith("\r\n.\r\n"))
//                         {
//                             line += reader.ReadLine() + "\r\n";
//                             if (line.StartsWith("Subject: "))
//                             {
//                                 subject = line.Substring(9);
//                             }
//                         }

//                         LastEmail = new Email
//                         {
//                             From = from,
//                             To = to,
//                             Subject = subject,
//                             Content = line,
//                             Received = DateTime.Now,
//                             Unread = true,
//                             HasAttachment = false,
//                             IsImportant = false
//                         };

//                         writer.WriteLine("250 OK");
//                     }
//                     else if (line.StartsWith("QUIT"))
//                     {
//                         writer.WriteLine("221 Bye");
//                         break;
//                     }

//                     writer.Flush();
//                 }

//                 client.Close();
//             }
//         }
//         public void Stop()
//         {
//             _isRunning = false;
//             _listener.Stop();
//         }
//     }