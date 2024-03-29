using System.Net.Sockets;
using System.Text;

namespace program
{
    public class Client
    {
        public string username { get; set; }
        public Mailbox mailbox { get; set; }
        public Client(string userName)
        {
            username = userName;
            mailbox = new Mailbox();
        }

        public void SendEmail(Email email)
        {
            // Establece los parámetros del correo electrónico: Define el servidor SMTP (localhost), el puerto SMTP (25), el correo electrónico del remitente (this.From), 
            // el correo electrónico del destinatario ("destinatario@example.com"), el asunto del correo electrónico (this.Subject) y el cuerpo del correo electrónico (this.Content).
            string server = "localhost";
            int port = 25; // Puerto SMTP por defecto
            string fromEmail = email.From;
            string toEmail = email.To; // Deberías agregar un campo 'To' en tu clase 'Email'
            string subject = email.Subject;
            string body = email.Content;

            // Crea una nueva conexión TCP al servidor SMTP: Utiliza la clase TcpClient para establecer una nueva conexión al servidor SMTP en el puerto especificado.
            TcpClient client = new TcpClient(server, port);
            // Obtiene el flujo de red: Utiliza el método GetStream para obtener el NetworkStream asociado a la conexión TCP. Este flujo se utiliza para enviar y recibir datos.
            NetworkStream ns = client.GetStream();
            byte[] dataBuffer;
            string responseString;

            // Envía el comando HELO al servidor SMTP: Este es el primer comando que se envía a un servidor SMTP para iniciar la conversación. El servidor debe responder con un 
            // código de estado 250 para indicar que está listo para aceptar comandos.
            dataBuffer = Encoding.ASCII.GetBytes($"HELO {server}\r\n");
            ns.Write(dataBuffer, 0, dataBuffer.Length);
            dataBuffer = new byte[1024];
            ns.Read(dataBuffer, 0, dataBuffer.Length);
            responseString = Encoding.ASCII.GetString(dataBuffer);

            // Envía el comando MAIL FROM al servidor SMTP: Este comando indica el remitente del correo electrónico. El servidor debe responder con un código de estado 250 para indicar 
            // que el remitente es aceptable.
            dataBuffer = Encoding.ASCII.GetBytes($"MAIL FROM:<{fromEmail}>\r\n");
            ns.Write(dataBuffer, 0, dataBuffer.Length);
            dataBuffer = new byte[1024];
            ns.Read(dataBuffer, 0, dataBuffer.Length);
            responseString = Encoding.ASCII.GetString(dataBuffer);

            // Envía el comando RCPT TO al servidor SMTP: Este comando indica el destinatario del correo electrónico. El servidor debe responder con un código de estado 250 para indicar 
            // que el destinatario es aceptable.
            dataBuffer = Encoding.ASCII.GetBytes($"RCPT TO:<{toEmail}>\r\n");
            ns.Write(dataBuffer, 0, dataBuffer.Length);
            dataBuffer = new byte[1024];
            ns.Read(dataBuffer, 0, dataBuffer.Length);
            responseString = Encoding.ASCII.GetString(dataBuffer);

            // Envía el comando DATA al servidor SMTP: Este comando indica al servidor que el cliente está listo para enviar el cuerpo del correo electrónico. El servidor debe responder 
            // con un código de estado 354 para indicar que está listo para recibir los datos.
            dataBuffer = Encoding.ASCII.GetBytes($"DATA\r\n");
            ns.Write(dataBuffer, 0, dataBuffer.Length);
            dataBuffer = new byte[1024];
            ns.Read(dataBuffer, 0, dataBuffer.Length);
            responseString = Encoding.ASCII.GetString(dataBuffer);

            // Envía el cuerpo del correo electrónico al servidor SMTP: En este punto, se envía el asunto y el cuerpo del correo electrónico. El cuerpo del correo electrónico se termina 
            // con una línea que contiene solo un punto (.\r\n). El servidor debe responder con un código de estado 250 para indicar que el correo electrónico ha sido aceptado para su entrega.
            dataBuffer = Encoding.ASCII.GetBytes($"Subject: {subject}\r\n\r\n{body}\r\n.\r\n");
            ns.Write(dataBuffer, 0, dataBuffer.Length);
            dataBuffer = new byte[1024];
            ns.Read(dataBuffer, 0, dataBuffer.Length);
            responseString = Encoding.ASCII.GetString(dataBuffer);

            // Envía el comando QUIT al servidor SMTP: Este comando indica al servidor que el cliente ha terminado de enviar comandos. El servidor debe responder con 
            // un código de estado 221 para indicar que está cerrando la conexión.
            dataBuffer = Encoding.ASCII.GetBytes("QUIT\r\n");
            ns.Write(dataBuffer, 0, dataBuffer.Length);
            dataBuffer = new byte[1024];
            ns.Read(dataBuffer, 0, dataBuffer.Length);
            responseString = Encoding.ASCII.GetString(dataBuffer);

            // Cierra el flujo de red y la conexión TCP: Finalmente, se cierra el NetworkStream y la conexión TCP con el servidor SMTP.
            ns.Close();
            client.Close();
        }

        public Email ReceiveEmail(int emailNumber)
        {
            string server = "localhost";
            int port = 25; // Puerto SMTP por defecto

            // Crea una nueva conexión TCP al servidor SMTP: Utiliza la clase TcpClient para establecer una nueva conexión al servidor SMTP en el puerto especificado.
            TcpClient client = new TcpClient(server, port);
            // Obtiene el flujo de red: Utiliza el método GetStream para obtener el NetworkStream asociado a la conexión TCP. Este flujo se utiliza para enviar y recibir datos.
            NetworkStream ns = client.GetStream();
            var reader = new StreamReader(ns);
            var writer = new StreamWriter(ns);


            // Envía el comando RETR
            writer.WriteLine($"RETR {emailNumber}\r\n");
            writer.Flush();

            // Lee la respuesta del servidor
            string response = "";
            response += reader.ReadLine();

            // Parsea la respuesta y crea un objeto Email
            Email email = Auxiliar.ParseEmail(response);

            return email;
        }
    }
    //Crear un email
    public class Email
    {
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Received { get; set; }
        public bool Unread { get; set; }
        public bool HasAttachment { get; set; }
        public bool IsImportant { get; set; }

        //Ver el Content del email
        public void Open()
        {
            Unread = false;
            Console.WriteLine("Contenido del correo: " + Content);
        }

        //Cambiar la prioridad de un email
        public void ChangePriority()
        {
            IsImportant = !IsImportant;
        }

        //Imprimir todos los detalles del email. Quizas lo quito, no me queda clara la utilidad
        public override string ToString()
        {
            return $"From: {From}\nTo: {To}\nSubject: {Subject}\nReceived: {Received}\nUnread: {Unread}\nHasAttachment: {HasAttachment}\nEs importante: {IsImportant}\n{Content}";
        }
    }

    //Crear un Mailbox (Buzon de correo)
    public class Mailbox
    {
        public List<Email> Emails { get; set; } = new List<Email>();

        //Ordenar por orden alfabetico segun el From de los correos
        public void SortByFrom()
        {
            Emails = Emails.OrderBy(email => email.From).ToList();
        }

        //Ordenar por orden alfabetico segun el Subject de los correos
        public void SortBySubject()
        {
            Emails = Emails.OrderBy(email => email.Subject).ToList();
        }

        //Ordenar por orden de llegada segun el Received de los correos
        public void SortByReceived()
        {
            Emails = Emails.OrderBy(email => email.Received).ToList();
        }

        //Buscar un correo por una aparicion de subject (lo que se busca) en algun Subject
        //Se imprimen todos los correos que matcheen
        public List<Email> SearchBySubject(string subject)
        {
            return Emails.Where(email => email.Subject.Contains(subject)).ToList();
        }

        //Buscar un correo por una aparicion de from (lo que se busca) en algun From
        //Se imprimen todos los correos que matcheen
        public List<Email> SearchByFrom(string from)
        {
            return Emails.Where(email => email.From.Contains(from)).ToList();
        }

        //Buscar un correo por una aparicion de received (lo que se busca) en algun Received
        //Se imprimen todos los correos que matcheen
        public List<Email> SearchByReceived(string received)
        {
            DateTime date = dateFormats(received);
            return Emails.Where(email => email.Received.Date == date.Date).ToList();
        }

        //Buscar un correo por una aparicion de content (lo que se busca) en algun Content
        //Se imprimen todos los correos que matcheen
        public List<Email> SearchByContent(string content)
        {
            return Emails.Where(email => email.Content.Contains(content)).ToList();
        }

        public List<Email> SearchEverything(string search)
        {
            var emailsBySubject = SearchBySubject(search);
            var emailsByFrom = SearchByFrom(search);
            var emailsByReceived = SearchByReceived(search);
            var emailsByContent = SearchByContent(search);

            var allEmails = emailsBySubject.Concat(emailsByFrom)
                                            .Concat(emailsByReceived)
                                            .Concat(emailsByContent)
                                            .Distinct() //Para eliminar duplicados
                                            .ToList();

            return allEmails;
        }

        //Imprimir el Mailbox completo
        public void PrintEmails(List<Email> emails)
        {
            Console.WriteLine("{0,-30} {1,-30} {2,-30}", "De", "Asunto", "Recibido");
            foreach (var email in emails)
            {
                Console.WriteLine("{0,-30} {1,-30} {2,-30}", email.From, email.Subject, email.Received);
            }
        }

        //Formatear las fechas de un string a un tipo DateTime
        private DateTime dateFormats(string Date)
        {
            string[] dateFormats = { "dd MMMM yyyy", "dd 'de' MMMM 'del' yyyy", "dd/MM/yyyy", "MM-dd-yyyy", "yyyy/MM/dd", "M/d/yyyy", "M/d/yy" };

            return DateTime.Now;
        }

        public void DeleteEmail(int index)
        {
            if (index >= 0 && index < Emails.Count)
            {
                Emails.RemoveAt(index);
                Console.WriteLine("Correo eliminado.");
            }
            else
            {
                Console.WriteLine("Índice de correo no válido.");
            }
        }


        //! Implementar el sistema de carpetas para los correos
        //? Mejorar el sistema de busqueda de correos (mas rapido y que no distinga entre mayusculas y minusculas y palabras con y sin tilde, etc)
    }

    public static class Auxiliar
    {
        public static Email ParseEmail(string rawEmail)
        {
            Email email = new Email();
            string[] lines = rawEmail.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (line.StartsWith("From: "))
                {
                    email.From = line.Substring(6);
                }
                else if (line.StartsWith("To: "))
                {
                    email.To = line.Substring(4);
                }
                else if (line.StartsWith("Subject: "))
                {
                    email.Subject = line.Substring(9);
                }
                else if (line.StartsWith("Received: "))
                {
                    email.Received = dateFormats(line.Substring(11));
                }
                else if (line.StartsWith("Unread: "))
                {
                    email.Unread = boolFormat(line.Substring(9));
                }
                else if (line.StartsWith("HasAttachment: "))
                {
                    email.HasAttachment = boolFormat(line.Substring(16));
                }
                else if (line.StartsWith("isImportant: "))
                {
                    email.IsImportant = boolFormat(line.Substring(14));
                }
                else
                {
                    email.Content += line + "\n";
                }
            }

            return email;
        }
        public static DateTime dateFormats(string Date)
        {
            string[] dateFormats = { "dd MMMM yyyy", "dd 'de' MMMM 'del' yyyy", "dd/MM/yyyy", "MM-dd-yyyy", "yyyy/MM/dd", "M/d/yyyy", "M/d/yy" };

            return DateTime.Now;
        }
        public static Boolean boolFormat(string boolean)
        {
            if (boolean == "true") return true;
            else if (boolean == "false") return false;
            else return false;
        }
    }
}