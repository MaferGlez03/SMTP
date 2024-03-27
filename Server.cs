using System.Net.Sockets;
using System.Net;
using SMTPClient;

namespace program
{
    public class SmtpServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private List<Email> emails = new List<Email>();

        // Variable para almacenar el correo electrónico
        public Email LastEmail { get; private set; } = new Email { };

        public SmtpServer()
        {
            _listener = new TcpListener(IPAddress.Any, 25);
            _isRunning = false;
        }

        public void Start()
        {
            Console.WriteLine("Start");
            _listener.Start();
            _isRunning = true;

            while (_isRunning)
            {
                //Esto evita aceptar una nueva conexión después de que el TcpListener haya sido detenido.
                if (!_listener.Pending())
                {
                    Thread.Sleep(500); // Espera medio segundo antes de comprobar de nuevo
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
                        int emailNumber = int.Parse(line.Substring(5));
                        if (emails.Count > 0)
                        {
                            Email email = emails[emailNumber];  // Suponiendo que "emails" es una lista de correos electrónicos

                            writer.WriteLine(email.ToString());
                            writer.Flush();
                            break;
                        }
                        return;
                    }
                    else if (line.StartsWith("HELO"))
                    {
                        writer.WriteLine("250 Hello");
                    }
                    else if (line.StartsWith("MAIL FROM:"))
                    {
                        from = line.Substring(10);
                        writer.WriteLine("250 OK");
                    }
                    else if (line.StartsWith("RCPT TO:"))
                    {
                        to = line.Substring(8);
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

                        LastEmail.Open();
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
    }
}