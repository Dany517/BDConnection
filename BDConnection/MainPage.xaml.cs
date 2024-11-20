

using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BDConnection
{
    public partial class MainPage : ContentPage
    {
        private string connectionString = "Server=databasepoe.cfko0iqhcsi0.us-east-1.rds.amazonaws.com;Database=mibasededatos;User ID=admin;Password=POE$2024";
        private string currentUser = string.Empty;
        private ObservableCollection<Book> libros;
        public bool IsUserLoggedIn => !string.IsNullOrEmpty(currentUser);

        public MainPage()
        {
            InitializeComponent();
            libros = new ObservableCollection<Book>();
            librosListView.ItemsSource = libros;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string username = await DisplayPromptAsync("Iniciar sesión", "Ingrese su usuario:");
            string password = await DisplayPromptAsync("Iniciar sesión", "Ingrese su contraseña:", keyboard: Keyboard.Numeric);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Por favor ingrese un usuario y una contraseña", "OK");
                return;
            }

            bool loginExitoso = await ValidarUsuarioAsync(username, password);

            if (loginExitoso)
            {
                currentUser = username;
                await DisplayAlert("Éxito", "Login exitoso", "OK");
                ModifyUserButton.IsVisible = true;
            }
            else
            {
                await DisplayAlert("Error", "Usuario o contraseña incorrectos", "OK");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string newUsername = await DisplayPromptAsync("Registrar Usuario", "Ingrese el nombre de usuario:");
            string newPassword = await DisplayPromptAsync("Registrar Usuario", "Ingrese la contraseña:", keyboard: Keyboard.Numeric);
            string email = await DisplayPromptAsync("Registrar Usuario", "Ingrese su correo electrónico:");

            if (string.IsNullOrEmpty(newUsername) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
                return;
            }

            if (!email.EndsWith("@udg.mx"))
            {
                await DisplayAlert("Error", "El correo debe terminar en @udg.mx", "OK");
                return;
            }

            bool registroExitoso = await RegistrarUsuarioAsync(newUsername, newPassword, email);

            if (registroExitoso)
            {
                await DisplayAlert("Éxito", "Usuario registrado exitosamente", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar el usuario", "OK");
            }
        }

        private async Task<bool> ModificarUsuarioAsync(string oldUsername, string newUsername)
        {
            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "UPDATE Usuarios SET Usuario = @NewUsuario WHERE Usuario = @OldUsuario";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@NewUsuario", newUsername);
                        cmd.Parameters.AddWithValue("@OldUsuario", oldUsername);

                        int resultado = await cmd.ExecuteNonQueryAsync();
                        return resultado > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al modificar el usuario: " + ex.Message, "OK");
                return false;
            }
        }
        private async void OnModifyUserClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentUser))
            {
                await DisplayAlert("Error", "Debe iniciar sesión primero", "OK");
                return;
            }

            string newUsername = await DisplayPromptAsync("Modificar Usuario", "Ingrese el nuevo nombre de usuario:");

            if (string.IsNullOrEmpty(newUsername))
            {
                await DisplayAlert("Error", "El nombre de usuario no puede estar vacío", "OK");
                return;
            }

            bool success = await ModificarUsuarioAsync(currentUser, newUsername);

            if (success)
            {
                currentUser = newUsername;
                await DisplayAlert("Éxito", "Usuario modificado exitosamente", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo modificar el usuario", "OK");
            }
        }
        private async void OnDeleteBookClicked(object sender, EventArgs e)
        {
            // Verificar si el usuario está logueado
            if (string.IsNullOrEmpty(currentUser))
            {
                await DisplayAlert("Error", "Debe iniciar sesión para eliminar un libro", "OK");
                return;
            }

            // Solicitar al usuario el nombre del libro que desea eliminar
            string bookName = await DisplayPromptAsync("Eliminar Libro", "Ingrese el nombre del libro a eliminar:");

            if (string.IsNullOrEmpty(bookName))
            {
                await DisplayAlert("Error", "Debe ingresar el nombre del libro", "OK");
                return;
            }

            bool confirmDelete = await DisplayAlert("Confirmación", $"¿Está seguro de que desea eliminar el libro '{bookName}'?", "Sí", "No");

            if (!confirmDelete)
                return;

            bool success = await EliminarLibroAsync(bookName);

            if (success)
            {
                await DisplayAlert("Éxito", "Libro eliminado exitosamente", "OK");
                await LoadLibrosAsync();  // Recargar la lista de libros
            }
            else
            {
                await DisplayAlert("Error", "No se pudo eliminar el libro", "OK");
            }
        }



        private async Task<bool> EliminarLibroAsync(string nombreLibro)
        {
            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "DELETE FROM Libros WHERE Nombre = @Nombre";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombreLibro);

                        int resultado = await cmd.ExecuteNonQueryAsync();
                        return resultado > 0;  // Si se eliminaron filas, devuelve true
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al eliminar el libro: " + ex.Message, "OK");
                return false;
            }
        }










        private async void OnDeleteUserClicked(object sender, EventArgs e)
        {
            string username = await DisplayPromptAsync("Eliminar Usuario", "Ingrese el nombre de usuario que desea eliminar:");

            if (string.IsNullOrEmpty(username))
            {
                await DisplayAlert("Error", "Debe ingresar el nombre de usuario", "OK");
                return;
            }

            bool confirmDelete = await DisplayAlert("Confirmación", "¿Está seguro de que desea eliminar este usuario?", "Sí", "No");
            if (!confirmDelete) return;

            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "DELETE FROM Usuarios WHERE Usuario = @Usuario";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", username);

                        int resultado = await cmd.ExecuteNonQueryAsync();
                        if (resultado > 0)
                        {
                            await DisplayAlert("Éxito", "Usuario eliminado exitosamente", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Error", "No se pudo eliminar el usuario", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al eliminar el usuario: " + ex.Message, "OK");
            }
        }

        private async void OnRegisterBookClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentUser))
            {
                await DisplayAlert("Error", "Debe iniciar sesión para registrar un libro", "OK");
                return;
            }

            string nombre = nombreLibroEntry.Text;
            string autor = autorLibroEntry.Text;
            string paginas = paginasLibroEntry.Text;
            string tomo = tomoLibroEntry.Text;

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(autor) || string.IsNullOrEmpty(paginas) || string.IsNullOrEmpty(tomo))
            {
                await DisplayAlert("Error", "Por favor complete todos los campos", "OK");
                return;
            }

            bool success = await RegistrarLibroAsync(nombre, autor, paginas, tomo);
            if (success)
            {
                await DisplayAlert("Éxito", "Libro registrado exitosamente", "OK");
                ClearFields();
                await LoadLibrosAsync();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar el libro", "OK");
            }
        }
        private async Task<bool> RegistrarLibroAsync(string nombre, string autor, string paginas, string tomo)
        {
            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    // Preparar la consulta para insertar un nuevo libro
                    string query = "INSERT INTO Libros (Nombre, Autor, Paginas, Tomo) VALUES (@Nombre, @Autor, @Paginas, @Tomo)";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        // Asignar los parámetros
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Autor", autor);
                        cmd.Parameters.AddWithValue("@Paginas", paginas);
                        cmd.Parameters.AddWithValue("@Tomo", tomo);

                        // Ejecutar la consulta de inserción
                        int resultado = await cmd.ExecuteNonQueryAsync();
                        return resultado > 0; // Devuelve true si se insertó correctamente
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de error, mostrar el mensaje
                await DisplayAlert("Error", "Error al registrar el libro: " + ex.Message, "OK");
                return false;
            }
        }
        private void ClearFields()
        {
            // Limpiar los campos de entrada de texto
            nombreLibroEntry.Text = string.Empty;
            autorLibroEntry.Text = string.Empty;
            paginasLibroEntry.Text = string.Empty;
            tomoLibroEntry.Text = string.Empty;
        }

        private async void OnSearchChanged(object sender, EventArgs e)
        {
            string bookName = buscadorLibroEntry.Text; // Usar el control que tiene el evento SearchChanged

            if (string.IsNullOrEmpty(bookName))
            {
                await DisplayAlert("Error", "Ingrese el nombre del libro a buscar", "OK");
                return;
            }

            var libro = await BuscarLibroAsync(bookName);
            if (libro != null)
            {
                await DisplayAlert("Libro encontrado",
                    $"Nombre: {libro.Nombre}\nAutor: {libro.Autor}\nPáginas: {libro.Paginas}\nTomo: {libro.Tomo}",
                    "OK");
            }
            else
            {
                await DisplayAlert("Error", "Libro no encontrado", "OK");
            }
        }






        private async Task<bool> RegistrarUsuarioAsync(string usuario, string contrasena, string correo)
        {
            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "INSERT INTO Usuarios (Usuario, Contrasena, Correo) VALUES (@Usuario, @Contrasena, @Correo)";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", usuario);
                        cmd.Parameters.AddWithValue("@Contrasena", contrasena);
                        cmd.Parameters.AddWithValue("@Correo", correo);

                        int resultado = await cmd.ExecuteNonQueryAsync();
                        return resultado > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al registrar el usuario: " + ex.Message, "OK");
                return false;
            }
        }

        private async Task<bool> ValidarUsuarioAsync(string usuario, string contrasena)
        {
            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario AND Contrasena = @Contrasena";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Usuario", usuario);
                        cmd.Parameters.AddWithValue("@Contrasena", contrasena);

                        var resultado = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt32(resultado) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al conectar con la base de datos: " + ex.Message, "OK");
                return false;
            }
        }

        private async void OnSearchBookClicked(object sender, EventArgs e)
        {
            string bookName = buscadorLibroEntry.Text; // Obtener el texto del cuadro de búsqueda

            if (string.IsNullOrEmpty(bookName))
            {
                await DisplayAlert("Error", "Debe ingresar el nombre del libro a buscar", "OK");
                return;
            }

            var libro = await BuscarLibroAsync(bookName); // Buscar el libro en la base de datos
            if (libro != null)
            {
                await DisplayAlert("Libro encontrado",
                    $"Nombre: {libro.Nombre}\nAutor: {libro.Autor}\nPáginas: {libro.Paginas}\nTomo: {libro.Tomo}",
                    "OK");
            }
            else
            {
                await DisplayAlert("Error", "Libro no encontrado", "OK");
            }
        }






        private async Task<Book> BuscarLibroAsync(string nombre)
        {
            try
            {
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "SELECT * FROM Libros WHERE Nombre = @Nombre";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombre);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Book
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre"),
                                    Autor = reader.GetString("Autor"),
                                    Paginas = reader.GetString("Paginas"),
                                    Tomo = reader.GetString("Tomo")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al buscar el libro: " + ex.Message, "OK");
            }
            return null;
        }

        private async Task LoadLibrosAsync()
        {
            try
            {
                libros.Clear();
                using (var conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    string query = "SELECT * FROM Libros";
                    using (var cmd = new MySqlCommand(query, conexion))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                libros.Add(new Book
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre"),
                                    Autor = reader.GetString("Autor"),
                                    Paginas = reader.GetString("Paginas"),
                                    Tomo = reader.GetString("Tomo")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error al cargar los libros: " + ex.Message, "OK");
            }
        }
    }







    public class Book
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Autor { get; set; }
        public string Paginas { get; set; }
        public string Tomo { get; set; }
    }
}
