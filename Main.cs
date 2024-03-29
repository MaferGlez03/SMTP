namespace program
{
    public class Program
    {
        public static void Main()
        {
            // Crear el servidor SMTP
            SmtpServer server = new SmtpServer();
            Thread serverThread = new Thread(new ThreadStart(server.Start));
            serverThread.Start();

            // Crear los clientes
            Client client1 = new Client("client1@example.com");
            Client client2 = new Client("client2@example.com");

            // El cliente1 envía un correo electrónico al cliente2
            Email email = new Email
            {
                From = "client1@example.com",
                To = "client2@example.com",
                Subject = "Hola",
                Content = "lorem ipsum",
                Received = DateTime.Now,
                Unread = true,
                HasAttachment = false,
                IsImportant = false
            };


            client1.SendEmail(email);
            // El cliente2 recibe el correo electrónico
            Email receivedEmail = client2.ReceiveEmail(0);
            Console.WriteLine($"Received email: {receivedEmail.Content}");
            Console.ReadLine();
            // Detener el servidor
            server.Stop();
        }
    }
}