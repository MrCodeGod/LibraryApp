using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace LibrarySimple
{
    public partial class MainWindow : Window
    {
        private DataTable booksTable = new DataTable();

        public static string ConnectionStringStatic => "Server=localhost;Database=library_simple;Uid=root;Pwd=topsecretpassword1488A!;SslMode=Preferred;";

        public MainWindow()
        {
            InitializeComponent();
            LoadAuthors();
            LoadBooks();
        }

        private void LoadBooks()
        {
            using var conn = new MySqlConnection(ConnectionStringStatic);
            conn.Open();

            string sql = @"
                SELECT 
                    books.id,
                    books.title AS 'Название',
                    authors.name AS 'Автор',
                    genres.name AS 'Жанр'
                FROM books
                LEFT JOIN authors ON books.author_id = authors.id
                LEFT JOIN genres ON books.genre_id = genres.id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            booksTable = new DataTable();
            booksTable.Load(cmd.ExecuteReader());

            BooksGrid.ItemsSource = booksTable.DefaultView;
        }

        private void LoadAuthors()
        {
            using var conn = new MySqlConnection(ConnectionStringStatic);
            conn.Open();

            string sql = "SELECT name FROM authors ORDER BY name;";
            using var cmd = new MySqlCommand(sql, conn);

            DataTable authors = new DataTable();
            authors.Load(cmd.ExecuteReader());

            AuthorFilter.Items.Clear();
            AuthorFilter.Items.Add("Все авторы");

            foreach (DataRow row in authors.Rows)
                AuthorFilter.Items.Add(row["name"].ToString());

            AuthorFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (booksTable == null || booksTable.Rows.Count == 0)
                return;

            string search = SearchBox.Text.ToLower();
            string author = AuthorFilter.SelectedItem?.ToString();

            string filter = "";

            if (!string.IsNullOrWhiteSpace(search))
                filter += $"Название LIKE '%{search}%'";

            if (author != "Все авторы")
            {
                if (filter.Length > 0) filter += " AND ";
                filter += $"Автор = '{author}'";
            }

            booksTable.DefaultView.RowFilter = filter;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void AuthorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var win = new EditBookWindow();
            if (win.ShowDialog() == true)
                LoadBooks();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите книгу");
                return;
            }

            DataRowView row = (DataRowView)BooksGrid.SelectedItem;
            int id = (int)row["id"];

            var win = new EditBookWindow(id);
            if (win.ShowDialog() == true)
                LoadBooks();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BooksGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите книгу");
                return;
            }

            if (!MessageBox.Show("Удалить запись?", "Подтвердите", MessageBoxButton.YesNo)
                .Equals(MessageBoxResult.Yes))
                return;

            DataRowView row = (DataRowView)BooksGrid.SelectedItem;
            int id = (int)row["id"];

            using var conn = new MySqlConnection(ConnectionStringStatic);
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM books WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            LoadBooks();
        }
    }
}