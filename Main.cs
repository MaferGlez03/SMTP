using program;

class Program
{
    static void Main(string[] args)
    {
        // Crear una instancia del servidor SMTP
        SmtpServer server = new SmtpServer();
        List<Client> clients = new List<Client>();
        Client mainClient = null!;
        string lastView = "";

        Thread serverThread = new Thread(new ThreadStart(server.Start));
        serverThread.Start();
        Console.WriteLine("Servidor iniciado.");

        while (true)
        {
            Console.WriteLine("Ingrese un comando:");
            string command = Console.ReadLine()!;

            if (command.StartsWith("login "))
            {
                string userNameToFind = command.Substring(6);
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

                if (parts.Length == 3)
                {
                    subjectToFind = parts[2];
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

                userNameToFind = foundEmail.To;
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

                server.emails.Add(foundEmail);
                foundClient.SendEmail(foundEmail);
                mainClient.mailbox.SendEmails.Add(foundEmail);
                mainClient.mailbox.DraftEmails.Remove(foundEmail);
                Console.WriteLine("Correo enviado satisfactoriamente.");
            }
            else if (command.StartsWith("view folder "))
            {
                if(mainClient == null)
                {
                    Console.WriteLine("Necesitas logearte primero");
                    continue;
                }
                string listView;

                string[] parts = command.Split(' ');
                if (parts.Length == 3)
                {
                    listView = parts[2];
                }
                else
                {
                    Console.WriteLine("El comando no tiene el formato correcto.");
                    continue;
                }

                List<Email> list = mainClient.mailbox.DraftEmails;
                if (listView == "send") list = mainClient.mailbox.SendEmails;
                else if (listView == "receive") list = mainClient.mailbox.ReceivedEmails;
                else if(listView == "draft") list = mainClient.mailbox.DraftEmails;
                else
                {
                    Console.WriteLine("La lista seleccionada no existe");
                    continue;
                }

                lastView = listView;
                Auxiliar.PrintEmails(list);
            }
            else if (command.StartsWith("view email ")) 
            {
                if(mainClient == null)
                {
                    Console.WriteLine("Necesitas logearte primero");
                    continue;
                }

                string[] parts = command.Split(' ');
                string subjectToFind;

                if (parts.Length == 3)
                {
                    subjectToFind = parts[2];
                }
                else
                {
                    Console.WriteLine("El comando no tiene el formato correcto.");
                    continue;
                }

                Email foundEmail = null!;
                List<Email> list = mainClient.mailbox.DraftEmails;
                if (lastView == "send") list = mainClient.mailbox.SendEmails;
                else if (lastView == "receive") list = mainClient.mailbox.ReceivedEmails;

                foreach (Email email in list)
                {
                    if (email.Subject == subjectToFind)
                    {
                        foundEmail = email;
                        break;
                    }
                }

                if (foundEmail == null)
                {
                    Console.WriteLine($"No se encontró ningún correo electrónico con el asunto '{subjectToFind}' en la última carpeta abierta");
                    continue;
                }

                foundEmail.Open();
            }
            else if (command == "receive email")
            {
                // Similarmente, aquí necesitarías buscar al cliente que está recibiendo el correo electrónico
                // y luego parsear el número de correo electrónico del comando
                string userNameToFind = mainClient!.username;
                
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

                
                foundClient.mailbox.ReceivedEmails.Add(foundClient.ReceiveEmail(i));
                Console.WriteLine("Correo recibido satisfactoriamente.");
            }
            else if (command == "help")
            {
                Console.WriteLine("Comandos disponibles:");
                Console.WriteLine("  login <nombre del cliente>: Permite acceder a codigos de cliente");
                Console.WriteLine("  create client <nombre>: Crea un nuevo cliente con el nombre especificado");
                Console.WriteLine("  create email *necesita login*: Crea un nuevo correo electrónico");
                Console.WriteLine("  send email <correo electrónico> *necesita login*: Envía un correo electrónico desde el cliente especificado");
                Console.WriteLine("  receive email *necesita login*: Recibe un correo electrónico en el cliente logueado");
                Console.WriteLine("  view folder <folder> *necesita login*: Imprime la carpeta de correos seleccionada");
                Console.WriteLine("  view email <correo electrónico> *necesita login*: Imprime el correo descrito");
                Console.WriteLine("  exit: Sale del programa");
            }
            else if (command == "exit")
            {
                server.Stop();
                Console.WriteLine("Servidor detenido.");
                break;
            }
            else
            {
                Console.WriteLine("Comando no reconocido. Escribe 'help' para ver la lista de comandos disponibles.");
            }
        }
    }
}
