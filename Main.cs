using NStack;
using program;

using System;
using System.ComponentModel.DataAnnotations;
using Terminal.Gui;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();
        var top = Application.Top;

        // Configurar el esquema de colores para el texto azul
        Colors.Base.Normal = Application.Driver.MakeAttribute(Color.BrightMagenta, Color.Black);
        Colors.Base.Focus = Application.Driver.MakeAttribute(Color.White, Color.DarkGray);
        Colors.Base.HotNormal = Application.Driver.MakeAttribute(Color.White, Color.BrightMagenta);
        Colors.Base.HotFocus = Application.Driver.MakeAttribute(Color.BrightMagenta, Color.Black);
        Colors.Base.Disabled = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black);
        var win = new Window("Cliente SMTP en Terminal")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        top.Add(win);

        // Configurar los campos de texto con bordes ovalados (solución alternativa)
        // Nota: Terminal.Gui no soporta directamente bordes ovalados, por lo que esta parte es conceptual.

        var scrollView = new ScrollView
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(100),
            Height = Dim.Percent(100)
        };
        win.Add(scrollView);
        SmtpServer server = new SmtpServer();
        Client mainClient = null!;
        string lastView = "";

        Thread serverThread = new Thread(new ThreadStart(server.Start));
        serverThread.Start();

        // Ejemplo de un campo de texto para el inicio de sesión

        var loginButton = new Button("Login") { X = 60, Y = 2 };
        loginButton.Clicked += () =>
        {
            Application.Top.Clear();
            var newWindow = new Window("Login")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Agregar contenido a la nueva ventana
            var label = new Label("Ingrese su nombre")
            {
                X = 3,
                Y = 1
            };
            newWindow.Add(label);

            // Agregar la nueva ventana al Application.Top
            var loginLabel = new Label("Login:") { X = 3, Y = 2 };
            var loginText = new TextField("") { X = 10, Y = 2, Width = 40 };
            var loginButton = new Button("Login") { X = 55, Y = 2 };
            var backButton = new Button("Back") { X = 20, Y = 5 };
            newWindow.Add(loginLabel, loginText, loginButton, backButton);
            loginButton.Clicked += () =>
            {
                try
                {

                    string userNameToFind = loginText.Text.ToString()!;
                    Client foundClient = null!;
                    foreach (Client client in server.clients)
                    {
                        if (client.username == userNameToFind)
                        {
                            foundClient = client;
                            break;
                        }
                    }

                    if (foundClient == null)
                    {
                        throw new Exception($"No se encontró ningún cliente con el nombre de usuario '{userNameToFind}'");


                    }
                    else
                    {
                        mainClient = foundClient;

                        MessageBox.Query(20, 7, "Login", "Login exitoso", "Ok");
                    }
                }
                catch (Exception e)
                {

                    MessageBox.Query(20, 7, "Error", e.Message, "Ok");
                }


            };
            backButton.Clicked += () =>
            {
                Application.RequestStop();
            };
            // Hacer visible la nueva ventana
            Application.Run(newWindow);
        };
        win.Add(loginButton);



        // Por ejemplo, un botón para crear un nuevo cliente
        var createClientButton = new Button("Create Client") { X = 60, Y = 4 };
        createClientButton.Clicked += () =>
        {
            Application.Top.Clear();
            var newWindow = new Window("Create Client")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            var label = new Label("Ingrese su nombre")
            {
                X = 3,
                Y = 1
            };
            newWindow.Add(label);

            // Agregar la nueva ventana al Application.Top
            var createLabel = new Label("UserName:") { X = 3, Y = 2 };
            var createText = new TextField("") { X = 10, Y = 2, Width = 40 };
            var createButton = new Button("Create") { X = 55, Y = 2 };
            var backButton = new Button("Back") { X = 20, Y = 5 };
            newWindow.Add(createButton, createText, createButton, backButton);
            createButton.Clicked += () =>
            {
                try
                {
                    string userName = createText.Text.ToString()!;
                    Client client = new Client(userName);
                    server.clients.Add(client);
                    MessageBox.Query(20, 7, "Create Client", $"Cliente {userName} creado.", "Ok");

                }
                catch (System.Exception)
                {

                    throw;
                }
            };
            backButton.Clicked += () =>
           {
               Application.RequestStop();
           };
            // Hacer visible la nueva ventana
            Application.Run(newWindow);
        };
        win.Add(createClientButton);

        // Ejemplo de un botón para enviar un correo electrónico
        var sendEmailButton = new Button("Send Email") { X = 60, Y = 6 };
        sendEmailButton.Clicked += () =>
        {
            if (mainClient != null)
            {


                Application.Top.Clear();
                var newWindow = new Window("Send email")
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };


                var portLabel = new Label("Puerto:") { X = 50, Y = 1 };
                var port = new TextField("25") { X = 50, Y = 2, Width = 40 };
                newWindow.Add(portLabel, port);

                var usernameLabel = new Label("Nombre de usuario:") { X = 3, Y = 1 };
                var username = new TextField(mainClient.username) { X = 3, Y = 2, Width = 40 };
                newWindow.Add(usernameLabel, username);


                var recipientLabel = new Label("Destinatario:") { X = 3, Y = 4 };
                var recipient = new TextField("") { X = 3, Y = 5, Width = 40 };
                newWindow.Add(recipientLabel, recipient);

                var subjectLabel = new Label("Asunto:") { X = 3, Y = 7 };
                var subject = new TextField("") { X = 3, Y = 8, Width = 40 };
                newWindow.Add(subjectLabel, subject);

                var bodyLabel = new Label("Cuerpo del mensaje:") { X = 3, Y = 10 };
                var body = new TextField("") { X = 3, Y = 11, Width = 50, Height = 50 }; // Aumentar la altura
                newWindow.Add(bodyLabel, body);

                var attachmentLabel = new Label("Archivo Adjunto:") { X = 3, Y = 13 };
                var attachment = new TextField("") { X = 3, Y = 14, Width = 30, Height = 1 }; // Aumentar la altura
                newWindow.Add(attachmentLabel, attachment);
                var backButton = new Button("Back") { X = 20, Y = 16 };
                newWindow.Add(backButton);

                var sendButton = new Button("Send ✉️") { X = 6, Y = 16 };
                sendButton.Clicked += () =>
                {
                    try
                    {

                        {

                            Email email = new Email()
                            {
                                From = username.Text.ToString()!,
                                To = recipient.Text.ToString()!,
                                Subject = subject.Text.ToString()!,
                                Content = body.Text.ToString()!,
                                Received = DateTime.Now,
                                Unread = true,
                                HasAttachment = false,
                                IsImportant = false
                            };
                        
                            string userNameToFind = email.To;
                            Client foundClient = null!;
                            foreach (Client client in server.clients)
                            {
                                if (client.username == userNameToFind)
                                {
                                    foundClient = client;
                                    break;
                                }
                            }

                            if (foundClient == null)
                            {
                                MessageBox.Query(20, 7, "Error", $"No se encontró ningún cliente con el nombre de usuario '{userNameToFind}'", "Ok");


                            }

                            server.emails.Add(email);
                            foundClient.SendEmail(email);
                            mainClient.mailbox.SendEmails.Add(email);
                            //mainClient.mailbox.DraftEmails.Remove(email);

                            MessageBox.Query(20, 7, "Éxito", "Correo enviado exitosamente.", "Ok");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Query(20, 7, "Error", "Error al enviar el correo: " + ex.Message, "Ok");
                    }
                };
                newWindow.Add(sendButton);
                backButton.Clicked += () =>
                {
                    Application.RequestStop();
                };
                // Hacer visible la nueva ventana
                Application.Run(newWindow);
            }
            else { MessageBox.Query(20, 7, "Error", "Necesita registrarse ", "Ok"); }
        };
        win.Add(sendEmailButton);

        // Ejemplo de un botón para recibir un correo electrónico
        var receiveEmailButton = new Button("Receive Email") { X = 60, Y = 8 };
        receiveEmailButton.Clicked += () =>
        {
            if (mainClient != null)
            {

                Application.Top.Clear();
                var newWindow = new Window("Receive email")
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                var backButton = new Button("Back") { X = 20, Y = 8 };
                newWindow.Add(backButton);
                var receiveEmailButton = new Button("Receive Email") { X = 3, Y = 8 };
                newWindow.Add(receiveEmailButton);
                receiveEmailButton.Clicked += () =>
                {
                    List<int> list = new List<int>();
                    string userNameToFind = mainClient!.username;

                    int i = 0;
                    foreach (Email email in server.emails)
                    {
                        if (email.To == userNameToFind)
                        {
                            list.Add(i);
                        }
                        i++;
                    }

                    if (list.Count() != 0)
                    {
                        Client foundClient = null!;
                        foreach (Client client in server.clients)
                        {
                            if (client.username == userNameToFind)
                            {
                                foundClient = client;
                                break;
                            }
                        }
                        if (foundClient == null)
                        {
                            MessageBox.Query(20, 7, "Error", $"No se encontró ningún cliente con el nombre de usuario '{userNameToFind}'", "Ok");
                        }

                        for (int j = 0; j < list.Count(); j++)
                        {
                            foundClient.mailbox.ReceivedEmails.Add(foundClient.ReceiveEmail(j));
                        }
                        MessageBox.Query(20, 7, "Receive Email", "Correo(s) recibido(s) satisfactoriamente", "Ok");
                    }
                    else
                    {
                        MessageBox.Query(20, 7, "Error", "No se encontró ningún email para recibir", "Ok");
                    }
                };
                backButton.Clicked += () =>
               {
                   Application.RequestStop();
               };
                Application.Run(newWindow);
            }
            else { MessageBox.Query(20, 7, "Error", "Necesita registrarse ", "Ok"); }
        };
        win.Add(receiveEmailButton);


        // Ejemplo de un botón para visualizar carpetas de correo
        var viewFolderButton = new Button("View Folder") { X = 60, Y = 10 };
        viewFolderButton.Clicked += () =>
        {
            if (mainClient != null)
            {

                Application.Top.Clear();
                var newWindow = new Window("View Folder")
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                var backButton = new Button("Back") { X = 20, Y = 10 };
                newWindow.Add(backButton);
                var viewFolderButton = new Button("View Folder") { X = 3, Y = 10 };
                newWindow.Add(viewFolderButton);
                var rect = new Rect(1, 1, 20, 3);

                // Definir las etiquetas de los botones de radio
                var radioLabels = new ustring[] { "send", "receive"};
                var radioGroup = new RadioGroup(rect, radioLabels, 0);
                newWindow.Add(radioGroup);


                viewFolderButton.Clicked += () =>
                {
                    if (mainClient != null)
                    {
                        
                        string listView;
                        var selectedIndex = radioGroup.SelectedItem;
                        var selectedLabel = radioLabels[selectedIndex];
                        listView = selectedLabel.ToString()!;
                        List<Email> list = new List<Email>();
                        if (listView == "receive") list = mainClient.mailbox.ReceivedEmails;
                        else if (listView == "send") list = mainClient.mailbox.SendEmails;

                        var head = new Label(string.Format("{0,-30} {1,-30} {2,-30}", "De", "Asunto", ""))
                        {
                            X = 3,
                            Y = 12,
                            Width = Dim.Fill(),
                            Height = 1
                        };
                        var head1 = new Label(string.Format("{0,-30} {1,-30} {2,-30}", "Para", "Asunto", ""))
                        {
                            X = 3,
                            Y = 12,
                            Width = Dim.Fill(),
                            Height = 1
                        };
                        if (listView == "send") newWindow.Add(head1);
                        else if (listView == "receive") newWindow.Add(head);

                        var rect1 = new Rect(3, 14, 20, 3);
                        var radioLabels1 = new List<ustring>();
                        foreach (var email in list)
                        {
                            radioLabels1.Add(string.Format("{0,-30} {1,-30} {2,-30}", email.To, email.Subject, email.Received));
                        }
                        var radioGroup1 = new RadioGroup(rect1, radioLabels1.ToArray(), 0);
                        newWindow.Add(radioGroup1);
                        var viewButton = new Button("View Email") { X = 100, Y = 16 };
                        newWindow.Add(viewButton);
                        viewButton.Clicked += () =>
                        {
                            
                            var selectedIndex = radioGroup1.SelectedItem;
                            var selectedLabel = radioLabels1[selectedIndex].ToString();
                            string[] partes = selectedLabel!.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            string subjectToFind = "";
                            for (int i = 1; i < partes.Length - 3; i++)
                            {
                                subjectToFind += partes[i] + " ";

                            }
                            string sinUltimoCaracter = subjectToFind.Substring(0, subjectToFind.Length - 1);
                            System.Console.WriteLine(sinUltimoCaracter);
                            System.Console.WriteLine(sinUltimoCaracter[sinUltimoCaracter.Length - 1]);
                            Email foundEmail = null!;
                            foreach (Email email in list)
                            {
                                if (email.Subject == sinUltimoCaracter)
                                {
                                    foundEmail = email;
                                    break;
                                }
                            }
                            var emailprinter = new Label("Contenido del correo\n" + foundEmail.Content)
                            {
                                X = 3,
                                Y = radioGroup1.Y + 6,
                                Width = Dim.Fill(),
                                Height = 25
                            };
                            newWindow.Add(emailprinter);


                        };

                    };


                };

                backButton.Clicked += () =>
              {
                  Application.RequestStop();
              };
                Application.Run(newWindow);
            }
            else { MessageBox.Query(20, 7, "Error", "Necesita registrarse ", "Ok"); }
        };
        win.Add(viewFolderButton);

        var exitButton = new Button("Exit") { X = 60, Y = 12 };
        exitButton.Clicked += () =>
        {
            server.Stop();
            Application.RequestStop();

        };
        win.Add(exitButton);
        Application.Run();
    }
}


