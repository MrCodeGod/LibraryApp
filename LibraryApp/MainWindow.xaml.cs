using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace LibrarySimple
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString =
            "Server=localhost;Database=library_simple;Uid=root;Pwd=Enter_Password_Here!;SslMode=Preferred;";

        public MainWindow()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
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

                var cmd = new MySqlCommand(sql, conn);

                DataTable table = new DataTable();
                table.Load(cmd.ExecuteReader());

                BooksGrid.ItemsSource = table.DefaultView;
            }
        }
    }
}