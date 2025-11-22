using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace LibrarySimple
{
    public partial class EditBookWindow : Window
    {
        public int? BookId { get; private set; }

        public EditBookWindow(int? bookId = null)
        {
            InitializeComponent();
            BookId = bookId;

            LoadAuthors();
            LoadGenres();

            if (BookId != null)
                LoadBook();
        }

        private void LoadAuthors()
        {
            using var conn = new MySqlConnection(MainWindow.ConnectionStringStatic);
            conn.Open();

            var cmd = new MySqlCommand("SELECT id, name FROM authors", conn);
            DataTable table = new DataTable();
            table.Load(cmd.ExecuteReader());

            AuthorBox.ItemsSource = table.DefaultView;
            AuthorBox.DisplayMemberPath = "name";
            AuthorBox.SelectedValuePath = "id";
        }

        private void LoadGenres()
        {
            using var conn = new MySqlConnection(MainWindow.ConnectionStringStatic);
            conn.Open();

            var cmd = new MySqlCommand("SELECT id, name FROM genres", conn);
            DataTable table = new DataTable();
            table.Load(cmd.ExecuteReader());

            GenreBox.ItemsSource = table.DefaultView;
            GenreBox.DisplayMemberPath = "name";
            GenreBox.SelectedValuePath = "id";
        }

        private void LoadBook()
        {
            using var conn = new MySqlConnection(MainWindow.ConnectionStringStatic);
            conn.Open();

            var cmd = new MySqlCommand(
                "SELECT title, author_id, genre_id FROM books WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", BookId);

            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                TitleBox.Text = r.GetString("title");
                AuthorBox.SelectedValue = r["author_id"];
                GenreBox.SelectedValue = r["genre_id"];
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            using var conn = new MySqlConnection(MainWindow.ConnectionStringStatic);
            conn.Open();

            MySqlCommand cmd;

            if (BookId == null)
            {
                cmd = new MySqlCommand(
                    "INSERT INTO books(title, author_id, genre_id) VALUES(@t,@a,@g)", conn);
            }
            else
            {
                cmd = new MySqlCommand(
                    "UPDATE books SET title=@t, author_id=@a, genre_id=@g WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", BookId);
            }

            cmd.Parameters.AddWithValue("@t", TitleBox.Text);
            cmd.Parameters.AddWithValue("@a", AuthorBox.SelectedValue);
            cmd.Parameters.AddWithValue("@g", GenreBox.SelectedValue);

            cmd.ExecuteNonQuery();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}