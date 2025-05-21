using System; // Dostęp do podstawowych klas .NET
using System.Windows; // Obsługa okien WPF
using System.Windows.Controls; // Dostęp do kontrolek WPF (np. Canvas, Label)
using System.Windows.Input; // Obsługa klawiatury i myszy
using System.Windows.Media.Imaging; // Obsługa obrazków w formacie bitmapy

namespace Gra2D // Przestrzeń nazw dla gry 2D
{
    public partial class MainWindow : Window // Główne okno gry
    {
        // Stałe definiujące rozmiary i prawdopodobieństwa w grze
        private const int TILE = 64; // Rozmiar jednego kafelka (64 piksele)
        private const int WIDTH = 24; // Liczba kafelków w poziomie
        private const int HEIGHT = 16; // Liczba kafelków w pionie
        private const double DRZEWO_SZANSA = 0.25; // Szansa na pojawienie się drzewa
        private const double SKALA_SZANSA = 0.1; // Szansa na pojawienie się skały

        // Zmienne przechowujące stan gry
        private int[,] mapa; // Dwuwymiarowa tablica reprezentująca typy pól na mapie
        private Image[,] kafelki; // Tablica graficznych kafelków (obiektów Image)
        private Image gracz; // Obiekt reprezentujący gracza
        private int graczX, graczY; // Pozycja gracza na mapie
        private int drewno; // Liczba zebranych drzew
        private Random los = new Random(); // Generator liczb losowych

        // Wczytane obrazki do gry
        private readonly BitmapImage imgGracz = Img("gracz.png"); // Obrazek gracza
        private readonly BitmapImage imgDrzewo = Img("las.png"); // Obrazek drzewa
        private readonly BitmapImage imgLaka = Img("laka.png"); // Obrazek łąki
        private readonly BitmapImage imgSkala = Img("skala.png"); // Obrazek skały


        public MainWindow() // Konstruktor okna gry
        {
            InitializeComponent(); // Inicjalizacja komponentów XAML
            WindowState = WindowState.Maximized; // Pełny ekran
            WindowStyle = WindowStyle.None; // Ukrycie paska tytułu
            ResizeMode = ResizeMode.NoResize; // Blokada zmiany rozmiaru okna
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NowaGra_Click(null, null); // Uruchomienie nowej gry po załadowaniu okna
        }

        // Funkcja ładująca obrazki z pliku
        private static BitmapImage Img(string nazwa) =>
            new BitmapImage(new Uri(nazwa, UriKind.Relative)); // Tworzy BitmapImage z pliku

        // Nowa gra - reset stanu i generacja mapy
        private void NowaGra_Click(object sender, RoutedEventArgs e)
        {
            gameCanvas.Children.Clear(); // Usunięcie starych obiektów z canvasu
            drewno = 0; // Reset liczby drewna
            lblDrewno.Content = $"Drewno: {drewno}"; // Aktualizacja etykiety
            GenerujMape(); // Generowanie nowej mapy
            UmiescGracza(); // Ustawienie gracza na mapie
        }

        // Generowanie nowej mapy z losowymi obiektami
        private void GenerujMape()
        {
            mapa = new int[HEIGHT, WIDTH]; // Inicjalizacja mapy
            kafelki = new Image[HEIGHT, WIDTH]; // Inicjalizacja graficznych kafelków

            for (int y = 0; y < HEIGHT; y++) // Iteracja po wierszach mapy
            {
                for (int x = 0; x < WIDTH; x++) // Iteracja po kolumnach mapy
                {
                    double szansa = los.NextDouble(); // Losowanie liczby od 0.0 do 1.0
                    if (szansa < SKALA_SZANSA) mapa[y, x] = 2; // Skała
                    else if (szansa < SKALA_SZANSA + DRZEWO_SZANSA) mapa[y, x] = 1; // Drzewo
                    else mapa[y, x] = 0; // Łąka

                    var kafel = new Image // Tworzenie graficznego kafelka
                    {
                        Width = TILE, // Ustawienie szerokości kafelka
                        Height = TILE, // Ustawienie wysokości kafelka
                        Source = Obrazek(mapa[y, x]), // Obrazek w zależności od typu pola
                        ToolTip = Opis(mapa[y, x]) // Podpowiedź po najechaniu myszą
                    };

                    Canvas.SetLeft(kafel, x * TILE); // Pozycjonowanie X na canvasie
                    Canvas.SetTop(kafel, y * TILE); // Pozycjonowanie Y na canvasie
                    gameCanvas.Children.Add(kafel); // Dodanie kafelka do canvasu
                    kafelki[y, x] = kafel; // Zapisanie referencji w tablicy
                }
            }

            gameCanvas.Width = WIDTH * TILE; // Szerokość canvasu na podstawie mapy
            gameCanvas.Height = HEIGHT * TILE; // Wysokość canvasu na podstawie mapy
        }

        // Zwraca obrazek odpowiadający danemu typowi pola
        private BitmapImage Obrazek(int typ) => typ switch
        {
            1 => imgDrzewo, // Typ 1 = drzewo
            2 => imgSkala, // Typ 2 = skała
            _ => imgLaka // Inne (0) = łąka
        };

        // Zwraca tekst do wyświetlenia po najechaniu na dany typ pola
        private string Opis(int typ) => typ switch
        {
            1 => "Drzewo (SPACJA = zetnij)",
            2 => "Skała (blokuje drogę)",
            _ => "Łąka (można wejść)"
        };

        // Umieszcza gracza w losowym miejscu na łące
        private void UmiescGracza()
        {
            int x, y; // Tymczasowe współrzędne
            do
            {
                x = los.Next(WIDTH); // Losowanie X w zakresie szerokości mapy
                y = los.Next(HEIGHT); // Losowanie Y w zakresie wysokości mapy
            } while (mapa[y, x] != 0); // Powtarzaj, dopóki nie trafisz na łąkę (0)

            graczX = x; // Ustawienie pozycji gracza X
            graczY = y; // Ustawienie pozycji gracza Y

            if (gracz == null) // Jeżeli gracz jeszcze nie istnieje graficznie
            {
                gracz = new Image
                {
                    Source = imgGracz, // Obrazek gracza
                    Width = TILE, // Szerokość
                    Height = TILE // Wysokość
                };
                gameCanvas.Children.Add(gracz); // Dodanie gracza do canvasu
            }

            Canvas.SetLeft(gracz, graczX * TILE); // Pozycja gracza X na canvasie
            Canvas.SetTop(gracz, graczY * TILE); // Pozycja gracza Y na canvasie
            Panel.SetZIndex(gracz, 1); // Gracz ma być nad innymi obiektami
        }

        // Obsługa klawiatury
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int nx = graczX, ny = graczY; // Nowe współrzędne gracza

            switch (e.Key) // Sprawdzenie naciśniętego klawisza
            {
                case Key.W: case Key.Up: ny--; break; // Góra
                case Key.S: case Key.Down: ny++; break; // Dół
                case Key.A: case Key.Left: nx--; break; // Lewo
                case Key.D: case Key.Right: nx++; break; // Prawo
                case Key.Space: Zetnij(); return; // Ścięcie drzewa
                case Key.Escape: Wyjd_Click(null, null); return; // Wyjście z gry
            }

            if (MoznaIsc(nx, ny)) // Sprawdzenie czy można się ruszyć
            {
                graczX = nx; // Zmiana pozycji gracza X
                graczY = ny; // Zmiana pozycji gracza Y
                Canvas.SetLeft(gracz, graczX * TILE); // Aktualizacja pozycji na canvasie
                Canvas.SetTop(gracz, graczY * TILE); // Aktualizacja pozycji na canvasie
            }
        }

        // Sprawdza czy pole jest dostępne (czy nie ma skały)
        private bool MoznaIsc(int x, int y)
        {
            if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT) // Poza mapą
                return false;
            return mapa[y, x] != 2; // Nie można wejść na skałę (typ 2)
        }

        // Próba ścięcia drzewa obok gracza
        private void Zetnij()
        {
            for (int y = Math.Max(0, graczY - 1); y <= Math.Min(HEIGHT - 1, graczY + 1); y++) // Sprawdź 3 wiersze wokół gracza
            {
                for (int x = Math.Max(0, graczX - 1); x <= Math.Min(WIDTH - 1, graczX + 1); x++) // Sprawdź 3 kolumny wokół gracza
                {
                    if (mapa[y, x] == 1) // Jeśli jest drzewo (typ 1)
                    {
                        mapa[y, x] = 0; // Zamień na łąkę (po ścięciu)

                        kafelki[y, x].ToolTip = "Pień (ściąte drzewo)"; // Zmień podpowiedź
                        drewno++; // Zwiększ licznik drewna
                        lblDrewno.Content = $"Drewno: {drewno}"; // Odśwież etykietę
                        System.Media.SystemSounds.Hand.Play(); // Odtwórz dźwięk systemowy
                        return; // Przerwij po pierwszym znalezionym drzewie
                    }
                }
            }
        }

        // Obsługa kliknięcia przycisku "Wyjdź"
        private void Wyjd_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Wyjść z gry?", "Potwierdź", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Close(); // Zamknięcie gry
        }

        // Potwierdzenie zamknięcia okna gry (np. kliknięcie X)
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Na pewno wychodzisz?", "Żegnaj", MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true; // Anulowanie zamknięcia okna
        }
    }
}

