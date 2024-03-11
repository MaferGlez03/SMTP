using System;
using System.Net.Mail;
using Terminal.Gui;
using SMTPClient;
using System.Net.Sockets;

namespace SmtpClientGui
{
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
            var smtpServerLabel = new Label("Servidor SMTP:") { X = 3, Y = 1 };
            var smtpServer = new TextField("") { X = 3, Y = 2, Width = 40 };
            win.Add(smtpServerLabel, smtpServer);

            var portLabel = new Label("Puerto:") { X = 50, Y = 1 };
            var port = new TextField("") { X = 50, Y = 2, Width = 40 };
            win.Add(portLabel, port);

            var usernameLabel = new Label("Nombre de usuario:") { X = 3, Y = 4 };
            var username = new TextField("") { X = 3, Y = 5, Width = 40 };
            win.Add(usernameLabel, username);

            var passwordLabel = new Label("Contraseña:") { X = 50, Y = 4 };
            var password = new TextField("") { X = 50, Y = 5, Width = 40 };
            win.Add(passwordLabel, password);

            var recipientLabel = new Label("Destinatario:") { X = 3, Y = 7 };
            var recipient = new TextField("") { X = 3, Y = 8, Width = 40 };
            win.Add(recipientLabel, recipient);

            var subjectLabel = new Label("Asunto:") { X = 3, Y = 9 };
            var subject = new TextField("") { X = 3, Y = 10, Width = 40 };
            win.Add(subjectLabel, subject);

            var bodyLabel = new Label("Cuerpo del mensaje:") { X = 3, Y = 11 };
            var body = new TextField("") { X = 3, Y = 12, Width = 30, Height = 1 }; // Aumentar la altura
            win.Add(bodyLabel, body);


            var sendButton = new Button("Enviar ✉️") { X = 6, Y = 17 };
            sendButton.Clicked += () =>
            {
                try
                {
                    
                    {
                        // client.Credentials = new System.Net.NetworkCredential(username.Text.ToString(), password.Text.ToString());
                        // client.EnableSsl = true;

                        Email email = new Email();
                        email.From = username.Text.ToString()!;
                        email.To = recipient.Text.ToString()!;
                        email.Subject = subject.Text.ToString()!;
                        email.Content = body.Text.ToString()!;

                        email.SendEmail();
                        MessageBox.Query(20, 7, "Éxito", "Correo enviado exitosamente.", "Ok");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Query(20, 7, "Error", "Error al enviar el correo: " + ex.Message, "Ok");
                }
            };

            //  var frameView = new FrameView("")
            // {
            //     X = 3,
            //     Y = 16,
            //     Width = Dim.Percent(50),
            //     Height = Dim.Percent(20)
            // };
            // frameView.Add(sendButton);
            win.Add(sendButton);

            Application.Run();
        }
    }
}
