using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AfficheParser
{
    public partial class MainWindow : Window
    {
        List<Film> films = new List<Film>();

        public MainWindow()
        {
            InitializeComponent();
            filmContent.Visibility = Visibility.Hidden;
        }

        private void ParseHtml()
        {
            Parser.ParseFilmsToJson();
            films = Parser.GetFilmsFromJson();
            InitFilms();
        }

        private void InitFilms()
        {
            filmsList.Items.Clear();
            foreach (var film in films)
                filmsList.Items.Add(film.title);
            filmsList.SelectedIndex = 0;
        }

        //Каталог фильмов
        private void ChangeFilm(object sender, SelectionChangedEventArgs e)
        {
            if (films.Count > 0 && filmsList.SelectedIndex > -1)
            {
                filmContent.Visibility = Visibility.Visible;
                Film f = films[filmsList.SelectedIndex];
                filmTitle.Content = f.title;
                filmDescription.Text = f.description;
                filmDirector.Content = f.director;
                filmStartDate.Content = f.startDate;
                filmEndDate.Content = f.endDate;
                filmDuration.Content = f.duration;
                filmActors.Text = f.actors;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(f.poster, UriKind.Absolute);
                bitmap.EndInit();
                filmImage.Source = bitmap;
            }
            else
                filmContent.Visibility = Visibility.Hidden;
        }

        //Поиск
        private void SearchClick(object sender, RoutedEventArgs e)
        {
            string q = filmSearch.Text;
            if (q.Length > 0)
            {
                bool success = false;
                foreach (var (film, id) in Parser.WithIndex(films))
                    if (film.title.ToLower().Contains(q.ToLower()))
                    {
                        filmsList.SelectedIndex = id;
                        success = true;
                    }
                if (!success)
                    MessageBox.Show("Фильм не найден!", "Поиск");
            }
            else
                MessageBox.Show("Введите название!", "Поиск");
        }

        //Изменение
        private void EditClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите изменить описание текущего фильма?", "Подтверждение изменения", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (films.Count > 0 && filmsList.SelectedIndex > -1)
                {
                    films[filmsList.SelectedIndex].description = filmDescription.Text;
                    Parser.SaveFilmsToFile(films);
                    MessageBox.Show("Новое описание сохранено!", "Редактирование");
                }
            }
        }

        //Удаление
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить текущий фильм?", "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (films.Count > 0)
                {
                    MessageBox.Show($"Фильм \"{films[filmsList.SelectedIndex].title}\" удален!", "Удаление");
                    films.RemoveAt(filmsList.SelectedIndex);
                }
                InitFilms();
                filmsList.SelectedIndex = 0;
                Parser.SaveFilmsToFile(films);
            }
        }

        //Обновление
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            ParseHtml();
            filmsList.SelectedIndex = 0;
        }
    }
}
