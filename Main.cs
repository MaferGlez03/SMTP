// namespace program
// {
//     public class Program
//     {
//         public static void Main()
//         {
//             // Crear el servidor SMTP
//             SmtpServer server = new SmtpServer();
//             Thread serverThread = new Thread(new ThreadStart(server.Start));
//             serverThread.Start();

//             // Crear los clientes
//             Client client1 = new Client("client1@example.com");
//             Client client2 = new Client("client2@example.com");

//             // El cliente1 envía un correo electrónico al cliente2
//             Email email = new Email
//             {
//                 From = "client1@example.com",
//                 To = "client2@example.com",
//                 Subject = "Hola",
//                 Content = "lorem ipsum",
//                 Received = DateTime.Now,
//                 Unread = true,
//                 HasAttachment = false,
//                 IsImportant = false
//             };


//             client1.SendEmail(email);
//             // El cliente2 recibe el correo electrónico
//             Email receivedEmail = client2.ReceiveEmail(0);
//             Console.WriteLine($"Received email: {receivedEmail.Content}");
//             Console.ReadLine();
//             // Detener el servidor
//             server.Stop();
//         }
//     }
// }

using System;
using program;

class Program
{
    static void Main(string[] args)
    {
        // Crear una instancia del servidor SMTP
        SmtpServer server = new SmtpServer();
        List<Client> clients = new List<Client>();
        bool serverStart = false;
        Client mainClient = null!;

        while (true)
        {
            Console.WriteLine("Ingrese un comando:");
            string command = Console.ReadLine()!;

            if (command == "start server")
            {
                if (serverStart)
                {
                    Console.WriteLine("Ya hay un server iniciado");
                    continue;
                }
                serverStart = true;
                Thread serverThread = new Thread(new ThreadStart(server.Start));
                serverThread.Start();
                Console.WriteLine("Servidor iniciado.");
            }
            else if (command == "stop server")
            {
                if (!serverStart)
                {
                    Console.WriteLine("No hay server iniciado");
                    continue;
                }
                server.Stop();
                Console.WriteLine("Servidor detenido.");
            }
            else if (command.StartsWith("login "))
            {
                string userNameToFind = command.Substring(5);
                Client foundClient = null!;
                foreach (Client client in clients)
                {
                    if (client.username == userNameToFind)
                    {
                        foundClient = client;
                        break;
                    }
                }

                if (foundClient == null)
                {
                    Console.WriteLine($"No se encontró ningún cliente con el nombre de usuario '{userNameToFind}'");
                    continue;
                }
                else
                {
                    mainClient = foundClient;
                    Console.WriteLine("Login exitoso");
                }
            }
            else if (command.StartsWith("create client "))
            {
                string userName = command.Substring("create client ".Length);
                Client client = new Client(userName);
                clients.Add(client);
                Console.WriteLine($"Cliente {userName} creado.");
            }
            else if (command == "create email")
            {
                // Aquí necesitarías alguna forma de parsear el resto del comando para crear el correo electrónico
                // Podrías pedirle al usuario que ingrese los detalles del correo electrónico después de este comando
                if(mainClient == null)
                {
                    Console.WriteLine("Necesitas logearte primero");
                    continue;
                }

                Console.WriteLine("Creating email...");
                Console.Write("From: ");
                string from = Console.ReadLine()!;
                Console.Write("To: ");
                string to = Console.ReadLine()!;
                Console.Write("Subject: ");
                string subject = Console.ReadLine()!;
                Console.Write("Content: ");
                string content = Console.ReadLine()!;

                Email email = new Email
                {
                    From = from,
                    To = to,
                    Subject = subject,
                    Content = content,
                    Received = DateTime.Now,
                    Unread = true,
                    HasAttachment = false,
                    IsImportant = false
                };

                mainClient.mailbox.DraftEmails.Add(email);
            }
            else if (command.StartsWith("send email "))
            {
                // Aquí necesitarías alguna forma de buscar al cliente que está enviando el correo electrónico
                // y luego parsear el resto del comando para crear el correo electrónico
                if(mainClient == null)
                {
                    Console.WriteLine("Necesitas logearte primero");
                    continue;
                }

                string[] parts = command.Split(' ');
                string userNameToFind;
                string subjectToFind;

                if (parts.Length == 4)
                {
                    userNameToFind = parts[2];
                    subjectToFind = parts[3];
                }
                else
                {
                    Console.WriteLine("El comando no tiene el formato correcto.");
                    continue;
                }

                Email foundEmail = null!;
                foreach (Email email in mainClient.mailbox.DraftEmails)
                {
                    if (email.Subject == subjectToFind)
                    {
                        foundEmail = email;
                        break;
                    }
                }

                if (foundEmail == null)
                {
                    Console.WriteLine($"No se encontró ningún correo electrónico con el asunto '{subjectToFind}'");
                    continue;
                }

                Client foundClient = null!;
                foreach (Client client in clients)
                {
                    if (client.username == userNameToFind)
                    {
                        foundClient = client;
                        break;
                    }
                }

                if (foundClient == null)
                {
                    Console.WriteLine($"No se encontró ningún cliente con el nombre de usuario '{userNameToFind}'");
                    continue;
                }

                foundClient.SendEmail(foundEmail);
                mainClient.mailbox.SendEmails.Add(foundEmail);
                mainClient.mailbox.DraftEmails.Remove(foundEmail);
                Console.WriteLine("Correo enviado satisfactoriamente.");
            }
            else if (command.StartsWith("receive email "))
            {
                // Similarmente, aquí necesitarías buscar al cliente que está recibiendo el correo electrónico
                // y luego parsear el número de correo electrónico del comando
                string[] parts = command.Split(' ');
                string userNameToFind = parts[2];
                
                int i = 0;
                foreach (Email email in server.emails)
                {
                    if (email.To == userNameToFind)
                    {
                        break;
                    }
                    i++;
                }

                if (i == server.emails.Count)
                {
                    Console.WriteLine("No se encontró ningún email para recibir");
                    continue;
                }
                
                Client foundClient = null!;
                foreach (Client client in clients)
                {
                    if (client.username == userNameToFind)
                    {
                        foundClient = client;
                        break;
                    }
                }


                if (foundClient == null)
                {
                    Console.WriteLine($"No se encontró ningún cliente con el nombre de usuario '{userNameToFind}'");
                    continue;
                }

                ;
                foundClient.mailbox.ReceivedEmails.Add(foundClient.ReceiveEmail(i));
                Console.WriteLine("Correo recibido satisfactoriamente.");
            }
            else if (command == "help")
            {
                Console.WriteLine("Comandos disponibles:");
                Console.WriteLine("  start server: Inicia el servidor SMTP");
                Console.WriteLine("  stop server: Detiene el servidor SMTP");
                Console.WriteLine("  create client <nombre>: Crea un nuevo cliente con el nombre especificado");
                Console.WriteLine("  create email: Crea un nuevo correo electrónico");
                Console.WriteLine("  send email <nombre del cliente> <correo electrónico>: Envía un correo electrónico desde el cliente especificado");
                Console.WriteLine("  receive email <nombre del cliente>: Recibe un correo electrónico en el cliente especificado");
                Console.WriteLine("  exit: Sale del programa");
            }
            else if (command == "exit")
            {
                break;
            }
            else
            {
                Console.WriteLine("Comando no reconocido. Escribe 'help' para ver la lista de comandos disponibles.");
            }
        }
    }
}
